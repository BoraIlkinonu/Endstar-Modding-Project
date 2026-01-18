using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionSelectorWindowController : UIDraggableWindowController
{
	[Header("UICharacterCosmeticsDefinitionSelectorWindowController")]
	[SerializeField]
	private UICharacterCosmeticsDefinitionSelectorWindowView view;

	[SerializeField]
	private UICharacterCosmeticsDefinitionSelector characterCosmeticsDefinitionSelector;

	protected override void Start()
	{
		base.Start();
		characterCosmeticsDefinitionSelector.OnSelectedId.AddListener(SetCharacterCosmeticsDefinition);
	}

	private void SetCharacterCosmeticsDefinition(SerializableGuid characterCosmeticsDefinitionAssetId)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetCharacterCosmeticsDefinition", characterCosmeticsDefinitionAssetId);
		}
		view.SelectAction?.Invoke(characterCosmeticsDefinitionAssetId);
	}
}
