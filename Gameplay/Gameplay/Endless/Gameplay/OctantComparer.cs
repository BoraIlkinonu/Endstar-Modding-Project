using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Endless.Gameplay
{
	// Token: 0x0200021B RID: 539
	public struct OctantComparer : IComparer<int>
	{
		// Token: 0x06000B37 RID: 2871 RVA: 0x0003D3F0 File Offset: 0x0003B5F0
		public OctantComparer(NativeArray<Octant> octants)
		{
			this.octants = octants;
		}

		// Token: 0x06000B38 RID: 2872 RVA: 0x0003D3FC File Offset: 0x0003B5FC
		public int Compare(int a, int b)
		{
			Octant octant = this.octants[a];
			Octant octant2 = this.octants[b];
			float num = octant.Center.y;
			int num2 = num.CompareTo(octant2.Center.y);
			if (num2 != 0)
			{
				return num2;
			}
			num = octant.Center.z;
			int num3 = num.CompareTo(octant2.Center.z);
			if (num3 != 0)
			{
				return num3;
			}
			num = octant.Center.x;
			return num.CompareTo(octant2.Center.x);
		}

		// Token: 0x04000A98 RID: 2712
		private NativeArray<Octant> octants;
	}
}
