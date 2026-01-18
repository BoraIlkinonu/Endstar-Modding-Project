using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryModerationWindowView : UIBaseWindowView
{
	[Header("UIGameLibraryModerationWindowView")]
	[SerializeField]
	private UIGameLibraryListModel gameLibraryListModel;

	public static UIGameLibraryModerationWindowView Display(Transform parent = null)
	{
		return (UIGameLibraryModerationWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIGameLibraryModerationWindowView>(parent);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		gameLibraryListModel.Synchronize();
	}

	public override void Close()
	{
		base.Close();
		gameLibraryListModel.Clear(triggerEvents: true);
	}
}
