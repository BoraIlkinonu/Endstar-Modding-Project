using System;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay;

[Serializable]
public class CharacterVisualsReference : AssetLibraryReferenceClass
{
	internal SerializableGuid AssetId => Id;

	internal CharacterCosmeticsDefinition GetDefinition()
	{
		if (Id.IsEmpty)
		{
			return null;
		}
		return MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmetics[Id];
	}

	public override string ToString()
	{
		return base.ToString() + " | " + ((GetDefinition() == null) ? "null" : GetDefinition().DisplayName);
	}
}
