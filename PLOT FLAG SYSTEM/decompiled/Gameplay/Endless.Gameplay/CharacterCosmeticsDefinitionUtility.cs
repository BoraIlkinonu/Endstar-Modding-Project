using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public static class CharacterCosmeticsDefinitionUtility
{
	public static Action<SerializableGuid> ClientCharacterCosmeticsDefinitionAssetSetAction;

	public static SerializableGuid GetClientCharacterVisualId()
	{
		if (!PlayerPrefs.HasKey("Character Visual"))
		{
			SerializableGuid empty = SerializableGuid.Empty;
			SetClientCharacterVisualId(empty);
			return empty;
		}
		return PlayerPrefs.GetString("Character Visual");
	}

	public static void SetClientCharacterVisualId(SerializableGuid characterCosmeticsDefinitionAssetId)
	{
		PlayerPrefs.SetString("Character Visual", characterCosmeticsDefinitionAssetId);
		ClientCharacterCosmeticsDefinitionAssetSetAction?.Invoke(characterCosmeticsDefinitionAssetId);
	}
}
