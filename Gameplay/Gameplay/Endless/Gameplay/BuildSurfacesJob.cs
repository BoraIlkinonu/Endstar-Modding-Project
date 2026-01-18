using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

namespace Endless.Gameplay
{
	// Token: 0x020001EE RID: 494
	[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
	public struct BuildSurfacesJob : IJobParallelFor
	{
		// Token: 0x06000A33 RID: 2611 RVA: 0x00036D18 File Offset: 0x00034F18
		public void Execute(int index)
		{
			int num = this.SectionKeyArray[index];
			float num2 = float.MaxValue;
			float num3 = float.MaxValue;
			float num4 = float.MaxValue;
			float num5 = float.MinValue;
			float num6 = float.MinValue;
			float num7 = float.MinValue;
			float num8 = 1f;
			bool flag = false;
			float num9 = 0f;
			bool flag2 = false;
			foreach (int num10 in this.SectionMap.GetValuesForKey(num))
			{
				ref Octant @ref = ref this.Octants.GetRef(num10);
				if (!flag)
				{
					num9 = @ref.Center.y;
					flag = true;
				}
				else if (!flag2 && math.abs(num9 - @ref.Center.y) > 0.01f)
				{
					flag2 = true;
				}
				num8 = @ref.Size.x;
				float3 min = @ref.Min;
				float3 max = @ref.Max;
				num2 = math.select(num2, min.x, min.x < num2);
				num3 = math.select(num3, min.y, min.y < num3);
				num4 = math.select(num4, min.z, min.z < num4);
				num5 = math.select(num5, max.x, max.x > num5);
				num6 = math.select(num6, max.y, max.y > num6);
				num7 = math.select(num7, max.z, max.z > num7);
			}
			float num11 = (num6 + num3) / 2f;
			float num12 = num6 - num3;
			float3 @float = new float3(num2, num11, num4);
			float3 float2 = new float3(num2, num11, num7);
			float3 float3 = new float3(num5, num11, num7);
			float3 float4 = new float3(num5, num11, num4);
			SectionSurface sectionSurface;
			if (num8 >= 1f)
			{
				@float.y = num3;
				float2.y = num3;
				float3.y = num3;
				float4.y = num3;
				sectionSurface = new SectionSurface(@float, float2, float3, float4);
				this.SurfaceMap.TryAdd(num, sectionSurface);
				return;
			}
			float3 float5 = new float3(0.025f, num12 / 2f + 0.01f, 0.025f);
			@float.y = this.GetEdgeHeight(float3, @float, float5);
			float2.y = this.GetEdgeHeight(float4, float2, float5);
			float3.y = this.GetEdgeHeight(@float, float3, float5);
			float4.y = this.GetEdgeHeight(float2, float4, float5);
			sectionSurface = new SectionSurface(@float, float2, float3, float4);
			this.SurfaceMap.TryAdd(num, sectionSurface);
		}

		// Token: 0x06000A34 RID: 2612 RVA: 0x00036FE0 File Offset: 0x000351E0
		private float GetEdgeHeight(float3 oppositeCorner, float3 cornerPoint, float3 extents)
		{
			for (int i = 0; i < 8; i++)
			{
				float num = (float)i / 8f;
				float3 @float = math.lerp(cornerPoint, oppositeCorner, num);
				NavMeshLocation navMeshLocation = this.Query.MapLocation(@float, extents, 0, -1);
				if (this.Query.IsValid(navMeshLocation))
				{
					return navMeshLocation.position.y;
				}
			}
			return cornerPoint.y;
		}

		// Token: 0x0400096F RID: 2415
		[ReadOnly]
		public NativeArray<int> SectionKeyArray;

		// Token: 0x04000970 RID: 2416
		[ReadOnly]
		public NativeParallelMultiHashMap<int, int> SectionMap;

		// Token: 0x04000971 RID: 2417
		[NativeDisableParallelForRestriction]
		public NativeArray<Octant> Octants;

		// Token: 0x04000972 RID: 2418
		[NativeDisableParallelForRestriction]
		public NavMeshQuery Query;

		// Token: 0x04000973 RID: 2419
		[WriteOnly]
		public NativeParallelHashMap<int, SectionSurface>.ParallelWriter SurfaceMap;
	}
}
