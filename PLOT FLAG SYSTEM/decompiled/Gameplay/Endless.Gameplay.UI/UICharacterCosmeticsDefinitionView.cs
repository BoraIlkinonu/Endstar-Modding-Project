using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionView : UIBaseView<CharacterCosmeticsDefinition, UICharacterCosmeticsDefinitionView.Styles>
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

	[field: Header("UICharacterCosmeticsDefinitionView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(CharacterCosmeticsDefinition model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model.DisplayName);
		}
		characterCosmeticsDefinitionPortrait.Display(model.AssetId);
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}
}
