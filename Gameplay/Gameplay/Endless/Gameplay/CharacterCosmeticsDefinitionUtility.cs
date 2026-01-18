using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002D0 RID: 720
	public static class CharacterCosmeticsDefinitionUtility
	{
		// Token: 0x06001057 RID: 4183 RVA: 0x00052D8B File Offset: 0x00050F8B
		public static SerializableGuid GetClientCharacterVisualId()
		{
			if (!PlayerPrefs.HasKey("Character Visual"))
			{
				SerializableGuid empty = SerializableGuid.Empty;
				CharacterCosmeticsDefinitionUtility.SetClientCharacterVisualId(empty);
				return empty;
			}
			return PlayerPrefs.GetString("Character Visual");
		}

		// Token: 0x06001058 RID: 4184 RVA: 0x00052DB4 File Offset: 0x00050FB4
		public static void SetClientCharacterVisualId(SerializableGuid characterCosmeticsDefinitionAssetId)
		{
			PlayerPrefs.SetString("Character Visual", characterCosmeticsDefinitionAssetId);
			Action<SerializableGuid> clientCharacterCosmeticsDefinitionAssetSetAction = CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction;
			if (clientCharacterCosmeticsDefinitionAssetSetAction == null)
			{
				return;
			}
			clientCharacterCosmeticsDefinitionAssetSetAction(characterCosmeticsDefinitionAssetId);
		}

		// Token: 0x04000E08 RID: 3592
		public static Action<SerializableGuid> ClientCharacterCosmeticsDefinitionAssetSetAction;
	}
}
