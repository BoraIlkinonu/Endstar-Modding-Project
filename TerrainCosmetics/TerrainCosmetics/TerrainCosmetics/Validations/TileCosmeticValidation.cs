using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using Endless.Validation;
using UnityEngine;

namespace Endless.TerrainCosmetics.Validations
{
	// Token: 0x02000015 RID: 21
	public class TileCosmeticValidation : Validator
	{
		// Token: 0x06000068 RID: 104 RVA: 0x0000312B File Offset: 0x0000132B
		public TileCosmeticValidation(TilesetCosmeticProfile profile)
		{
			this.profile = profile;
		}

		// Token: 0x06000069 RID: 105 RVA: 0x0000313C File Offset: 0x0000133C
		public override List<ValidationResult> PassesValidation()
		{
			if (this.profile.InheritedTilesetId != SerializableGuid.Empty && this.profile.TileCosmeticInheritance == InheritanceState.Inherit)
			{
				return ValidationResult.Pass(null);
			}
			int num;
			switch (this.profile.TilesetType)
			{
			case TilesetType.Base:
				num = 64;
				break;
			case TilesetType.Slope:
				num = 16;
				break;
			case TilesetType.Pillar:
				num = 3;
				break;
			case TilesetType.Horizontal:
				num = 16;
				break;
			default:
				return ValidationResult.Fail("Unknown Tileset Type: " + this.profile.TilesetType.ToString(), this.profile);
			}
			if (this.profile.TileCosmetics.Length != num)
			{
				return ValidationResult.Fail(string.Format("Expected {0} tiles in tile cosmetics, but only found {1}", num, this.profile.TileCosmetics.Length), this.profile);
			}
			foreach (TileCosmetic tileCosmetic in this.profile.TileCosmetics)
			{
				if (tileCosmetic.Visuals.Any((Transform visual) => !visual))
				{
					return ValidationResult.Fail(string.Format("Tile Cosmetics contained invalid visuals. Index: {0}", tileCosmetic.Index), this.profile);
				}
				if (tileCosmetic.Visuals.FirstOrDefault((Transform visual) => visual.GetComponent<MeshRenderer>() != null) && tileCosmetic.VariantMaterialBaseNames.Count == 0)
				{
					return ValidationResult.Fail(string.Format("Tile Cosmetics must contain at least one material id. Index: {0}", tileCosmetic.Index), this.profile);
				}
			}
			return ValidationResult.Pass(null);
		}

		// Token: 0x0400005A RID: 90
		private const int EXPECTED_TILE_COUNT_BASE = 64;

		// Token: 0x0400005B RID: 91
		private const int EXPECTED_TILE_COUNT_SLOPE = 16;

		// Token: 0x0400005C RID: 92
		private const int EXPECTED_TILE_COUNT_HORIZONTAL = 16;

		// Token: 0x0400005D RID: 93
		private const int EXPECTED_TILE_COUNT_PILLAR = 3;

		// Token: 0x0400005E RID: 94
		private TilesetCosmeticProfile profile;
	}
}
