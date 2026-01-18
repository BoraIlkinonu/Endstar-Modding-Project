using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionPortraitView : UIGameObject
{
	[SerializeField]
	private Image portraitImage;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private CharacterCosmeticsList characterCosmeticsList;

	[SerializeField]
	private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public CharacterCosmeticsDefinition CharacterCosmeticsDefinition { get; private set; }

	public void Display(SerializableGuid characterCosmeticsDefinitionAssetId)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Display", "characterCosmeticsDefinitionAssetId", characterCosmeticsDefinitionAssetId), this);
		}
		if (characterCosmeticsDefinitionAssetId == SerializableGuid.Empty)
		{
			Display(fallbackCharacterCosmeticsDefinition);
			return;
		}
		if (characterCosmeticsList.TryGetDefinition(characterCosmeticsDefinitionAssetId, out var definition))
		{
			CharacterCosmeticsDefinition = definition;
			Display(CharacterCosmeticsDefinition);
			return;
		}
		DebugUtility.LogWarning(string.Format("could not find {0} of {1} in {2}! Using {3}!", "characterCosmeticsDefinitionAssetId", characterCosmeticsDefinitionAssetId, "characterCosmeticsList", "fallbackCharacterCosmeticsDefinition"), this);
		Display(fallbackCharacterCosmeticsDefinition);
	}

	private void Display(CharacterCosmeticsDefinition characterCosmeticsDefinition)
	{
		if (verboseLogging)
		{
			DebugUtility.Log("Display ( DisplayName: " + characterCosmeticsDefinition.DisplayName + " )", this);
		}
		CharacterCosmeticsDefinition = characterCosmeticsDefinition;
		if ((bool)portraitImage)
		{
			portraitImage.sprite = characterCosmeticsDefinition.PortraitSprite;
		}
		if ((bool)displayNameText)
		{
			displayNameText.text = characterCosmeticsDefinition.DisplayName;
		}
	}
}
