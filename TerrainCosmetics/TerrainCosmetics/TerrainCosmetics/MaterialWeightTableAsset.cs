using System;
using Endless.Data.WeightTables;
using UnityEngine;

namespace Endless.TerrainCosmetics
{
	// Token: 0x0200000A RID: 10
	public class MaterialWeightTableAsset : ScriptableObject
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600001C RID: 28 RVA: 0x0000233A File Offset: 0x0000053A
		public string MaterialId
		{
			get
			{
				return this.materialId;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002342 File Offset: 0x00000542
		public MaterialVariantWeightTable Table
		{
			get
			{
				return this.table;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600001E RID: 30 RVA: 0x0000234A File Offset: 0x0000054A
		public InheritanceState[] AllowedInheritanceStates
		{
			get
			{
				return this.allowedInheritanceStates;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600001F RID: 31 RVA: 0x00002352 File Offset: 0x00000552
		public InheritanceState InheritanceState
		{
			get
			{
				return this.inheritanceState;
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000235A File Offset: 0x0000055A
		public void CopyTableFrom(WeightTable<Material> copy)
		{
			this.table.CopyFrom(copy);
		}

		// Token: 0x0400000E RID: 14
		[SerializeField]
		private string materialId;

		// Token: 0x0400000F RID: 15
		[SerializeField]
		private MaterialVariantWeightTable table;

		// Token: 0x04000010 RID: 16
		[SerializeField]
		private InheritanceState inheritanceState;

		// Token: 0x04000011 RID: 17
		[SerializeField]
		private InheritanceState[] allowedInheritanceStates;
	}
}
