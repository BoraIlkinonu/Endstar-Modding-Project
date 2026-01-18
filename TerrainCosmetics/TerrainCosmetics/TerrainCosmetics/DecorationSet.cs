using System;
using System.Collections.Generic;
using Endless.Data.WeightTables;
using UnityEngine;

namespace Endless.TerrainCosmetics
{
	// Token: 0x02000008 RID: 8
	public class DecorationSet : ScriptableObject
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000016 RID: 22 RVA: 0x000022DB File Offset: 0x000004DB
		public IReadOnlyCollection<Transform> Values
		{
			get
			{
				return this.table.Values;
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000022E8 File Offset: 0x000004E8
		private void Reset()
		{
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000022EC File Offset: 0x000004EC
		public Transform GenerateDecoration(global::System.Random random)
		{
			Transform randomWeightedEntry = this.table.GetRandomWeightedEntry(random);
			if (randomWeightedEntry == null)
			{
				return null;
			}
			return global::UnityEngine.Object.Instantiate<Transform>(randomWeightedEntry);
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002317 File Offset: 0x00000517
		public void CopyTableFrom(DecorationSet parentDecorationSet)
		{
			this.table.CopyFrom(parentDecorationSet.table);
		}

		// Token: 0x0400000D RID: 13
		[SerializeField]
		private TransformWeightTable table;
	}
}
