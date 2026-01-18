using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Validation;
using UnityEngine;

namespace Endless.TerrainCosmetics.Validations
{
	// Token: 0x02000013 RID: 19
	public class PurityValidator : Validator
	{
		// Token: 0x06000062 RID: 98 RVA: 0x00002D8C File Offset: 0x00000F8C
		public PurityValidator(TilesetCosmeticProfile profile)
		{
			this.profile = profile;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00002DDC File Offset: 0x00000FDC
		public override List<ValidationResult> PassesValidation()
		{
			List<ValidationResult> list = new List<ValidationResult>();
			TileCosmetic[] tileCosmetics = this.profile.TileCosmetics;
			for (int i = 0; i < tileCosmetics.Length; i++)
			{
				foreach (Transform transform in tileCosmetics[i].Visuals)
				{
					list.AddRange(this.ValidateOnlyAllowedScripts(transform));
				}
			}
			MaterialWeightTableAsset[] materialVariantWeightTables = this.profile.MaterialVariantWeightTables;
			for (int i = 0; i < materialVariantWeightTables.Length; i++)
			{
				foreach (Material material in materialVariantWeightTables[i].Table.Values)
				{
					if (material.shader.name != "Shader Graphs/Endless_Shader_Terrain")
					{
						list.AddRange(ValidationResult.Fail("Invalid Shader applied to material. Material Name: " + material.name + ", Shader Name: " + material.shader.name, material));
					}
				}
			}
			if (this.profile.TopDecorationSet)
			{
				foreach (Transform transform2 in this.profile.TopDecorationSet.Values)
				{
					if (transform2)
					{
						list.AddRange(this.ValidateOnlyAllowedScripts(transform2));
					}
				}
			}
			if (this.profile.SideDecorationSet)
			{
				foreach (Transform transform3 in this.profile.SideDecorationSet.Values)
				{
					if (transform3)
					{
						list.AddRange(this.ValidateOnlyAllowedScripts(transform3));
					}
				}
			}
			if (this.profile.BottomDecorationSet)
			{
				foreach (Transform transform4 in this.profile.BottomDecorationSet.Values)
				{
					if (transform4)
					{
						list.AddRange(this.ValidateOnlyAllowedScripts(transform4));
					}
				}
			}
			return list;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003050 File Offset: 0x00001250
		private List<ValidationResult> ValidateOnlyAllowedScripts(Transform transform)
		{
			List<ValidationResult> list = new List<ValidationResult>();
			foreach (Component component in transform.GetComponents<Component>())
			{
				component.GetType();
				if (!this.whitelistedTypes.Contains(component.GetType()))
				{
					return ValidationResult.Fail("Found Type: " + component.GetType().Name + ", which is not allowed", component);
				}
			}
			for (int j = 0; j < transform.childCount; j++)
			{
				list.AddRange(this.ValidateOnlyAllowedScripts(transform.GetChild(j)));
			}
			return list;
		}

		// Token: 0x04000052 RID: 82
		private Type[] whitelistedTypes = new Type[]
		{
			typeof(Transform),
			typeof(MeshFilter),
			typeof(MeshRenderer)
		};

		// Token: 0x04000053 RID: 83
		private TilesetCosmeticProfile profile;
	}
}
