using System;
using System.Collections;
using Endless.Shared;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Endless.Gameplay
{
	// Token: 0x020001C0 RID: 448
	public static class JumpConnectionGenerator
	{
		// Token: 0x060009D1 RID: 2513 RVA: 0x0002E8F5 File Offset: 0x0002CAF5
		public static IEnumerator BuildJumpConnections(NativeArray<Octant> octants, NativeArray<int> sectionKeyArray, NativeParallelMultiHashMap<int, int> sectionMap, NativeParallelHashMap<int, SectionSurface> surfaceMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap, NativeParallelHashSet<BidirectionalConnection> walkConnections, Action<JumpConnectionGenerator.Results> getResults)
		{
			NativeQueue<BidirectionalConnection> potentialJumpConnections = new NativeQueue<BidirectionalConnection>(AllocatorManager.Persistent);
			JumpConnectionGenerator.FindJumpConnectionsJob findJumpConnectionsJob = new JumpConnectionGenerator.FindJumpConnectionsJob
			{
				Octants = octants,
				SectionKeyArray = sectionKeyArray,
				SectionMap = sectionMap,
				SurfaceMap = surfaceMap,
				WalkableOctantDataMap = walkableOctantDataMap,
				WalkConnections = walkConnections,
				JumpConnections = potentialJumpConnections.AsParallelWriter()
			};
			int num = sectionKeyArray.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle = findJumpConnectionsJob.Schedule(sectionKeyArray.Length, num, default(JobHandle));
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			NativeParallelHashSet<BidirectionalConnection> jumpConnections = new NativeParallelHashSet<BidirectionalConnection>(1024, AllocatorManager.Persistent);
			JobHandle jobHandle2 = new JumpConnectionGenerator.ConvertQueueToHashset
			{
				Connections = potentialJumpConnections,
				JumpConnections = jumpConnections
			}.Schedule(default(JobHandle));
			potentialJumpConnections.Dispose(jobHandle2);
			yield return JobUtilities.WaitForJobToComplete(jobHandle2, false);
			getResults(new JumpConnectionGenerator.Results
			{
				JumpConnections = jumpConnections
			});
			yield break;
		}

		// Token: 0x020001C1 RID: 449
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct FindJumpConnectionsJob : IJobParallelFor
		{
			// Token: 0x060009D2 RID: 2514 RVA: 0x0002E934 File Offset: 0x0002CB34
			public void Execute(int index)
			{
				int num = this.SectionKeyArray[index];
				NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(64, AllocatorManager.Temp);
				UnsafeHashSet<int> unsafeHashSet = new UnsafeHashSet<int>(64, AllocatorManager.Temp);
				foreach (int num2 in this.SectionMap.GetValuesForKey(num))
				{
					NpcEnum.Edge edge = this.WalkableOctantDataMap[num2].Edge;
					int num3;
					if (Hint.Unlikely(!OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(this.Octants.GetRef(num2).Center, this.Octants, 1f, out num3)))
					{
						throw new Exception("This shouldn't be possible");
					}
					if (nativeHashSet.Add(num3))
					{
						float3 center = this.Octants.GetRef(num3).Center;
						if ((edge & NpcEnum.Edge.North) != NpcEnum.Edge.None)
						{
							for (int i = 0; i < 165; i++)
							{
								float3 jumpDirectionalOffset = BurstPathfindingUtilities.GetJumpDirectionalOffset(i, math.forward());
								int num4;
								if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpDirectionalOffset, this.Octants, 1f, out num4))
								{
									Octant octant = this.Octants[num4];
									if (octant.IsWalkable || octant.HasWalkableChildren)
									{
										WalkableOctantData walkableOctantData;
										if (octant.HasWalkableChildren)
										{
											for (int j = 0; j < 8; j++)
											{
												int num5;
												if (octant.TryGetChildIndex(j, out num5))
												{
													ref Octant @ref = ref this.Octants.GetRef(num5);
													for (int k = 0; k < 8; k++)
													{
														int num6;
														if (@ref.TryGetChildIndex(k, out num6) && this.Octants.GetRef(num6).IsWalkable)
														{
															unsafeHashSet.Add(this.WalkableOctantDataMap[num6].SectionKey);
														}
													}
												}
											}
										}
										else if (this.WalkableOctantDataMap.TryGetValue(num4, out walkableOctantData))
										{
											unsafeHashSet.Add(walkableOctantData.SectionKey);
										}
									}
								}
							}
						}
						if ((edge & NpcEnum.Edge.East) != NpcEnum.Edge.None)
						{
							for (int l = 0; l < 165; l++)
							{
								float3 jumpDirectionalOffset2 = BurstPathfindingUtilities.GetJumpDirectionalOffset(l, math.right());
								int num7;
								if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpDirectionalOffset2, this.Octants, 1f, out num7))
								{
									Octant octant2 = this.Octants[num7];
									if (octant2.IsWalkable || octant2.HasWalkableChildren)
									{
										WalkableOctantData walkableOctantData2;
										if (octant2.HasWalkableChildren)
										{
											for (int m = 0; m < 8; m++)
											{
												int num8;
												if (octant2.TryGetChildIndex(m, out num8))
												{
													ref Octant ref2 = ref this.Octants.GetRef(num8);
													for (int n = 0; n < 8; n++)
													{
														int num9;
														if (ref2.TryGetChildIndex(n, out num9) && this.Octants.GetRef(num9).IsWalkable)
														{
															unsafeHashSet.Add(this.WalkableOctantDataMap[num9].SectionKey);
														}
													}
												}
											}
										}
										else if (this.WalkableOctantDataMap.TryGetValue(num7, out walkableOctantData2))
										{
											unsafeHashSet.Add(walkableOctantData2.SectionKey);
										}
									}
								}
							}
						}
						if ((edge & NpcEnum.Edge.South) != NpcEnum.Edge.None)
						{
							for (int num10 = 0; num10 < 165; num10++)
							{
								float3 jumpDirectionalOffset3 = BurstPathfindingUtilities.GetJumpDirectionalOffset(num10, math.back());
								int num11;
								if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpDirectionalOffset3, this.Octants, 1f, out num11))
								{
									Octant octant3 = this.Octants[num11];
									if (octant3.IsWalkable || octant3.HasWalkableChildren)
									{
										WalkableOctantData walkableOctantData3;
										if (octant3.HasWalkableChildren)
										{
											for (int num12 = 0; num12 < 8; num12++)
											{
												int num13;
												if (octant3.TryGetChildIndex(num12, out num13))
												{
													ref Octant ref3 = ref this.Octants.GetRef(num13);
													for (int num14 = 0; num14 < 8; num14++)
													{
														int num15;
														if (ref3.TryGetChildIndex(num14, out num15) && this.Octants.GetRef(num15).IsWalkable)
														{
															unsafeHashSet.Add(this.WalkableOctantDataMap[num15].SectionKey);
														}
													}
												}
											}
										}
										else if (this.WalkableOctantDataMap.TryGetValue(num11, out walkableOctantData3))
										{
											unsafeHashSet.Add(walkableOctantData3.SectionKey);
										}
									}
								}
							}
						}
						if ((edge & NpcEnum.Edge.West) != NpcEnum.Edge.None)
						{
							for (int num16 = 0; num16 < 165; num16++)
							{
								float3 jumpDirectionalOffset4 = BurstPathfindingUtilities.GetJumpDirectionalOffset(num16, math.left());
								int num17;
								if (OctreeHelperMethods.TryGetSmallestOctantIndexForPoint(center + jumpDirectionalOffset4, this.Octants, 1f, out num17))
								{
									Octant octant4 = this.Octants[num17];
									if (octant4.IsWalkable || octant4.HasWalkableChildren)
									{
										WalkableOctantData walkableOctantData4;
										if (octant4.HasWalkableChildren)
										{
											for (int num18 = 0; num18 < 8; num18++)
											{
												int num19;
												if (octant4.TryGetChildIndex(num18, out num19))
												{
													ref Octant ref4 = ref this.Octants.GetRef(num19);
													for (int num20 = 0; num20 < 8; num20++)
													{
														int num21;
														if (ref4.TryGetChildIndex(num20, out num21) && this.Octants.GetRef(num21).IsWalkable)
														{
															unsafeHashSet.Add(this.WalkableOctantDataMap[num21].SectionKey);
														}
													}
												}
											}
										}
										else if (this.WalkableOctantDataMap.TryGetValue(num17, out walkableOctantData4))
										{
											unsafeHashSet.Add(walkableOctantData4.SectionKey);
										}
									}
								}
							}
						}
						foreach (int num22 in unsafeHashSet)
						{
							if (num != num22)
							{
								BidirectionalConnection bidirectionalConnection = new BidirectionalConnection(num, num22);
								if (!this.WalkConnections.Contains(bidirectionalConnection))
								{
									SectionSurface sectionSurface = this.SurfaceMap[num];
									SectionSurface sectionSurface2 = this.SurfaceMap[num22];
									foreach (BurstPathfindingUtilities.SurfacePair surfacePair in BurstPathfindingUtilities.GetSortedJumpPairs(sectionSurface, sectionSurface2))
									{
										if (BurstPathfindingUtilities.CanReachJumpPosition(in surfacePair.JumpPoint, in surfacePair.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
										{
											float num23 = BurstPathfindingUtilities.EstimateLaunchAngle(in surfacePair.JumpPoint, in surfacePair.LandPoint);
											float3 @float;
											float num24;
											if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in surfacePair.JumpPoint, in surfacePair.LandPoint, num23, NpcMovementValues.Gravity, out @float, out num24) && !float.IsNaN(num24) && num24 > 0f && !BurstPathfindingUtilities.IsPathBlocked(surfacePair.JumpPoint, surfacePair.LandPoint, num24, @float, this.Octants))
											{
												this.JumpConnections.Enqueue(bidirectionalConnection);
												break;
											}
										}
									}
								}
							}
						}
						nativeHashSet.Clear();
						unsafeHashSet.Clear();
					}
				}
			}

			// Token: 0x0400081A RID: 2074
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x0400081B RID: 2075
			[ReadOnly]
			public NativeArray<int> SectionKeyArray;

			// Token: 0x0400081C RID: 2076
			[ReadOnly]
			public NativeParallelMultiHashMap<int, int> SectionMap;

			// Token: 0x0400081D RID: 2077
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x0400081E RID: 2078
			[ReadOnly]
			public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

			// Token: 0x0400081F RID: 2079
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> WalkConnections;

			// Token: 0x04000820 RID: 2080
			[WriteOnly]
			public NativeQueue<BidirectionalConnection>.ParallelWriter JumpConnections;
		}

		// Token: 0x020001C2 RID: 450
		[BurstCompile]
		private struct ConvertQueueToHashset : IJob
		{
			// Token: 0x060009D3 RID: 2515 RVA: 0x0002F018 File Offset: 0x0002D218
			public void Execute()
			{
				while (this.Connections.Count > 0)
				{
					this.JumpConnections.Add(this.Connections.Dequeue());
				}
			}

			// Token: 0x04000821 RID: 2081
			public NativeQueue<BidirectionalConnection> Connections;

			// Token: 0x04000822 RID: 2082
			[WriteOnly]
			public NativeParallelHashSet<BidirectionalConnection> JumpConnections;
		}

		// Token: 0x020001C3 RID: 451
		public struct Results
		{
			// Token: 0x04000823 RID: 2083
			public NativeParallelHashSet<BidirectionalConnection> JumpConnections;
		}
	}
}
