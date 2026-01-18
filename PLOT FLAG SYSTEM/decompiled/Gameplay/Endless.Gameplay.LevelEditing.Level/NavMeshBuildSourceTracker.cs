using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay.LevelEditing.Level;

public class NavMeshBuildSourceTracker
{
	private readonly List<NavMeshBuildSource> terrainSources = new List<NavMeshBuildSource>();

	private readonly Dictionary<GameObject, List<NavMeshBuildSource>> propSources = new Dictionary<GameObject, List<NavMeshBuildSource>>();

	public List<NavMeshBuildSource> GetNavMeshBuildSources()
	{
		List<NavMeshBuildSource> list = new List<NavMeshBuildSource>(terrainSources);
		foreach (KeyValuePair<GameObject, List<NavMeshBuildSource>> propSource in propSources)
		{
			list.AddRange(propSource.Value);
		}
		return list;
	}

	public void AddTerrainSource(Collider collider)
	{
		if (collider.gameObject.layer != LayerMask.NameToLayer("Default") || !collider.gameObject.activeInHierarchy)
		{
			return;
		}
		NavMeshBuildSource navMeshBuildSource;
		if (!(collider is MeshCollider meshCollider))
		{
			if (!(collider is BoxCollider boxCollider))
			{
				throw new ArgumentException();
			}
			navMeshBuildSource = new NavMeshBuildSource
			{
				area = 0,
				component = boxCollider,
				shape = NavMeshBuildSourceShape.Box,
				transform = boxCollider.transform.localToWorldMatrix * Matrix4x4.Translate(boxCollider.center),
				size = boxCollider.size
			};
		}
		else
		{
			navMeshBuildSource = new NavMeshBuildSource
			{
				area = 0,
				component = meshCollider,
				shape = NavMeshBuildSourceShape.Mesh,
				transform = meshCollider.transform.localToWorldMatrix,
				sourceObject = meshCollider.sharedMesh
			};
		}
		NavMeshBuildSource item = navMeshBuildSource;
		terrainSources.Add(item);
	}

	public void AddPropToSources(GameObject propObject)
	{
		if (propSources.ContainsKey(propObject))
		{
			propSources[propObject] = new List<NavMeshBuildSource>();
		}
		else
		{
			propSources.Add(propObject, new List<NavMeshBuildSource>());
		}
		Collider[] componentsInChildren = propObject.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.gameObject.layer != LayerMask.NameToLayer("Default"))
			{
				continue;
			}
			NavMeshBuildSource item = new NavMeshBuildSource
			{
				area = 0,
				component = collider
			};
			if (!(collider is MeshCollider meshCollider))
			{
				if (!(collider is BoxCollider boxCollider))
				{
					if (!(collider is CapsuleCollider capsuleCollider))
					{
						if (collider is SphereCollider sphereCollider)
						{
							Matrix4x4 transform = collider.transform.localToWorldMatrix * Matrix4x4.Translate(sphereCollider.center);
							item.shape = NavMeshBuildSourceShape.Sphere;
							item.transform = transform;
							item.size = Vector3.one * sphereCollider.radius;
						}
					}
					else
					{
						Transform transform2 = collider.transform;
						Quaternion q = capsuleCollider.direction switch
						{
							0 => Quaternion.Euler(0f, 0f, 90f), 
							2 => Quaternion.Euler(90f, 0f, 0f), 
							_ => Quaternion.identity, 
						};
						Matrix4x4 transform3 = transform2.localToWorldMatrix * Matrix4x4.Translate(capsuleCollider.center) * Matrix4x4.Rotate(q);
						item.shape = NavMeshBuildSourceShape.Capsule;
						item.transform = transform3;
						item.size = new Vector3(capsuleCollider.radius * 2f, capsuleCollider.height, capsuleCollider.radius * 2f);
					}
				}
				else
				{
					Matrix4x4 transform4 = collider.transform.localToWorldMatrix * Matrix4x4.Translate(boxCollider.center);
					item.shape = NavMeshBuildSourceShape.Box;
					item.transform = transform4;
					item.size = boxCollider.size;
				}
			}
			else
			{
				item.shape = NavMeshBuildSourceShape.Mesh;
				item.transform = collider.transform.localToWorldMatrix;
				item.sourceObject = meshCollider.sharedMesh;
			}
			propSources[propObject].Add(item);
		}
	}
}
