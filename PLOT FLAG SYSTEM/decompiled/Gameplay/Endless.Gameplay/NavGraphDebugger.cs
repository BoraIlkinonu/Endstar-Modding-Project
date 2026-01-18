using System;
using System.Collections;
using Endless.Shared;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Endless.Gameplay;

public class NavGraphDebugger : MonoBehaviour
{
	[Flags]
	private enum OctantDisplayMode
	{
		Empty = 1,
		Walkable = 2,
		Blocking = 4
	}

	[SerializeField]
	private OctantDisplayMode octantDisplayMode;

	private NativeArray<Octant> octantArray;

	private NativeParallelMultiHashMap<int, int> sectionMap;

	private NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap;

	private NativeParallelHashMap<int, SectionSurface> surfaceMap;

	private NativeParallelMultiHashMap<int, int> islandToSectionMap;

	private NativeParallelMultiHashMap<int, int> areaMap;

	private NativeParallelMultiHashMap<int, Edge> edgeMap;

	public void SetCollections(NativeArray<Octant> octants, NativeParallelMultiHashMap<int, int> sections, NativeParallelHashMap<int, WalkableOctantData> octantData, NativeParallelHashMap<int, SectionSurface> surfaces, NativeParallelMultiHashMap<int, int> islands, NativeParallelMultiHashMap<int, int> areas, NativeParallelMultiHashMap<int, Edge> edges)
	{
		octantArray = octants;
		sectionMap = sections;
		walkableOctantDataMap = octantData;
		surfaceMap = surfaces;
		islandToSectionMap = islands;
		areaMap = areas;
		edgeMap = edges;
	}

	[ContextMenu("DisplayOctutree")]
	public void DisplayOctutree()
	{
		StartCoroutine(DisplayOctutreeRoutine());
	}

	[ContextMenu("DisplayIslands")]
	public void DisplayIslands()
	{
		StartCoroutine(DisplayIslandsRoutine());
	}

	[ContextMenu("Display Sections")]
	public void DisplaySections()
	{
		StartCoroutine(DisplaySectionsRoutine());
	}

	[ContextMenu("Display Surfaces")]
	public void DisplaySurfaces()
	{
		StartCoroutine(DisplaySurfacesRoutine());
	}

	[ContextMenu("Display Edges")]
	public void DisplayEdges()
	{
		StartCoroutine(DisplayEdgesRoutine());
	}

	private IEnumerator DisplayEdgesRoutine()
	{
		(NativeArray<int>, int) keys = sectionMap.GetUniqueKeyArray(AllocatorManager.Persistent);
		for (int i = 0; i < keys.Item2; i++)
		{
			foreach (int item in sectionMap.GetValuesForKey(keys.Item1[i]))
			{
				Octant octant = octantArray[item];
				if (octant.IsWalkable)
				{
					WalkableOctantData walkableOctantData = walkableOctantDataMap[item];
					Vector3 center = octant.Center;
					float offset = octant.Size.x / 2f;
					if (walkableOctantData.Edge.HasFlag(NpcEnum.Edge.North))
					{
						DrawEdge(center, offset, NpcEnum.Edge.North);
					}
					if (walkableOctantData.Edge.HasFlag(NpcEnum.Edge.East))
					{
						DrawEdge(center, offset, NpcEnum.Edge.East);
					}
					if (walkableOctantData.Edge.HasFlag(NpcEnum.Edge.South))
					{
						DrawEdge(center, offset, NpcEnum.Edge.South);
					}
					if (walkableOctantData.Edge.HasFlag(NpcEnum.Edge.West))
					{
						DrawEdge(center, offset, NpcEnum.Edge.West);
					}
				}
			}
			yield return new WaitForSeconds(2f);
		}
	}

	private static void DrawEdge(Vector3 center, float offset, NpcEnum.Edge edge)
	{
		Vector3 vector = edge switch
		{
			NpcEnum.Edge.North => Vector3.forward, 
			NpcEnum.Edge.East => Vector3.right, 
			NpcEnum.Edge.South => Vector3.back, 
			NpcEnum.Edge.West => Vector3.left, 
			_ => throw new ArgumentOutOfRangeException("edge", edge, null), 
		};
		Vector3 vector2 = Vector3.Normalize(Vector3.Cross(vector, Vector3.up));
		center += vector * offset;
		Debug.DrawLine(center + vector2 * offset, center - vector2 * offset, Color.green, 1f);
	}

	private IEnumerator DisplaySurfacesRoutine()
	{
		foreach (KeyValue<int, SectionSurface> item in surfaceMap)
		{
			for (int i = 0; i < 4; i++)
			{
				SectionEdge sectionEdge = item.Value.GetSectionEdge(i);
				Debug.DrawLine(sectionEdge.p1, sectionEdge.p2, Color.cyan, 3f);
			}
			yield return new WaitForSeconds(3f);
		}
	}

	[ContextMenu("Display Island Surfaces")]
	public void DisplaySectionSurfaces()
	{
		StartCoroutine(DisplayIslandSurface());
	}

	private IEnumerator DisplayIslandSurface()
	{
		(NativeArray<int>, int) islandKeys = islandToSectionMap.GetUniqueKeyArray(AllocatorManager.Persistent);
		for (int index = 0; index < islandKeys.Item2; index++)
		{
			int key = islandKeys.Item1[index];
			foreach (int item in islandToSectionMap.GetValuesForKey(key))
			{
				SectionSurface sectionSurface = surfaceMap[item];
				for (int i = 0; i < 4; i++)
				{
					SectionEdge sectionEdge = sectionSurface.GetSectionEdge(i);
					Debug.DrawLine(sectionEdge.p1, sectionEdge.p2, Color.cyan, 3f);
				}
			}
			yield return new WaitForSeconds(3f);
		}
		islandKeys.Item1.Dispose();
	}

	[ContextMenu("Display Areas")]
	public void DisplayAreas()
	{
		StartCoroutine(DisplayAreasRoutine());
	}

	private IEnumerator DisplayAreasRoutine()
	{
		(NativeArray<int>, int) areaKeys = areaMap.GetUniqueKeyArray(AllocatorManager.Persistent);
		for (int i = 0; i < areaKeys.Item2; i++)
		{
			foreach (int item in areaMap.GetValuesForKey(areaKeys.Item1[i]))
			{
				foreach (int item2 in islandToSectionMap.GetValuesForKey(item))
				{
					SectionSurface sectionSurface = surfaceMap[item2];
					for (int j = 0; j < 4; j++)
					{
						SectionEdge sectionEdge = sectionSurface.GetSectionEdge(j);
						Debug.DrawLine(sectionEdge.p1, sectionEdge.p2, Color.cyan, 3f);
					}
				}
			}
			yield return new WaitForSeconds(3f);
		}
		areaKeys.Item1.Dispose();
	}

	[ContextMenu("Display All Walk Connections")]
	private void DisplayAllWalkConnections()
	{
		foreach (KeyValue<int, Edge> item in edgeMap)
		{
			if (item.Value.Connection == ConnectionKind.Walk)
			{
				Vector3 start = surfaceMap[item.Key].Center;
				Vector3 end = surfaceMap[item.Value.ConnectedNodeKey].Center;
				Debug.DrawLine(start, end, Color.cyan, 3f);
			}
		}
	}

	[ContextMenu("Display Jump Connections")]
	private void DisplayJumpConnections()
	{
		StartCoroutine(DisplayJumpConnectionsRoutine());
	}

	private IEnumerator DisplayJumpConnectionsRoutine()
	{
		foreach (KeyValue<int, Edge> item in edgeMap)
		{
			if (item.Value.Connection == ConnectionKind.Jump)
			{
				Vector3 start = surfaceMap[item.Key].Center;
				Vector3 end = surfaceMap[item.Value.ConnectedNodeKey].Center;
				Debug.DrawLine(start, end, Color.cyan, 3f);
				yield return new WaitForSeconds(3f);
			}
		}
	}

	[ContextMenu("Display All Jump Connections")]
	private void DisplayAllJumpConnections()
	{
		foreach (KeyValue<int, Edge> item in edgeMap)
		{
			if (item.Value.Connection == ConnectionKind.Jump)
			{
				Vector3 start = surfaceMap[item.Key].Center;
				Vector3 end = surfaceMap[item.Value.ConnectedNodeKey].Center;
				Debug.DrawLine(start, end, Color.cyan, 3f);
			}
		}
	}

	[ContextMenu("Display Drop Connections")]
	private void DisplayDropConnections()
	{
		StartCoroutine(DisplayDropConnectionsRoutine());
	}

	private IEnumerator DisplayDropConnectionsRoutine()
	{
		foreach (KeyValue<int, Edge> item in edgeMap)
		{
			if (item.Value.Connection == ConnectionKind.Dropdown)
			{
				Vector3 start = surfaceMap[item.Key].Center;
				Vector3 end = surfaceMap[item.Value.ConnectedNodeKey].Center;
				Debug.DrawLine(start, end, Color.cyan, 3f);
				yield return new WaitForSeconds(3f);
			}
		}
	}

	[ContextMenu("Display All Drop Connections")]
	private void DisplayAllDropConnections()
	{
		foreach (KeyValue<int, Edge> item in edgeMap)
		{
			if (item.Value.Connection == ConnectionKind.Dropdown)
			{
				Vector3 start = surfaceMap[item.Key].Center;
				Vector3 end = surfaceMap[item.Value.ConnectedNodeKey].Center;
				Debug.DrawLine(start, end, Color.cyan, 3f);
			}
		}
	}

	[ContextMenu("Display All Threshold Connections")]
	private void DisplayAllThresholdConnections()
	{
		foreach (KeyValue<int, Edge> item in edgeMap)
		{
			if (item.Value.Connection == ConnectionKind.Threshold)
			{
				Vector3 start = surfaceMap[item.Key].Center;
				Vector3 end = surfaceMap[item.Value.ConnectedNodeKey].Center;
				Debug.DrawLine(start, end, Color.cyan, 3f);
			}
		}
	}

	private IEnumerator DisplayOctutreeRoutine()
	{
		int num = 0;
		foreach (Octant item in octantArray)
		{
			Color color = (octantDisplayMode.HasFlag(OctantDisplayMode.Empty) ? new Color(0.5f, 0.5f, 0.5f, 0.25f) : Color.clear);
			Color color2 = (octantDisplayMode.HasFlag(OctantDisplayMode.Blocking) ? Color.blue : Color.clear);
			Color color3 = (octantDisplayMode.HasFlag(OctantDisplayMode.Walkable) ? Color.cyan : Color.clear);
			Color magenta = Color.magenta;
			Color color4 = ((item.IsWalkable && item.IsBlocking) ? magenta : (item.IsWalkable ? color3 : (item.IsBlocking ? color2 : color)));
			GridUtilities.DrawDebugCube(item.Center, item.Size, color4, 5f, topOnly: false, depthTest: true);
			num++;
			if (num > 400)
			{
				yield return null;
				num = 0;
			}
		}
	}

	private IEnumerator DisplaySectionsRoutine()
	{
		(NativeArray<int>, int) sectionKeys = sectionMap.GetUniqueKeyArray(AllocatorManager.Persistent);
		for (int i = 0; i < sectionKeys.Item2; i++)
		{
			foreach (int item in sectionMap.GetValuesForKey(sectionKeys.Item1[i]))
			{
				Octant octant = octantArray[item];
				GridUtilities.DrawDebugCube(octant.Center, octant.Size, Color.cyan, 2f);
			}
			yield return new WaitForSeconds(2f);
		}
		sectionKeys.Item1.Dispose();
	}

	private IEnumerator DisplayIslandsRoutine()
	{
		(NativeArray<int>, int) islandKeys = islandToSectionMap.GetUniqueKeyArray(AllocatorManager.Persistent);
		for (int i = 0; i < islandKeys.Item2; i++)
		{
			foreach (int item in islandToSectionMap.GetValuesForKey(islandKeys.Item1[i]))
			{
				foreach (int item2 in sectionMap.GetValuesForKey(item))
				{
					Octant octant = octantArray[item2];
					GridUtilities.DrawDebugCube(octant.Center, octant.Size, Color.cyan, 2f);
				}
			}
			yield return new WaitForSeconds(2f);
		}
		islandKeys.Item1.Dispose();
	}

	public void DisplayUpdatedNeighbors(NativeParallelHashMap<int, Octant> updatedOctants, NativeParallelMultiHashMap<int, int> updatedNeighbors)
	{
		foreach (KeyValue<int, Octant> item in updatedOctants)
		{
			if (!item.Value.IsWalkable)
			{
				continue;
			}
			GridUtilities.DrawDebugCube(item.Value.Center, item.Value.Size, Color.green, 5f);
			foreach (int item2 in updatedNeighbors.GetValuesForKey(item.Key))
			{
				Octant octant = octantArray[item2];
				Debug.DrawLine(item.Value.Center, octant.Center, Color.magenta, 5f);
			}
		}
	}
}
