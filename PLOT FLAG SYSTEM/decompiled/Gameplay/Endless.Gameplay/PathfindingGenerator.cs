using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public static class PathfindingGenerator
{
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct PathfindingJob : IJobParallelFor
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct NodeComparer : IComparer<PathfindingNode>
		{
			[BurstCompile]
			public int Compare(PathfindingNode x, PathfindingNode y)
			{
				return x.TotalCost.CompareTo(y.TotalCost);
			}
		}

		private const int MAX_ITERATIONS = 600;

		[ReadOnly]
		public NativeArray<NativePathfindingRequest> PathfindingRequests;

		[ReadOnly]
		public int MaxPathLength;

		[ReadOnly]
		public NativeParallelMultiHashMap<int, Edge> EdgeMap;

		[ReadOnly]
		public NativeParallelHashMap<int, GraphNode> GraphNodeMap;

		[ReadOnly]
		public float FallOffHeight;

		[ReadOnly]
		public NativeArray<UnsafeHashSet<ExcludedEdge>> ExcludedThresholdEdges;

		[NativeDisableParallelForRestriction]
		public NativeArray<UnsafeList<PathfindingNode>> OpenSets;

		[NativeDisableParallelForRestriction]
		public NativeArray<UnsafeHashMap<int, PathfindingNode>> PathfindingNodeMaps;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> NavigationPaths;

		[NativeDisableParallelForRestriction]
		public NativeArray<ConnectionKind> ConnectionKinds;

		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<int> NavigationPathLengths;

		public void Execute(int index)
		{
			NativePathfindingRequest request = PathfindingRequests[index];
			int startNodeKey = request.StartNodeKey;
			int endNodeKey = request.EndNodeKey;
			if (startNodeKey == -1)
			{
				return;
			}
			float3 y = request.EndPosition;
			UnsafeHashSet<ExcludedEdge> excludedThresholdEdges = ExcludedThresholdEdges[index];
			UnsafeList<PathfindingNode> list = OpenSets[index];
			UnsafeHashMap<int, PathfindingNode> unsafeHashMap = PathfindingNodeMaps[index];
			int num = 0;
			NativeArray<int> subArray = NavigationPaths.GetSubArray(MaxPathLength * index, MaxPathLength);
			NativeArray<ConnectionKind> subArray2 = ConnectionKinds.GetSubArray(MaxPathLength * index, MaxPathLength);
			int num2 = 0;
			PathfindingNode value = GraphNodeMap[startNodeKey].GetPathfindingNode();
			PathfindingNode item = new PathfindingNode
			{
				Key = -1,
				Center = Float3Extensions.Invalid
			};
			unsafeHashMap.Add(startNodeKey, value);
			unsafeHashMap.Add(-1, item);
			list.Add(in value);
			while (list.Length > 0 && num < 600)
			{
				num++;
				list.Sort(default(NodeComparer));
				PathfindingNode currentNode = list[0];
				list.RemoveAt(0);
				if (currentNode.Key.Equals(endNodeKey))
				{
					while (!currentNode.Key.Equals(-1))
					{
						subArray[num2] = currentNode.Key;
						PathfindingNode pathfindingNode = unsafeHashMap[currentNode.Parent];
						if (!pathfindingNode.Key.Equals(-1))
						{
							subArray2[num2] = currentNode.ParentConnection;
						}
						currentNode = pathfindingNode;
						num2++;
					}
					NavigationPathLengths[index] = num2;
					int num3 = num2 / 2;
					for (int i = 0; i < num3; i++)
					{
						int index2 = i;
						int index3 = num2 - 1 - i;
						int num4 = subArray[num2 - 1 - i];
						int num5 = subArray[i];
						int num6 = (subArray[index2] = num4);
						num6 = (subArray[index3] = num5);
					}
					int num9 = (num2 - 1) / 2;
					for (int j = 0; j < num9; j++)
					{
						int index3 = j;
						int index2 = num2 - 2 - j;
						ConnectionKind connectionKind = subArray2[num2 - 2 - j];
						ConnectionKind connectionKind2 = subArray2[j];
						ConnectionKind connectionKind3 = (subArray2[index3] = connectionKind);
						connectionKind3 = (subArray2[index2] = connectionKind2);
					}
					break;
				}
				foreach (Edge connectedNode in GetConnectedNodes(currentNode.Key))
				{
					if (unsafeHashMap.ContainsKey(connectedNode.ConnectedNodeKey))
					{
						if (connectedNode.ConnectedNodeKey == startNodeKey)
						{
							continue;
						}
						PathfindingNode value2 = unsafeHashMap[connectedNode.ConnectedNodeKey];
						if (list.Contains(value2))
						{
							float num10 = currentNode.CostToNode + connectedNode.Cost;
							if (num10 < value2.CostToNode)
							{
								int index4 = list.IndexOf(value2);
								value2.CostToNode = num10;
								value2.Parent = currentNode.Key;
								value2.ParentConnection = connectedNode.Connection;
								list[index4] = value2;
								unsafeHashMap[connectedNode.ConnectedNodeKey] = value2;
							}
						}
						else
						{
							float num11 = currentNode.CostToNode + connectedNode.Cost;
							if (value2.CostToNode > num11)
							{
								value2.CostToNode = num11;
								value2.Parent = currentNode.Key;
								value2.ParentConnection = connectedNode.Connection;
								list.Add(in value2);
								unsafeHashMap[connectedNode.ConnectedNodeKey] = value2;
							}
						}
					}
					else if (!IsEdgeExcluded(excludedThresholdEdges, currentNode, connectedNode) && !IsOutNodeOutOfRange(request, connectedNode) && !(GraphNodeMap[connectedNode.ConnectedNodeKey].Center.y <= FallOffHeight))
					{
						PathfindingNode value3 = GraphNodeMap[connectedNode.ConnectedNodeKey].GetPathfindingNode();
						value3.CostToNode = currentNode.CostToNode + connectedNode.Cost;
						value3.CostToGoal = math.distance(value3.Center, y);
						value3.ParentConnection = connectedNode.Connection;
						value3.Parent = currentNode.Key;
						list.Add(in value3);
						unsafeHashMap.Add(connectedNode.ConnectedNodeKey, value3);
					}
				}
			}
		}

		private static bool IsEdgeExcluded(UnsafeHashSet<ExcludedEdge> excludedThresholdEdges, PathfindingNode currentNode, Edge edge)
		{
			if (excludedThresholdEdges.Count != 0)
			{
				return excludedThresholdEdges.Contains(new ExcludedEdge
				{
					OriginNodeKey = currentNode.Key,
					EndNodeKey = edge.ConnectedNodeKey
				});
			}
			return false;
		}

		private bool IsOutNodeOutOfRange(NativePathfindingRequest request, Edge edge)
		{
			switch (request.PathfindingRange)
			{
			case PathfindingRange.Island:
			{
				ConnectionKind connection = edge.Connection;
				return connection != ConnectionKind.Walk && connection != ConnectionKind.Threshold;
			}
			case PathfindingRange.Area:
				return GraphNodeMap[request.StartNodeKey].AreaKey != GraphNodeMap[edge.ConnectedNodeKey].AreaKey;
			case PathfindingRange.Global:
				return false;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private NativeParallelMultiHashMap<int, Edge>.Enumerator GetConnectedNodes(int nodeKey)
		{
			return EdgeMap.GetValuesForKey(nodeKey);
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
	private struct CollapsePathsJob : IJobParallelFor
	{
		[ReadOnly]
		public int MaxPathLength;

		[ReadOnly]
		public NativeArray<int> NavigationPaths;

		[ReadOnly]
		public NativeArray<ConnectionKind> ConnectionKindData;

		[ReadOnly]
		public NativeArray<int> NavigationPathLengths;

		[NativeDisableParallelForRestriction]
		public NativeArray<NavPath.Segment> CollapsedPaths;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> CollapsedPathLengths;

		public void Execute(int index)
		{
			if (NavigationPathLengths[index] == 0)
			{
				CollapsedPathLengths[index] = 0;
				return;
			}
			int num = NavigationPathLengths[index];
			NativeArray<int> subArray = NavigationPaths.GetSubArray(MaxPathLength * index, num);
			NativeArray<ConnectionKind> subArray2 = ConnectionKindData.GetSubArray(MaxPathLength * index, num - 1);
			NativeArray<NavPath.Segment> subArray3 = CollapsedPaths.GetSubArray(MaxPathLength * index, MaxPathLength);
			int num2 = 0;
			int index2 = 0;
			int i = 1;
			while (i < subArray.Length)
			{
				if (subArray2[index2] != ConnectionKind.Walk)
				{
					subArray3[num2] = new NavPath.Segment
					{
						StartSection = subArray[index2],
						EndSection = subArray[i],
						ConnectionKind = subArray2[index2]
					};
					num2++;
					index2 = i;
					i++;
					continue;
				}
				for (; i < subArray.Length - 1 && subArray2[i] == ConnectionKind.Walk; i++)
				{
				}
				subArray3[num2] = new NavPath.Segment
				{
					StartSection = subArray[index2],
					EndSection = subArray[i],
					ConnectionKind = ConnectionKind.Walk
				};
				num2++;
				index2 = i;
				i++;
			}
			CollapsedPathLengths[index] = num2;
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
	private struct PadJumpSegmentsJob : IJobParallelFor
	{
		[ReadOnly]
		public int MaxPathLength;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> NavigationPathLengths;

		[NativeDisableParallelForRestriction]
		public NativeArray<NavPath.Segment> NavigationPaths;

		public void Execute(int index)
		{
			if (NavigationPathLengths[index] == 0)
			{
				return;
			}
			NativeArray<NavPath.Segment> subArray = NavigationPaths.GetSubArray(MaxPathLength * index, MaxPathLength);
			int num = NavigationPathLengths[index];
			for (int i = 0; i < num - 1; i++)
			{
				if (ShouldInsertWalkSegment(subArray[i].ConnectionKind, subArray[i + 1].ConnectionKind))
				{
					if (num + 1 >= MaxPathLength)
					{
						Debug.LogWarning("Padded length would exceed maximum length, aborting padding");
						NavigationPathLengths[index] = 0;
						return;
					}
					NavPath.Segment segment = new NavPath.Segment
					{
						StartSection = subArray[i].EndSection,
						EndSection = subArray[i].EndSection,
						ConnectionKind = ConnectionKind.Walk
					};
					Insert(subArray, i + 1, num, segment);
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
				Insert(subArray, 0, num, segment2);
				num++;
			}
			if (num > 1 && subArray[num - 1].ConnectionKind != ConnectionKind.Walk)
			{
				int endSection = subArray[num - 1].EndSection;
				NavPath.Segment value = new NavPath.Segment
				{
					StartSection = endSection,
					EndSection = endSection,
					ConnectionKind = ConnectionKind.Walk
				};
				subArray[num] = value;
				num++;
			}
			NavigationPathLengths[index] = num;
			for (int j = 0; j < num - 1; j++)
			{
				NavPath.Segment segment3 = subArray[j];
				NavPath.Segment segment4 = subArray[j + 1];
				if (segment3.EndSection != segment4.StartSection)
				{
					Debug.Log("MalformedPath");
				}
			}
		}

		private static void Insert(NativeArray<NavPath.Segment> segments, int index, int length, NavPath.Segment segment)
		{
			for (int num = length - 1; num >= index; num--)
			{
				segments[num + 1] = segments[num];
			}
			segments[index] = segment;
		}

		private static bool ShouldInsertWalkSegment(ConnectionKind segmentKind, ConnectionKind nextSectionKind)
		{
			if (segmentKind != ConnectionKind.Walk)
			{
				return nextSectionKind != ConnectionKind.Walk;
			}
			return false;
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
	private struct ConvertPathSectionKeysToPointsJob : IJobParallelFor
	{
		private readonly struct JumpPointPair : IComparable<JumpPointPair>
		{
			public readonly float3 JumpPoint;

			public readonly float3 LandPoint;

			private readonly float totalDistance;

			public JumpPointPair(float3 jumpAnchor, float3 jumpPoint, float3 landPoint, float3 landAnchor)
			{
				JumpPoint = jumpPoint;
				LandPoint = landPoint;
				totalDistance = math.distance(jumpAnchor, jumpPoint) + math.distance(jumpPoint, landPoint) * NpcMovementValues.JumpCostScalar + math.distance(landPoint, landAnchor);
			}

			public int CompareTo(JumpPointPair other)
			{
				float num = totalDistance;
				return num.CompareTo(other.totalDistance);
			}
		}

		[ReadOnly]
		public int MaxPathLength;

		[ReadOnly]
		public NativeArray<NativePathfindingRequest> Requests;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		[NativeDisableParallelForRestriction]
		public NativeArray<NavPath.Segment> NavigationPaths;

		[NativeDisableParallelForRestriction]
		public NativeArray<int> NavigationPathLengths;

		public void Execute(int index)
		{
			if (NavigationPathLengths[index] == 0)
			{
				return;
			}
			NativeArray<NavPath.Segment> subArray = NavigationPaths.GetSubArray(MaxPathLength * index, NavigationPathLengths[index]);
			NativePathfindingRequest nativePathfindingRequest = Requests[index];
			NavPath.Segment value = subArray[0];
			NavPath.Segment value2 = subArray[subArray.Length - 1];
			value.StartPosition = nativePathfindingRequest.StartPosition;
			value2.EndPosition = nativePathfindingRequest.EndPosition;
			subArray[0] = value;
			subArray[subArray.Length - 1] = value2;
			if (subArray.Length % 2 == 0)
			{
				Debug.Log("Malformed path. Unable to convert from segments to points");
				NavigationPathLengths[index] = 0;
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
						NavPath.Segment value6 = subArray[i];
						NavPath.Segment value7 = subArray[i + 1];
						NavPath.Segment value8 = subArray[i + 2];
						value6.EndPosition = SurfaceMap[value6.EndSection].Center;
						value7.StartPosition = SurfaceMap[value6.EndSection].Center;
						value7.EndPosition = SurfaceMap[value7.EndSection].Center;
						value8.StartPosition = SurfaceMap[value7.EndSection].Center;
						subArray[i] = value6;
						subArray[i + 1] = value7;
						subArray[i + 2] = value8;
						break;
					}
					case ConnectionKind.Jump:
					case ConnectionKind.Dropdown:
					{
						NavPath.Segment value3 = subArray[i];
						NavPath.Segment value4 = subArray[i + 1];
						NavPath.Segment value5 = subArray[i + 2];
						SectionSurface jumpSurface = SurfaceMap[value3.EndSection];
						SectionSurface landSurface = SurfaceMap[value4.EndSection];
						float3 startPosition = value3.StartPosition;
						float3 center = SurfaceMap[value5.EndSection].Center;
						float3 @float = float3.zero;
						float3 float2 = float3.zero;
						foreach (JumpPointPair bestJumpPointsPair in GetBestJumpPointsPairs(startPosition, jumpSurface, landSurface, center))
						{
							JumpPointPair current = bestJumpPointsPair;
							if (BurstPathfindingUtilities.CanReachJumpPosition(in current.JumpPoint, in current.LandPoint, NpcMovementValues.MaxVerticalVelocity * 1.2f, NpcMovementValues.MaxHorizontalVelocity * 1.2f, NpcMovementValues.Gravity))
							{
								float launchAngleDegrees = BurstPathfindingUtilities.EstimateLaunchAngle(in current.JumpPoint, in current.LandPoint);
								if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in current.JumpPoint, in current.LandPoint, launchAngleDegrees, NpcMovementValues.Gravity, out var initialVelocity, out var timeOfFlight) && !float.IsNaN(timeOfFlight) && !(timeOfFlight <= 0f) && !BurstPathfindingUtilities.IsPathBlocked(current.JumpPoint, current.LandPoint, timeOfFlight, initialVelocity, Octants))
								{
									@float = current.JumpPoint;
									float2 = current.LandPoint;
									break;
								}
							}
						}
						value3.EndPosition = @float;
						value4.StartPosition = @float;
						value4.EndPosition = float2;
						value5.StartPosition = float2;
						subArray[i] = value3;
						subArray[i + 1] = value4;
						subArray[i + 2] = value5;
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
					NavPath.Segment value9 = subArray[i];
					value9.EndPosition = nativePathfindingRequest.EndPosition;
					subArray[i] = value9;
				}
			}
		}

		private static NativeList<JumpPointPair> GetBestJumpPointsPairs(float3 startAnchor, SectionSurface jumpSurface, SectionSurface landSurface, float3 landAnchor)
		{
			NativeList<JumpPointPair> nativeList = new NativeList<JumpPointPair>(128, AllocatorManager.Temp);
			foreach (float3 surfaceSamplePoint in jumpSurface.GetSurfaceSamplePoints())
			{
				foreach (float3 surfaceSamplePoint2 in landSurface.GetSurfaceSamplePoints())
				{
					nativeList.Add(new JumpPointPair(startAnchor, surfaceSamplePoint, surfaceSamplePoint2, landAnchor));
				}
			}
			nativeList.Sort();
			return nativeList;
		}
	}

	public struct Results
	{
		public Pathfinding.Request[] Requests;

		public NavPath.Segment[] CompletedPaths;

		public int[] CompletedPathLengths;
	}

	public static IEnumerator GeneratePathfindingResults(List<Pathfinding.Request> pathfindingRequests, NativeParallelHashMap<int, GraphNode> nodeMap, NativeParallelMultiHashMap<int, Edge> edgeMap, NativeParallelHashMap<int, SectionSurface> surfaceMap, NativeArray<Octant> octants, int numSections, Action<Results> getResults)
	{
		Pathfinding.Request[] requests = pathfindingRequests.ToArray();
		int num = requests.Length;
		NativeArray<NativePathfindingRequest> nativeArray = new NativeArray<NativePathfindingRequest>(requests.Length, Allocator.TempJob);
		NativeArray<UnsafeList<PathfindingNode>> openSets = new NativeArray<UnsafeList<PathfindingNode>>(num, Allocator.TempJob);
		NativeArray<UnsafeHashMap<int, PathfindingNode>> pathfindingNodeMaps = new NativeArray<UnsafeHashMap<int, PathfindingNode>>(num, Allocator.TempJob);
		NativeArray<UnsafeHashSet<ExcludedEdge>> excludedThresholdEdges = new NativeArray<UnsafeHashSet<ExcludedEdge>>(num, Allocator.TempJob);
		NativeArray<int> navigationPaths = new NativeArray<int>(num * numSections * 2, Allocator.TempJob);
		NativeArray<ConnectionKind> nativeArray2 = new NativeArray<ConnectionKind>(num * numSections * 2, Allocator.TempJob);
		NativeArray<int> navigationPathLengths = new NativeArray<int>(num, Allocator.TempJob);
		NativeArray<NavPath.Segment> collapsedPaths = new NativeArray<NavPath.Segment>(numSections * num * 2, Allocator.TempJob);
		NativeArray<int> collapsedPathLengths = new NativeArray<int>(num, Allocator.TempJob);
		NativeArray<Octant> pathfindingOctants = new NativeArray<Octant>(octants, Allocator.Persistent);
		for (int i = 0; i < requests.Length; i++)
		{
			nativeArray[i] = pathfindingRequests[i].GetNativePathfindingRequest();
			openSets[i] = new UnsafeList<PathfindingNode>(numSections, Allocator.TempJob);
			pathfindingNodeMaps[i] = new UnsafeHashMap<int, PathfindingNode>(numSections, Allocator.TempJob);
			UnsafeHashSet<ExcludedEdge> value = new UnsafeHashSet<ExcludedEdge>(pathfindingRequests[i].ExcludedEdges.Count, Allocator.TempJob);
			foreach (ExcludedEdge excludedEdge in pathfindingRequests[i].ExcludedEdges)
			{
				value.Add(excludedEdge);
			}
			excludedThresholdEdges[i] = value;
		}
		PathfindingJob jobData = new PathfindingJob
		{
			PathfindingRequests = nativeArray,
			MaxPathLength = numSections * 2,
			EdgeMap = edgeMap,
			GraphNodeMap = nodeMap,
			FallOffHeight = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight,
			OpenSets = openSets,
			PathfindingNodeMaps = pathfindingNodeMaps,
			NavigationPaths = navigationPaths,
			ConnectionKinds = nativeArray2,
			NavigationPathLengths = navigationPathLengths,
			ExcludedThresholdEdges = excludedThresholdEdges
		};
		CollapsePathsJob jobData2 = new CollapsePathsJob
		{
			MaxPathLength = numSections * 2,
			NavigationPaths = navigationPaths,
			ConnectionKindData = nativeArray2,
			NavigationPathLengths = navigationPathLengths,
			CollapsedPaths = collapsedPaths,
			CollapsedPathLengths = collapsedPathLengths
		};
		PadJumpSegmentsJob jobData3 = new PadJumpSegmentsJob
		{
			MaxPathLength = numSections * 2,
			NavigationPaths = collapsedPaths,
			NavigationPathLengths = collapsedPathLengths
		};
		ConvertPathSectionKeysToPointsJob jobData4 = new ConvertPathSectionKeysToPointsJob
		{
			MaxPathLength = numSections * 2,
			Requests = nativeArray,
			SurfaceMap = surfaceMap,
			Octants = pathfindingOctants,
			NavigationPaths = collapsedPaths,
			NavigationPathLengths = collapsedPathLengths
		};
		int innerloopBatchCount = num / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
		JobHandle dependsOn = IJobParallelForExtensions.Schedule(jobData, num, innerloopBatchCount);
		dependsOn = IJobParallelForExtensions.Schedule(jobData2, num, innerloopBatchCount, dependsOn);
		navigationPaths.Dispose(dependsOn);
		nativeArray2.Dispose(dependsOn);
		navigationPathLengths.Dispose(dependsOn);
		dependsOn = IJobParallelForExtensions.Schedule(jobData3, num, innerloopBatchCount, dependsOn);
		dependsOn = IJobParallelForExtensions.Schedule(jobData4, num, innerloopBatchCount, dependsOn);
		nativeArray.Dispose(dependsOn);
		yield return JobUtilities.WaitForJobToComplete(dependsOn, enforceTempCompletion: true);
		getResults(new Results
		{
			Requests = requests,
			CompletedPaths = collapsedPaths.ToArray(),
			CompletedPathLengths = collapsedPathLengths.ToArray()
		});
		JobHandle inputDeps = default(JobHandle);
		for (int j = 0; j < requests.Length; j++)
		{
			inputDeps = openSets[j].Dispose(inputDeps);
			inputDeps = pathfindingNodeMaps[j].Dispose(inputDeps);
			inputDeps = excludedThresholdEdges[j].Dispose(inputDeps);
		}
		openSets.Dispose(inputDeps);
		pathfindingNodeMaps.Dispose(inputDeps);
		excludedThresholdEdges.Dispose(inputDeps);
		pathfindingOctants.Dispose(default(JobHandle));
		collapsedPaths.Dispose(default(JobHandle));
		collapsedPathLengths.Dispose(default(JobHandle));
	}
}
