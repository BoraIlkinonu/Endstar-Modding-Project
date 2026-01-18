using System;
using UnityEngine;

namespace Endless.Shared
{
	// Token: 0x02000004 RID: 4
	public class MaxAttribute : PropertyAttribute
	{
		// Token: 0x06000004 RID: 4 RVA: 0x000020C6 File Offset: 0x000002C6
		public MaxAttribute(float maxValue)
		{
			this.maxValue = maxValue;
		}

		// Token: 0x04000001 RID: 1
		public float maxValue;
	}
}
