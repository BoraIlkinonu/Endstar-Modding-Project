using System;
using System.Collections;
using Endless.Shared;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001FA RID: 506
	public class NavGraphDebugger : MonoBehaviour
	{
		// Token: 0x06000A63 RID: 2659 RVA: 0x00038FD9 File Offset: 0x000371D9
		public void SetCollections(NativeArray<Octant> octants, NativeParallelMultiHashMap<int, int> sections, NativeParallelHashMap<int, WalkableOctantData> octantData, NativeParallelHashMap<int, SectionSurface> surfaces, NativeParallelMultiHashMap<int, int> islands, NativeParallelMultiHashMap<int, int> areas, NativeParallelMultiHashMap<int, Edge> edges)
		{
			this.octantArray = octants;
			this.sectionMap = sections;
			this.walkableOctantDataMap = octantData;
			this.surfaceMap = surfaces;
			this.islandToSectionMap = islands;
			this.areaMap = areas;
			this.edgeMap = edges;
		}

		// Token: 0x06000A64 RID: 2660 RVA: 0x00039010 File Offset: 0x00037210
		[ContextMenu("DisplayOctutree")]
		public void DisplayOctutree()
		{
			base.StartCoroutine(this.DisplayOctutreeRoutine());
		}

		// Token: 0x06000A65 RID: 2661 RVA: 0x0003901F File Offset: 0x0003721F
		[ContextMenu("DisplayIslands")]
		public void DisplayIslands()
		{
			base.StartCoroutine(this.DisplayIslandsRoutine());
		}

		// Token: 0x06000A66 RID: 2662 RVA: 0x0003902E File Offset: 0x0003722E
		[ContextMenu("Display Sections")]
		public void DisplaySections()
		{
			base.StartCoroutine(this.DisplaySectionsRoutine());
		}

		// Token: 0x06000A67 RID: 2663 RVA: 0x0003903D File Offset: 0x0003723D
		[ContextMenu("Display Surfaces")]
		public void DisplaySurfaces()
		{
			base.StartCoroutine(this.DisplaySurfacesRoutine());
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x0003904C File Offset: 0x0003724C
		[ContextMenu("Display Edges")]
		public void DisplayEdges()
		{
			base.StartCoroutine(this.DisplayEdgesRoutine());
		}

		// Token: 0x06000A69 RID: 2665 RVA: 0x0003905B File Offset: 0x0003725B
		private IEnumerator DisplayEdgesRoutine()
		{
			ValueTuple<NativeArray<int>, int> keys = this.sectionMap.GetUniqueKeyArray(AllocatorManager.Persistent);
			int num3;
			for (int i = 0; i < keys.Item2; i = num3 + 1)
			{
				foreach (int num in this.sectionMap.GetValuesForKey(keys.Item1[i]))
				{
					Octant octant = this.octantArray[num];
					if (octant.IsWalkable)
					{
						WalkableOctantData walkableOctantData = this.walkableOctantDataMap[num];
						Vector3 vector = octant.Center;
						float num2 = octant.Size.x / 2f;
						if (walkableOctantData.Edge.HasFlag(NpcEnum.Edge.North))
						{
							NavGraphDebugger.DrawEdge(vector, num2, NpcEnum.Edge.North);
						}
						if (walkableOctantData.Edge.HasFlag(NpcEnum.Edge.East))
						{
							NavGraphDebugger.DrawEdge(vector, num2, NpcEnum.Edge.East);
						}
						if (walkableOctantData.Edge.HasFlag(NpcEnum.Edge.South))
						{
							NavGraphDebugger.DrawEdge(vector, num2, NpcEnum.Edge.South);
						}
						if (walkableOctantData.Edge.HasFlag(NpcEnum.Edge.West))
						{
							NavGraphDebugger.DrawEdge(vector, num2, NpcEnum.Edge.West);
						}
					}
				}
				yield return new WaitForSeconds(2f);
				num3 = i;
			}
			yield break;
		}

		// Token: 0x06000A6A RID: 2666 RVA: 0x0003906C File Offset: 0x0003726C
		private static void DrawEdge(Vector3 center, float offset, NpcEnum.Edge edge)
		{
			Vector3 vector;
			switch (edge)
			{
			case NpcEnum.Edge.North:
				vector = Vector3.forward;
				goto IL_0050;
			case NpcEnum.Edge.East:
				vector = Vector3.right;
				goto IL_0050;
			case NpcEnum.Edge.North | NpcEnum.Edge.East:
				break;
			case NpcEnum.Edge.South:
				vector = Vector3.back;
				goto IL_0050;
			default:
				if (edge == NpcEnum.Edge.West)
				{
					vector = Vector3.left;
					goto IL_0050;
				}
				break;
			}
			throw new ArgumentOutOfRangeException("edge", edge, null);
			IL_0050:
			Vector3 vector2 = vector;
			Vector3 vector3 = Vector3.Normalize(Vector3.Cross(vector2, Vector3.up));
			center += vector2 * offset;
			Debug.DrawLine(center + vector3 * offset, center - vector3 * offset, Color.green, 1f);
		}

		// Token: 0x06000A6B RID: 2667 RVA: 0x00039114 File Offset: 0x00037314
		private IEnumerator DisplaySurfacesRoutine()
		{
			foreach (KeyValue<int, SectionSurface> keyValue in this.surfaceMap)
			{
				for (int i = 0; i < 4; i++)
				{
					SectionEdge sectionEdge = keyValue.Value.GetSectionEdge(i);
					Debug.DrawLine(sectionEdge.p1, sectionEdge.p2, Color.cyan, 3f);
				}
				yield return new WaitForSeconds(3f);
			}
			NativeParallelHashMap<int, SectionSurface>.Enumerator enumerator = default(NativeParallelHashMap<int, SectionSurface>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000A6C RID: 2668 RVA: 0x00039123 File Offset: 0x00037323
		[ContextMenu("Display Island Surfaces")]
		public void DisplaySectionSurfaces()
		{
			base.StartCoroutine(this.DisplayIslandSurface());
		}

		// Token: 0x06000A6D RID: 2669 RVA: 0x00039132 File Offset: 0x00037332
		private IEnumerator DisplayIslandSurface()
		{
			ValueTuple<NativeArray<int>, int> islandKeys = this.islandToSectionMap.GetUniqueKeyArray(AllocatorManager.Persistent);
			int num3;
			for (int index = 0; index < islandKeys.Item2; index = num3 + 1)
			{
				int num = islandKeys.Item1[index];
				foreach (int num2 in this.islandToSectionMap.GetValuesForKey(num))
				{
					SectionSurface sectionSurface = this.surfaceMap[num2];
					for (int i = 0; i < 4; i++)
					{
						SectionEdge sectionEdge = sectionSurface.GetSectionEdge(i);
						Debug.DrawLine(sectionEdge.p1, sectionEdge.p2, Color.cyan, 3f);
					}
				}
				yield return new WaitForSeconds(3f);
				num3 = index;
			}
			islandKeys.Item1.Dispose();
			yield break;
		}

		// Token: 0x06000A6E RID: 2670 RVA: 0x00039141 File Offset: 0x00037341
		[ContextMenu("Display Areas")]
		public void DisplayAreas()
		{
			base.StartCoroutine(this.DisplayAreasRoutine());
		}

		// Token: 0x06000A6F RID: 2671 RVA: 0x00039150 File Offset: 0x00037350
		private IEnumerator DisplayAreasRoutine()
		{
			ValueTuple<NativeArray<int>, int> areaKeys = this.areaMap.GetUniqueKeyArray(AllocatorManager.Persistent);
			int num3;
			for (int i = 0; i < areaKeys.Item2; i = num3 + 1)
			{
				NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.areaMap.GetValuesForKey(areaKeys.Item1[i]);
				foreach (int num in enumerator)
				{
					foreach (int num2 in this.islandToSectionMap.GetValuesForKey(num))
					{
						SectionSurface sectionSurface = this.surfaceMap[num2];
						for (int j = 0; j < 4; j++)
						{
							SectionEdge sectionEdge = sectionSurface.GetSectionEdge(j);
							Debug.DrawLine(sectionEdge.p1, sectionEdge.p2, Color.cyan, 3f);
						}
					}
				}
				yield return new WaitForSeconds(3f);
				num3 = i;
			}
			areaKeys.Item1.Dispose();
			yield break;
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x00039160 File Offset: 0x00037360
		[ContextMenu("Display All Walk Connections")]
		private void DisplayAllWalkConnections()
		{
			foreach (KeyValue<int, Edge> keyValue in this.edgeMap)
			{
				if (keyValue.Value.Connection == ConnectionKind.Walk)
				{
					Vector3 vector = this.surfaceMap[keyValue.Key].Center;
					Vector3 vector2 = this.surfaceMap[keyValue.Value.ConnectedNodeKey].Center;
					Debug.DrawLine(vector, vector2, Color.cyan, 3f);
				}
			}
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x00039210 File Offset: 0x00037410
		[ContextMenu("Display Jump Connections")]
		private void DisplayJumpConnections()
		{
			base.StartCoroutine(this.DisplayJumpConnectionsRoutine());
		}

		// Token: 0x06000A72 RID: 2674 RVA: 0x0003921F File Offset: 0x0003741F
		private IEnumerator DisplayJumpConnectionsRoutine()
		{
			foreach (KeyValue<int, Edge> keyValue in this.edgeMap)
			{
				if (keyValue.Value.Connection == ConnectionKind.Jump)
				{
					Vector3 vector = this.surfaceMap[keyValue.Key].Center;
					Vector3 vector2 = this.surfaceMap[keyValue.Value.ConnectedNodeKey].Center;
					Debug.DrawLine(vector, vector2, Color.cyan, 3f);
					yield return new WaitForSeconds(3f);
				}
			}
			NativeParallelMultiHashMap<int, Edge>.KeyValueEnumerator keyValueEnumerator = default(NativeParallelMultiHashMap<int, Edge>.KeyValueEnumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000A73 RID: 2675 RVA: 0x00039230 File Offset: 0x00037430
		[ContextMenu("Display All Jump Connections")]
		private void DisplayAllJumpConnections()
		{
			foreach (KeyValue<int, Edge> keyValue in this.edgeMap)
			{
				if (keyValue.Value.Connection == ConnectionKind.Jump)
				{
					Vector3 vector = this.surfaceMap[keyValue.Key].Center;
					Vector3 vector2 = this.surfaceMap[keyValue.Value.ConnectedNodeKey].Center;
					Debug.DrawLine(vector, vector2, Color.cyan, 3f);
				}
			}
		}

		// Token: 0x06000A74 RID: 2676 RVA: 0x000392E0 File Offset: 0x000374E0
		[ContextMenu("Display Drop Connections")]
		private void DisplayDropConnections()
		{
			base.StartCoroutine(this.DisplayDropConnectionsRoutine());
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x000392EF File Offset: 0x000374EF
		private IEnumerator DisplayDropConnectionsRoutine()
		{
			foreach (KeyValue<int, Edge> keyValue in this.edgeMap)
			{
				if (keyValue.Value.Connection == ConnectionKind.Dropdown)
				{
					Vector3 vector = this.surfaceMap[keyValue.Key].Center;
					Vector3 vector2 = this.surfaceMap[keyValue.Value.ConnectedNodeKey].Center;
					Debug.DrawLine(vector, vector2, Color.cyan, 3f);
					yield return new WaitForSeconds(3f);
				}
			}
			NativeParallelMultiHashMap<int, Edge>.KeyValueEnumerator keyValueEnumerator = default(NativeParallelMultiHashMap<int, Edge>.KeyValueEnumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x00039300 File Offset: 0x00037500
		[ContextMenu("Display All Drop Connections")]
		private void DisplayAllDropConnections()
		{
			foreach (KeyValue<int, Edge> keyValue in this.edgeMap)
			{
				if (keyValue.Value.Connection == ConnectionKind.Dropdown)
				{
					Vector3 vector = this.surfaceMap[keyValue.Key].Center;
					Vector3 vector2 = this.surfaceMap[keyValue.Value.ConnectedNodeKey].Center;
					Debug.DrawLine(vector, vector2, Color.cyan, 3f);
				}
			}
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x000393B0 File Offset: 0x000375B0
		[ContextMenu("Display All Threshold Connections")]
		private void DisplayAllThresholdConnections()
		{
			foreach (KeyValue<int, Edge> keyValue in this.edgeMap)
			{
				if (keyValue.Value.Connection == ConnectionKind.Threshold)
				{
					Vector3 vector = this.surfaceMap[keyValue.Key].Center;
					Vector3 vector2 = this.surfaceMap[keyValue.Value.ConnectedNodeKey].Center;
					Debug.DrawLine(vector, vector2, Color.cyan, 3f);
				}
			}
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x00039460 File Offset: 0x00037660
		private IEnumerator DisplayOctutreeRoutine()
		{
			int num = 0;
			foreach (Octant octant in this.octantArray)
			{
				Color color = (this.octantDisplayMode.HasFlag(NavGraphDebugger.OctantDisplayMode.Empty) ? new Color(0.5f, 0.5f, 0.5f, 0.25f) : Color.clear);
				Color color2 = (this.octantDisplayMode.HasFlag(NavGraphDebugger.OctantDisplayMode.Blocking) ? Color.blue : Color.clear);
				Color color3 = (this.octantDisplayMode.HasFlag(NavGraphDebugger.OctantDisplayMode.Walkable) ? Color.cyan : Color.clear);
				Color magenta = Color.magenta;
				Color color4 = ((octant.IsWalkable && octant.IsBlocking) ? magenta : (octant.IsWalkable ? color3 : (octant.IsBlocking ? color2 : color)));
				GridUtilities.DrawDebugCube(octant.Center, octant.Size, color4, 5f, false, true);
				num++;
				if (num > 400)
				{
					yield return null;
					num = 0;
				}
			}
			NativeArray<Octant>.Enumerator enumerator = default(NativeArray<Octant>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x0003946F File Offset: 0x0003766F
		private IEnumerator DisplaySectionsRoutine()
		{
			ValueTuple<NativeArray<int>, int> sectionKeys = this.sectionMap.GetUniqueKeyArray(AllocatorManager.Persistent);
			int num2;
			for (int i = 0; i < sectionKeys.Item2; i = num2 + 1)
			{
				foreach (int num in this.sectionMap.GetValuesForKey(sectionKeys.Item1[i]))
				{
					Octant octant = this.octantArray[num];
					GridUtilities.DrawDebugCube(octant.Center, octant.Size, Color.cyan, 2f, false, false);
				}
				yield return new WaitForSeconds(2f);
				num2 = i;
			}
			sectionKeys.Item1.Dispose();
			yield break;
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x0003947E File Offset: 0x0003767E
		private IEnumerator DisplayIslandsRoutine()
		{
			ValueTuple<NativeArray<int>, int> islandKeys = this.islandToSectionMap.GetUniqueKeyArray(AllocatorManager.Persistent);
			int num3;
			for (int i = 0; i < islandKeys.Item2; i = num3 + 1)
			{
				NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.islandToSectionMap.GetValuesForKey(islandKeys.Item1[i]);
				foreach (int num in enumerator)
				{
					foreach (int num2 in this.sectionMap.GetValuesForKey(num))
					{
						Octant octant = this.octantArray[num2];
						GridUtilities.DrawDebugCube(octant.Center, octant.Size, Color.cyan, 2f, false, false);
					}
				}
				yield return new WaitForSeconds(2f);
				num3 = i;
			}
			islandKeys.Item1.Dispose();
			yield break;
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x00039490 File Offset: 0x00037690
		public void DisplayUpdatedNeighbors(NativeParallelHashMap<int, Octant> updatedOctants, NativeParallelMultiHashMap<int, int> updatedNeighbors)
		{
			foreach (KeyValue<int, Octant> keyValue in updatedOctants)
			{
				if (keyValue.Value.IsWalkable)
				{
					GridUtilities.DrawDebugCube(keyValue.Value.Center, keyValue.Value.Size, Color.green, 5f, false, false);
					foreach (int num in updatedNeighbors.GetValuesForKey(keyValue.Key))
					{
						Octant octant = this.octantArray[num];
						Debug.DrawLine(keyValue.Value.Center, octant.Center, Color.magenta, 5f);
					}
				}
			}
		}

		// Token: 0x040009E4 RID: 2532
		[SerializeField]
		private NavGraphDebugger.OctantDisplayMode octantDisplayMode;

		// Token: 0x040009E5 RID: 2533
		private NativeArray<Octant> octantArray;

		// Token: 0x040009E6 RID: 2534
		private NativeParallelMultiHashMap<int, int> sectionMap;

		// Token: 0x040009E7 RID: 2535
		private NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap;

		// Token: 0x040009E8 RID: 2536
		private NativeParallelHashMap<int, SectionSurface> surfaceMap;

		// Token: 0x040009E9 RID: 2537
		private NativeParallelMultiHashMap<int, int> islandToSectionMap;

		// Token: 0x040009EA RID: 2538
		private NativeParallelMultiHashMap<int, int> areaMap;

		// Token: 0x040009EB RID: 2539
		private NativeParallelMultiHashMap<int, Edge> edgeMap;

		// Token: 0x020001FB RID: 507
		[Flags]
		private enum OctantDisplayMode
		{
			// Token: 0x040009ED RID: 2541
			Empty = 1,
			// Token: 0x040009EE RID: 2542
			Walkable = 2,
			// Token: 0x040009EF RID: 2543
			Blocking = 4
		}
	}
}
