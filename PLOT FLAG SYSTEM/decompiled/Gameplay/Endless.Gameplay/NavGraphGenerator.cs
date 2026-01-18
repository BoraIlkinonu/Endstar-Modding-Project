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

namespace Endless.Gameplay;

public static class NavGraphGenerator
{
	public struct Results
	{
		public NativeArray<Octant> Octants;

		public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

		public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

		public Dictionary<SerializableGuid, HashSet<int3>> DynamicCellMap;

		public NativeParallelMultiHashMap<int, int> SectionMap;

		public NativeParallelMultiHashMap<int, int> NeighborMap;

		public NativeParallelMultiHashMap<int, int> AreaMap;

		public NativeParallelMultiHashMap<int, int> AreaGraph;

		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		public NativeParallelHashSet<BidirectionalConnection> NativeWalkConnections;

		public NativeParallelHashSet<BidirectionalConnection> NativeJumpConnections;

		public NativeParallelHashSet<Connection> NativeDropConnections;

		public NativeParallelMultiHashMap<int, int> IslandToSectionMap;

		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		public NativeParallelHashMap<int, GraphNode> NodeMap;

		public NativeParallelHashMap<int, Edge> EdgeMap;

		public NativeParallelHashSet<BidirectionalConnection> ThresholdConnections;
	}

	public static IEnumerator GenerateNavGraph(Action<Results> getResults)
	{
		NavMeshQuery query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent);
		NativeArray<Octant> octants = default(NativeArray<Octant>);
		NativeParallelHashMap<int, SerializableGuid> associatedPropMap = default(NativeParallelHashMap<int, SerializableGuid>);
		NativeParallelHashMap<int, SlopeNeighbors> slopeMap = default(NativeParallelHashMap<int, SlopeNeighbors>);
		int numWalkableOctants = 0;
		Dictionary<SerializableGuid, HashSet<int3>> dynamicCellMap = null;
		NativeList<int> sectionKeys = default(NativeList<int>);
		NativeList<int> islandKeys = default(NativeList<int>);
		NativeParallelMultiHashMap<int, int> neighborMap = default(NativeParallelMultiHashMap<int, int>);
		NativeParallelMultiHashMap<int, int> islandKeyToSectionKeyMap = default(NativeParallelMultiHashMap<int, int>);
		NativeParallelMultiHashMap<int, int> sectionMap = default(NativeParallelMultiHashMap<int, int>);
		NativeParallelHashMap<int, SectionSurface> surfaceMap = default(NativeParallelHashMap<int, SectionSurface>);
		NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap = default(NativeParallelHashMap<int, WalkableOctantData>);
		NativeParallelMultiHashMap<int, int> areaMap = default(NativeParallelMultiHashMap<int, int>);
		NativeParallelMultiHashMap<int, int> areaGraph = default(NativeParallelMultiHashMap<int, int>);
		NativeParallelHashSet<BidirectionalConnection> walkConnections = default(NativeParallelHashSet<BidirectionalConnection>);
		NativeParallelHashSet<BidirectionalConnection> verifiedJumpConnections = default(NativeParallelHashSet<BidirectionalConnection>);
		NativeParallelHashSet<Connection> verifiedDropdownConnections = default(NativeParallelHashSet<Connection>);
		NativeParallelHashSet<BidirectionalConnection> thresholdConnections = new NativeParallelHashSet<BidirectionalConnection>(0, Allocator.Persistent);
		yield return OctreeGenerator.GenerateOctree(query, OctreeGenerated);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Octree", "BuildNavigationGraphRoutine");
		yield return IslandGenerator.GenerateIslands(octants, associatedPropMap, slopeMap, query, numWalkableOctants, IslandsGenerated);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Islands", "BuildNavigationGraphRoutine");
		yield return WalkGenerator.GenerateWalkConnections(sectionKeys, neighborMap, walkableOctantDataMap, sectionMap, WalkConnectionsGenerated);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Walk Connections", "BuildNavigationGraphRoutine");
		yield return JumpConnectionGenerator.BuildJumpConnections(octants, sectionKeys.AsArray(), sectionMap, surfaceMap, walkableOctantDataMap, walkConnections, JumpsGenerated);
		Debug.Log($"Num jump connections{verifiedJumpConnections.Count()}");
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Jump Connections", "BuildNavigationGraphRoutine");
		yield return DropGenerator.GenerateDropConnections(sectionKeys, surfaceMap, octants, verifiedJumpConnections, DropsGenerated);
		Debug.Log($"Num drop connections{verifiedDropdownConnections.Count()}");
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Drop Connections", "BuildNavigationGraphRoutine");
		yield return NavShortcutMapGenerator.GenerateShortcutMaps(walkConnections, verifiedJumpConnections, verifiedDropdownConnections, islandKeys, sectionMap, islandKeyToSectionKeyMap, walkableOctantDataMap, sectionKeys, NavShortcutsGenerated);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Generate Shortcut Maps", "BuildNavigationGraphRoutine");
		query.Dispose();
		getResults(new Results
		{
			Octants = octants,
			AssociatedPropMap = associatedPropMap,
			SlopeMap = slopeMap,
			DynamicCellMap = dynamicCellMap,
			AreaMap = areaMap,
			AreaGraph = areaGraph,
			NeighborMap = neighborMap,
			SectionMap = sectionMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			SurfaceMap = surfaceMap,
			NativeWalkConnections = walkConnections,
			NativeJumpConnections = verifiedJumpConnections,
			NativeDropConnections = verifiedDropdownConnections,
			IslandToSectionMap = islandKeyToSectionKeyMap,
			ThresholdConnections = thresholdConnections
		});
		void DropsGenerated(DropGenerator.Results results)
		{
			verifiedDropdownConnections = results.DropConnections;
		}
		void IslandsGenerated(IslandGenerator.Results results)
		{
			sectionKeys = results.SectionKeys;
			islandKeys = results.IslandKeys;
			neighborMap = results.NeighborMap;
			islandKeyToSectionKeyMap = results.IslandKeyToSectionKeyMap;
			sectionMap = results.SectionMap;
			surfaceMap = results.SurfaceMap;
			walkableOctantDataMap = results.WalkableOctantDataMap;
		}
		void JumpsGenerated(JumpConnectionGenerator.Results results)
		{
			verifiedJumpConnections = results.JumpConnections;
		}
		void NavShortcutsGenerated(NavShortcutMapGenerator.Results results)
		{
			areaMap = results.AreaMap;
			areaGraph = results.AreaGraph;
		}
		void OctreeGenerated(OctreeGenerator.Results results)
		{
			octants = results.Octants;
			associatedPropMap = results.AssociatedPropMap;
			slopeMap = results.SlopeMap;
			dynamicCellMap = results.DynamicCellMap;
			numWalkableOctants = results.NumWalkableOctants;
		}
		void WalkConnectionsGenerated(WalkGenerator.Results results)
		{
			walkConnections = results.WalkConnections;
		}
	}
}
