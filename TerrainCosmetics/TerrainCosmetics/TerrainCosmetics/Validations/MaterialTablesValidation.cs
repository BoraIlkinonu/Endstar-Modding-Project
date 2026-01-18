using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using Endless.Validation;

namespace Endless.TerrainCosmetics.Validations
{
	// Token: 0x02000011 RID: 17
	public class MaterialTablesValidation : Validator
	{
		// Token: 0x0600005D RID: 93 RVA: 0x0000289C File Offset: 0x00000A9C
		public MaterialTablesValidation(TilesetCosmeticProfile profile)
		{
			this.profile = profile;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x000028AC File Offset: 0x00000AAC
		public override List<ValidationResult> PassesValidation()
		{
			HashSet<string> ids = new HashSet<string>();
			bool flag = this.profile.InheritedTilesetId != SerializableGuid.Empty;
			if (flag)
			{
				if (this.profile.MaterialVariantWeightTables.All((MaterialWeightTableAsset table) => table.InheritanceState == InheritanceState.Inherit))
				{
					return ValidationResult.Fail("Profile is inherited and material tables are set to inherit!", this.profile);
				}
			}
			ids.Clear();
			TileCosmetic[] tileCosmetics = this.profile.TileCosmetics;
			int i;
			for (i = 0; i < tileCosmetics.Length; i++)
			{
				foreach (string text in tileCosmetics[i].VariantMaterialBaseNames)
				{
					ids.Add(text);
				}
			}
			if (ids.Count == 0)
			{
				return ValidationResult.Fail("No Ids were present on any of the tile cosmetics", this.profile);
			}
			int index;
			Func<MaterialWeightTableAsset, bool> <>9__1;
			for (index = 0; index < ids.Count; index = i + 1)
			{
				IEnumerable<MaterialWeightTableAsset> materialVariantWeightTables = this.profile.MaterialVariantWeightTables;
				Func<MaterialWeightTableAsset, bool> func;
				if ((func = <>9__1) == null)
				{
					func = (<>9__1 = (MaterialWeightTableAsset materialTable) => materialTable.MaterialId == ids.ElementAt(index));
				}
				if (!materialVariantWeightTables.Any(func))
				{
					return ValidationResult.Fail("Material Id on Tile Cosmetic did not have a matching table. Id: " + ids.ElementAt(index), this.profile);
				}
				i = index;
			}
			for (int j = 0; j < this.profile.MaterialVariantWeightTables.Length; j++)
			{
				if ((!flag || this.profile.MaterialVariantWeightTables[j].InheritanceState != InheritanceState.Inherit) && this.profile.MaterialVariantWeightTables[j].Table.EntryCount <= 0)
				{
					return ValidationResult.Fail("Material Id Table did not have at least one material in it. Material ID: " + this.profile.MaterialVariantWeightTables[j].MaterialId, this.profile);
				}
			}
			return ValidationResult.Pass(null);
		}

		// Token: 0x0400004C RID: 76
		private TilesetCosmeticProfile profile;
	}
}
