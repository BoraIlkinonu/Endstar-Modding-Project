using System;
using UnityEngine;

namespace Endless.Data.WeightTables
{
	// Token: 0x02000005 RID: 5
	[Serializable]
	public class WeightEntry
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000020D0 File Offset: 0x000002D0
		public WeightEntry()
		{
			this.minimumThreshold = 0;
			this.maximumThreshold = this.weight;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000020F2 File Offset: 0x000002F2
		public WeightEntry(int weight)
		{
			this.weight = weight;
			this.minimumThreshold = 0;
			this.maximumThreshold = weight;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002116 File Offset: 0x00000316
		public bool WeightIsBetweenThresholds(int weight)
		{
			return weight >= this.minimumThreshold && weight < this.maximumThreshold;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000212C File Offset: 0x0000032C
		public WeightEntry Copy()
		{
			return new WeightEntry
			{
				weight = this.weight,
				minimumThreshold = this.minimumThreshold,
				maximumThreshold = this.maximumThreshold
			};
		}

		// Token: 0x04000001 RID: 1
		[SerializeField]
		private int weight = 1;

		// Token: 0x04000002 RID: 2
		[SerializeField]
		private int minimumThreshold;

		// Token: 0x04000003 RID: 3
		[SerializeField]
		private int maximumThreshold;
	}
}
