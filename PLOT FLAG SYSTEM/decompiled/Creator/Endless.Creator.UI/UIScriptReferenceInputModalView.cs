using System.Collections.Generic;
using Endless.Props;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScriptReferenceInputModalView : UIScriptModalView
{
	[Header("UIScriptReferenceInputModalView")]
	[SerializeField]
	private UIInputField nameInCodeInputField;

	[SerializeField]
	private UITransformIdentifierListModel transformIdentifierListModel;

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		nameInCodeInputField.Clear();
		UIBaseWindowView displayed = MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed;
		if (displayed == null)
		{
			DebugUtility.LogError("displayedWindow is null!", this);
			Close();
		}
		UIScriptWindowView obj = (UIScriptWindowView)displayed;
		if (obj == null)
		{
			DebugUtility.LogError("displayedWindow is not a type of UIScriptWindowView!", this);
			Close();
		}
		TransformIdentifier[] componentsInChildren = obj.Model.GetRuntimePropInfo().EndlessProp.gameObject.GetComponentsInChildren<TransformIdentifier>(includeInactive: true);
		List<UITransformIdentifier> list = new List<UITransformIdentifier>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			TransformIdentifier transformIdentifier = componentsInChildren[i];
			string text = transformIdentifier.gameObject.name;
			if (i == 0)
			{
				text = text.Replace("(Clone)", string.Empty);
			}
			UITransformIdentifier item = new UITransformIdentifier(transformIdentifier, text);
			list.Add(item);
		}
		transformIdentifierListModel.Set(list, triggerEvents: true);
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}
}
