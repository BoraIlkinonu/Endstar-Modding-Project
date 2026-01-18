using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Endless.Gameplay
{
	// Token: 0x0200021D RID: 541
	public readonly struct SectionSurface
	{
		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000B3D RID: 2877 RVA: 0x0003D559 File Offset: 0x0003B759
		public float3 Center { get; }

		// Token: 0x06000B3E RID: 2878 RVA: 0x0003D564 File Offset: 0x0003B764
		public SectionSurface(float3 p1, float3 p2, float3 p3, float3 p4)
		{
			this.p1 = p1;
			this.p2 = p2;
			this.p3 = p3;
			this.p4 = p4;
			this.Center = (p1 + p2 + p3 + p4) / 4f;
		}

		// Token: 0x06000B3F RID: 2879 RVA: 0x0003D5B4 File Offset: 0x0003B7B4
		public SectionEdge GetSectionEdge(int edgeIndex)
		{
			SectionEdge sectionEdge;
			switch (edgeIndex)
			{
			case 0:
				sectionEdge = new SectionEdge(this.p1, this.p2);
				break;
			case 1:
				sectionEdge = new SectionEdge(this.p2, this.p3);
				break;
			case 2:
				sectionEdge = new SectionEdge(this.p3, this.p4);
				break;
			case 3:
				sectionEdge = new SectionEdge(this.p4, this.p1);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return sectionEdge;
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x0003D630 File Offset: 0x0003B830
		[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
		public SectionEdge GetClosestEdgeToPoint(float3 point)
		{
			NativeArray<float3> nativeArray = new NativeArray<float3>(4, Allocator.Temp, NativeArrayOptions.ClearMemory);
			nativeArray[0] = this.p1;
			nativeArray[1] = this.p2;
			nativeArray[2] = this.p3;
			nativeArray[3] = this.p4;
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			int num3 = -1;
			int num4 = -1;
			for (int i = 0; i < nativeArray.Length; i++)
			{
				float num5 = math.distancesq(nativeArray[i], point);
				if (num5 < num)
				{
					num2 = num;
					num4 = num3;
					num = num5;
					num3 = i;
				}
				else if (num5 < num2)
				{
					num2 = num5;
					num4 = i;
				}
			}
			return new SectionEdge(nativeArray[num3], nativeArray[num4]);
		}

		// Token: 0x1700021C RID: 540
		private float3 this[int i]
		{
			get
			{
				float3 @float;
				switch (i)
				{
				case 0:
					@float = this.p1;
					break;
				case 1:
					@float = this.p2;
					break;
				case 2:
					@float = this.p3;
					break;
				case 3:
					@float = this.p4;
					break;
				default:
					throw new ArgumentOutOfRangeException("i", i, null);
				}
				return @float;
			}
		}

		// Token: 0x06000B42 RID: 2882 RVA: 0x0003D748 File Offset: 0x0003B948
		private float3 GetPlaneNormal()
		{
			return math.normalize(math.cross(this.p2 - this.p1, this.p4 - this.p1));
		}

		// Token: 0x06000B43 RID: 2883 RVA: 0x0003D778 File Offset: 0x0003B978
		public float3 TranslateSurfacePoint(float3 point, float3 direction, float distance)
		{
			if (distance == 0f || direction.Equals(float3.zero))
			{
				return point;
			}
			float3 planeNormal = this.GetPlaneNormal();
			float3 @float = math.normalize(math.cross(math.cross(direction, planeNormal), planeNormal));
			@float = math.select(-@float, @float, math.dot(direction, @float) > 0f);
			point += @float * distance;
			if (this.IsPointInQuad(point))
			{
				return point;
			}
			float3 float2 = point;
			float num = float.MaxValue;
			for (int i = 0; i < 4; i++)
			{
				float3 float3 = this[i];
				float3 float4 = this[(i + 1) % 4];
				SectionEdge sectionEdge = new SectionEdge(float3, float4);
				float3 closestPointOnEdge = sectionEdge.GetClosestPointOnEdge(point, 0.1f);
				float num2 = math.distancesq(point, closestPointOnEdge);
				if (num2 < num)
				{
					num = num2;
					float2 = closestPointOnEdge;
				}
			}
			return float2;
		}

		// Token: 0x06000B44 RID: 2884 RVA: 0x0003D84C File Offset: 0x0003BA4C
		public NativeList<float3> GetSurfaceSamplePoints()
		{
			float num = math.distance(this.p1, this.p2);
			float num2 = math.distance(this.p2, this.p3);
			float3 @float = math.normalize(this.p3 - this.p2);
			int num3 = math.clamp((int)math.round(num) + 1, 2, 5);
			int num4 = math.clamp((int)math.round(num2) + 1, 2, 5);
			NativeList<float3> nativeList = new NativeList<float3>(num3 * num4, AllocatorManager.Temp);
			float num5 = 1f / (float)(num3 - 1);
			float num6 = 1f / (float)(num4 - 1);
			for (int i = 0; i < num3; i++)
			{
				float3 float2 = math.lerp(this.p1, this.p2, (float)i * num5);
				for (int j = 0; j < num4; j++)
				{
					float num7 = math.lerp(0f, num2, (float)j * num6);
					float3 float3 = float2 + @float * num7;
					nativeList.Add(in float3);
				}
			}
			return nativeList;
		}

		// Token: 0x06000B45 RID: 2885 RVA: 0x0003D944 File Offset: 0x0003BB44
		public NativeList<float3> GetSurfaceEdgeSamplePoints()
		{
			NativeList<float3> nativeList = new NativeList<float3>(16, AllocatorManager.Temp);
			for (int i = 0; i < 4; i++)
			{
				SectionEdge sectionEdge = this.GetSectionEdge(i);
				int num = (int)math.ceil(math.distance(sectionEdge.p1, sectionEdge.p2) / 0.5f);
				num = math.clamp(num, 3, 5);
				for (int j = 0; j < num; j++)
				{
					float3 @float = math.lerp(sectionEdge.p1, sectionEdge.p2, (float)j / ((float)num - 1f));
					nativeList.Add(in @float);
				}
			}
			return nativeList;
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x0003D9D4 File Offset: 0x0003BBD4
		private bool IsPointInQuad(float3 pt)
		{
			float3 planeNormal = this.GetPlaneNormal();
			for (int i = 0; i < 4; i++)
			{
				float3 @float = this[i];
				float3 float2 = this[(i + 1) % 4] - @float;
				float3 float3 = pt - @float;
				if (math.dot(math.cross(float2, float3), planeNormal) < 0f)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x04000A9B RID: 2715
		private readonly float3 p1;

		// Token: 0x04000A9C RID: 2716
		private readonly float3 p2;

		// Token: 0x04000A9D RID: 2717
		private readonly float3 p3;

		// Token: 0x04000A9E RID: 2718
		private readonly float3 p4;
	}
}
