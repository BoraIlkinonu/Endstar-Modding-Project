using System;
using Newtonsoft.Json;

namespace Endless.Gameplay.Stats
{
	// Token: 0x02000386 RID: 902
	public class ComparativeStat : NumericPlayerStat
	{
		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x0600170C RID: 5900 RVA: 0x0006BA4E File Offset: 0x00069C4E
		internal bool IsValid
		{
			get
			{
				return this.statMap.Values.Count > 0;
			}
		}

		// Token: 0x0600170D RID: 5901 RVA: 0x0006BA64 File Offset: 0x00069C64
		internal BasicStat ToBasicStat()
		{
			int num = -1;
			float num2 = ((this.Comparison == ComparativeStat.ValueComparison.Most) ? float.MinValue : float.MaxValue);
			foreach (int num3 in this.statMap.Keys)
			{
				if (num3 == -1)
				{
					num = num3;
					num2 = this.statMap[num];
				}
				else
				{
					ComparativeStat.ValueComparison comparison = this.Comparison;
					if (comparison != ComparativeStat.ValueComparison.Most)
					{
						if (comparison != ComparativeStat.ValueComparison.Least)
						{
							throw new ArgumentOutOfRangeException();
						}
						if (this.statMap[num3] < num2)
						{
							num = num3;
							num2 = this.statMap[num3];
						}
					}
					else if (this.statMap[num3] > num2)
					{
						num = num3;
						num2 = this.statMap[num3];
					}
				}
			}
			return new BasicStat
			{
				Message = this.Message,
				Identifier = this.Identifier,
				Order = this.Order,
				InventoryIcon = this.InventoryIcon,
				UserId = num,
				Value = StatBase.GetFormattedString(num2, this.DisplayFormat)
			};
		}

		// Token: 0x0600170E RID: 5902 RVA: 0x0006B81F File Offset: 0x00069A1F
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		// Token: 0x0600170F RID: 5903 RVA: 0x0006BB88 File Offset: 0x00069D88
		public void LoadFromString(string stringData)
		{
			ComparativeStat comparativeStat = JsonConvert.DeserializeObject<ComparativeStat>(stringData);
			base.CopyFrom(comparativeStat);
			this.Comparison = comparativeStat.Comparison;
			this.DisplayFormat = comparativeStat.DisplayFormat;
			this.statMap = comparativeStat.statMap;
		}

		// Token: 0x04001278 RID: 4728
		public ComparativeStat.ValueComparison Comparison;

		// Token: 0x02000387 RID: 903
		public enum ValueComparison
		{
			// Token: 0x0400127A RID: 4730
			Most,
			// Token: 0x0400127B RID: 4731
			Least
		}
	}
}
