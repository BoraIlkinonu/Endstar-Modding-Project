using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay
{
	// Token: 0x020001C5 RID: 453
	public static class NavGraphGenerator
	{
		// Token: 0x060009DA RID: 2522 RVA: 0x0002F1E7 File Offset: 0x0002D3E7
		public static IEnumerator GenerateNavGraph(Action<NavGraphGenerator.Results> getResults)
		{
			NavGraphGenerator.<>c__DisplayClass0_0 CS$<>8__locals1 = new NavGraphGenerator.<>c__DisplayClass0_0();
			NavMeshQuery query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, 0);
			CS$<>8__locals1.octants = default(NativeArray<Octant>);
			CS$<>8__locals1.associatedPropMap = default(NativeParallelHashMap<int, SerializableGuid>);
			CS$<>8__locals1.slopeMap = default(NativeParallelHashMap<int, SlopeNeighbors>);
			CS$<>8__locals1.numWalkableOctants = 0;
			CS$<>8__locals1.dynamicCellMap = null;
			CS$<>8__locals1.sectionKeys = default(NativeList<int>);
			CS$<>8__locals1.islandKeys = default(NativeList<int>);
			CS$<>8__locals1.neighborMap = default(NativeParallelMultiHashMap<int, int>);
			CS$<>8__locals1.islandKeyToSectionKeyMap = default(NativeParallelMultiHashMap<int, int>);
			CS$<>8__locals1.sectionMap = default(NativeParallelMultiHashMap<int, int>);
			CS$<>8__locals1.surfaceMap = default(NativeParallelHashMap<int, SectionSurface>);
			CS$<>8__locals1.walkableOctantDataMap = default(NativeParallelHashMap<int, WalkableOctantData>);
			CS$<>8__locals1.areaMap = default(NativeParallelMultiHashMap<int, int>);
			CS$<>8__locals1.areaGraph = default(NativeParallelMultiHashMap<int, int>);
			CS$<>8__locals1.walkConnections = default(NativeParallelHashSet<BidirectionalConnection>);
			CS$<>8__locals1.verifiedJumpConnections = default(NativeParallelHashSet<BidirectionalConnection>);
			CS$<>8__locals1.verifiedDropdownConnections = default(NativeParallelHashSet<Connection>);
			NativeParallelHashSet<BidirectionalConnection> thresholdConnections = new NativeParallelHashSet<BidirectionalConnection>(0, Allocator.Persistent);
			yield return OctreeGenerator.GenerateOctree(query, new Action<OctreeGenerator.Results>(CS$<>8__locals1.<GenerateNavGraph>g__OctreeGenerated|0));
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Octree", "BuildNavigationGraphRoutine");
			yield return IslandGenerator.GenerateIslands(CS$<>8__locals1.octants, CS$<>8__locals1.associatedPropMap, CS$<>8__locals1.slopeMap, query, CS$<>8__locals1.numWalkableOctants, new Action<IslandGenerator.Results>(CS$<>8__locals1.<GenerateNavGraph>g__IslandsGenerated|1));
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Islands", "BuildNavigationGraphRoutine");
			yield return WalkGenerator.GenerateWalkConnections(CS$<>8__locals1.sectionKeys, CS$<>8__locals1.neighborMap, CS$<>8__locals1.walkableOctantDataMap, CS$<>8__locals1.sectionMap, new Action<WalkGenerator.Results>(CS$<>8__locals1.<GenerateNavGraph>g__WalkConnectionsGenerated|2));
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Walk Connections", "BuildNavigationGraphRoutine");
			yield return JumpConnectionGenerator.BuildJumpConnections(CS$<>8__locals1.octants, CS$<>8__locals1.sectionKeys.AsArray(), CS$<>8__locals1.sectionMap, CS$<>8__locals1.surfaceMap, CS$<>8__locals1.walkableOctantDataMap, CS$<>8__locals1.walkConnections, new Action<JumpConnectionGenerator.Results>(CS$<>8__locals1.<GenerateNavGraph>g__JumpsGenerated|3));
			Debug.Log(string.Format("Num jump connections{0}", CS$<>8__locals1.verifiedJumpConnections.Count()));
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Jump Connections", "BuildNavigationGraphRoutine");
			yield return DropGenerator.GenerateDropConnections(CS$<>8__locals1.sectionKeys, CS$<>8__locals1.surfaceMap, CS$<>8__locals1.octants, CS$<>8__locals1.verifiedJumpConnections, new Action<DropGenerator.Results>(CS$<>8__locals1.<GenerateNavGraph>g__DropsGenerated|4));
			Debug.Log(string.Format("Num drop connections{0}", CS$<>8__locals1.verifiedDropdownConnections.Count()));
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Drop Connections", "BuildNavigationGraphRoutine");
			yield return NavShortcutMapGenerator.GenerateShortcutMaps(CS$<>8__locals1.walkConnections, CS$<>8__locals1.verifiedJumpConnections, CS$<>8__locals1.verifiedDropdownConnections, CS$<>8__locals1.islandKeys, CS$<>8__locals1.sectionMap, CS$<>8__locals1.islandKeyToSectionKeyMap, CS$<>8__locals1.walkableOctantDataMap, CS$<>8__locals1.sectionKeys, new Action<NavShortcutMapGenerator.Results>(CS$<>8__locals1.<GenerateNavGraph>g__NavShortcutsGenerated|5));
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Shortcut Maps", "BuildNavigationGraphRoutine");
			query.Dispose();
			getResults(new NavGraphGenerator.Results
			{
				Octants = CS$<>8__locals1.octants,
				AssociatedPropMap = CS$<>8__locals1.associatedPropMap,
				SlopeMap = CS$<>8__locals1.slopeMap,
				DynamicCellMap = CS$<>8__locals1.dynamicCellMap,
				AreaMap = CS$<>8__locals1.areaMap,
				AreaGraph = CS$<>8__locals1.areaGraph,
				NeighborMap = CS$<>8__locals1.neighborMap,
				SectionMap = CS$<>8__locals1.sectionMap,
				WalkableOctantDataMap = CS$<>8__locals1.walkableOctantDataMap,
				SurfaceMap = CS$<>8__locals1.surfaceMap,
				NativeWalkConnections = CS$<>8__locals1.walkConnections,
				NativeJumpConnections = CS$<>8__locals1.verifiedJumpConnections,
				NativeDropConnections = CS$<>8__locals1.verifiedDropdownConnections,
				IslandToSectionMap = CS$<>8__locals1.islandKeyToSectionKeyMap,
				ThresholdConnections = thresholdConnections
			});
			yield break;
		}

		// Token: 0x020001C6 RID: 454
		public struct Results
		{
			// Token: 0x0400082F RID: 2095
			public NativeArray<Octant> Octants;

			// Token: 0x04000830 RID: 2096
			public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

			// Token: 0x04000831 RID: 2097
			public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

			// Token: 0x04000832 RID: 2098
			public Dictionary<SerializableGuid, HashSet<int3>> DynamicCellMap;

			// Token: 0x04000833 RID: 2099
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x04000834 RID: 2100
			public NativeParallelMultiHashMap<int, int> NeighborMap;

			// Token: 0x04000835 RID: 2101
			public NativeParallelMultiHashMap<int, int> AreaMap;

			// Token: 0x04000836 RID: 2102
			public NativeParallelMultiHashMap<int, int> AreaGraph;

			// Token: 0x04000837 RID: 2103
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x04000838 RID: 2104
			public NativeParallelHashSet<BidirectionalConnection> NativeWalkConnections;

			// Token: 0x04000839 RID: 2105
			public NativeParallelHashSet<BidirectionalConnection> NativeJumpConnections;

			// Token: 0x0400083A RID: 2106
			public NativeParallelHashSet<Connection> NativeDropConnections;

			// Token: 0x0400083B RID: 2107
			public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

			// Token: 0x0400083C RID: 2108
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x0400083D RID: 2109
			public NativeParallelHashMap<int, GraphNode> NodeMap;

			// Token: 0x0400083E RID: 2110
			public NativeParallelHashMap<int, Edge> EdgeMap;

			// Token: 0x0400083F RID: 2111
			public NativeParallelHashSet<BidirectionalConnection> ThresholdConnections;
		}
	}
}
