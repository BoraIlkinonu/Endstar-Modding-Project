using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay
{
	// Token: 0x0200022C RID: 556
	[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
	public static class OctreeHelperMethods
	{
		// Token: 0x06000B7D RID: 2941 RVA: 0x0003F074 File Offset: 0x0003D274
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
				int num;
				if (octant.TryGetChildIndex(i, out num))
				{
					ref Octant @ref = ref octants.GetRef(num);
					for (int j = 0; j < 8; j++)
					{
						int num2;
						if (@ref.TryGetChildIndex(j, out num2) && octants.GetRef(num2).IsWalkable)
						{
							walkableChildrenIndices.Add(in num2);
						}
					}
				}
			}
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x0003F0E0 File Offset: 0x0003D2E0
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AreChildrenNeighbors(int octantIndex, int otherOctantIndex, NativeArray<Octant> octants, NativeParallelMultiHashMap<int, int> neighborMap, NativeList<int> childIndices, NativeList<int> neighborChildIndices)
		{
			ref Octant @ref = ref octants.GetRef(octantIndex);
			ref Octant ref2 = ref octants.GetRef(otherOctantIndex);
			OctreeHelperMethods.GetWalkableChildrenIndices(octants, neighborChildIndices, ref ref2);
			OctreeHelperMethods.GetWalkableChildrenIndices(octants, childIndices, ref @ref);
			foreach (int num in childIndices)
			{
				foreach (int num2 in neighborMap.GetValuesForKey(num))
				{
					if (neighborChildIndices.Contains(num2))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000B7F RID: 2943 RVA: 0x0003F1A4 File Offset: 0x0003D3A4
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref Octant GetSmallestOctantForPoint(float3 point, NativeArray<Octant> octants, float minSize)
		{
			ref Octant ptr = ref octants.GetRef(0);
			if (math.distancesq(ptr.ClosestPoint(point), point) > 0.010000001f)
			{
				ptr = Octant.GetInvalidOctant();
				return ref ptr;
			}
			while (ptr.Size.x - minSize > 0.1f)
			{
				int num;
				if (!ptr.TryGetChildOctantIndexFromPoint(point, out num))
				{
					return ref ptr;
				}
				ptr = octants.GetRef(num);
			}
			return ref ptr;
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x0003F204 File Offset: 0x0003D404
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool CheckBlockingAtPoint(float3 point, NativeArray<Octant> octants)
		{
			ref Octant ptr = ref octants.GetRef(0);
			if (Hint.Unlikely(math.distancesq(ptr.ClosestPoint(point), point) > 0.010000001f))
			{
				return false;
			}
			int num;
			while (ptr.TryGetChildOctantIndexFromPoint(point, out num))
			{
				ptr = octants.GetRef(num);
				if (ptr.IsBlocking)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000B81 RID: 2945 RVA: 0x0003F254 File Offset: 0x0003D454
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
				int num2;
				if (!octant.TryGetChildOctantIndexFromPoint(point, out num2))
				{
					index = num;
					return true;
				}
				num = num2;
				octant = octants[num];
			}
			index = num;
			return true;
		}

		// Token: 0x06000B82 RID: 2946 RVA: 0x0003F2BC File Offset: 0x0003D4BC
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetOffsetPosition(int relativeIndex, float offsetDistance)
		{
			float num = (((relativeIndex & 1) == 0) ? offsetDistance : (-offsetDistance));
			float num2 = (((relativeIndex & 2) == 0) ? offsetDistance : (-offsetDistance));
			float num3 = (((relativeIndex & 4) == 0) ? offsetDistance : (-offsetDistance));
			return new float3(num2, num3, num);
		}

		// Token: 0x06000B83 RID: 2947 RVA: 0x0003F2F0 File Offset: 0x0003D4F0
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetNeighborOffset(int index, float offsetDistance)
		{
			float3 @float;
			switch (index)
			{
			case 0:
				@float = new float3(offsetDistance, 0f, 0f);
				break;
			case 1:
				@float = new float3(0f, 0f, offsetDistance);
				break;
			case 2:
				@float = new float3(-offsetDistance, 0f, 0f);
				break;
			case 3:
				@float = new float3(0f, 0f, -offsetDistance);
				break;
			default:
				throw new IndexOutOfRangeException();
			}
			return @float;
		}

		// Token: 0x06000B84 RID: 2948 RVA: 0x0003F36C File Offset: 0x0003D56C
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetSubCellNeighborOffset(int index, float offsetDistance)
		{
			float3 @float;
			switch (index)
			{
			case 0:
				@float = new float3(offsetDistance, offsetDistance, 0f);
				break;
			case 1:
				@float = new float3(0f, offsetDistance, offsetDistance);
				break;
			case 2:
				@float = new float3(-offsetDistance, offsetDistance, 0f);
				break;
			case 3:
				@float = new float3(0f, offsetDistance, -offsetDistance);
				break;
			case 4:
				@float = new float3(offsetDistance, 0f, 0f);
				break;
			case 5:
				@float = new float3(0f, 0f, offsetDistance);
				break;
			case 6:
				@float = new float3(-offsetDistance, 0f, 0f);
				break;
			case 7:
				@float = new float3(0f, 0f, -offsetDistance);
				break;
			case 8:
				@float = new float3(offsetDistance, -offsetDistance, 0f);
				break;
			case 9:
				@float = new float3(0f, -offsetDistance, offsetDistance);
				break;
			case 10:
				@float = new float3(-offsetDistance, -offsetDistance, 0f);
				break;
			case 11:
				@float = new float3(0f, -offsetDistance, -offsetDistance);
				break;
			default:
				throw new IndexOutOfRangeException();
			}
			return @float;
		}

		// Token: 0x06000B85 RID: 2949 RVA: 0x0003F498 File Offset: 0x0003D698
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetBigToSmallNeighborOffset(int index)
		{
			float3 @float;
			switch (index)
			{
			case 0:
				@float = new float3(0.625f, -0.375f, -0.375f);
				break;
			case 1:
				@float = new float3(0.625f, -0.375f, -0.125f);
				break;
			case 2:
				@float = new float3(0.625f, -0.375f, 0.125f);
				break;
			case 3:
				@float = new float3(0.625f, -0.375f, 0.375f);
				break;
			case 4:
				@float = new float3(0.375f, -0.375f, 0.625f);
				break;
			case 5:
				@float = new float3(0.125f, -0.375f, 0.625f);
				break;
			case 6:
				@float = new float3(-0.125f, -0.375f, 0.625f);
				break;
			case 7:
				@float = new float3(-0.375f, -0.375f, 0.625f);
				break;
			case 8:
				@float = new float3(-0.625f, -0.375f, 0.375f);
				break;
			case 9:
				@float = new float3(-0.625f, -0.375f, 0.125f);
				break;
			case 10:
				@float = new float3(-0.625f, -0.375f, -0.125f);
				break;
			case 11:
				@float = new float3(-0.625f, -0.375f, -0.375f);
				break;
			case 12:
				@float = new float3(-0.375f, -0.375f, -0.625f);
				break;
			case 13:
				@float = new float3(-0.125f, -0.375f, -0.625f);
				break;
			case 14:
				@float = new float3(0.125f, -0.375f, -0.625f);
				break;
			case 15:
				@float = new float3(0.375f, -0.375f, -0.625f);
				break;
			default:
				throw new IndexOutOfRangeException();
			}
			return @float;
		}

		// Token: 0x06000B86 RID: 2950 RVA: 0x0003F688 File Offset: 0x0003D888
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float3 GetTerrainToSmallBelowOffset(int index)
		{
			float3 @float;
			switch (index)
			{
			case 0:
				@float = new float3(0.625f, -0.625f, -0.375f);
				break;
			case 1:
				@float = new float3(0.625f, -0.625f, -0.125f);
				break;
			case 2:
				@float = new float3(0.625f, -0.625f, 0.125f);
				break;
			case 3:
				@float = new float3(0.625f, -0.625f, 0.375f);
				break;
			case 4:
				@float = new float3(0.375f, -0.625f, 0.625f);
				break;
			case 5:
				@float = new float3(0.125f, -0.625f, 0.625f);
				break;
			case 6:
				@float = new float3(-0.125f, -0.625f, 0.625f);
				break;
			case 7:
				@float = new float3(-0.375f, -0.625f, 0.625f);
				break;
			case 8:
				@float = new float3(-0.625f, -0.625f, 0.375f);
				break;
			case 9:
				@float = new float3(-0.625f, -0.625f, 0.125f);
				break;
			case 10:
				@float = new float3(-0.625f, -0.625f, -0.125f);
				break;
			case 11:
				@float = new float3(-0.625f, -0.625f, -0.375f);
				break;
			case 12:
				@float = new float3(-0.375f, -0.625f, -0.625f);
				break;
			case 13:
				@float = new float3(-0.125f, -0.625f, -0.625f);
				break;
			case 14:
				@float = new float3(0.125f, -0.625f, -0.625f);
				break;
			case 15:
				@float = new float3(0.375f, -0.625f, -0.625f);
				break;
			default:
				throw new IndexOutOfRangeException();
			}
			return @float;
		}

		// Token: 0x06000B87 RID: 2951 RVA: 0x0003F878 File Offset: 0x0003DA78
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
			float3 float2 = @float - octant2.Center;
			if (float2.Equals(float3.zero))
			{
				return false;
			}
			float3 float3 = math.normalize(float2);
			float3 float4 = math.cross(new float3(0f, 1f, 0f), float3);
			for (int i = 0; i < 10; i++)
			{
				float num2 = (float)(i / 2) / 10f;
				num2 *= ((i % 2 == 0) ? 1f : (-1f));
				float3 float5 = @float + float4 * num2 * num;
				NavMeshLocation navMeshLocation = query.MapLocation(float5 + float3 * 0.03f, new Vector3(0.01f, octant2.Size.x, 0.01f), 0, -1);
				NavMeshLocation navMeshLocation2 = query.MapLocation(float5 - float3 * 0.03f, new Vector3(0.01f, octant2.Size.x, 0.01f), 0, -1);
				if (query.IsValid(navMeshLocation) && query.IsValid(navMeshLocation2))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000B88 RID: 2952 RVA: 0x0003F9F4 File Offset: 0x0003DBF4
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void GetSamplePositions(NativeList<Vector3> samplePositions, float3 min, float3 max, int divisions)
		{
			samplePositions.Clear();
			float num = (max.x - min.x) / (float)(divisions - 1);
			float num2 = (min.y + max.y) / 2f;
			for (int i = 0; i < divisions; i++)
			{
				float num3 = min.x + num * (float)i;
				for (int j = 0; j < divisions; j++)
				{
					float num4 = min.z + num * (float)j;
					Vector3 vector = new Vector3(num3, num2, num4);
					samplePositions.Add(in vector);
				}
			}
		}

		// Token: 0x04000AC9 RID: 2761
		private const float squareTolerance = 0.010000001f;
	}
}
