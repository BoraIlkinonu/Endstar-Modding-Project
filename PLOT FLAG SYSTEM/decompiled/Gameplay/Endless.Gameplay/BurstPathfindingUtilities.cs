using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

[BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
public static class BurstPathfindingUtilities
{
	public struct SurfacePair : IComparable<SurfacePair>
	{
		public float3 JumpPoint;

		public float3 LandPoint;

		private readonly float distance;

		public SurfacePair(float3 jumpPoint, float3 landPoint)
		{
			JumpPoint = jumpPoint;
			LandPoint = landPoint;
			distance = math.distancesq(jumpPoint, landPoint);
		}

		public int CompareTo(SurfacePair other)
		{
			float num = distance;
			return num.CompareTo(other.distance);
		}
	}

	public readonly struct GraphKeyPair : IEquatable<GraphKeyPair>
	{
		public readonly int Key;

		public readonly int Value;

		public GraphKeyPair(int key, int value)
		{
			Key = key;
			Value = value;
		}

		public bool Equals(GraphKeyPair other)
		{
			if (Key == other.Key)
			{
				return Value == other.Value;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is GraphKeyPair other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Key, Value);
		}
	}

	private const float invFlatThreshold = 0.27027026f;

	private const float invSteepThreshold = 0.5f;

	private const float verticalEpsilon = 0.001f;

	private const float timeStep = 0.04f;

	private static readonly float3 nudgeUp = math.up() * 0.1f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CanReachJumpPosition(in float3 position1, in float3 position2, float maxInitialVerticalVelocity, float maxHorizontalVelocity, float gravity)
	{
		if (maxInitialVerticalVelocity <= 0f || maxHorizontalVelocity <= 0f)
		{
			return false;
		}
		float num = TimeToMaxHeight(maxInitialVerticalVelocity, gravity);
		if (position1.y + VerticalDisplacementAtTime(maxInitialVerticalVelocity, num, gravity) < position2.y)
		{
			return false;
		}
		float num2 = position2.x - position1.x;
		float num3 = position2.z - position1.z;
		float num4 = math.sqrt(num2 * num2 + num3 * num3) / maxHorizontalVelocity;
		if (num4 < num)
		{
			return true;
		}
		return position1.y + VerticalDisplacementAtTime(maxInitialVerticalVelocity, num4, gravity) >= position2.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float TimeToMaxHeight(float vY, float gravity)
	{
		return vY / gravity;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float VerticalDisplacementAtTime(float vY, float time, float gravity)
	{
		return vY * time - 0.5f * gravity * time * time;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float EstimateLaunchAngle(in float3 from, in float3 to)
	{
		float3 @float = to - from;
		float num = math.length(new float2(@float.x, @float.z));
		bool num2 = @float.y < 0.001f;
		float start = (num2 ? 0f : 90f);
		float num3 = (num2 ? 0.27027026f : 0.5f);
		float t = math.saturate(num * num3);
		return math.lerp(start, 45f, t);
	}

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
		math.sincos(launchAngleDegrees * (MathF.PI / 180f), out var s, out var c);
		float num2 = 1f / c;
		float num3 = s * num2;
		float num4 = c * c;
		float num5 = 2f * num4 * (num * num3 - y);
		if (num5 <= 0f)
		{
			return false;
		}
		float num6 = gravity * num * num / num5;
		if (num6 < 0f)
		{
			return false;
		}
		float num7 = math.sqrt(num6);
		float num8 = 1f / num;
		float num9 = num7 * c * num8;
		initialVelocity = new float3(@float.x * num9, num7 * s, @float.z * num9);
		timeOfFlight = num / (num7 * c);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 GetPointOnCurve(float3 startPosition, float3 initialVelocity, float time, float gravity)
	{
		float x = startPosition.x + initialVelocity.x * time;
		float y = startPosition.y + initialVelocity.y * time - 0.5f * gravity * time * time;
		float z = startPosition.z + initialVelocity.z * time;
		return new float3(x, y, z);
	}

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
			float3 float6 = GetPointOnCurve(jumpPoint, initialVelocity, num2, NpcMovementValues.Gravity) + nudgeUp;
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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static NativeList<SurfacePair> GetSortedJumpPairs(SectionSurface jumpSurface, SectionSurface landSurface)
	{
		NativeList<float3> surfaceSamplePoints = jumpSurface.GetSurfaceSamplePoints();
		NativeList<float3> surfaceSamplePoints2 = landSurface.GetSurfaceSamplePoints();
		NativeList<SurfacePair> nativeList = new NativeList<SurfacePair>(surfaceSamplePoints.Length * surfaceSamplePoints2.Length, AllocatorManager.Temp);
		foreach (float3 surfaceSamplePoint in jumpSurface.GetSurfaceSamplePoints())
		{
			foreach (float3 surfaceSamplePoint2 in landSurface.GetSurfaceSamplePoints())
			{
				nativeList.Add(new SurfacePair(surfaceSamplePoint, surfaceSamplePoint2));
			}
		}
		nativeList.MergeSort();
		return nativeList;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static NativeList<SurfacePair> GetSortedDropPairs(SectionSurface jumpSurface, SectionSurface landSurface)
	{
		NativeList<float3> surfaceEdgeSamplePoints = jumpSurface.GetSurfaceEdgeSamplePoints();
		NativeList<float3> surfaceSamplePoints = landSurface.GetSurfaceSamplePoints();
		NativeList<SurfacePair> nativeList = new NativeList<SurfacePair>(surfaceEdgeSamplePoints.Length * surfaceSamplePoints.Length, AllocatorManager.Temp);
		foreach (float3 item in surfaceEdgeSamplePoints)
		{
			foreach (float3 item2 in surfaceSamplePoints)
			{
				nativeList.Add(new SurfacePair(item, item2));
			}
		}
		nativeList.MergeSort();
		return nativeList;
	}

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
			int value = queue.Dequeue();
			if (value == destination)
			{
				do
				{
					value = path[value];
					result.Add(in value);
				}
				while (path[value] != -1);
				break;
			}
			foreach (int item in areaGraph.GetValuesForKey(value))
			{
				if (distance[item] == -1)
				{
					distance[item] = distance[value] + 1;
					path[item] = value;
					queue.Enqueue(item);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void ConsolidateAreas(int rootArea, NativeHashSet<int> areasToConsolidate, NativeParallelMultiHashMap<int, int> areaGraph, NativeParallelMultiHashMap<int, int> areaMap, NativeParallelHashMap<int, int> reverseAreaMap, NativeParallelMultiHashMap<int, int> islandSectionMap, NativeParallelMultiHashMap<int, int> sectionMap, NativeParallelHashMap<int, WalkableOctantData> walkableOctantDataMap)
	{
		NativeHashSet<int> nativeHashSet = new NativeHashSet<int>(areaGraph.CountValuesForKey(rootArea), AllocatorManager.Temp);
		foreach (int item in areaGraph.GetValuesForKey(rootArea))
		{
			if (!areasToConsolidate.Contains(item))
			{
				nativeHashSet.Add(item);
			}
		}
		areaGraph.Remove(rootArea);
		foreach (int item2 in areasToConsolidate)
		{
			foreach (int item3 in areaGraph.GetValuesForKey(item2))
			{
				if (item3 != rootArea && !areasToConsolidate.Contains(item3))
				{
					nativeHashSet.Add(item3);
				}
			}
		}
		foreach (int item4 in nativeHashSet)
		{
			areaGraph.Add(rootArea, item4);
		}
		foreach (int item5 in areasToConsolidate)
		{
			areaGraph.Remove(item5);
		}
		NativeHashSet<GraphKeyPair> nativeHashSet2 = new NativeHashSet<GraphKeyPair>(64, AllocatorManager.Temp);
		NativeHashSet<GraphKeyPair> nativeHashSet3 = new NativeHashSet<GraphKeyPair>(64, AllocatorManager.Temp);
		foreach (KeyValue<int, int> item6 in areaGraph)
		{
			if (areasToConsolidate.Contains(item6.Value))
			{
				nativeHashSet2.Add(new GraphKeyPair(item6.Key, item6.Value));
				nativeHashSet3.Add(new GraphKeyPair(item6.Key, rootArea));
			}
		}
		foreach (GraphKeyPair item7 in nativeHashSet2)
		{
			areaGraph.Remove(item7.Key, item7.Value);
		}
		foreach (GraphKeyPair item8 in nativeHashSet3)
		{
			areaGraph.Add(item8.Key, item8.Value);
		}
		NativeList<int> nativeList = new NativeList<int>(64, AllocatorManager.Temp);
		foreach (int item9 in areasToConsolidate)
		{
			nativeList.Clear();
			foreach (int item10 in areaMap.GetValuesForKey(item9))
			{
				nativeList.Add(item10);
			}
			areaMap.Remove(item9);
			foreach (int item11 in nativeList)
			{
				areaMap.Add(rootArea, item11);
				reverseAreaMap[item11] = rootArea;
				foreach (int item12 in islandSectionMap.GetValuesForKey(item11))
				{
					foreach (int item13 in sectionMap.GetValuesForKey(item12))
					{
						WalkableOctantData value = walkableOctantDataMap[item13];
						value.AreaKey = rootArea;
						walkableOctantDataMap[item13] = value;
					}
				}
			}
		}
	}

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetIslandFromSectionKey(int sectionKey)
	{
		return sectionKey / 100000;
	}

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
		float3 @float = math.up() * num;
		float3 float2 = math.cross(direction, new float3(0f, 1f, 0f)) * num5;
		return direction * num4 + float2 + @float;
	}

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
		float3 @float = new float3(0f, -2f, 0f) + math.up() * num;
		float3 float2 = math.cross(direction, new float3(0f, 1f, 0f)) * num5;
		return direction * num4 + float2 + @float;
	}
}
