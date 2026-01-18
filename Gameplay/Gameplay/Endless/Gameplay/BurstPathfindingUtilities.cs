using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000222 RID: 546
	[BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
	public static class BurstPathfindingUtilities
	{
		// Token: 0x06000B52 RID: 2898 RVA: 0x0003DF3C File Offset: 0x0003C13C
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CanReachJumpPosition(in float3 position1, in float3 position2, float maxInitialVerticalVelocity, float maxHorizontalVelocity, float gravity)
		{
			if (maxInitialVerticalVelocity <= 0f || maxHorizontalVelocity <= 0f)
			{
				return false;
			}
			float num = BurstPathfindingUtilities.TimeToMaxHeight(maxInitialVerticalVelocity, gravity);
			if (position1.y + BurstPathfindingUtilities.VerticalDisplacementAtTime(maxInitialVerticalVelocity, num, gravity) < position2.y)
			{
				return false;
			}
			float num2 = position2.x - position1.x;
			float num3 = position2.z - position1.z;
			float num4 = math.sqrt(num2 * num2 + num3 * num3) / maxHorizontalVelocity;
			return num4 < num || position1.y + BurstPathfindingUtilities.VerticalDisplacementAtTime(maxInitialVerticalVelocity, num4, gravity) >= position2.y;
		}

		// Token: 0x06000B53 RID: 2899 RVA: 0x0003DFC8 File Offset: 0x0003C1C8
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float TimeToMaxHeight(float vY, float gravity)
		{
			return vY / gravity;
		}

		// Token: 0x06000B54 RID: 2900 RVA: 0x0003DFCD File Offset: 0x0003C1CD
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float VerticalDisplacementAtTime(float vY, float time, float gravity)
		{
			return vY * time - 0.5f * gravity * time * time;
		}

		// Token: 0x06000B55 RID: 2901 RVA: 0x0003DFE0 File Offset: 0x0003C1E0
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float EstimateLaunchAngle(in float3 from, in float3 to)
		{
			float3 @float = to - from;
			float num = math.length(new float2(@float.x, @float.z));
			bool flag = @float.y < 0.001f;
			float num2 = (flag ? 0f : 90f);
			float num3 = (flag ? 0.27027026f : 0.5f);
			float num4 = math.saturate(num * num3);
			return math.lerp(num2, 45f, num4);
		}

		// Token: 0x06000B56 RID: 2902 RVA: 0x0003E058 File Offset: 0x0003C258
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CalculateJumpVelocityWithAngle(in float3 from, in float3 to, float launchAngleDegrees, float gravity, out float3 initialVelocity, out float timeOfFlight)
		{
			initialVelocity = float3.zero;
			timeOfFlight = 0f;
			float3 @float = to - from;
			float num = math.length(new float2(@float.x, @float.z));
			float y = @float.y;
			if (num == 0f && y == 0f)
			{
				return false;
			}
			float num2;
			float num3;
			math.sincos(launchAngleDegrees * 0.017453292f, out num2, out num3);
			float num4 = 1f / num3;
			float num5 = num2 * num4;
			float num6 = num3 * num3;
			float num7 = 2f * num6 * (num * num5 - y);
			if (num7 <= 0f)
			{
				return false;
			}
			float num8 = gravity * num * num / num7;
			if (num8 < 0f)
			{
				return false;
			}
			float num9 = math.sqrt(num8);
			float num10 = 1f / num;
			float num11 = num9 * num3 * num10;
			initialVelocity = new float3(@float.x * num11, num9 * num2, @float.z * num11);
			timeOfFlight = num / (num9 * num3);
			return true;
		}

		// Token: 0x06000B57 RID: 2903 RVA: 0x0003E15C File Offset: 0x0003C35C
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetPointOnCurve(float3 startPosition, float3 initialVelocity, float time, float gravity)
		{
			float num = startPosition.x + initialVelocity.x * time;
			float num2 = startPosition.y + initialVelocity.y * time - 0.5f * gravity * time * time;
			float num3 = startPosition.z + initialVelocity.z * time;
			return new float3(num, num2, num3);
		}

		// Token: 0x06000B58 RID: 2904 RVA: 0x0003E1AC File Offset: 0x0003C3AC
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPathBlocked(float3 jumpPoint, float3 landPoint, float timeOfFlight, float3 initialVelocity, NativeArray<Octant> octants)
		{
			float3 @float = math.cross(math.normalize(new float3(landPoint.x - jumpPoint.x, 0f, landPoint.z - jumpPoint.z)), math.up());
			float num = NpcMovementValues.NpcHeight - 0.1f;
			float3 float2 = @float * NpcMovementValues.NpcRadius;
			float3 float3 = math.up() * (num - NpcMovementValues.NpcRadius);
			float3 float4 = float3 - float2;
			float3 float5 = float3 + float2;
			for (float num2 = 0.04f; num2 < timeOfFlight; num2 += 0.04f)
			{
				float3 float6 = BurstPathfindingUtilities.GetPointOnCurve(jumpPoint, initialVelocity, num2, NpcMovementValues.Gravity) + BurstPathfindingUtilities.nudgeUp;
				if (OctreeHelperMethods.CheckBlockingAtPoint(float6, octants))
				{
					return true;
				}
				if (OctreeHelperMethods.CheckBlockingAtPoint(float6 + float4, octants))
				{
					return true;
				}
				if (OctreeHelperMethods.CheckBlockingAtPoint(float6 + float5, octants))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000B59 RID: 2905 RVA: 0x0003E28C File Offset: 0x0003C48C
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NativeList<BurstPathfindingUtilities.SurfacePair> GetSortedJumpPairs(SectionSurface jumpSurface, SectionSurface landSurface)
		{
			NativeList<float3> surfaceSamplePoints = jumpSurface.GetSurfaceSamplePoints();
			NativeList<float3> surfaceSamplePoints2 = landSurface.GetSurfaceSamplePoints();
			NativeList<BurstPathfindingUtilities.SurfacePair> nativeList = new NativeList<BurstPathfindingUtilities.SurfacePair>(surfaceSamplePoints.Length * surfaceSamplePoints2.Length, AllocatorManager.Temp);
			foreach (float3 @float in jumpSurface.GetSurfaceSamplePoints())
			{
				foreach (float3 float2 in landSurface.GetSurfaceSamplePoints())
				{
					BurstPathfindingUtilities.SurfacePair surfacePair = new BurstPathfindingUtilities.SurfacePair(@float, float2);
					nativeList.Add(in surfacePair);
				}
			}
			nativeList.MergeSort<BurstPathfindingUtilities.SurfacePair>();
			return nativeList;
		}

		// Token: 0x06000B5A RID: 2906 RVA: 0x0003E368 File Offset: 0x0003C568
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NativeList<BurstPathfindingUtilities.SurfacePair> GetSortedDropPairs(SectionSurface jumpSurface, SectionSurface landSurface)
		{
			NativeList<float3> surfaceEdgeSamplePoints = jumpSurface.GetSurfaceEdgeSamplePoints();
			NativeList<float3> surfaceSamplePoints = landSurface.GetSurfaceSamplePoints();
			NativeList<BurstPathfindingUtilities.SurfacePair> nativeList = new NativeList<BurstPathfindingUtilities.SurfacePair>(surfaceEdgeSamplePoints.Length * surfaceSamplePoints.Length, AllocatorManager.Temp);
			foreach (float3 @float in surfaceEdgeSamplePoints)
			{
				foreach (float3 float2 in surfaceSamplePoints)
				{
					BurstPathfindingUtilities.SurfacePair surfacePair = new BurstPathfindingUtilities.SurfacePair(@float, float2);
					nativeList.Add(in surfacePair);
				}
			}
			nativeList.MergeSort<BurstPathfindingUtilities.SurfacePair>();
			return nativeList;
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x0003E430 File Offset: 0x0003C630
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FindAreaPath(int destination, int origin, NativeParallelMultiHashMap<int, int> areaGraph, NativeList<int> result, NativeQueue<int> queue, NativeArray<int> distance, NativeArray<int> path)
		{
			result.Clear();
			queue.Clear();
			for (int i = 0; i < distance.Length; i++)
			{
				distance[i] = -1;
				path[i] = -1;
			}
			distance[origin] = 0;
			path[origin] = -1;
			queue.Enqueue(origin);
			while (queue.Count > 0)
			{
				int num = queue.Dequeue();
				if (num == destination)
				{
					do
					{
						num = path[num];
						result.Add(in num);
					}
					while (path[num] != -1);
					return;
				}
				foreach (int num2 in areaGraph.GetValuesForKey(num))
				{
					if (distance[num2] == -1)
					{
						distance[num2] = distance[num] + 1;
						path[num2] = num;
						queue.Enqueue(num2);
					}
				}
			}
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x0003E53C File Offset: 0x0003C73C
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static void ConsolidateAreas(int rootArea, NativeHashSet<int> areasToConsolidate, NativeParallelMultiHashMap<int, int> areaGraph, NativeParallelMultiHashMap<int, int> areaMap, NativeParallelHashMap<int, int> reverseAreaMap, NativeParallelMultiHashMap<int, int> islandSectionMap, NativeParallelMultiHashMap<int, int> sectionMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap)
		{
			NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(areaGraph.CountValuesForKey(rootArea), AllocatorManager.Temp);
			NativeParallelMultiHashMap<int, int>.Enumerator enumerator = areaGraph.GetValuesForKey(rootArea);
			foreach (int num in enumerator)
			{
				if (!areasToConsolidate.Contains(num))
				{
					nativeHashSet.Add(num);
				}
			}
			areaGraph.Remove(rootArea);
			foreach (int num2 in areasToConsolidate)
			{
				enumerator = areaGraph.GetValuesForKey(num2);
				foreach (int num3 in enumerator)
				{
					if (num3 != rootArea && !areasToConsolidate.Contains(num3))
					{
						nativeHashSet.Add(num3);
					}
				}
			}
			foreach (int num4 in nativeHashSet)
			{
				areaGraph.Add(rootArea, num4);
			}
			foreach (int num5 in areasToConsolidate)
			{
				areaGraph.Remove(num5);
			}
			NativeHashSet<BurstPathfindingUtilities.GraphKeyPair> nativeHashSet2 = new NativeHashSet<BurstPathfindingUtilities.GraphKeyPair>(64, AllocatorManager.Temp);
			NativeHashSet<BurstPathfindingUtilities.GraphKeyPair> nativeHashSet3 = new NativeHashSet<BurstPathfindingUtilities.GraphKeyPair>(64, AllocatorManager.Temp);
			foreach (KeyValue<int, int> keyValue in areaGraph)
			{
				if (areasToConsolidate.Contains(*keyValue.Value))
				{
					nativeHashSet2.Add(new BurstPathfindingUtilities.GraphKeyPair(keyValue.Key, *keyValue.Value));
					nativeHashSet3.Add(new BurstPathfindingUtilities.GraphKeyPair(keyValue.Key, rootArea));
				}
			}
			foreach (BurstPathfindingUtilities.GraphKeyPair graphKeyPair in nativeHashSet2)
			{
				areaGraph.Remove(graphKeyPair.Key, graphKeyPair.Value);
			}
			foreach (BurstPathfindingUtilities.GraphKeyPair graphKeyPair2 in nativeHashSet3)
			{
				areaGraph.Add(graphKeyPair2.Key, graphKeyPair2.Value);
			}
			NativeList<int> nativeList = new NativeList<int>(64, AllocatorManager.Temp);
			foreach (int num6 in areasToConsolidate)
			{
				nativeList.Clear();
				enumerator = areaMap.GetValuesForKey(num6);
				foreach (int num7 in enumerator)
				{
					nativeList.Add(in num7);
				}
				areaMap.Remove(num6);
				foreach (int num8 in nativeList)
				{
					areaMap.Add(rootArea, num8);
					reverseAreaMap[num8] = rootArea;
					enumerator = islandSectionMap.GetValuesForKey(num8);
					foreach (int num9 in enumerator)
					{
						foreach (int num10 in sectionMap.GetValuesForKey(num9))
						{
							WalkableOctantData walkableOctantData = walkableOctantDataMap[num10];
							walkableOctantData.AreaKey = rootArea;
							walkableOctantDataMap[num10] = walkableOctantData;
						}
					}
				}
			}
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x0003EA54 File Offset: 0x0003CC54
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetSectionKey(int islandIndex, int sectionIndex)
		{
			int num = 100000 * islandIndex + sectionIndex;
			if (num < 0)
			{
				Debug.LogError("Forming bad section keys");
			}
			return num;
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x0003EA6D File Offset: 0x0003CC6D
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetIslandFromSectionKey(int sectionKey)
		{
			return sectionKey / 100000;
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x0003EA78 File Offset: 0x0003CC78
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetJumpDirectionalOffset(int index, float3 direction)
		{
			int num = index / 55;
			int num2 = index % 55;
			if (num < 0 || num >= 3)
			{
				throw new ArgumentOutOfRangeException("index", index, null);
			}
			int num3 = num2 / 5;
			int num4 = num2 % 5 + 1;
			int num5 = num3 - 5;
			float3 @float = math.up() * (float)num;
			float3 float2 = math.cross(direction, new float3(0f, 1f, 0f)) * (float)num5;
			return direction * (float)num4 + float2 + @float;
		}

		// Token: 0x06000B60 RID: 2912 RVA: 0x0003EAFC File Offset: 0x0003CCFC
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetJumpUpdateDirectionalOffset(int index, float3 direction)
		{
			int num = index / 55;
			int num2 = index % 55;
			if (num < 0 || num >= 5)
			{
				throw new ArgumentOutOfRangeException("index", index, null);
			}
			int num3 = num2 / 5;
			int num4 = num2 % 5 + 1;
			int num5 = num3 - 5;
			float3 @float = new float3(0f, -2f, 0f) + math.up() * (float)num;
			float3 float2 = math.cross(direction, new float3(0f, 1f, 0f)) * (float)num5;
			return direction * (float)num4 + float2 + @float;
		}

		// Token: 0x04000AAE RID: 2734
		private const float invFlatThreshold = 0.27027026f;

		// Token: 0x04000AAF RID: 2735
		private const float invSteepThreshold = 0.5f;

		// Token: 0x04000AB0 RID: 2736
		private const float verticalEpsilon = 0.001f;

		// Token: 0x04000AB1 RID: 2737
		private const float timeStep = 0.04f;

		// Token: 0x04000AB2 RID: 2738
		private static readonly float3 nudgeUp = math.up() * 0.1f;

		// Token: 0x02000223 RID: 547
		public struct SurfacePair : IComparable<BurstPathfindingUtilities.SurfacePair>
		{
			// Token: 0x06000B62 RID: 2914 RVA: 0x0003EBAF File Offset: 0x0003CDAF
			public SurfacePair(float3 jumpPoint, float3 landPoint)
			{
				this.JumpPoint = jumpPoint;
				this.LandPoint = landPoint;
				this.distance = math.distancesq(jumpPoint, landPoint);
			}

			// Token: 0x06000B63 RID: 2915 RVA: 0x0003EBCC File Offset: 0x0003CDCC
			public int CompareTo(BurstPathfindingUtilities.SurfacePair other)
			{
				return this.distance.CompareTo(other.distance);
			}

			// Token: 0x04000AB3 RID: 2739
			public float3 JumpPoint;

			// Token: 0x04000AB4 RID: 2740
			public float3 LandPoint;

			// Token: 0x04000AB5 RID: 2741
			private readonly float distance;
		}

		// Token: 0x02000224 RID: 548
		public readonly struct GraphKeyPair : IEquatable<BurstPathfindingUtilities.GraphKeyPair>
		{
			// Token: 0x06000B64 RID: 2916 RVA: 0x0003EBED File Offset: 0x0003CDED
			public GraphKeyPair(int key, int value)
			{
				this.Key = key;
				this.Value = value;
			}

			// Token: 0x06000B65 RID: 2917 RVA: 0x0003EBFD File Offset: 0x0003CDFD
			public bool Equals(BurstPathfindingUtilities.GraphKeyPair other)
			{
				return this.Key == other.Key && this.Value == other.Value;
			}

			// Token: 0x06000B66 RID: 2918 RVA: 0x0003EC20 File Offset: 0x0003CE20
			public override bool Equals(object obj)
			{
				if (obj is BurstPathfindingUtilities.GraphKeyPair)
				{
					BurstPathfindingUtilities.GraphKeyPair graphKeyPair = (BurstPathfindingUtilities.GraphKeyPair)obj;
					return this.Equals(graphKeyPair);
				}
				return false;
			}

			// Token: 0x06000B67 RID: 2919 RVA: 0x0003EC45 File Offset: 0x0003CE45
			public override int GetHashCode()
			{
				return HashCode.Combine<int, int>(this.Key, this.Value);
			}

			// Token: 0x04000AB6 RID: 2742
			public readonly int Key;

			// Token: 0x04000AB7 RID: 2743
			public readonly int Value;
		}
	}
}
