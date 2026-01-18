using System;
using System.Collections;
using Endless.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Endless.Gameplay
{
	// Token: 0x020001B4 RID: 436
	public static class DropGenerator
	{
		// Token: 0x060009BC RID: 2492 RVA: 0x0002CC8A File Offset: 0x0002AE8A
		public static IEnumerator GenerateDropConnections(NativeList<int> sectionKeys, NativeParallelHashMap<int, SectionSurface> surfaceMap, NativeArray<Octant> octants, NativeParallelHashSet<BidirectionalConnection> jumpConnections, Action<DropGenerator.Results> getResults)
		{
			NativeQueue<Connection> dropConnectionQueue = new NativeQueue<Connection>(AllocatorManager.Persistent);
			DropGenerator.FindDropdownConnectionsJob findDropdownConnectionsJob = new DropGenerator.FindDropdownConnectionsJob
			{
				SectionKeyArray = sectionKeys.AsArray(),
				Octants = octants,
				NumSectionKeys = sectionKeys.Length,
				SurfaceMap = surfaceMap,
				VerifiedJumpConnections = jumpConnections,
				DropdownConnections = dropConnectionQueue.AsParallelWriter()
			};
			int num = sectionKeys.Length / MonoBehaviourSingleton<NavGraph>.Instance.MaxNumBatches;
			JobHandle jobHandle = findDropdownConnectionsJob.Schedule(sectionKeys.Length, num, default(JobHandle));
			yield return JobUtilities.WaitForJobToComplete(jobHandle, false);
			NativeParallelHashSet<Connection> dropConnections = new NativeParallelHashSet<Connection>(1024, AllocatorManager.Persistent);
			JobHandle jobHandle2 = new DropGenerator.ConvertQueueToHashset
			{
				Connections = dropConnectionQueue,
				DropConnections = dropConnections
			}.Schedule(default(JobHandle));
			jobHandle2 = dropConnectionQueue.Dispose(jobHandle2);
			yield return JobUtilities.WaitForJobToComplete(jobHandle2, false);
			getResults(new DropGenerator.Results
			{
				DropConnections = dropConnections
			});
			yield break;
		}

		// Token: 0x020001B5 RID: 437
		[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
		private struct FindDropdownConnectionsJob : IJobParallelFor
		{
			// Token: 0x060009BD RID: 2493 RVA: 0x0002CCB8 File Offset: 0x0002AEB8
			public void Execute(int index)
			{
				int num = this.SectionKeyArray[index];
				SectionSurface sectionSurface = this.SurfaceMap[num];
				for (int i = 0; i < this.NumSectionKeys; i++)
				{
					int num2 = this.SectionKeyArray[i];
					if (num != num2)
					{
						SectionSurface sectionSurface2 = this.SurfaceMap[num2];
						if (sectionSurface2.Center.y < sectionSurface.Center.y - 1f)
						{
							float3 @float = sectionSurface.Center - sectionSurface2.Center;
							@float.y = 0f;
							if (math.length(@float) <= 24f)
							{
								SectionEdge closestEdgeToPoint = sectionSurface.GetClosestEdgeToPoint(sectionSurface2.Center);
								SectionEdge closestEdgeToPoint2 = sectionSurface2.GetClosestEdgeToPoint(sectionSurface.Center);
								float3 float2 = closestEdgeToPoint.GetClosestPointOnEdge(sectionSurface2.Center, 0f);
								float3 float3 = closestEdgeToPoint2.GetClosestPointOnEdge(float2, 0f);
								float2 = closestEdgeToPoint.GetClosestPointOnEdge(float3, 0f);
								float3 = closestEdgeToPoint2.GetClosestPointOnEdge(float2, 0f);
								if (BurstPathfindingUtilities.CanReachJumpPosition(in float2, in float3, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity) && !this.VerifiedJumpConnections.Contains(new BidirectionalConnection(num, num2)))
								{
									foreach (BurstPathfindingUtilities.SurfacePair surfacePair in BurstPathfindingUtilities.GetSortedDropPairs(sectionSurface, sectionSurface2))
									{
										if (!BurstPathfindingUtilities.CanReachJumpPosition(in surfacePair.JumpPoint, in surfacePair.LandPoint, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
										{
											break;
										}
										float num3 = BurstPathfindingUtilities.EstimateLaunchAngle(in surfacePair.JumpPoint, in surfacePair.LandPoint);
										float3 float4;
										float num4;
										if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in surfacePair.JumpPoint, in surfacePair.LandPoint, num3, NpcMovementValues.Gravity, out float4, out num4) && !float.IsNaN(num4) && num4 > 0f && !BurstPathfindingUtilities.IsPathBlocked(surfacePair.JumpPoint, surfacePair.LandPoint, num4, float4, this.Octants))
										{
											this.DropdownConnections.Enqueue(new Connection(num, num2));
											break;
										}
									}
								}
							}
						}
					}
				}
			}

			// Token: 0x040007DD RID: 2013
			[ReadOnly]
			public NativeArray<int> SectionKeyArray;

			// Token: 0x040007DE RID: 2014
			[NativeDisableParallelForRestriction]
			public NativeArray<Octant> Octants;

			// Token: 0x040007DF RID: 2015
			public int NumSectionKeys;

			// Token: 0x040007E0 RID: 2016
			[ReadOnly]
			public NativeParallelHashMap<int, SectionSurface> SurfaceMap;

			// Token: 0x040007E1 RID: 2017
			[ReadOnly]
			public NativeParallelHashSet<BidirectionalConnection> VerifiedJumpConnections;

			// Token: 0x040007E2 RID: 2018
			[WriteOnly]
			public NativeQueue<Connection>.ParallelWriter DropdownConnections;
		}

		// Token: 0x020001B6 RID: 438
		[BurstCompile]
		private struct ConvertQueueToHashset : IJob
		{
			// Token: 0x060009BE RID: 2494 RVA: 0x0002CEF8 File Offset: 0x0002B0F8
			public void Execute()
			{
				while (this.Connections.Count > 0)
				{
					this.DropConnections.Add(this.Connections.Dequeue());
				}
			}

			// Token: 0x040007E3 RID: 2019
			public NativeQueue<Connection> Connections;

			// Token: 0x040007E4 RID: 2020
			[WriteOnly]
			public NativeParallelHashSet<Connection> DropConnections;
		}

		// Token: 0x020001B7 RID: 439
		public struct Results
		{
			// Token: 0x040007E5 RID: 2021
			public NativeParallelHashSet<Connection> DropConnections;
		}
	}
}
