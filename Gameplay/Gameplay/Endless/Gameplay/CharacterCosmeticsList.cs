using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000086 RID: 134
	public class CharacterCosmeticsList : ScriptableObject
	{
		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000269 RID: 617 RVA: 0x0000D77F File Offset: 0x0000B97F
		public IReadOnlyList<CharacterCosmeticsDefinition> Cosmetics
		{
			get
			{
				return this.cosmetics;
			}
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x0600026A RID: 618 RVA: 0x0000D788 File Offset: 0x0000B988
		private Dictionary<SerializableGuid, CharacterCosmeticsDefinition> DefinitionMap
		{
			get
			{
				if (this.definitionMap == null)
				{
					this.definitionMap = new Dictionary<SerializableGuid, CharacterCosmeticsDefinition>();
					foreach (CharacterCosmeticsDefinition characterCosmeticsDefinition in this.cosmetics)
					{
						this.definitionMap.Add(characterCosmeticsDefinition.AssetId, characterCosmeticsDefinition);
					}
				}
				return this.definitionMap;
			}
		}

		// Token: 0x17000078 RID: 120
		public CharacterCosmeticsDefinition this[SerializableGuid assetId]
		{
			get
			{
				return this.DefinitionMap[assetId];
			}
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0000D80E File Offset: 0x0000BA0E
		public bool TryGetDefinition(SerializableGuid assetId, out CharacterCosmeticsDefinition definition)
		{
			if (this.DefinitionMap.ContainsKey(assetId))
			{
				definition = this.definitionMap[assetId];
				return true;
			}
			definition = null;
			return false;
		}

		// Token: 0x0600026D RID: 621 RVA: 0x0000D834 File Offset: 0x0000BA34
		public static bool CharacterCosmeticsDefinitionIsMissingData(CharacterCosmeticsDefinition characterCosmeticsDefinition)
		{
			return characterCosmeticsDefinition.DisplayName.IsNullOrEmptyOrWhiteSpace() || characterCosmeticsDefinition.AssetId.IsEmpty || characterCosmeticsDefinition.IsMissingAsset || characterCosmeticsDefinition.PortraitSprite == null;
		}

		// Token: 0x04000258 RID: 600
		[SerializeField]
		private List<CharacterCosmeticsDefinition> cosmetics = new List<CharacterCosmeticsDefinition>();

		// Token: 0x04000259 RID: 601
		private Dictionary<SerializableGuid, CharacterCosmeticsDefinition> definitionMap;
	}
}
