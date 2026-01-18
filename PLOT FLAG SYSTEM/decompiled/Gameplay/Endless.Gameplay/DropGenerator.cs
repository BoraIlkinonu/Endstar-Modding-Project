using System;
using System.Collections;
using Endless.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Endless.Gameplay;

public static class DropGenerator
{
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	private struct FindDropdownConnectionsJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeArray<int> SectionKeyArray;

		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		public int NumSectionKeys;

		[ReadOnly]
		public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

		[ReadOnly]
		public NativeParallelHashSet<BidirectionalConnection> VerifiedJumpConnections;

		[WriteOnly]
		public NativeQueue<Connection>.ParallelWriter DropdownConnections;

		public void Execute(int index)
		{
			int num = SectionKeyArray[index];
			SectionSurface jumpSurface = SurfaceMap[num];
			for (int i = 0; i < NumSectionKeys; i++)
			{
				int num2 = SectionKeyArray[i];
				if (num == num2)
				{
					continue;
				}
				SectionSurface landSurface = SurfaceMap[num2];
				if (landSurface.Center.y >= jumpSurface.Center.y - 1f)
				{
					continue;
				}
				float3 x = jumpSurface.Center - landSurface.Center;
				x.y = 0f;
				if (math.length(x) > 24f)
				{
					continue;
				}
				SectionEdge closestEdgeToPoint = jumpSurface.GetClosestEdgeToPoint(landSurface.Center);
				SectionEdge closestEdgeToPoint2 = landSurface.GetClosestEdgeToPoint(jumpSurface.Center);
				float3 closestPointOnEdge = closestEdgeToPoint.GetClosestPointOnEdge(landSurface.Center);
				float3 closestPointOnEdge2 = closestEdgeToPoint2.GetClosestPointOnEdge(closestPointOnEdge);
				closestPointOnEdge = closestEdgeToPoint.GetClosestPointOnEdge(closestPointOnEdge2);
				if (!BurstPathfindingUtilities.CanReachJumpPosition(in closestPointOnEdge, closestEdgeToPoint2.GetClosestPointOnEdge(closestPointOnEdge), NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity) || VerifiedJumpConnections.Contains(new BidirectionalConnection(num, num2)))
				{
					continue;
				}
				foreach (BurstPathfindingUtilities.SurfacePair sortedDropPair in BurstPathfindingUtilities.GetSortedDropPairs(jumpSurface, landSurface))
				{
					BurstPathfindingUtilities.SurfacePair current = sortedDropPair;
					if (!BurstPathfindingUtilities.CanReachJumpPosition(in current.JumpPoint, in current.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
					{
						break;
					}
					float launchAngleDegrees = BurstPathfindingUtilities.EstimateLaunchAngle(in current.JumpPoint, in current.LandPoint);
					if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in current.JumpPoint, in current.LandPoint, launchAngleDegrees, NpcMovementValues.Gravity, out var initialVelocity, out var timeOfFlight) && !float.IsNaN(timeOfFlight) && !(timeOfFlight <= 0f) && !BurstPathfindingUtilities.IsPathBlocked(current.JumpPoint, current.LandPoint, timeOfFlight, initialVelocity, Octants))
					{
						DropdownConnections.Enqueue(new Connection(num, num2));
						break;
					}
				}
			}
		}
	}

	[BurstCompile]
	private struct ConvertQueueToHashset : IJob
	{
		public NativeQueue<Connection> Connections;

		[WriteOnly]
		public NativeParallelHashSet<Connection> DropConnections;

		public void Execute()
		{
			while (Connections.Count > 0)
			{
				DropConnections.Add(Connections.Dequeue());
			}
		}
	}

	public struct Results
	{
		public NativeParallelHashSet<Connection> DropConnections;
	}

	public static IEnumerator GenerateDropConnections(NativeList<int> sectionKeys, NativeParallelHashMap<int, SectionSurface> surfaceMap, NativeArray<Octant> octants, NativeParallelHashSet<BidirectionalConnection> jumpConnections, Action<Results> getResults)
	{
		NativeQueue<Connection> dropConnectionQueue = new NativeQueue<Connection>(AllocatorManager.Persistent);
		JobHandle handle = new FindDropdownConnectionsJob
		{
			SectionKeyArray = sectionKeys.AsArray(),
			Octants = octants,
			NumSectionKeys = sectionKeys.Length,
			SurfaceMap = surfaceMap,
			VerifiedJumpConnections = jumpConnections,
			DropdownConnections = dropConnectionQueue.AsParallelWriter()
		}.Schedule(innerloopBatchCount: sectionKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches, arrayLength: sectionKeys.Length);
		yield return JobUtilities.WaitForJobToComplete(handle);
		NativeParallelHashSet<Connection> dropConnections = new NativeParallelHashSet<Connection>(1024, AllocatorManager.Persistent);
		JobHandle inputDeps = new ConvertQueueToHashset
		{
			Connections = dropConnectionQueue,
			DropConnections = dropConnections
		}.Schedule();
		inputDeps = dropConnectionQueue.Dispose(inputDeps);
		yield return JobUtilities.WaitForJobToComplete(inputDeps);
		getResults(new Results
		{
			DropConnections = dropConnections
		});
	}
}
