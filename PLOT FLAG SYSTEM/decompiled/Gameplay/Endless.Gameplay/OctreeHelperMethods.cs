using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay;

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
public static class OctreeHelperMethods
{
	private const float squareTolerance = 0.010000001f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetWalkableChildrenIndices(NativeArray<Octant> octants, NativeList<int> walkableChildrenIndices, ref Octant octant)
	{
		walkableChildrenIndices.Clear();
		if (!octant.HasChildren)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			if (!octant.TryGetChildIndex(i, out var key))
			{
				continue;
			}
			ref Octant reference = ref octants.GetRef(key);
			for (int j = 0; j < 8; j++)
			{
				if (reference.TryGetChildIndex(j, out var key2) && octants.GetRef(key2).IsWalkable)
				{
					walkableChildrenIndices.Add(in key2);
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool AreChildrenNeighbors(int octantIndex, int otherOctantIndex, NativeArray<Octant> octants, NativeParallelMultiHashMap<int, int> neighborMap, NativeList<int> childIndices, NativeList<int> neighborChildIndices)
	{
		ref Octant octant = ref octants.GetRef(octantIndex);
		GetWalkableChildrenIndices(octants, neighborChildIndices, ref octants.GetRef(otherOctantIndex));
		GetWalkableChildrenIndices(octants, childIndices, ref octant);
		foreach (int item in childIndices)
		{
			foreach (int item2 in neighborMap.GetValuesForKey(item))
			{
				if (neighborChildIndices.Contains(item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref Octant GetSmallestOctantForPoint(float3 point, NativeArray<Octant> octants, float minSize)
	{
		ref Octant reference = ref octants.GetRef(0);
		if (math.distancesq(reference.ClosestPoint(point), point) > 0.010000001f)
		{
			reference = Octant.GetInvalidOctant();
			return ref reference;
		}
		while (reference.Size.x - minSize > 0.1f)
		{
			if (!reference.TryGetChildOctantIndexFromPoint(point, out var index))
			{
				return ref reference;
			}
			reference = ref octants.GetRef(index);
		}
		return ref reference;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CheckBlockingAtPoint(float3 point, NativeArray<Octant> octants)
	{
		ref Octant reference = ref octants.GetRef(0);
		if (Hint.Unlikely(math.distancesq(reference.ClosestPoint(point), point) > 0.010000001f))
		{
			return false;
		}
		int index;
		while (reference.TryGetChildOctantIndexFromPoint(point, out index))
		{
			reference = ref octants.GetRef(index);
			if (reference.IsBlocking)
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetSmallestOctantIndexForPoint(float3 point, NativeArray<Octant> octants, float minSize, out int index)
	{
		Octant octant = octants[0];
		if (math.distancesq(octant.ClosestPoint(point), point) > 0.010000001f)
		{
			index = -1;
			return false;
		}
		int num = 0;
		while (octant.Size.x - minSize > 0.1f)
		{
			if (!octant.TryGetChildOctantIndexFromPoint(point, out var index2))
			{
				index = num;
				return true;
			}
			num = index2;
			octant = octants[num];
		}
		index = num;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 GetOffsetPosition(int relativeIndex, float offsetDistance)
	{
		float z = (((relativeIndex & 1) == 0) ? offsetDistance : (0f - offsetDistance));
		float x = (((relativeIndex & 2) == 0) ? offsetDistance : (0f - offsetDistance));
		float y = (((relativeIndex & 4) == 0) ? offsetDistance : (0f - offsetDistance));
		return new float3(x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 GetNeighborOffset(int index, float offsetDistance)
	{
		return index switch
		{
			0 => new float3(offsetDistance, 0f, 0f), 
			1 => new float3(0f, 0f, offsetDistance), 
			2 => new float3(0f - offsetDistance, 0f, 0f), 
			3 => new float3(0f, 0f, 0f - offsetDistance), 
			_ => throw new IndexOutOfRangeException(), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 GetSubCellNeighborOffset(int index, float offsetDistance)
	{
		return index switch
		{
			0 => new float3(offsetDistance, offsetDistance, 0f), 
			1 => new float3(0f, offsetDistance, offsetDistance), 
			2 => new float3(0f - offsetDistance, offsetDistance, 0f), 
			3 => new float3(0f, offsetDistance, 0f - offsetDistance), 
			4 => new float3(offsetDistance, 0f, 0f), 
			5 => new float3(0f, 0f, offsetDistance), 
			6 => new float3(0f - offsetDistance, 0f, 0f), 
			7 => new float3(0f, 0f, 0f - offsetDistance), 
			8 => new float3(offsetDistance, 0f - offsetDistance, 0f), 
			9 => new float3(0f, 0f - offsetDistance, offsetDistance), 
			10 => new float3(0f - offsetDistance, 0f - offsetDistance, 0f), 
			11 => new float3(0f, 0f - offsetDistance, 0f - offsetDistance), 
			_ => throw new IndexOutOfRangeException(), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 GetBigToSmallNeighborOffset(int index)
	{
		return index switch
		{
			0 => new float3(0.625f, -0.375f, -0.375f), 
			1 => new float3(0.625f, -0.375f, -0.125f), 
			2 => new float3(0.625f, -0.375f, 0.125f), 
			3 => new float3(0.625f, -0.375f, 0.375f), 
			4 => new float3(0.375f, -0.375f, 0.625f), 
			5 => new float3(0.125f, -0.375f, 0.625f), 
			6 => new float3(-0.125f, -0.375f, 0.625f), 
			7 => new float3(-0.375f, -0.375f, 0.625f), 
			8 => new float3(-0.625f, -0.375f, 0.375f), 
			9 => new float3(-0.625f, -0.375f, 0.125f), 
			10 => new float3(-0.625f, -0.375f, -0.125f), 
			11 => new float3(-0.625f, -0.375f, -0.375f), 
			12 => new float3(-0.375f, -0.375f, -0.625f), 
			13 => new float3(-0.125f, -0.375f, -0.625f), 
			14 => new float3(0.125f, -0.375f, -0.625f), 
			15 => new float3(0.375f, -0.375f, -0.625f), 
			_ => throw new IndexOutOfRangeException(), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 GetTerrainToSmallBelowOffset(int index)
	{
		return index switch
		{
			0 => new float3(0.625f, -0.625f, -0.375f), 
			1 => new float3(0.625f, -0.625f, -0.125f), 
			2 => new float3(0.625f, -0.625f, 0.125f), 
			3 => new float3(0.625f, -0.625f, 0.375f), 
			4 => new float3(0.375f, -0.625f, 0.625f), 
			5 => new float3(0.125f, -0.625f, 0.625f), 
			6 => new float3(-0.125f, -0.625f, 0.625f), 
			7 => new float3(-0.375f, -0.625f, 0.625f), 
			8 => new float3(-0.625f, -0.625f, 0.375f), 
			9 => new float3(-0.625f, -0.625f, 0.125f), 
			10 => new float3(-0.625f, -0.625f, -0.125f), 
			11 => new float3(-0.625f, -0.625f, -0.375f), 
			12 => new float3(-0.375f, -0.625f, -0.625f), 
			13 => new float3(-0.125f, -0.625f, -0.625f), 
			14 => new float3(0.125f, -0.625f, -0.625f), 
			15 => new float3(0.375f, -0.625f, -0.625f), 
			_ => throw new IndexOutOfRangeException(), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SweepNeighboringEdge(Octant octant1, Octant octant2, NavMeshQuery query)
	{
		float num = math.min(octant1.Size.x, octant2.Size.x);
		if (octant1.Size.x < octant2.Size.x)
		{
			Octant octant3 = octant2;
			Octant octant4 = octant1;
			octant1 = octant3;
			octant2 = octant4;
		}
		float3 @float = octant1.ClosestPoint(octant2.Center);
		float3 x = @float - octant2.Center;
		if (x.Equals(float3.zero))
		{
			return false;
		}
		float3 float2 = math.normalize(x);
		float3 float3 = math.cross(new float3(0f, 1f, 0f), float2);
		for (int i = 0; i < 10; i++)
		{
			float num2 = (float)(i / 2) / 10f;
			num2 *= ((i % 2 == 0) ? 1f : (-1f));
			float3 float4 = @float + float3 * num2 * num;
			NavMeshLocation location = query.MapLocation(float4 + float2 * 0.03f, new Vector3(0.01f, octant2.Size.x, 0.01f), 0);
			NavMeshLocation location2 = query.MapLocation(float4 - float2 * 0.03f, new Vector3(0.01f, octant2.Size.x, 0.01f), 0);
			if (query.IsValid(location) && query.IsValid(location2))
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetSamplePositions(NativeList<Vector3> samplePositions, float3 min, float3 max, int divisions)
	{
		samplePositions.Clear();
		float num = (max.x - min.x) / (float)(divisions - 1);
		float y = (min.y + max.y) / 2f;
		for (int i = 0; i < divisions; i++)
		{
			float x = min.x + num * (float)i;
			for (int j = 0; j < divisions; j++)
			{
				float z = min.z + num * (float)j;
				samplePositions.Add(new Vector3(x, y, z));
			}
		}
	}
}
