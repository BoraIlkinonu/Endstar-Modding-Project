using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200055C RID: 1372
	public class NavMeshBuildSourceTracker
	{
		// Token: 0x06002108 RID: 8456 RVA: 0x00094B20 File Offset: 0x00092D20
		public List<NavMeshBuildSource> GetNavMeshBuildSources()
		{
			List<NavMeshBuildSource> list = new List<NavMeshBuildSource>(this.terrainSources);
			foreach (KeyValuePair<GameObject, List<NavMeshBuildSource>> keyValuePair in this.propSources)
			{
				list.AddRange(keyValuePair.Value);
			}
			return list;
		}

		// Token: 0x06002109 RID: 8457 RVA: 0x00094B88 File Offset: 0x00092D88
		public void AddTerrainSource(Collider collider)
		{
			if (collider.gameObject.layer != LayerMask.NameToLayer("Default"))
			{
				return;
			}
			if (!collider.gameObject.activeInHierarchy)
			{
				return;
			}
			MeshCollider meshCollider = collider as MeshCollider;
			NavMeshBuildSource navMeshBuildSource;
			if (meshCollider == null)
			{
				BoxCollider boxCollider = collider as BoxCollider;
				if (boxCollider == null)
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
			NavMeshBuildSource navMeshBuildSource2 = navMeshBuildSource;
			this.terrainSources.Add(navMeshBuildSource2);
		}

		// Token: 0x0600210A RID: 8458 RVA: 0x00094C80 File Offset: 0x00092E80
		public void AddPropToSources(GameObject propObject)
		{
			if (this.propSources.ContainsKey(propObject))
			{
				this.propSources[propObject] = new List<NavMeshBuildSource>();
			}
			else
			{
				this.propSources.Add(propObject, new List<NavMeshBuildSource>());
			}
			foreach (Collider collider in propObject.GetComponentsInChildren<Collider>())
			{
				if (collider.gameObject.layer == LayerMask.NameToLayer("Default"))
				{
					NavMeshBuildSource navMeshBuildSource = new NavMeshBuildSource
					{
						area = 0,
						component = collider
					};
					MeshCollider meshCollider = collider as MeshCollider;
					if (meshCollider == null)
					{
						BoxCollider boxCollider = collider as BoxCollider;
						if (boxCollider == null)
						{
							CapsuleCollider capsuleCollider = collider as CapsuleCollider;
							if (capsuleCollider == null)
							{
								SphereCollider sphereCollider = collider as SphereCollider;
								if (sphereCollider != null)
								{
									Matrix4x4 matrix4x = collider.transform.localToWorldMatrix * Matrix4x4.Translate(sphereCollider.center);
									navMeshBuildSource.shape = NavMeshBuildSourceShape.Sphere;
									navMeshBuildSource.transform = matrix4x;
									navMeshBuildSource.size = Vector3.one * sphereCollider.radius;
								}
							}
							else
							{
								Transform transform = collider.transform;
								int direction = capsuleCollider.direction;
								Quaternion quaternion;
								if (direction != 0)
								{
									if (direction != 2)
									{
										quaternion = Quaternion.identity;
									}
									else
									{
										quaternion = Quaternion.Euler(90f, 0f, 0f);
									}
								}
								else
								{
									quaternion = Quaternion.Euler(0f, 0f, 90f);
								}
								Quaternion quaternion2 = quaternion;
								Matrix4x4 matrix4x2 = transform.localToWorldMatrix * Matrix4x4.Translate(capsuleCollider.center) * Matrix4x4.Rotate(quaternion2);
								navMeshBuildSource.shape = NavMeshBuildSourceShape.Capsule;
								navMeshBuildSource.transform = matrix4x2;
								navMeshBuildSource.size = new Vector3(capsuleCollider.radius * 2f, capsuleCollider.height, capsuleCollider.radius * 2f);
							}
						}
						else
						{
							Matrix4x4 matrix4x3 = collider.transform.localToWorldMatrix * Matrix4x4.Translate(boxCollider.center);
							navMeshBuildSource.shape = NavMeshBuildSourceShape.Box;
							navMeshBuildSource.transform = matrix4x3;
							navMeshBuildSource.size = boxCollider.size;
						}
					}
					else
					{
						navMeshBuildSource.shape = NavMeshBuildSourceShape.Mesh;
						navMeshBuildSource.transform = collider.transform.localToWorldMatrix;
						navMeshBuildSource.sourceObject = meshCollider.sharedMesh;
					}
					this.propSources[propObject].Add(navMeshBuildSource);
				}
			}
		}

		// Token: 0x04001A4C RID: 6732
		private readonly List<NavMeshBuildSource> terrainSources = new List<NavMeshBuildSource>();

		// Token: 0x04001A4D RID: 6733
		private readonly Dictionary<GameObject, List<NavMeshBuildSource>> propSources = new Dictionary<GameObject, List<NavMeshBuildSource>>();
	}
}
