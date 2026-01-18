using System;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay.Stats
{
	// Token: 0x02000384 RID: 900
	public class NumericPlayerStat : StatBase
	{
		// Token: 0x06001703 RID: 5891 RVA: 0x0006B86C File Offset: 0x00069A6C
		public void ModifyStat(Context playerContext, float delta)
		{
			int num;
			if (base.TryGetUserId(playerContext, out num) && !this.statMap.TryAdd(num, delta))
			{
				Dictionary<int, float> dictionary = this.statMap;
				int num2 = num;
				dictionary[num2] += delta;
			}
		}

		// Token: 0x06001704 RID: 5892 RVA: 0x0006B8AC File Offset: 0x00069AAC
		internal void SetStats(Dictionary<int, float> newStatMap)
		{
			this.statMap = newStatMap;
		}

		// Token: 0x04001274 RID: 4724
		public NumericDisplayFormat DisplayFormat;

		// Token: 0x04001275 RID: 4725
		[JsonProperty]
		protected Dictionary<int, float> statMap = new Dictionary<int, float>();
	}
}
