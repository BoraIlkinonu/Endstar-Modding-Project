using System;
using Endless.Gameplay;
using Endless.Gameplay.Jobs;
using Unity.Jobs;
using UnityEngine;

// Token: 0x020005B8 RID: 1464
[DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__528940793
{
	// Token: 0x060022FD RID: 8957 RVA: 0x000A0B04 File Offset: 0x0009ED04
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobParallelForExtensions.EarlyJobInit<DropGenerator.FindDropdownConnectionsJob>();
			IJobExtensions.EarlyJobInit<DropGenerator.ConvertQueueToHashset>();
			IJobParallelForExtensions.EarlyJobInit<IslandGenerator.FindNeighborsJob>();
			IJobExtensions.EarlyJobInit<IslandGenerator.BuildIslandsJob>();
			IJobParallelForExtensions.EarlyJobInit<IslandGenerator.BuildSectionsJob>();
			IJobExtensions.EarlyJobInit<IslandGenerator.PruneNeighborsJob>();
			IJobParallelForExtensions.EarlyJobInit<JumpConnectionGenerator.FindJumpConnectionsJob>();
			IJobExtensions.EarlyJobInit<JumpConnectionGenerator.ConvertQueueToHashset>();
			IJobExtensions.EarlyJobInit<NavGraphUpdateGenerator.ClearAssociatedCellDataJob>();
			IJobExtensions.EarlyJobInit<NavGraphUpdateGenerator.ConstructBoxcastCommandsJob>();
			IJobParallelForExtensions.EarlyJobInit<NavGraphUpdateGenerator.UpdateOctantData>();
			IJobParallelForExtensions.EarlyJobInit<NavGraphUpdateGenerator.FindNeighborsJob>();
			IJobExtensions.EarlyJobInit<NavGraphUpdateGenerator.CleanNeighborsJob>();
			IJobExtensions.EarlyJobInit<NavGraphUpdateGenerator.BuildIslandsJob>();
			IJobParallelForExtensions.EarlyJobInit<NavGraphUpdateGenerator.BuildSectionsJob>();
			IJobExtensions.EarlyJobInit<NavGraphUpdateGenerator.PruneNeighborsJob>();
			IJobParallelForExtensions.EarlyJobInit<NavGraphUpdateGenerator.FindWalkConnectionsJob>();
			IJobParallelForExtensions.EarlyJobInit<NavGraphUpdateGenerator.FindDropdownConnectionsJob>();
			IJobExtensions.EarlyJobInit<NavGraphUpdateGenerator.ConvertConnectionQueueToHashset>();
			IJobExtensions.EarlyJobInit<NavGraphUpdateGenerator.AddUpdateDataToGraph>();
			IJobExtensions.EarlyJobInit<NavGraphUpdateGenerator.ConstructAreaJobs>();
			IJobParallelForExtensions.EarlyJobInit<NavGraphUpdateGenerator.FindJumpConnectionsJob>();
			IJobParallelForExtensions.EarlyJobInit<NavGraphUpdateGenerator.BuildThresholdConnections>();
			IJobExtensions.EarlyJobInit<ConvertQueueToHashset>();
			IJobExtensions.EarlyJobInit<ConvertQueueToParallelHashset>();
			IJobExtensions.EarlyJobInit<NavShortcutMapGenerator.ConvertConnectionsToMapsJob>();
			IJobExtensions.EarlyJobInit<NavShortcutMapGenerator.ConstructAreaJobs>();
			IJobExtensions.EarlyJobInit<OctreeGenerator.OctreeGenerationJob>();
			IJobExtensions.EarlyJobInit<OctreeGenerator.AddSplitBlockingOctants>();
			IJobExtensions.EarlyJobInit<OctreeGenerator.PruneOctantsJob>();
			IJobParallelForExtensions.EarlyJobInit<WalkGenerator.FindWalkConnectionsJob>();
			IJobParallelForExtensions.EarlyJobInit<BuildSurfacesJob>();
			IJobExtensions.EarlyJobInit<CleanJumpConnections>();
			IJobParallelForExtensions.EarlyJobInit<FindEdgeOctantsJob>();
			IJobExtensions.EarlyJobInit<NavGraph.BuildPathfindingGraphJob>();
			IJobExtensions.EarlyJobInit<NavGraph.CopyNativeCollectionsJob>();
			IJobExtensions.EarlyJobInit<NavGraph.CopyNativeCollectionsGameplayJob>();
			IJobParallelForExtensions.EarlyJobInit<PathfindingGenerator.PathfindingJob>();
			IJobParallelForExtensions.EarlyJobInit<PathfindingGenerator.CollapsePathsJob>();
			IJobParallelForExtensions.EarlyJobInit<PathfindingGenerator.PadJumpSegmentsJob>();
			IJobParallelForExtensions.EarlyJobInit<PathfindingGenerator.ConvertPathSectionKeysToPointsJob>();
			IJobParallelForExtensions.EarlyJobInit<PerceptionJob>();
			IJobExtensions.EarlyJobInit<GetUniqueKeysJob>();
			IJobExtensions.EarlyJobInit<GetLargestKey>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	// Token: 0x060022FE RID: 8958 RVA: 0x000A0C10 File Offset: 0x0009EE10
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		__JobReflectionRegistrationOutput__528940793.CreateJobReflectionData();
	}
}
