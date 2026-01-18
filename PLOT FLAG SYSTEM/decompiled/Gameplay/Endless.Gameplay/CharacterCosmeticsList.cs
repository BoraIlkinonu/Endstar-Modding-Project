using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public class CharacterCosmeticsList : ScriptableObject
{
	[SerializeField]
	private List<CharacterCosmeticsDefinition> cosmetics = new List<CharacterCosmeticsDefinition>();

	private Dictionary<SerializableGuid, CharacterCosmeticsDefinition> definitionMap;

	public IReadOnlyList<CharacterCosmeticsDefinition> Cosmetics => cosmetics;

	private Dictionary<SerializableGuid, CharacterCosmeticsDefinition> DefinitionMap
	{
		get
		{
			if (definitionMap == null)
			{
				definitionMap = new Dictionary<SerializableGuid, CharacterCosmeticsDefinition>();
				foreach (CharacterCosmeticsDefinition cosmetic in cosmetics)
				{
					definitionMap.Add(cosmetic.AssetId, cosmetic);
				}
			}
			return definitionMap;
		}
	}

	public CharacterCosmeticsDefinition this[SerializableGuid assetId] => DefinitionMap[assetId];

	public bool TryGetDefinition(SerializableGuid assetId, out CharacterCosmeticsDefinition definition)
	{
		if (DefinitionMap.ContainsKey(assetId))
		{
			definition = definitionMap[assetId];
			return true;
		}
		definition = null;
		return false;
	}

	public static bool CharacterCosmeticsDefinitionIsMissingData(CharacterCosmeticsDefinition characterCosmeticsDefinition)
	{
		if (!characterCosmeticsDefinition.DisplayName.IsNullOrEmptyOrWhiteSpace() && !characterCosmeticsDefinition.AssetId.IsEmpty && !characterCosmeticsDefinition.IsMissingAsset)
		{
			return characterCosmeticsDefinition.PortraitSprite == null;
		}
		return true;
	}
}
