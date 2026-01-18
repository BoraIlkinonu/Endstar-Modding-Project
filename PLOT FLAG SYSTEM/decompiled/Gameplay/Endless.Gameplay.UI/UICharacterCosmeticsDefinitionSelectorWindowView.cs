using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionSelectorWindowView : UIBaseWindowView
{
	[Header("UICharacterCosmeticsDefinitionSelectorWindowView")]
	[SerializeField]
	private UICharacterCosmeticsDefinitionSelector characterCosmeticsDefinitionSelector;

	public Action<SerializableGuid> SelectAction { get; private set; }

	public static UIBaseWindowView Display(SerializableGuid selectedClientCharacterVisualId, Action<SerializableGuid> selectAction = null, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{ "selectedClientCharacterVisualId", selectedClientCharacterVisualId },
			{ "SelectAction", selectAction }
		};
		return MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UICharacterCosmeticsDefinitionSelectorWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		SerializableGuid assetId = (SerializableGuid)supplementalData["selectedClientCharacterVisualId"];
		SelectAction = (Action<SerializableGuid>)supplementalData["SelectAction"];
		characterCosmeticsDefinitionSelector.SetSelected(assetId, triggerOnSelected: false);
	}
}
