using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Validation;
using UnityEngine;

namespace Endless.TerrainCosmetics.Validations
{
	// Token: 0x02000014 RID: 20
	public class TextureSizeValidation : Validator
	{
		// Token: 0x06000065 RID: 101 RVA: 0x000030E1 File Offset: 0x000012E1
		public TextureSizeValidation(TilesetCosmeticProfile profile)
		{
			this.profile = profile;
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00003106 File Offset: 0x00001306
		public override List<ValidationResult> PassesValidation()
		{
			return ValidationResult.Pass(null);
		}

		// Token: 0x04000054 RID: 84
		private string[] dependencies;

		// Token: 0x04000055 RID: 85
		private SerializableGuid lastCheckedGuid = SerializableGuid.Empty;

		// Token: 0x04000056 RID: 86
		private string assetPath = string.Empty;

		// Token: 0x04000057 RID: 87
		private Hash128 previousHash;

		// Token: 0x04000058 RID: 88
		private static readonly string[] WhiteListedTextureTypes = new string[] { ".png", ".tga" };

		// Token: 0x04000059 RID: 89
		private TilesetCosmeticProfile profile;
	}
}
