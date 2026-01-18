using System;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x0200001F RID: 31
	public struct UserReportMetric
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x0000463B File Offset: 0x0000283B
		public double Average
		{
			get
			{
				return this.Sum / (double)this.Count;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000B6 RID: 182 RVA: 0x0000464B File Offset: 0x0000284B
		// (set) Token: 0x060000B7 RID: 183 RVA: 0x00004653 File Offset: 0x00002853
		public int Count { readonly get; set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000B8 RID: 184 RVA: 0x0000465C File Offset: 0x0000285C
		// (set) Token: 0x060000B9 RID: 185 RVA: 0x00004664 File Offset: 0x00002864
		public double Maximum { readonly get; set; }

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000BA RID: 186 RVA: 0x0000466D File Offset: 0x0000286D
		// (set) Token: 0x060000BB RID: 187 RVA: 0x00004675 File Offset: 0x00002875
		public double Minimum { readonly get; set; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000BC RID: 188 RVA: 0x0000467E File Offset: 0x0000287E
		// (set) Token: 0x060000BD RID: 189 RVA: 0x00004686 File Offset: 0x00002886
		public string Name { readonly get; set; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000BE RID: 190 RVA: 0x0000468F File Offset: 0x0000288F
		// (set) Token: 0x060000BF RID: 191 RVA: 0x00004697 File Offset: 0x00002897
		public double Sum { readonly get; set; }

		// Token: 0x060000C0 RID: 192 RVA: 0x000046A0 File Offset: 0x000028A0
		public void Sample(double value)
		{
			if (this.Count == 0)
			{
				this.Minimum = double.MaxValue;
				this.Maximum = double.MinValue;
			}
			int count = this.Count;
			this.Count = count + 1;
			this.Sum += value;
			this.Minimum = Math.Min(this.Minimum, value);
			this.Maximum = Math.Max(this.Maximum, value);
		}
	}
}
