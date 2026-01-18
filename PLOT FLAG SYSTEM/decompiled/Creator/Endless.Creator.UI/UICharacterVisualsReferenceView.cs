using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.UI;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.UI;

public class UICharacterVisualsReferenceView : UIBaseAssetLibraryReferenceClassView<CharacterVisualsReference, UICharacterVisualsReferenceView.Styles>
{
	public enum Styles
	{
		DefaultReadWrite,
		DefaultReadOnly
	}

	[SerializeField]
	private CharacterCosmeticsList characterCosmeticsList;

	[SerializeField]
	private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

	[field: Header("UICharacterVisualsReferenceView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(CharacterVisualsReference model)
	{
		base.View(model);
		SerializableGuid id = InspectorReferenceUtility.GetId(model);
		characterCosmeticsDefinitionPortrait.Display(id);
	}

	protected override string GetReferenceName(CharacterVisualsReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetReferenceName", model);
		}
		if (!model.IsReferenceEmpty())
		{
			return "None";
		}
		SerializableGuid assetId = InspectorReferenceUtility.GetId(model);
		CharacterCosmeticsDefinition characterCosmeticsDefinition = characterCosmeticsList.Cosmetics.FirstOrDefault((CharacterCosmeticsDefinition item) => item.AssetId == assetId);
		if ((bool)characterCosmeticsDefinition)
		{
			return characterCosmeticsDefinition.DisplayName;
		}
		return "Missing";
	}
}
