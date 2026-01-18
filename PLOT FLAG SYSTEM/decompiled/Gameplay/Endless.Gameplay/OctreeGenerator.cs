using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay;

public static class OctreeGenerator
{
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct OctreeGenerationJob : IJob
	{
		[ReadOnly]
		public NativeList<NativeCellData> FullCellData;

		[ReadOnly]
		public NativeList<NativeCellData> SplitCellData;

		public NavMeshQuery Query;

		public NativeReference<Octant> RootOctant;

		public NativeArray<Octant> Octants;

		public int octantDivisions;

		public float MinSplitCellAreaPercentage;

		public QueryParameters QueryParameters;

		[WriteOnly]
		public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

		[WriteOnly]
		public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

		[WriteOnly]
		public NativeQueue<OverlapBoxCommand> DynamicOctantBoxcastCommands;

		[WriteOnly]
		public NativeQueue<PotentialOctantData> DynamicOctants;

		[WriteOnly]
		public NativeQueue<OverlapBoxCommand> BlockingOctantBoxcastCommands;

		[WriteOnly]
		public NativeQueue<PotentialOctantData> PotentialBlockingOctants;

		[WriteOnly]
		public NativeQueue<OverlapBoxCommand> WalkableOctantBoxcastCommands;

		[WriteOnly]
		public NativeQueue<PotentialOctantData> PotentialWalkableOctants;

		public NativeReference<int> NumOctants;

		public NativeReference<int> NumWalkableOctants;

		private int octantHead;

		private int numWalkableOctants;

		public void Execute()
		{
			octantHead = 1;
			numWalkableOctants = 0;
			Octant value = RootOctant.Value;
			Octants[0] = value;
			float3 @float = new float3(0.125f, 0.125f, 0.125f);
			Vector3 halfExtents = new Vector3(0.1f, 0.1f, 0.1f);
			Vector3 halfExtents2 = new Vector3(0.001f, 0.001f, 0.001f);
			Vector3 extents = new Vector3(0.001f, 0.125f, 0.001f);
			Vector3 extents2 = new Vector3(0.5f, 0.5f, 0.5f);
			Vector3 vector = new Vector3(0f, -0.001f, 0f);
			int num = octantDivisions * octantDivisions;
			NativeList<Vector3> samplePositions = new NativeList<Vector3>(octantDivisions * octantDivisions, AllocatorManager.Temp);
			foreach (NativeCellData fullCellDatum in FullCellData)
			{
				float3 position = fullCellDatum.Position;
				int insertedOctantIndex;
				if (fullCellDatum.IsTerrain)
				{
					Octant octantToInsert = Octant.Factory.BuildTerrainCellOctant(position);
					TryInsertCellOctantIntoOctree(ref octantToInsert, out insertedOctantIndex);
					continue;
				}
				NavMeshLocation location = Query.MapLocation(position, extents2, 0);
				if (Query.IsValid(location))
				{
					Octant octantToInsert2 = Octant.Factory.BuildWalkableCellOctant(position);
					if (TryInsertCellOctantIntoOctree(ref octantToInsert2, out insertedOctantIndex))
					{
						numWalkableOctants++;
					}
				}
			}
			foreach (NativeCellData splitCellDatum in SplitCellData)
			{
				if (splitCellDatum.IsConditionallyNavigable)
				{
					float3 position2 = splitCellDatum.Position;
					Octant octantToInsert3 = Octant.Factory.BuildSplitOctant(position2, isConditionalOctant: true, splitCellDatum.IsTerrain);
					TryInsertCellOctantIntoOctree(ref octantToInsert3, out var insertedOctantIndex2);
					AssociatedPropMap.Add(insertedOctantIndex2, splitCellDatum.AssociatedProp);
					if (splitCellDatum.IsTerrain)
					{
						SlopeMap.Add(insertedOctantIndex2, splitCellDatum.SlopeNeighbors);
					}
					ref Octant reference = ref Octants.GetRef(insertedOctantIndex2);
					reference.HasChildren = true;
					for (int i = 0; i < 8; i++)
					{
						float3 float2 = position2 + OctreeHelperMethods.GetOffsetPosition(i, 0.25f);
						Octants[octantHead] = Octant.Factory.BuildSplitOctantChild(float2, isConditionalOctant: true, splitCellDatum.IsTerrain);
						reference.SetChildIndex(i, octantHead);
						AssociatedPropMap.Add(octantHead, splitCellDatum.AssociatedProp);
						if (splitCellDatum.IsTerrain)
						{
							SlopeMap.Add(octantHead, splitCellDatum.SlopeNeighbors);
						}
						ref Octant reference2 = ref Octants.GetRef(octantHead);
						reference2.HasChildren = true;
						octantHead++;
						for (int j = 0; j < 8; j++)
						{
							float3 float3 = float2 + OctreeHelperMethods.GetOffsetPosition(j, 0.125f);
							Octants[octantHead] = Octant.Factory.BuildSplitOctantGrandchild(float3, isWalkable: false, isBlocking: false, isConditionalOctant: true, splitCellDatum.IsTerrain);
							AssociatedPropMap.Add(octantHead, splitCellDatum.AssociatedProp);
							if (splitCellDatum.IsTerrain)
							{
								SlopeMap.Add(octantHead, splitCellDatum.SlopeNeighbors);
							}
							DynamicOctants.Enqueue(new PotentialOctantData
							{
								center = float3,
								IsSlope = splitCellDatum.IsTerrain,
								Slope = splitCellDatum.SlopeNeighbors
							});
							DynamicOctantBoxcastCommands.Enqueue(new OverlapBoxCommand(float3, halfExtents, Quaternion.identity, QueryParameters));
							reference2.SetChildIndex(j, octantHead);
							octantHead++;
						}
					}
					continue;
				}
				float3 position3 = splitCellDatum.Position;
				for (int k = 0; k < 8; k++)
				{
					float3 float4 = position3 + OctreeHelperMethods.GetOffsetPosition(k, 0.25f);
					for (int l = 0; l < 8; l++)
					{
						float3 float5 = float4 + OctreeHelperMethods.GetOffsetPosition(l, 0.125f);
						int num2 = 0;
						Vector3 vector2 = Vector3.zero;
						OctreeHelperMethods.GetSamplePositions(samplePositions, float5 - @float, float5 + @float, octantDivisions);
						foreach (Vector3 item in samplePositions)
						{
							NavMeshLocation location2 = Query.MapLocation(item + vector, extents, 0);
							if (Query.IsValid(location2))
							{
								num2++;
								vector2 = location2.position;
							}
						}
						if ((float)num2 / (float)num > MinSplitCellAreaPercentage)
						{
							PotentialWalkableOctants.Enqueue(new PotentialOctantData
							{
								center = float5,
								IsSlope = splitCellDatum.IsTerrain,
								Slope = splitCellDatum.SlopeNeighbors
							});
							WalkableOctantBoxcastCommands.Enqueue(new OverlapBoxCommand(vector2 + Vector3.up, halfExtents2, Quaternion.identity, QueryParameters));
						}
						else
						{
							PotentialBlockingOctants.Enqueue(new PotentialOctantData
							{
								center = float5,
								IsSlope = splitCellDatum.IsTerrain,
								Slope = splitCellDatum.SlopeNeighbors
							});
							BlockingOctantBoxcastCommands.Enqueue(new OverlapBoxCommand(float5, halfExtents, Quaternion.identity, QueryParameters));
						}
					}
				}
			}
			NumWalkableOctants.Value = numWalkableOctants;
			NumOctants.Value = octantHead;
			RootOctant.Value = Octants[0];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool TryInsertCellOctantIntoOctree(ref Octant octantToInsert, out int insertedOctantIndex)
		{
			ref Octant reference = ref Octants.GetRef(0);
			int index;
			while (reference.Size.x > 1f && reference.TryGetChildOctantIndexFromPoint(octantToInsert.Center, out index))
			{
				reference = ref Octants.GetRef(index);
			}
			if (reference.Size.x < 2f)
			{
				insertedOctantIndex = -1;
				return false;
			}
			int octantRelativeOctantIndex;
			while (reference.Size.x > 2f)
			{
				octantRelativeOctantIndex = Octant.GetOctantRelativeOctantIndex(reference.Center, octantToInsert.Center);
				float3 size = reference.Size / 2f;
				float offsetDistance = size.x / 2f;
				float3 center = reference.Center + OctreeHelperMethods.GetOffsetPosition(octantRelativeOctantIndex, offsetDistance);
				reference.HasChildren = true;
				reference.SetChildIndex(octantRelativeOctantIndex, octantHead);
				Octants[octantHead] = Octant.Factory.BuildContainingOctant(center, size);
				reference = ref Octants.GetRef(octantHead);
				octantHead++;
			}
			octantRelativeOctantIndex = Octant.GetOctantRelativeOctantIndex(reference.Center, octantToInsert.Center);
			reference.SetChildIndex(octantRelativeOctantIndex, octantHead);
			reference.HasChildren = true;
			Octants[octantHead] = octantToInsert;
			insertedOctantIndex = octantHead;
			octantHead++;
			return true;
		}
	}

	[BurstCompile]
	private struct AddSplitBlockingOctants : IJob
	{
		[ReadOnly]
		public NativeArray<PotentialOctantData> PotentialBlockingOctants;

		[ReadOnly]
		public NativeArray<PotentialOctantData> PotentialWalkableOctants;

		[ReadOnly]
		public NativeArray<PotentialOctantData> DynamicOctants;

		[ReadOnly]
		public NativeArray<ColliderHit> BlockingOverlapBoxResults;

		[ReadOnly]
		public NativeArray<ColliderHit> WalkableOverlapBoxResults;

		[ReadOnly]
		public NativeArray<ColliderHit> DynamicOverlapBoxResults;

		public NativeReference<int> NumOctants;

		public NativeArray<Octant> Octants;

		public NativeReference<int> NumWalkableOctantsReference;

		[WriteOnly]
		public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

		private int octantHead;

		public void Execute()
		{
			octantHead = NumOctants.Value;
			for (int i = 0; i < DynamicOctants.Length; i++)
			{
				if (DynamicOverlapBoxResults[i].instanceID != 0)
				{
					OctreeHelperMethods.GetSmallestOctantForPoint(DynamicOctants[i].center, Octants, 0.25f).IsBlocking = true;
				}
			}
			for (int j = 0; j < PotentialBlockingOctants.Length; j++)
			{
				if (BlockingOverlapBoxResults[j].instanceID != 0)
				{
					PotentialOctantData potentialOctantData = PotentialBlockingOctants[j];
					Octant octantToInsert = Octant.Factory.BuildSplitOctantGrandchild(potentialOctantData.center, isWalkable: false, isBlocking: true, isConditionalOctant: false, potentialOctantData.IsSlope);
					if (TryInsertCellOctantIntoOctree(ref octantToInsert, potentialOctantData.Slope) && potentialOctantData.IsSlope)
					{
						SlopeMap.Add(octantHead - 1, potentialOctantData.Slope);
					}
					OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(potentialOctantData.center, Octants, 1f, out var index);
					Octants.GetRef(index).HasChildren = true;
				}
			}
			for (int k = 0; k < PotentialWalkableOctants.Length; k++)
			{
				if (WalkableOverlapBoxResults[k].instanceID == 0)
				{
					PotentialOctantData potentialOctantData2 = PotentialWalkableOctants[k];
					Octant octantToInsert2 = Octant.Factory.BuildSplitOctantGrandchild(potentialOctantData2.center, isWalkable: true, isBlocking: false, isConditionalOctant: false, potentialOctantData2.IsSlope);
					if (TryInsertCellOctantIntoOctree(ref octantToInsert2, potentialOctantData2.Slope) && potentialOctantData2.IsSlope)
					{
						SlopeMap.Add(octantHead - 1, potentialOctantData2.Slope);
					}
					OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(potentialOctantData2.center, Octants, 1f, out var index2);
					ref Octant reference = ref Octants.GetRef(index2);
					reference.HasChildren = true;
					reference.HasWalkableChildren = true;
					OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(potentialOctantData2.center, Octants, 1f, out var index3);
					ref Octant reference2 = ref Octants.GetRef(index3);
					reference2.HasChildren = true;
					reference2.HasWalkableChildren = true;
					NumWalkableOctantsReference.Value++;
				}
			}
			NumOctants.Value = octantHead;
		}

		private bool TryInsertCellOctantIntoOctree(ref Octant octantToInsert, SlopeNeighbors slope)
		{
			ref Octant reference = ref Octants.GetRef(0);
			int index;
			while (reference.Size.x > 0.5f && reference.TryGetChildOctantIndexFromPoint(octantToInsert.Center, out index))
			{
				reference = ref Octants.GetRef(index);
			}
			int octantRelativeOctantIndex;
			while (reference.Size.x > 2f)
			{
				octantRelativeOctantIndex = Octant.GetOctantRelativeOctantIndex(reference.Center, octantToInsert.Center);
				float3 size = reference.Size / 2f;
				float offsetDistance = size.x / 2f;
				float3 center = reference.Center + OctreeHelperMethods.GetOffsetPosition(octantRelativeOctantIndex, offsetDistance);
				reference.HasChildren = true;
				reference.SetChildIndex(octantRelativeOctantIndex, octantHead);
				Octants[octantHead] = Octant.Factory.BuildContainingOctant(center, size);
				reference = ref Octants.GetRef(octantHead);
				octantHead++;
			}
			float x = reference.Size.x;
			if (x.Equals(2f))
			{
				octantRelativeOctantIndex = Octant.GetOctantRelativeOctantIndex(reference.Center, octantToInsert.Center);
				float offsetDistance2 = reference.Size.x / 4f;
				float3 center2 = reference.Center + OctreeHelperMethods.GetOffsetPosition(octantRelativeOctantIndex, offsetDistance2);
				Octants[octantHead] = Octant.Factory.BuildSplitOctant(center2, octantToInsert.IsConditionallyNavigable, octantToInsert.IsSlope);
				if (octantToInsert.IsSlope)
				{
					SlopeMap.Add(octantHead, slope);
				}
				reference.HasChildren = true;
				reference.SetChildIndex(octantRelativeOctantIndex, octantHead);
				reference = ref Octants.GetRef(octantHead);
				octantHead++;
			}
			x = reference.Size.x;
			if (x.Equals(1f))
			{
				octantRelativeOctantIndex = Octant.GetOctantRelativeOctantIndex(reference.Center, octantToInsert.Center);
				float offsetDistance3 = reference.Size.x / 4f;
				float3 center3 = reference.Center + OctreeHelperMethods.GetOffsetPosition(octantRelativeOctantIndex, offsetDistance3);
				Octants[octantHead] = Octant.Factory.BuildSplitOctantChild(center3, octantToInsert.IsConditionallyNavigable, octantToInsert.IsSlope);
				if (octantToInsert.IsSlope)
				{
					SlopeMap.Add(octantHead, slope);
				}
				reference.HasChildren = true;
				reference.SetChildIndex(octantRelativeOctantIndex, octantHead);
				reference = ref Octants.GetRef(octantHead);
				octantHead++;
			}
			octantRelativeOctantIndex = Octant.GetOctantRelativeOctantIndex(reference.Center, octantToInsert.Center);
			reference.SetChildIndex(octantRelativeOctantIndex, octantHead);
			reference.HasChildren = true;
			Octants[octantHead] = octantToInsert;
			octantHead++;
			return true;
		}
	}

	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct PruneOctantsJob : IJob
	{
		[ReadOnly]
		public NativeArray<Octant> RawOctants;

		[WriteOnly]
		public NativeArray<Octant> Octants;

		public int NumOctants;

		public void Execute()
		{
			RawOctants.GetSubArray(0, NumOctants).CopyTo(Octants);
		}
	}

	public struct Results
	{
		public NativeArray<Octant> Octants;

		public NativeParallelHashMap<int, SerializableGuid> AssociatedPropMap;

		public NativeParallelHashMap<int, SlopeNeighbors> SlopeMap;

		public Dictionary<SerializableGuid, HashSet<int3>> DynamicCellMap;

		public int NumWalkableOctants;
	}

	private struct PotentialOctantData
	{
		public float3 center;

		public bool IsSlope;

		public SlopeNeighbors Slope;
	}

	public static IEnumerator GenerateOctree(NavMeshQuery query, Action<Results> getResults)
	{
		Octant value = BuildRootOctant();
		IReadOnlyList<Cell> cells = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Cells;
		NativeReference<Octant> rootOctantReference = new NativeReference<Octant>(value, AllocatorManager.Persistent);
		NativeList<NativeCellData> fullCells = new NativeList<NativeCellData>(cells.Count, AllocatorManager.Persistent);
		NativeList<NativeCellData> splitCells = new NativeList<NativeCellData>(cells.Count, AllocatorManager.Persistent);
		Dictionary<SerializableGuid, HashSet<int3>> dynamicCellMap = new Dictionary<SerializableGuid, HashSet<int3>>();
		yield return ExtractCellDataForOctutree(cells, splitCells, fullCells, dynamicCellMap);
		NativeReference<int> numOctantsReference = new NativeReference<int>(0, AllocatorManager.Persistent);
		NativeReference<int> numWalkableOctantsReference = new NativeReference<int>(0, AllocatorManager.Persistent);
		NativeArray<Octant> rawOctants = new NativeArray<Octant>(fullCells.Length + splitCells.Length * 73 + 65536, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		NativeParallelHashMap<int, SerializableGuid> associatedPropMap = new NativeParallelHashMap<int, SerializableGuid>(1024, AllocatorManager.Persistent);
		NativeParallelHashMap<int, SlopeNeighbors> slopeMap = new NativeParallelHashMap<int, SlopeNeighbors>(splitCells.Length, AllocatorManager.Persistent);
		NativeQueue<OverlapBoxCommand> blockingOverlapCommands = new NativeQueue<OverlapBoxCommand>(AllocatorManager.Persistent);
		NativeQueue<OverlapBoxCommand> walkableOverlapCommands = new NativeQueue<OverlapBoxCommand>(AllocatorManager.Persistent);
		NativeQueue<OverlapBoxCommand> dynamicBlockingOctants = new NativeQueue<OverlapBoxCommand>(AllocatorManager.Persistent);
		NativeQueue<PotentialOctantData> potentialBlockingOctants = new NativeQueue<PotentialOctantData>(AllocatorManager.Persistent);
		NativeQueue<PotentialOctantData> potentialWalkableOctants = new NativeQueue<PotentialOctantData>(AllocatorManager.Persistent);
		NativeQueue<PotentialOctantData> dynamicOctants = new NativeQueue<PotentialOctantData>(AllocatorManager.Persistent);
		JobHandle jobHandle = new OctreeGenerationJob
		{
			FullCellData = fullCells,
			SplitCellData = splitCells,
			Query = query,
			RootOctant = rootOctantReference,
			Octants = rawOctants,
			AssociatedPropMap = associatedPropMap,
			SlopeMap = slopeMap,
			NumOctants = numOctantsReference,
			NumWalkableOctants = numWalkableOctantsReference,
			octantDivisions = NavGraph.SplitCellDivisions,
			MinSplitCellAreaPercentage = NavGraph.MinSplitCellAreaPercentage,
			DynamicOctantBoxcastCommands = dynamicBlockingOctants,
			DynamicOctants = dynamicOctants,
			BlockingOctantBoxcastCommands = blockingOverlapCommands,
			PotentialBlockingOctants = potentialBlockingOctants,
			PotentialWalkableOctants = potentialWalkableOctants,
			WalkableOctantBoxcastCommands = walkableOverlapCommands,
			QueryParameters = new QueryParameters(NpcMovementValues.JumpSweepMask)
		}.Schedule();
		fullCells.Dispose(jobHandle);
		splitCells.Dispose(jobHandle);
		rootOctantReference.Dispose(jobHandle);
		yield return JobUtilities.WaitForJobToComplete(jobHandle);
		NativeArray<OverlapBoxCommand> staticOverlapBoxArray = blockingOverlapCommands.ToArray(AllocatorManager.Persistent);
		blockingOverlapCommands.Dispose(default(JobHandle));
		NativeArray<OverlapBoxCommand> walkableOverlapArray = walkableOverlapCommands.ToArray(AllocatorManager.Persistent);
		NativeArray<ColliderHit> walkableResults = new NativeArray<ColliderHit>(walkableOverlapCommands.Count, Allocator.Persistent);
		walkableOverlapCommands.Dispose(default(JobHandle));
		NativeArray<PotentialOctantData> blockingOctantData = potentialBlockingOctants.ToArray(AllocatorManager.Persistent);
		potentialBlockingOctants.Dispose(default(JobHandle));
		NativeArray<PotentialOctantData> potentialWalkableOctants2 = potentialWalkableOctants.ToArray(AllocatorManager.Persistent);
		potentialWalkableOctants.Dispose(default(JobHandle));
		NativeArray<ColliderHit> blockingResults = new NativeArray<ColliderHit>(staticOverlapBoxArray.Length, Allocator.Persistent);
		NativeArray<OverlapBoxCommand> dynamicBoxCommands = dynamicBlockingOctants.ToArray(AllocatorManager.Persistent);
		dynamicBlockingOctants.Dispose(default(JobHandle));
		NativeArray<PotentialOctantData> dynamicOctantData = dynamicOctants.ToArray(AllocatorManager.Persistent);
		dynamicOctants.Dispose(default(JobHandle));
		NativeArray<ColliderHit> dynamicResults = new NativeArray<ColliderHit>(dynamicBoxCommands.Length, Allocator.Persistent);
		AddSplitBlockingOctants jobData = new AddSplitBlockingOctants
		{
			PotentialBlockingOctants = blockingOctantData,
			PotentialWalkableOctants = potentialWalkableOctants2,
			DynamicOctants = dynamicOctantData,
			BlockingOverlapBoxResults = blockingResults,
			WalkableOverlapBoxResults = walkableResults,
			DynamicOverlapBoxResults = dynamicResults,
			SlopeMap = slopeMap,
			NumOctants = numOctantsReference,
			Octants = rawOctants,
			NumWalkableOctantsReference = numWalkableOctantsReference
		};
		JobHandle dependsOn = OverlapBoxCommand.ScheduleBatch(dependsOn: OverlapBoxCommand.ScheduleBatch(dependsOn: OverlapBoxCommand.ScheduleBatch(staticOverlapBoxArray, blockingResults, 30, 1), commands: walkableOverlapArray, results: walkableResults, minCommandsPerJob: 30, maxHits: 1), commands: dynamicBoxCommands, results: dynamicResults, minCommandsPerJob: 30, maxHits: 1);
		dependsOn = jobData.Schedule(dependsOn);
		potentialWalkableOctants2.Dispose(dependsOn);
		yield return JobUtilities.WaitForJobToComplete(dependsOn);
		dynamicBoxCommands.Dispose(default(JobHandle));
		dynamicOctantData.Dispose(default(JobHandle));
		staticOverlapBoxArray.Dispose(default(JobHandle));
		blockingResults.Dispose(default(JobHandle));
		walkableResults.Dispose(default(JobHandle));
		walkableOverlapArray.Dispose(default(JobHandle));
		dynamicResults.Dispose(default(JobHandle));
		blockingOctantData.Dispose(default(JobHandle));
		int value2 = numOctantsReference.Value;
		numOctantsReference.Dispose(default(JobHandle));
		NativeArray<Octant> octants = new NativeArray<Octant>(value2, Allocator.Persistent);
		JobHandle jobHandle2 = new PruneOctantsJob
		{
			RawOctants = rawOctants,
			Octants = octants,
			NumOctants = value2
		}.Schedule();
		rawOctants.Dispose(jobHandle2);
		yield return JobUtilities.WaitForJobToComplete(jobHandle2);
		getResults(new Results
		{
			Octants = octants,
			NumWalkableOctants = numWalkableOctantsReference.Value,
			DynamicCellMap = dynamicCellMap,
			AssociatedPropMap = associatedPropMap,
			SlopeMap = slopeMap
		});
		numWalkableOctantsReference.Dispose(default(JobHandle));
	}

	private static Octant BuildRootOctant()
	{
		Vector3Int vector3Int = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MinimumExtents - Vector3Int.one * 10;
		Vector3Int vector3Int2 = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MaximumExtents + Vector3Int.one * 10;
		int x = vector3Int2.x - vector3Int.x;
		int y = vector3Int2.y - vector3Int.y;
		int x2 = math.max(vector3Int2.z - vector3Int.z, math.max(x, y));
		x2 = math.ceilpow2(x2);
		float3 @float = new float3(x2, x2, x2);
		return Octant.Factory.BuildContainingOctant(vector3Int.ToInt3() + new float3(-0.5f) + @float / 2f, @float);
	}

	private static HashSet<Door> GetNpcOpenableDoorProps(List<PropCell> propCells)
	{
		HashSet<Door> hashSet = new HashSet<Door>();
		foreach (PropCell propCell in propCells)
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId).GetComponentInChildren<WorldObject>().TryGetUserComponent(typeof(Door), out var component))
			{
				Door item = (Door)component;
				hashSet.Add(item);
			}
		}
		return hashSet;
	}

	private static IEnumerator ExtractCellDataForOctutree(IReadOnlyList<Cell> cells, NativeList<NativeCellData> splitCells, NativeList<NativeCellData> fullCells, Dictionary<SerializableGuid, HashSet<int3>> dynamicCellMap)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		Dictionary<int3, NativeCellData> cellMap = new Dictionary<int3, NativeCellData>(cells.Count);
		List<PropCell> propCells = new List<PropCell>(cells.Count);
		List<TerrainCell> terrainCells = new List<TerrainCell>(cells.Count);
		Dictionary<SerializableGuid, WorldObject> propWorldObjects = new Dictionary<SerializableGuid, WorldObject>(cells.Count);
		foreach (Cell cell in cells)
		{
			if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.05f)
			{
				yield return null;
				realtimeSinceStartup = Time.realtimeSinceStartup;
			}
			if (!(cell is PropCell propCell))
			{
				if (cell is TerrainCell item)
				{
					terrainCells.Add(item);
				}
			}
			else
			{
				propCells.Add(propCell);
				GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(propCell.InstanceId);
				WorldObject value = (gameObjectFromInstanceId ? gameObjectFromInstanceId.GetComponentInChildren<WorldObject>() : null);
				propWorldObjects[propCell.InstanceId] = value;
			}
		}
		foreach (TerrainCell terrainCell in terrainCells)
		{
			if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.05f)
			{
				yield return null;
				realtimeSinceStartup = Time.realtimeSinceStartup;
			}
			Vector3 vector = terrainCell.Coordinate;
			int num;
			if (terrainCell.IsSlope())
			{
				SlopeNeighbors slope = terrainCell.GetSlope();
				num = ((slope != SlopeNeighbors.TopNeighbor && slope != SlopeNeighbors.AllNeighbors) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			bool isSplitCell = (byte)num != 0;
			NativeCellData value2 = new NativeCellData
			{
				Position = vector,
				AssociatedProp = SerializableGuid.Empty,
				IsSplitCell = isSplitCell,
				SlopeNeighbors = terrainCell.GetSlope(),
				IsTerrain = true
			};
			cellMap.Add(terrainCell.Coordinate.ToInt3(), value2);
		}
		HashSet<Door> doors = GetNpcOpenableDoorProps(propCells);
		Dictionary<int3, SerializableGuid> capturedCells = new Dictionary<int3, SerializableGuid>();
		foreach (Door door in doors)
		{
			dynamicCellMap.Add(door.WorldObject.InstanceId, new HashSet<int3>());
			foreach (int3 cell2 in door.CaptureClosedCells())
			{
				if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.05f)
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
				capturedCells.Add(cell2, door.WorldObject.InstanceId);
				dynamicCellMap[door.WorldObject.InstanceId].Add(cell2);
			}
		}
		foreach (Door door in doors)
		{
			foreach (int3 cell2 in door.OpenForwardAndReturnCells())
			{
				if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.05f)
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
				if (capturedCells.TryAdd(cell2, door.WorldObject.InstanceId))
				{
					dynamicCellMap[door.WorldObject.InstanceId].Add(cell2);
				}
			}
			foreach (int3 cell2 in door.OpenBackwardAndReturnCells())
			{
				if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.05f)
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
				if (capturedCells.TryAdd(cell2, door.WorldObject.InstanceId))
				{
					dynamicCellMap[door.WorldObject.InstanceId].Add(cell2);
				}
			}
			door.Close();
		}
		foreach (KeyValuePair<int3, SerializableGuid> capturedCell in capturedCells)
		{
			if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.05f)
			{
				yield return null;
				realtimeSinceStartup = Time.realtimeSinceStartup;
			}
			NativeCellData value3 = new NativeCellData
			{
				Position = capturedCell.Key,
				IsSplitCell = true,
				IsConditionallyNavigable = true,
				AssociatedProp = capturedCell.Value
			};
			if (!cellMap.TryAdd(capturedCell.Key, value3))
			{
				NativeCellData value4 = cellMap[capturedCell.Key];
				if (value4.IsSplitCell)
				{
					value4.IsConditionallyNavigable = true;
					cellMap[capturedCell.Key] = value4;
				}
			}
		}
		foreach (PropCell propCell2 in propCells)
		{
			if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.05f)
			{
				yield return null;
				realtimeSinceStartup = Time.realtimeSinceStartup;
			}
			Vector3 vector2 = propCell2.Coordinate;
			int3 @int = propCell2.Coordinate.ToInt3();
			WorldObject worldObject = propWorldObjects[propCell2.InstanceId];
			if (worldObject.EndlessProp.NavValue != NavType.Intangible)
			{
				NativeCellData value5 = new NativeCellData
				{
					Position = vector2,
					IsSplitCell = true,
					AssociatedProp = propCell2.InstanceId,
					IsConditionallyNavigable = (worldObject.EndlessProp.NavValue == NavType.Dynamic)
				};
				if (worldObject.TryGetUserComponent(typeof(DynamicNavigationComponent), out var _))
				{
					dynamicCellMap.TryAdd(worldObject.InstanceId, new HashSet<int3>());
					dynamicCellMap[worldObject.InstanceId].Add(@int);
				}
				if (!cellMap.TryAdd(@int, value5))
				{
					NativeCellData value6 = cellMap[@int];
					value6.AssociatedProp = propCell2.InstanceId;
					cellMap[@int] = value6;
				}
			}
		}
		foreach (PropCell propCell2 in propCells)
		{
			if (Time.realtimeSinceStartup - realtimeSinceStartup > 0.05f)
			{
				yield return null;
				realtimeSinceStartup = Time.realtimeSinceStartup;
			}
			Vector3 vector3 = propCell2.Coordinate;
			WorldObject worldObject2 = propWorldObjects[propCell2.InstanceId];
			if (worldObject2.EndlessProp.NavValue != NavType.Intangible)
			{
				NativeCellData value7 = new NativeCellData
				{
					Position = vector3 + Vector3.down,
					IsSplitCell = true,
					IsConditionallyNavigable = (worldObject2.EndlessProp.NavValue == NavType.Dynamic)
				};
				int3 int2 = propCell2.Coordinate.ToInt3();
				int3 int3 = int2 + new int3(0, -1, 0);
				if (cellMap.TryAdd(int3, value7) && worldObject2.EndlessProp.NavValue == NavType.Dynamic)
				{
					dynamicCellMap.TryAdd(worldObject2.InstanceId, new HashSet<int3>());
					dynamicCellMap[worldObject2.InstanceId].Add(int3);
				}
				NativeCellData value8 = new NativeCellData
				{
					Position = vector3 + Vector3.up,
					IsSplitCell = true,
					IsConditionallyNavigable = (worldObject2.EndlessProp.NavValue == NavType.Dynamic)
				};
				int3 int4 = int2 + new int3(0, 1, 0);
				if (cellMap.TryAdd(int4, value8) && worldObject2.EndlessProp.NavValue == NavType.Dynamic)
				{
					dynamicCellMap.TryAdd(worldObject2.InstanceId, new HashSet<int3>());
					dynamicCellMap[worldObject2.InstanceId].Add(int4);
				}
			}
		}
		foreach (TerrainCell item2 in terrainCells)
		{
			if (item2.IsSlope())
			{
				SlopeNeighbors slope = item2.GetSlope();
				if (slope != SlopeNeighbors.AllNeighbors && slope != SlopeNeighbors.TopNeighbor)
				{
					continue;
				}
			}
			int3 key = item2.Coordinate.ToInt3() + new int3(0, 1, 0);
			if (!cellMap.ContainsKey(key))
			{
				Vector3 vector4 = item2.Coordinate;
				NativeCellData value9 = new NativeCellData
				{
					Position = vector4 + Vector3.up,
					IsSplitCell = false,
					AssociatedProp = SerializableGuid.Empty
				};
				cellMap.Add(key, value9);
			}
		}
		foreach (NativeCellData value11 in cellMap.Values)
		{
			NativeCellData value10 = value11;
			if (value10.IsSplitCell)
			{
				splitCells.Add(in value10);
			}
			else
			{
				fullCells.Add(in value10);
			}
		}
	}
}
