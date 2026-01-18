using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UICharacterVisualsReferencePresenter : UIBaseAssetLibraryReferenceClassPresenter<CharacterVisualsReference>
{
	[Header("UICharacterVisualsReferencePresenter")]
	[SerializeField]
	private CharacterCosmeticsList characterCosmeticsList;

	[SerializeField]
	private CharacterCosmeticsDefinition defaultCharacterCosmeticsDefinition;

	protected override IEnumerable<object> SelectionOptions => characterCosmeticsList.Cosmetics;

	protected override string IEnumerableWindowTitle => "Select a Character Visual Reference";

	protected override SelectionType SelectionType => SelectionType.MustSelect1;

	protected override void SetSelection(List<object> selection)
	{
		List<object> list = new List<object>();
		foreach (object item2 in selection)
		{
			CharacterVisualsReference item = ReferenceFactory.CreateCharacterVisualsReference((item2 as CharacterCosmeticsDefinition).AssetId);
			list.Add(item);
		}
		base.SetSelection(list);
	}

	protected override CharacterVisualsReference CreateDefaultModel()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateDefaultModel");
		}
		return ReferenceFactory.CreateCharacterVisualsReference(defaultCharacterCosmeticsDefinition.AssetId);
	}

	protected override void UpdateOriginalSelection(CharacterVisualsReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateOriginalSelection", model);
		}
		if (model.IsReferenceEmpty())
		{
			originalSelection.Add(defaultCharacterCosmeticsDefinition);
			return;
		}
		SerializableGuid id = InspectorReferenceUtility.GetId(model);
		CharacterCosmeticsDefinition definition;
		CharacterCosmeticsDefinition item = (characterCosmeticsList.TryGetDefinition(id, out definition) ? definition : defaultCharacterCosmeticsDefinition);
		originalSelection.Add(item);
	}
}
