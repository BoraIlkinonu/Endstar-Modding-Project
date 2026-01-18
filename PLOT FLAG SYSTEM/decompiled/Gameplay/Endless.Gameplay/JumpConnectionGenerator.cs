using System;
using System.Collections;
using Endless.Shared;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Endless.Gameplay;

public static class JumpConnectionGenerator
{
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct FindJumpConnectionsJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[ReadOnly]
		public NativeArray<int> SectionKeyArray;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[ReadOnly]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> WalkConnections;

		[WriteOnly]
		public NativeQueue<BidirectionalConnection>.ParallelWriter JumpConnections;

		public void Execute(int index)
		{
			int num = SectionKeyArray[index];
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Temp);
			UnsafeHashSet<int> unsafeHashSet = new UnsafeHashSet<int>(64, AllocatorManager.Temp);
			foreach (int item5 in SectionMap.GetValuesForKey(num))
			{
				NpcEnum.Edge edge = WalkableOctantDataMap[item5].Edge;
				if (Hint.Unlikely(!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(Octants.GetRef(item5).Center, Octants, 1f, out var index2)))
				{
					throw new Exception("This shouldn't be possible");
				}
				if (!nativeHashSet.Add(index2))
				{
					continue;
				}
				float3 center = Octants.GetRef(index2).Center;
				if ((edge & NpcEnum.Edge.North) != NpcEnum.Edge.None)
				{
					for (int i = 0; i < 165; i++)
					{
						float3 jumpDirectionalOffset = BurstPathfindingUtilities.GetJumpDirectionalOffset(i, math.forward());
						if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpDirectionalOffset, Octants, 1f, out var index3))
						{
							continue;
						}
						Octant octant = Octants[index3];
						if (!octant.IsWalkable && !octant.HasWalkableChildren)
						{
							continue;
						}
						WalkableOctantData item;
						if (octant.HasWalkableChildren)
						{
							for (int j = 0; j < 8; j++)
							{
								if (!octant.TryGetChildIndex(j, out var key))
								{
									continue;
								}
								ref Octant reference = ref Octants.GetRef(key);
								for (int k = 0; k < 8; k++)
								{
									if (reference.TryGetChildIndex(k, out var key2) && Octants.GetRef(key2).IsWalkable)
									{
										unsafeHashSet.Add(WalkableOctantDataMap[key2].SectionKey);
									}
								}
							}
						}
						else if (WalkableOctantDataMap.TryGetValue(index3, out item))
						{
							unsafeHashSet.Add(item.SectionKey);
						}
					}
				}
				if ((edge & NpcEnum.Edge.East) != NpcEnum.Edge.None)
				{
					for (int l = 0; l < 165; l++)
					{
						float3 jumpDirectionalOffset2 = BurstPathfindingUtilities.GetJumpDirectionalOffset(l, math.right());
						if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpDirectionalOffset2, Octants, 1f, out var index4))
						{
							continue;
						}
						Octant octant2 = Octants[index4];
						if (!octant2.IsWalkable && !octant2.HasWalkableChildren)
						{
							continue;
						}
						WalkableOctantData item2;
						if (octant2.HasWalkableChildren)
						{
							for (int m = 0; m < 8; m++)
							{
								if (!octant2.TryGetChildIndex(m, out var key3))
								{
									continue;
								}
								ref Octant reference2 = ref Octants.GetRef(key3);
								for (int n = 0; n < 8; n++)
								{
									if (reference2.TryGetChildIndex(n, out var key4) && Octants.GetRef(key4).IsWalkable)
									{
										unsafeHashSet.Add(WalkableOctantDataMap[key4].SectionKey);
									}
								}
							}
						}
						else if (WalkableOctantDataMap.TryGetValue(index4, out item2))
						{
							unsafeHashSet.Add(item2.SectionKey);
						}
					}
				}
				if ((edge & NpcEnum.Edge.South) != NpcEnum.Edge.None)
				{
					for (int num2 = 0; num2 < 165; num2++)
					{
						float3 jumpDirectionalOffset3 = BurstPathfindingUtilities.GetJumpDirectionalOffset(num2, math.back());
						if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpDirectionalOffset3, Octants, 1f, out var index5))
						{
							continue;
						}
						Octant octant3 = Octants[index5];
						if (!octant3.IsWalkable && !octant3.HasWalkableChildren)
						{
							continue;
						}
						WalkableOctantData item3;
						if (octant3.HasWalkableChildren)
						{
							for (int num3 = 0; num3 < 8; num3++)
							{
								if (!octant3.TryGetChildIndex(num3, out var key5))
								{
									continue;
								}
								ref Octant reference3 = ref Octants.GetRef(key5);
								for (int num4 = 0; num4 < 8; num4++)
								{
									if (reference3.TryGetChildIndex(num4, out var key6) && Octants.GetRef(key6).IsWalkable)
									{
										unsafeHashSet.Add(WalkableOctantDataMap[key6].SectionKey);
									}
								}
							}
						}
						else if (WalkableOctantDataMap.TryGetValue(index5, out item3))
						{
							unsafeHashSet.Add(item3.SectionKey);
						}
					}
				}
				if ((edge & NpcEnum.Edge.West) != NpcEnum.Edge.None)
				{
					for (int num5 = 0; num5 < 165; num5++)
					{
						float3 jumpDirectionalOffset4 = BurstPathfindingUtilities.GetJumpDirectionalOffset(num5, math.left());
						if (!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpDirectionalOffset4, Octants, 1f, out var index6))
						{
							continue;
						}
						Octant octant4 = Octants[index6];
						if (!octant4.IsWalkable && !octant4.HasWalkableChildren)
						{
							continue;
						}
						WalkableOctantData item4;
						if (octant4.HasWalkableChildren)
						{
							for (int num6 = 0; num6 < 8; num6++)
							{
								if (!octant4.TryGetChildIndex(num6, out var key7))
								{
									continue;
								}
								ref Octant reference4 = ref Octants.GetRef(key7);
								for (int num7 = 0; num7 < 8; num7++)
								{
									if (reference4.TryGetChildIndex(num7, out var key8) && Octants.GetRef(key8).IsWalkable)
									{
										unsafeHashSet.Add(WalkableOctantDataMap[key8].SectionKey);
									}
								}
							}
						}
						else if (WalkableOctantDataMap.TryGetValue(index6, out item4))
						{
							unsafeHashSet.Add(item4.SectionKey);
						}
					}
				}
				foreach (int item6 in unsafeHashSet)
				{
					if (num == item6)
					{
						continue;
					}
					BidirectionalConnection bidirectionalConnection = new BidirectionalConnection(num, item6);
					if (WalkConnections.Contains(bidirectionalConnection))
					{
						continue;
					}
					SectionSurface jumpSurface = SurfaceMap[num];
					SectionSurface landSurface = SurfaceMap[item6];
					foreach (BurstPathfindingUtilities.SurfacePair sortedJumpPair in BurstPathfindingUtilities.GetSortedJumpPairs(jumpSurface, landSurface))
					{
						BurstPathfindingUtilities.SurfacePair current3 = sortedJumpPair;
						if (BurstPathfindingUtilities.CanReachJumpPosition(in current3.JumpPoint, in current3.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
						{
							float launchAngleDegrees = BurstPathfindingUtilities.EstimateLaunchAngle(in current3.JumpPoint, in current3.LandPoint);
							if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in current3.JumpPoint, in current3.LandPoint, launchAngleDegrees, NpcMovementValues.Gravity, out var initialVelocity, out var timeOfFlight) && !float.IsNaN(timeOfFlight) && !(timeOfFlight <= 0f) && !BurstPathfindingUtilities.IsPathBlocked(current3.JumpPoint, current3.LandPoint, timeOfFlight, initialVelocity, Octants))
							{
								JumpConnections.Enqueue(bidirectionalConnection);
								break;
							}
						}
					}
				}
				nativeHashSet.Clear();
				unsafeHashSet.Clear();
			}
		}
	}

	[BurstCompile]
	private struct ConvertQueueToHashset : IJob
	{
		public NativeQueue<BidirectionalConnection> Connections;

		[WriteOnly]
		public NativeParallelHashSet<BidirectionalConnection> JumpConnections;

		public void Execute()
		{
			while (Connections.Count > 0)
			{
				JumpConnections.Add(Connections.Dequeue());
			}
		}
	}

	public struct Results
	{
		public NativeParallelHashSet<BidirectionalConnection> JumpConnections;
	}

	public static IEnumerator BuildJumpConnections(NativeArray<Octant> octants, NativeArray<int> sectionKeyArray, NativeParallelMultiHashMap<int, int> sectionMap, NativeParallelHashMap<int, SectionSurface> surfaceMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeParallelHashSet<BidirectionalConnection> walkConnections, Action<Results> getResults)
	{
		NativeQueue<BidirectionalConnection> potentialJumpConnections = new NativeQueue<BidirectionalConnection>(AllocatorManager.Persistent);
		JobHandle handle = new FindJumpConnectionsJob
		{
			Octants = octants,
			SectionKeyArray = sectionKeyArray,
			SectionMap = sectionMap,
			SurfaceMap = surfaceMap,
			WalkableOctantDataMap = walkableOctantDataMap,
			WalkConnections = walkConnections,
			JumpConnections = potentialJumpConnections.AsParallelWriter()
		}.Schedule(innerloopBatchCount: sectionKeyArray.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches, arrayLength: sectionKeyArray.Length);
		yield return JobUtilities.WaitForJobToComplete(handle);
		NativeParallelHashSet<BidirectionalConnection> jumpConnections = new NativeParallelHashSet<BidirectionalConnection>(1024, AllocatorManager.Persistent);
		JobHandle jobHandle = new ConvertQueueToHashset
		{
			Connections = potentialJumpConnections,
			JumpConnections = jumpConnections
		}.Schedule();
		potentialJumpConnections.Dispose(jobHandle);
		yield return JobUtilities.WaitForJobToComplete(jobHandle);
		getResults(new Results
		{
			JumpConnections = jumpConnections
		});
	}
}
