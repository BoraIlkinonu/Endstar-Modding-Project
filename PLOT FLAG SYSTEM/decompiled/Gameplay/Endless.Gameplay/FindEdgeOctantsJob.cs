using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
public struct FindEdgeOctantsJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<int> SectionKeys;

	[ReadOnly]
	public NativeParallelMultiHashMap<int, int> SectionMap;

	[ReadOnly]
	public NativeParallelMultiHashMap<int, int> NeighborMap;

	[NativeDisableParallelForRestriction]
	public NativeArray<Octant> Octants;

	[NativeDisableParallelForRestriction]
	public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;

	public void Execute(int index)
	{
		int key = SectionKeys[index];
		foreach (int item in SectionMap.GetValuesForKey(key))
		{
			Octant octant = Octants[item];
			int num = NeighborMap.CountValuesForKey(item);
			float x = octant.Size.x;
			int num2 = num;
			WalkableOctantData value;
			if (num2 < 4)
			{
				if (num2 == 0)
				{
					value = WalkableOctantDataMap[item];
					value.Edge = NpcEnum.Edge.All;
					WalkableOctantDataMap[item] = value;
					continue;
				}
			}
			else if (x < 1f)
			{
				value = WalkableOctantDataMap[item];
				value.Edge = NpcEnum.Edge.None;
				WalkableOctantDataMap[item] = value;
				continue;
			}
			NpcEnum.Edge edge = NpcEnum.Edge.All;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			foreach (int item2 in NeighborMap.GetValuesForKey(item))
			{
				Octant octant2 = Octants[item2];
				float x2 = octant2.Size.x;
				NpcEnum.Edge cardinalDirection = GetCardinalDirection(octant.Center, octant2.Center);
				if (Math.Abs(x - x2) < 0.1f || x < 1f)
				{
					edge ^= cardinalDirection;
					continue;
				}
				switch (cardinalDirection)
				{
				case NpcEnum.Edge.North:
					num3++;
					break;
				case NpcEnum.Edge.East:
					num4++;
					break;
				case NpcEnum.Edge.South:
					num5++;
					break;
				case NpcEnum.Edge.West:
					num6++;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case NpcEnum.Edge.None:
				case NpcEnum.Edge.All:
					break;
				}
			}
			if (num3 >= 4)
			{
				edge ^= NpcEnum.Edge.North;
			}
			if (num4 >= 4)
			{
				edge ^= NpcEnum.Edge.East;
			}
			if (num5 >= 4)
			{
				edge ^= NpcEnum.Edge.South;
			}
			if (num6 >= 4)
			{
				edge ^= NpcEnum.Edge.West;
			}
			value = WalkableOctantDataMap[item];
			value.Edge = edge;
			WalkableOctantDataMap[item] = value;
		}
	}

	private static NpcEnum.Edge GetCardinalDirection(Vector3 origin, Vector3 terminus)
	{
		Vector3 vector = terminus - origin;
		if (math.abs(vector.x) < 0.0001f && math.abs(vector.z) < 0.0001f)
		{
			return NpcEnum.Edge.None;
		}
		if (math.abs(vector.x) >= math.abs(vector.z))
		{
			if (!(vector.x > 0f))
			{
				return NpcEnum.Edge.West;
			}
			return NpcEnum.Edge.East;
		}
		if (!(vector.z > 0f))
		{
			return NpcEnum.Edge.South;
		}
		return NpcEnum.Edge.North;
	}
}
