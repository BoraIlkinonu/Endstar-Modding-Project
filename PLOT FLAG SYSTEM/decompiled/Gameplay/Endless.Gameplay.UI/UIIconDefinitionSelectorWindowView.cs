using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIIconDefinitionSelectorWindowView : UIBaseWindowView
{
	[Header("UIIconDefinitionSelectorWindowView")]
	[SerializeField]
	private UIIconDefinitionSelector selector;

	public Action<IconDefinition> OnSelect { get; private set; }

	public static UIIconDefinitionSelectorWindowView Display(IconDefinition currentSelection, Action<IconDefinition> onSelect, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{ "currentSelection", currentSelection },
			{ "onSelect", onSelect }
		};
		return (UIIconDefinitionSelectorWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIIconDefinitionSelectorWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		IconDefinition selection = (IconDefinition)supplementalData["currentSelection"];
		Action<IconDefinition> onSelect = (Action<IconDefinition>)supplementalData["onSelect"];
		selector.Initialize(selection, triggerOnSelected: false);
		OnSelect = onSelect;
	}
}
