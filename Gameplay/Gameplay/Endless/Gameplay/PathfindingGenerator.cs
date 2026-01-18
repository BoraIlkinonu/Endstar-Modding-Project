using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200020E RID: 526
	public static class PathfindingGenerator
	{
		// Token: 0x06000B00 RID: 2816 RVA: 0x0003BC11 File Offset: 0x00039E11
		public static IEnumerator GeneratePathfindingResults(List<Pathfinding.Request> pathfindingRequests, NativeParallelHashMap<int, GraphNode> nodeMap, NativeParallelMultiHashMap<int, Edge> edgeMap, NativeParallelHashMap<int, SectionSurface> surfaceMap, NativeArray<Octant> octants, int numSections, Action<PathfindingGenerator.Results> getResults)
		{
			Pathfinding.Request[] requests = pathfindingRequests.ToArray();
			int num = requests.Length;
			NativeArray<NativePathfindingRequest> nativeArray = new NativeArray<NativePathfindingRequest>(requests.Length, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<UnsafeList<PathfindingNode>> openSets = new NativeArray<UnsafeList<PathfindingNode>>(num, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<UnsafeHashMap<int, PathfindingNode>> pathfindingNodeMaps = new NativeArray<UnsafeHashMap<int, PathfindingNode>>(num, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<UnsafeHashSet<ExcludedEdge>> excludedThresholdEdges = new NativeArray<UnsafeHashSet<ExcludedEdge>>(num, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<int> nativeArray2 = new NativeArray<int>(num * numSections * 2, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<ConnectionKind> nativeArray3 = new NativeArray<ConnectionKind>(num * numSections * 2, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<int> nativeArray4 = new NativeArray<int>(num, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<NavPath.Segment> collapsedPaths = new NativeArray<NavPath.Segment>(numSections * num * 2, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<int> collapsedPathLengths = new NativeArray<int>(num, Allocator.TempJob, NativeArrayOptions.ClearMemory);
			NativeArray<Octant> pathfindingOctants = new NativeArray<Octant>(octants, Allocator.Persistent);
			for (int i = 0; i < requests.Length; i++)
			{
				nativeArray[i] = pathfindingRequests[i].GetNativePathfindingRequest();
				openSets[i] = new UnsafeList<PathfindingNode>(numSections, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				pathfindingNodeMaps[i] = new UnsafeHashMap<int, PathfindingNode>(numSections, Allocator.TempJob);
				UnsafeHashSet<ExcludedEdge> unsafeHashSet = new UnsafeHashSet<ExcludedEdge>(pathfindingRequests[i].ExcludedEdges.Count, Allocator.TempJob);
				foreach (ExcludedEdge excludedEdge in pathfindingRequests[i].ExcludedEdges)
				{
					unsafeHashSet.Add(excludedEdge);
				}
				excludedThresholdEdges[i] = unsafeHashSet;
			}
			PathfindingGenerator.PathfindingJob pathfindingJob = new PathfindingGenerator.PathfindingJob
			{
				PathfindingRequests = nativeArray,
				MaxPathLength = numSections * 2,
				EdgeMap = edgeMap,
				GraphNodeMap = nodeMap,
				FallOffHeight = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight,
				OpenSets = openSets,
				PathfindingNodeMaps = pathfindingNodeMaps,
				NavigationPaths = nativeArray2,
				ConnectionKinds = nativeArray3,
				NavigationPathLengths = nativeArray4,
				ExcludedThresholdEdges = excludedThresholdEdges
			};
			PathfindingGenerator.CollapsePathsJob collapsePathsJob = new PathfindingGenerator.CollapsePathsJob
			{
				MaxPathLength = numSections * 2,
				NavigationPaths = nativeArray2,
				ConnectionKindData = nativeArray3,
				NavigationPathLengths = nativeArray4,
				CollapsedPaths = collapsedPaths,
				CollapsedPathLengths = collapsedPathLengths
			};
			PathfindingGenerator.PadJumpSegmentsJob padJumpSegmentsJob = new PathfindingGenerator.PadJumpSegmentsJob
			{
				MaxPathLength = numSections * 2,
				NavigationPaths = collapsedPaths,
				NavigationPathLengths = collapsedPathLengths
			};
			PathfindingGenerator.ConvertPathSectionKeysToPointsJob convertPathSectionKeysToPointsJob = new PathfindingGenerator.ConvertPathSectionKeysToPointsJob
			{
				MaxPathLength = numSections * 2,
				Requests = nativeArray,
				SurfaceMap = surfaceMap,
				Octants = pathfindingOctants,
				NavigationPaths = collapsedPaths,
				NavigationPathLengths = collapsedPathLengths
			};
			int num2 = num / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle = pathfindingJob.Schedule(num, num2, default(JobHandle));
			jobHandle = collapsePathsJob.Schedule(num, num2, jobHandle);
			nativeArray2.Dispose(jobHandle);
			nativeArray3.Dispose(jobHandle);
			nativeArray4.Dispose(jobHandle);
			jobHandle = padJumpSegmentsJob.Schedule(num, num2, jobHandle);
			jobHandle = convertPathSectionKeysToPointsJob.Schedule(num, num2, jobHandle);
			nativeArray.Dispose(jobHandle);
			yield return JobUtilities.WaitForJobToComplete(jobHandle, true);
			getResults(new PathfindingGenerator.Results
			{
				Requests = requests,
				CompletedPaths = collapsedPaths.ToArray(),
				CompletedPathLengths = collapsedPathLengths.ToArray()
			});
			JobHandle jobHandle2 = default(JobHandle);
			for (int j = 0; j < requests.Length; j++)
			{
				jobHandle2 = openSets[j].Dispose(jobHandle2);
				jobHandle2 = pathfindingNodeMaps[j].Dispose(jobHandle2);
				jobHandle2 = excludedThresholdEdges[j].Dispose(jobHandle2);
			}
			openSets.Dispose(jobHandle2);
			pathfindingNodeMaps.Dispose(jobHandle2);
			excludedThresholdEdges.Dispose(jobHandle2);
			pathfindingOctants.Dispose(default(JobHandle));
			collapsedPaths.Dispose(default(JobHandle));
			collapsedPathLengths.Dispose(default(JobHandle));
			yield break;
		}

		// Token: 0x0200020F RID: 527
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct PathfindingJob : IJobParallelFor
		{
			// Token: 0x06000B01 RID: 2817 RVA: 0x0003BC50 File Offset: 0x00039E50
			public void Execute(int index)
			{
				NativePathfindingRequest nativePathfindingRequest = this.PathfindingRequests[index];
				int startNodeKey = nativePathfindingRequest.StartNodeKey;
				int endNodeKey = nativePathfindingRequest.EndNodeKey;
				if (startNodeKey == -1)
				{
					return;
				}
				float3 @float = nativePathfindingRequest.EndPosition;
				UnsafeHashSet<ExcludedEdge> unsafeHashSet = this.ExcludedThresholdEdges[index];
				UnsafeList<PathfindingNode> unsafeList = this.OpenSets[index];
				UnsafeHashMap<int, PathfindingNode> unsafeHashMap = this.PathfindingNodeMaps[index];
				int num = 0;
				NativeArray<int> subArray = this.NavigationPaths.GetSubArray(this.MaxPathLength * index, this.MaxPathLength);
				NativeArray<ConnectionKind> subArray2 = this.ConnectionKinds.GetSubArray(this.MaxPathLength * index, this.MaxPathLength);
				int num2 = 0;
				PathfindingNode pathfindingNode = this.GraphNodeMap[startNodeKey].GetPathfindingNode();
				PathfindingNode pathfindingNode2 = new PathfindingNode
				{
					Key = -1,
					Center = Float3Extensions.Invalid
				};
				unsafeHashMap.Add(startNodeKey, pathfindingNode);
				unsafeHashMap.Add(-1, pathfindingNode2);
				unsafeList.Add(in pathfindingNode);
				while (unsafeList.Length > 0 && num < 600)
				{
					num++;
					unsafeList.Sort(default(PathfindingGenerator.PathfindingJob.NodeComparer));
					PathfindingNode pathfindingNode3 = unsafeList[0];
					unsafeList.RemoveAt(0);
					if (pathfindingNode3.Key.Equals(endNodeKey))
					{
						while (!pathfindingNode3.Key.Equals(-1))
						{
							subArray[num2] = pathfindingNode3.Key;
							PathfindingNode pathfindingNode4 = unsafeHashMap[pathfindingNode3.Parent];
							if (!pathfindingNode4.Key.Equals(-1))
							{
								subArray2[num2] = pathfindingNode3.ParentConnection;
							}
							pathfindingNode3 = pathfindingNode4;
							num2++;
						}
						this.NavigationPathLengths[index] = num2;
						int num3 = num2 / 2;
						for (int i = 0; i < num3; i++)
						{
							int num4 = i;
							ref NativeArray<int> ptr = ref subArray;
							int num5 = num2 - 1 - i;
							int num6 = subArray[num2 - 1 - i];
							int num7 = subArray[i];
							subArray[num4] = num6;
							ptr[num5] = num7;
						}
						int num8 = (num2 - 1) / 2;
						for (int j = 0; j < num8; j++)
						{
							int num5 = j;
							ref NativeArray<ConnectionKind> ptr2 = ref subArray2;
							int num4 = num2 - 2 - j;
							ConnectionKind connectionKind = subArray2[num2 - 2 - j];
							ConnectionKind connectionKind2 = subArray2[j];
							subArray2[num5] = connectionKind;
							ptr2[num4] = connectionKind2;
						}
						return;
					}
					foreach (Edge edge in this.GetConnectedNodes(pathfindingNode3.Key))
					{
						if (unsafeHashMap.ContainsKey(edge.ConnectedNodeKey))
						{
							if (edge.ConnectedNodeKey != startNodeKey)
							{
								PathfindingNode pathfindingNode5 = unsafeHashMap[edge.ConnectedNodeKey];
								if (unsafeList.Contains(pathfindingNode5))
								{
									float num9 = pathfindingNode3.CostToNode + edge.Cost;
									if (num9 < pathfindingNode5.CostToNode)
									{
										int num10 = unsafeList.IndexOf(pathfindingNode5);
										pathfindingNode5.CostToNode = num9;
										pathfindingNode5.Parent = pathfindingNode3.Key;
										pathfindingNode5.ParentConnection = edge.Connection;
										unsafeList[num10] = pathfindingNode5;
										unsafeHashMap[edge.ConnectedNodeKey] = pathfindingNode5;
									}
								}
								else
								{
									float num11 = pathfindingNode3.CostToNode + edge.Cost;
									if (pathfindingNode5.CostToNode > num11)
									{
										pathfindingNode5.CostToNode = num11;
										pathfindingNode5.Parent = pathfindingNode3.Key;
										pathfindingNode5.ParentConnection = edge.Connection;
										unsafeList.Add(in pathfindingNode5);
										unsafeHashMap[edge.ConnectedNodeKey] = pathfindingNode5;
									}
								}
							}
						}
						else if (!PathfindingGenerator.PathfindingJob.IsEdgeExcluded(unsafeHashSet, pathfindingNode3, edge) && !this.IsOutNodeOutOfRange(nativePathfindingRequest, edge) && this.GraphNodeMap[edge.ConnectedNodeKey].Center.y > this.FallOffHeight)
						{
							PathfindingNode pathfindingNode6 = this.GraphNodeMap[edge.ConnectedNodeKey].GetPathfindingNode();
							pathfindingNode6.CostToNode = pathfindingNode3.CostToNode + edge.Cost;
							pathfindingNode6.CostToGoal = math.distance(pathfindingNode6.Center, @float);
							pathfindingNode6.ParentConnection = edge.Connection;
							pathfindingNode6.Parent = pathfindingNode3.Key;
							unsafeList.Add(in pathfindingNode6);
							unsafeHashMap.Add(edge.ConnectedNodeKey, pathfindingNode6);
						}
					}
				}
			}

			// Token: 0x06000B02 RID: 2818 RVA: 0x0003C0EC File Offset: 0x0003A2EC
			private static bool IsEdgeExcluded(UnsafeHashSet<ExcludedEdge> excludedThresholdEdges, PathfindingNode currentNode, Edge edge)
			{
				return excludedThresholdEdges.Count != 0 && excludedThresholdEdges.Contains(new ExcludedEdge
				{
					OriginNodeKey = currentNode.Key,
					EndNodeKey = edge.ConnectedNodeKey
				});
			}

			// Token: 0x06000B03 RID: 2819 RVA: 0x0003C130 File Offset: 0x0003A330
			private bool IsOutNodeOutOfRange(NativePathfindingRequest request, Edge edge)
			{
				bool flag;
				switch (request.PathfindingRange)
				{
				case PathfindingRange.Island:
				{
					ConnectionKind connection = edge.Connection;
					flag = connection != ConnectionKind.Walk && connection != ConnectionKind.Threshold;
					break;
				}
				case PathfindingRange.Area:
					flag = this.GraphNodeMap[request.StartNodeKey].AreaKey != this.GraphNodeMap[edge.ConnectedNodeKey].AreaKey;
					break;
				case PathfindingRange.Global:
					flag = false;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				return flag;
			}

			// Token: 0x06000B04 RID: 2820 RVA: 0x0003C1AF File Offset: 0x0003A3AF
			private NativeParallelMultiHashMap<int, Edge>.Enumerator GetConnectedNodes(int nodeKey)
			{
				return this.EdgeMap.GetValuesForKey(nodeKey);
			}

			// Token: 0x04000A4C RID: 2636
			private const int MAX_ITERATIONS = 600;

			// Token: 0x04000A4D RID: 2637
			[ReadOnly]
			public NativeArray<NativePathfindingRequest> PathfindingRequests;

			// Token: 0x04000A4E RID: 2638
			[ReadOnly]
			public int MaxPathLength;

			// Token: 0x04000A4F RID: 2639
			[ReadOnly]
			public NativeParallelMultiHashMap<int, Edge> EdgeMap;

			// Token: 0x04000A50 RID: 2640
			[ReadOnly]
			public NativeParallelHashMap<int, GraphNode> GraphNodeMap;

			// Token: 0x04000A51 RID: 2641
			[ReadOnly]
			public float FallOffHeight;

			// Token: 0x04000A52 RID: 2642
			[ReadOnly]
			public NativeArray<UnsafeHashSet<ExcludedEdge>> ExcludedThresholdEdges;

			// Token: 0x04000A53 RID: 2643
			[NativeDisableParallelForRestriction]
			public NativeArray<UnsafeList<PathfindingNode>> OpenSets;

			// Token: 0x04000A54 RID: 2644
			[NativeDisableParallelForRestriction]
			public NativeArray<UnsafeHashMap<int, PathfindingNode>> PathfindingNodeMaps;

			// Token: 0x04000A55 RID: 2645
			[NativeDisableParallelForRestriction]
			public NativeArray<int> NavigationPaths;

			// Token: 0x04000A56 RID: 2646
			[NativeDisableParallelForRestriction]
			public NativeArray<ConnectionKind> ConnectionKinds;

			// Token: 0x04000A57 RID: 2647
			[WriteOnly]
			[NativeDisableParallelForRestriction]
			public NativeArray<int> NavigationPathLengths;

			// Token: 0x02000210 RID: 528
			private struct NodeComparer : IComparer<PathfindingNode>
			{
				// Token: 0x06000B05 RID: 2821 RVA: 0x0003C1C0 File Offset: 0x0003A3C0
				[BurstCompile]
				public int Compare(PathfindingNode x, PathfindingNode y)
				{
					return x.TotalCost.CompareTo(y.TotalCost);
				}
			}
		}

		// Token: 0x02000211 RID: 529
		[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
		private struct CollapsePathsJob : IJobParallelFor
		{
			// Token: 0x06000B06 RID: 2822 RVA: 0x0003C1E4 File Offset: 0x0003A3E4
			public void Execute(int index)
			{
				if (this.NavigationPathLengths[index] == 0)
				{
					this.CollapsedPathLengths[index] = 0;
					return;
				}
				int num = this.NavigationPathLengths[index];
				NativeArray<int> subArray = this.NavigationPaths.GetSubArray(this.MaxPathLength * index, num);
				NativeArray<ConnectionKind> subArray2 = this.ConnectionKindData.GetSubArray(this.MaxPathLength * index, num - 1);
				NativeArray<NavPath.Segment> subArray3 = this.CollapsedPaths.GetSubArray(this.MaxPathLength * index, this.MaxPathLength);
				int num2 = 0;
				int num3 = 0;
				int i = 1;
				while (i < subArray.Length)
				{
					if (subArray2[num3] != ConnectionKind.Walk)
					{
						subArray3[num2] = new NavPath.Segment
						{
							StartSection = subArray[num3],
							EndSection = subArray[i],
							ConnectionKind = subArray2[num3]
						};
						num2++;
						num3 = i;
						i++;
					}
					else
					{
						while (i < subArray.Length - 1 && subArray2[i] == ConnectionKind.Walk)
						{
							i++;
						}
						subArray3[num2] = new NavPath.Segment
						{
							StartSection = subArray[num3],
							EndSection = subArray[i],
							ConnectionKind = ConnectionKind.Walk
						};
						num2++;
						num3 = i;
						i++;
					}
				}
				this.CollapsedPathLengths[index] = num2;
			}

			// Token: 0x04000A58 RID: 2648
			[ReadOnly]
			public int MaxPathLength;

			// Token: 0x04000A59 RID: 2649
			[ReadOnly]
			public NativeArray<int> NavigationPaths;

			// Token: 0x04000A5A RID: 2650
			[ReadOnly]
			public NativeArray<ConnectionKind> ConnectionKindData;

			// Token: 0x04000A5B RID: 2651
			[ReadOnly]
			public NativeArray<int> NavigationPathLengths;

			// Token: 0x04000A5C RID: 2652
			[NativeDisableParallelForRestriction]
			public NativeArray<NavPath.Segment> CollapsedPaths;

			// Token: 0x04000A5D RID: 2653
			[NativeDisableParallelForRestriction]
			public NativeArray<int> CollapsedPathLengths;
		}

		// Token: 0x02000212 RID: 530
		[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
		private struct PadJumpSegmentsJob : IJobParallelFor
		{
			// Token: 0x06000B07 RID: 2823 RVA: 0x0003C354 File Offset: 0x0003A554
			public void Execute(int index)
			{
				if (this.NavigationPathLengths[index] == 0)
				{
					return;
				}
				NativeArray<NavPath.Segment> subArray = this.NavigationPaths.GetSubArray(this.MaxPathLength * index, this.MaxPathLength);
				int num = this.NavigationPathLengths[index];
				for (int i = 0; i < num - 1; i++)
				{
					if (PathfindingGenerator.PadJumpSegmentsJob.ShouldInsertWalkSegment(subArray[i].ConnectionKind, subArray[i + 1].ConnectionKind))
					{
						if (num + 1 >= this.MaxPathLength)
						{
							Debug.LogWarning("Padded length would exceed maximum length, aborting padding");
							this.NavigationPathLengths[index] = 0;
							return;
						}
						NavPath.Segment segment = new NavPath.Segment
						{
							StartSection = subArray[i].EndSection,
							EndSection = subArray[i].EndSection,
							ConnectionKind = ConnectionKind.Walk
						};
						PathfindingGenerator.PadJumpSegmentsJob.Insert(subArray, i + 1, num, segment);
						num++;
						i++;
					}
				}
				if (subArray[0].ConnectionKind != ConnectionKind.Walk)
				{
					int startSection = subArray[0].StartSection;
					NavPath.Segment segment2 = new NavPath.Segment
					{
						StartSection = startSection,
						EndSection = startSection,
						ConnectionKind = ConnectionKind.Walk
					};
					PathfindingGenerator.PadJumpSegmentsJob.Insert(subArray, 0, num, segment2);
					num++;
				}
				if (num > 1 && subArray[num - 1].ConnectionKind != ConnectionKind.Walk)
				{
					int endSection = subArray[num - 1].EndSection;
					NavPath.Segment segment3 = new NavPath.Segment
					{
						StartSection = endSection,
						EndSection = endSection,
						ConnectionKind = ConnectionKind.Walk
					};
					subArray[num] = segment3;
					num++;
				}
				this.NavigationPathLengths[index] = num;
				for (int j = 0; j < num - 1; j++)
				{
					ref NavPath.Segment ptr = subArray[j];
					NavPath.Segment segment4 = subArray[j + 1];
					if (ptr.EndSection != segment4.StartSection)
					{
						Debug.Log("MalformedPath");
					}
				}
			}

			// Token: 0x06000B08 RID: 2824 RVA: 0x0003C53C File Offset: 0x0003A73C
			private static void Insert(NativeArray<NavPath.Segment> segments, int index, int length, NavPath.Segment segment)
			{
				for (int i = length - 1; i >= index; i--)
				{
					segments[i + 1] = segments[i];
				}
				segments[index] = segment;
			}

			// Token: 0x06000B09 RID: 2825 RVA: 0x0003C572 File Offset: 0x0003A772
			private static bool ShouldInsertWalkSegment(ConnectionKind segmentKind, ConnectionKind nextSectionKind)
			{
				return segmentKind != ConnectionKind.Walk && nextSectionKind > ConnectionKind.Walk;
			}

			// Token: 0x04000A5E RID: 2654
			[ReadOnly]
			public int MaxPathLength;

			// Token: 0x04000A5F RID: 2655
			[NativeDisableParallelForRestriction]
			public NativeArray<int> NavigationPathLengths;

			// Token: 0x04000A60 RID: 2656
			[NativeDisableParallelForRestriction]
			public NativeArray<NavPath.Segment> NavigationPaths;
		}

		// Token: 0x02000213 RID: 531
		[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
		private struct ConvertPathSectionKeysToPointsJob : IJobParallelFor
		{
			// Token: 0x06000B0A RID: 2826 RVA: 0x0003C580 File Offset: 0x0003A780
			public void Execute(int index)
			{
				if (this.NavigationPathLengths[index] == 0)
				{
					return;
				}
				NativeArray<NavPath.Segment> subArray = this.NavigationPaths.GetSubArray(this.MaxPathLength * index, this.NavigationPathLengths[index]);
				NativePathfindingRequest nativePathfindingRequest = this.Requests[index];
				NavPath.Segment segment = subArray[0];
				NavPath.Segment segment2 = subArray[subArray.Length - 1];
				segment.StartPosition = nativePathfindingRequest.StartPosition;
				segment2.EndPosition = nativePathfindingRequest.EndPosition;
				subArray[0] = segment;
				subArray[subArray.Length - 1] = segment2;
				if (subArray.Length % 2 == 0)
				{
					Debug.Log("Malformed path. Unable to convert from segments to points");
					this.NavigationPathLengths[index] = 0;
					return;
				}
				for (int i = 0; i < subArray.Length; i += 2)
				{
					if (i < subArray.Length - 2)
					{
						switch (subArray[i + 1].ConnectionKind)
						{
						case ConnectionKind.Walk:
							Debug.LogWarning("We shouldn't ever get here.");
							break;
						case ConnectionKind.Threshold:
						{
							NavPath.Segment segment3 = subArray[i];
							NavPath.Segment segment4 = subArray[i + 1];
							NavPath.Segment segment5 = subArray[i + 2];
							segment3.EndPosition = this.SurfaceMap[segment3.EndSection].Center;
							segment4.StartPosition = this.SurfaceMap[segment3.EndSection].Center;
							segment4.EndPosition = this.SurfaceMap[segment4.EndSection].Center;
							segment5.StartPosition = this.SurfaceMap[segment4.EndSection].Center;
							subArray[i] = segment3;
							subArray[i + 1] = segment4;
							subArray[i + 2] = segment5;
							break;
						}
						case ConnectionKind.Jump:
						case ConnectionKind.Dropdown:
						{
							NavPath.Segment segment6 = subArray[i];
							NavPath.Segment segment7 = subArray[i + 1];
							NavPath.Segment segment8 = subArray[i + 2];
							SectionSurface sectionSurface = this.SurfaceMap[segment6.EndSection];
							SectionSurface sectionSurface2 = this.SurfaceMap[segment7.EndSection];
							float3 startPosition = segment6.StartPosition;
							float3 center = this.SurfaceMap[segment8.EndSection].Center;
							float3 @float = float3.zero;
							float3 float2 = float3.zero;
							foreach (PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair jumpPointPair in PathfindingGenerator.ConvertPathSectionKeysToPointsJob.GetBestJumpPointsPairs(startPosition, sectionSurface, sectionSurface2, center))
							{
								if (BurstPathfindingUtilities.CanReachJumpPosition(in jumpPointPair.JumpPoint, in jumpPointPair.LandPoint, NpcMovementValues.MaxVerticalVelocity * 1.2f, NpcMovementValues.MaxHorizontalVelocity * 1.2f, NpcMovementValues.Gravity))
								{
									float num = BurstPathfindingUtilities.EstimateLaunchAngle(in jumpPointPair.JumpPoint, in jumpPointPair.LandPoint);
									float3 float3;
									float num2;
									if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in jumpPointPair.JumpPoint, in jumpPointPair.LandPoint, num, NpcMovementValues.Gravity, out float3, out num2) && !float.IsNaN(num2) && num2 > 0f && !BurstPathfindingUtilities.IsPathBlocked(jumpPointPair.JumpPoint, jumpPointPair.LandPoint, num2, float3, this.Octants))
									{
										@float = jumpPointPair.JumpPoint;
										float2 = jumpPointPair.LandPoint;
										break;
									}
								}
							}
							segment6.EndPosition = @float;
							segment7.StartPosition = @float;
							segment7.EndPosition = float2;
							segment8.StartPosition = float2;
							subArray[i] = segment6;
							subArray[i + 1] = segment7;
							subArray[i + 2] = segment8;
							break;
						}
						case ConnectionKind.Swim:
							Debug.LogError("Swim is not yet supported");
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
					}
					else
					{
						NavPath.Segment segment9 = subArray[i];
						segment9.EndPosition = nativePathfindingRequest.EndPosition;
						subArray[i] = segment9;
					}
				}
			}

			// Token: 0x06000B0B RID: 2827 RVA: 0x0003C984 File Offset: 0x0003AB84
			private static NativeList<PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair> GetBestJumpPointsPairs(float3 startAnchor, SectionSurface jumpSurface, SectionSurface landSurface, float3 landAnchor)
			{
				NativeList<PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair> nativeList = new NativeList<PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair>(128, AllocatorManager.Temp);
				foreach (float3 @float in jumpSurface.GetSurfaceSamplePoints())
				{
					foreach (float3 float2 in landSurface.GetSurfaceSamplePoints())
					{
						PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair jumpPointPair = new PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair(startAnchor, @float, float2, landAnchor);
						nativeList.Add(in jumpPointPair);
					}
				}
				nativeList.Sort<PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair>();
				return nativeList;
			}

			// Token: 0x04000A61 RID: 2657
			[ReadOnly]
			public int MaxPathLength;

			// Token: 0x04000A62 RID: 2658
			[ReadOnly]
			public NativeArray<NativePathfindingRequest> Requests;

			// Token: 0x04000A63 RID: 2659
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x04000A64 RID: 2660
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x04000A65 RID: 2661
			[NativeDisableParallelForRestriction]
			public NativeArray<NavPath.Segment> NavigationPaths;

			// Token: 0x04000A66 RID: 2662
			[NativeDisableParallelForRestriction]
			public NativeArray<int> NavigationPathLengths;

			// Token: 0x02000214 RID: 532
			private readonly struct JumpPointPair : IComparable<PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair>
			{
				// Token: 0x06000B0C RID: 2828 RVA: 0x0003CA44 File Offset: 0x0003AC44
				public JumpPointPair(float3 jumpAnchor, float3 jumpPoint, float3 landPoint, float3 landAnchor)
				{
					this.JumpPoint = jumpPoint;
					this.LandPoint = landPoint;
					this.totalDistance = math.distance(jumpAnchor, jumpPoint) + math.distance(jumpPoint, landPoint) * NpcMovementValues.JumpCostScalar + math.distance(landPoint, landAnchor);
				}

				// Token: 0x06000B0D RID: 2829 RVA: 0x0003CA78 File Offset: 0x0003AC78
				public int CompareTo(PathfindingGenerator.ConvertPathSectionKeysToPointsJob.JumpPointPair other)
				{
					return this.totalDistance.CompareTo(other.totalDistance);
				}

				// Token: 0x04000A67 RID: 2663
				public readonly float3 JumpPoint;

				// Token: 0x04000A68 RID: 2664
				public readonly float3 LandPoint;

				// Token: 0x04000A69 RID: 2665
				private readonly float totalDistance;
			}
		}

		// Token: 0x02000215 RID: 533
		public struct Results
		{
			// Token: 0x04000A6A RID: 2666
			public Pathfinding.Request[] Requests;

			// Token: 0x04000A6B RID: 2667
			public NavPath.Segment[] CompletedPaths;

			// Token: 0x04000A6C RID: 2668
			public int[] CompletedPathLengths;
		}
	}
}
