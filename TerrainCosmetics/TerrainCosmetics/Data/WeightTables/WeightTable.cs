using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Data.WeightTables
{
	// Token: 0x02000006 RID: 6
	[Serializable]
	public class WeightTable<T>
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000009 RID: 9 RVA: 0x00002157 File Offset: 0x00000357
		public int EntryCount
		{
			get
			{
				return this.entries.Count;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000A RID: 10 RVA: 0x00002164 File Offset: 0x00000364
		public IReadOnlyCollection<T> Values
		{
			get
			{
				return this.values;
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000216C File Offset: 0x0000036C
		public WeightTable()
		{
			this.values = Array.Empty<T>();
			this.totalWeight = 0;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002194 File Offset: 0x00000394
		public T GetRandomWeightedEntry(global::System.Random random)
		{
			int num = random.Next(this.totalWeight);
			int num2 = 0;
			while (num2 < this.entries.Count && !this.entries[num2].WeightIsBetweenThresholds(num))
			{
				num2++;
			}
			return this.values[num2];
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000021E4 File Offset: 0x000003E4
		public WeightTable<T> Copy()
		{
			WeightTable<T> weightTable = new WeightTable<T>();
			weightTable.totalWeight = this.totalWeight;
			weightTable.entries = this.entries.Select((WeightEntry entry) => entry.Copy()).ToList<WeightEntry>();
			weightTable.values = this.values;
			return weightTable;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002244 File Offset: 0x00000444
		public void CopyFrom(WeightTable<T> sourceTable)
		{
			this.entries = sourceTable.entries.Select((WeightEntry entry) => entry.Copy()).ToList<WeightEntry>();
			this.values = sourceTable.values.ToArray<T>();
			this.totalWeight = sourceTable.totalWeight;
		}

		// Token: 0x04000004 RID: 4
		[SerializeReference]
		private T[] values;

		// Token: 0x04000005 RID: 5
		[SerializeField]
		private List<WeightEntry> entries = new List<WeightEntry>();

		// Token: 0x04000006 RID: 6
		[SerializeField]
		private int totalWeight;
	}
}
