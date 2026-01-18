using System;
using Unity.Burst;
using Unity.Mathematics;

namespace Endless.Gameplay
{
	// Token: 0x0200021C RID: 540
	public readonly struct SectionEdge
	{
		// Token: 0x06000B39 RID: 2873 RVA: 0x0003D48C File Offset: 0x0003B68C
		public SectionEdge(float3 p1, float3 p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x06000B3A RID: 2874 RVA: 0x0003D49C File Offset: 0x0003B69C
		public float Length
		{
			get
			{
				return math.length(this.p2 - this.p1);
			}
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x06000B3B RID: 2875 RVA: 0x0003D4B4 File Offset: 0x0003B6B4
		public float3 Center
		{
			get
			{
				return (this.p1 + this.p2) / 2f;
			}
		}

		// Token: 0x06000B3C RID: 2876 RVA: 0x0003D4D4 File Offset: 0x0003B6D4
		[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Low)]
		public float3 GetClosestPointOnEdge(float3 point, float edgeTolerance = 0f)
		{
			float3 @float = this.p2 - this.p1;
			float3 float2 = point - this.p1;
			float num = math.dot(@float, @float);
			if (num == 0f)
			{
				return this.p1;
			}
			float num2 = math.dot(float2, @float) / num;
			float num3 = edgeTolerance / math.distance(this.p1, this.p2);
			num2 = math.clamp(num2, num3, 1f - num3);
			return this.p1 + num2 * @float;
		}

		// Token: 0x04000A99 RID: 2713
		public readonly float3 p1;

		// Token: 0x04000A9A RID: 2714
		public readonly float3 p2;
	}
}
