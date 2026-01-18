using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001F0 RID: 496
	[BurstCompile(FloatPrecision = FloatPrecision.Low, FloatMode = FloatMode.Fast)]
	public struct FindEdgeOctantsJob : IJobParallelFor
	{
		// Token: 0x06000A36 RID: 2614 RVA: 0x00037300 File Offset: 0x00035500
		public void Execute(int index)
		{
			int num = this.SectionKeys[index];
			NativeParallelMultiHashMap<int, int>.Enumerator enumerator = this.SectionMap.GetValuesForKey(num);
			foreach (int num2 in enumerator)
			{
				Octant octant = this.Octants[num2];
				int num3 = this.NeighborMap.CountValuesForKey(num2);
				float x = octant.Size.x;
				int num4 = num3;
				WalkableOctantData walkableOctantData;
				if (num4 < 4)
				{
					if (num4 == 0)
					{
						walkableOctantData = this.WalkableOctantDataMap[num2];
						walkableOctantData.Edge = NpcEnum.Edge.All;
						this.WalkableOctantDataMap[num2] = walkableOctantData;
						continue;
					}
				}
				else if (x < 1f)
				{
					walkableOctantData = this.WalkableOctantDataMap[num2];
					walkableOctantData.Edge = NpcEnum.Edge.None;
					this.WalkableOctantDataMap[num2] = walkableOctantData;
					continue;
				}
				NpcEnum.Edge edge = NpcEnum.Edge.All;
				int num5 = 0;
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				foreach (int num9 in this.NeighborMap.GetValuesForKey(num2))
				{
					Octant octant2 = this.Octants[num9];
					float x2 = octant2.Size.x;
					NpcEnum.Edge cardinalDirection = FindEdgeOctantsJob.GetCardinalDirection(octant.Center, octant2.Center);
					if (Math.Abs(x - x2) >= 0.1f && x >= 1f)
					{
						switch (cardinalDirection)
						{
						case NpcEnum.Edge.None:
							continue;
						case NpcEnum.Edge.North:
							num5++;
							continue;
						case NpcEnum.Edge.East:
							num6++;
							continue;
						case NpcEnum.Edge.North | NpcEnum.Edge.East:
						case NpcEnum.Edge.North | NpcEnum.Edge.South:
						case NpcEnum.Edge.East | NpcEnum.Edge.South:
						case NpcEnum.Edge.North | NpcEnum.Edge.East | NpcEnum.Edge.South:
							break;
						case NpcEnum.Edge.South:
							num7++;
							continue;
						case NpcEnum.Edge.West:
							num8++;
							continue;
						default:
							if (cardinalDirection == NpcEnum.Edge.All)
							{
								continue;
							}
							break;
						}
						throw new ArgumentOutOfRangeException();
					}
					edge ^= cardinalDirection;
				}
				if (num5 >= 4)
				{
					edge ^= NpcEnum.Edge.North;
				}
				if (num6 >= 4)
				{
					edge ^= NpcEnum.Edge.East;
				}
				if (num7 >= 4)
				{
					edge ^= NpcEnum.Edge.South;
				}
				if (num8 >= 4)
				{
					edge ^= NpcEnum.Edge.West;
				}
				walkableOctantData = this.WalkableOctantDataMap[num2];
				walkableOctantData.Edge = edge;
				this.WalkableOctantDataMap[num2] = walkableOctantData;
			}
		}

		// Token: 0x06000A37 RID: 2615 RVA: 0x00037574 File Offset: 0x00035774
		private static NpcEnum.Edge GetCardinalDirection(Vector3 origin, Vector3 terminus)
		{
			Vector3 vector = terminus - origin;
			if (math.abs(vector.x) < 0.0001f && math.abs(vector.z) < 0.0001f)
			{
				return NpcEnum.Edge.None;
			}
			if (math.abs(vector.x) >= math.abs(vector.z))
			{
				if (vector.x <= 0f)
				{
					return NpcEnum.Edge.West;
				}
				return NpcEnum.Edge.East;
			}
			else
			{
				if (vector.z <= 0f)
				{
					return NpcEnum.Edge.South;
				}
				return NpcEnum.Edge.North;
			}
		}

		// Token: 0x04000976 RID: 2422
		[ReadOnly]
		public NativeArray<int> SectionKeys;

		// Token: 0x04000977 RID: 2423
		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		// Token: 0x04000978 RID: 2424
		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> NeighborMap;

		// Token: 0x04000979 RID: 2425
		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		// Token: 0x0400097A RID: 2426
		[NativeDisableParallelForRestriction]
		public NativeParallelHashMap<int, WalkableOctantData> WalkableOctantDataMap;
	}
}
