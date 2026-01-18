using System;
using Unity.Mathematics;

namespace Endless.Gameplay
{
	// Token: 0x02000225 RID: 549
	public static class Float3Utilities
	{
		// Token: 0x06000B68 RID: 2920 RVA: 0x0003EC58 File Offset: 0x0003CE58
		public static float3 WithY(this float3 a, float y)
		{
			return new float3(a.x, y, a.z);
		}
	}
}
