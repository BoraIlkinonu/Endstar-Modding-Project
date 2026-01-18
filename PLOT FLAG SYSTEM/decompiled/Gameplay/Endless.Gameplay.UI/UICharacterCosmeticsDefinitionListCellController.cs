using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionListCellController : UIBaseListCellController<CharacterCosmeticsDefinition>
{
	[Header("UICharacterCosmeticsDefinitionListCellController")]
	[SerializeField]
	private UICharacterCosmeticsDefinitionPortraitController characterCosmeticsDefinitionPortraitController;

	protected override void Start()
	{
		base.Start();
		characterCosmeticsDefinitionPortraitController.SelectUnityEvent.AddListener(OnSelected);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	private void OnSelected(CharacterCosmeticsDefinition characterCosmeticsDefinition)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelected", characterCosmeticsDefinition.DisplayName);
		}
		base.ListModel.Select(base.DataIndex, triggerEvents: true);
	}
}
