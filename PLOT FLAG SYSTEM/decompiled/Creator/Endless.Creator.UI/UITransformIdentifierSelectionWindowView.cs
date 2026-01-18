using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Props;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITransformIdentifierSelectionWindowView : UIBaseWindowView
{
	[Header("UITransformIdentifierSelectionWindowView")]
	[SerializeField]
	private UITransformIdentifierListModel transformIdentifierListModel;

	public Action<TransformIdentifier> OnTransformIdentifierSelect { get; private set; }

	public static UITransformIdentifierSelectionWindowView Display(UITransformIdentifier[] transformIdentifiers, Action<UITransformIdentifier> onTransformIdentifierSelect, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object>
		{
			{ "transformIdentifiers", transformIdentifiers },
			{ "onTransformIdentifierSelect", onTransformIdentifierSelect }
		};
		return (UITransformIdentifierSelectionWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UITransformIdentifierSelectionWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		transformIdentifierListModel.Clear(triggerEvents: false);
		UITransformIdentifier[] source = (UITransformIdentifier[])supplementalData["transformIdentifiers"];
		OnTransformIdentifierSelect = (Action<TransformIdentifier>)supplementalData["transformIdentifiers".CapitalizeFirstCharacter()];
		transformIdentifierListModel.Set(source.ToList(), triggerEvents: true);
	}
}
