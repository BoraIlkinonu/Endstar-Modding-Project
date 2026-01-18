using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000005 RID: 5
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class MinMaxAttribute : PropertyAttribute
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000020D5 File Offset: 0x000002D5
		public MinMaxAttribute(float min, float max)
		{
			this.minValue = min;
			this.maxValue = max;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000020EB File Offset: 0x000002EB
		public MinMaxAttribute(float min, float max, string name)
		{
			this.minValue = min;
			this.maxValue = max;
			this.displayName = name;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002108 File Offset: 0x00000308
		public MinMaxAttribute(float min, float max, string name, string prefix)
		{
			this.minValue = min;
			this.maxValue = max;
			this.displayName = name;
			this.prefixName = prefix;
		}

		// Token: 0x04000002 RID: 2
		public float minValue;

		// Token: 0x04000003 RID: 3
		public float maxValue;

		// Token: 0x04000004 RID: 4
		public string displayName;

		// Token: 0x04000005 RID: 5
		public string prefixName;
	}
}
