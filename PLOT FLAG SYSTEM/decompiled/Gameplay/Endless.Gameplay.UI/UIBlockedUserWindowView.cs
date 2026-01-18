using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIBlockedUserWindowView : UIBaseWindowView
{
	[Header("UIBlockedUserWindowView")]
	[SerializeField]
	private UIIEnumerablePresenter blockedUsers;

	public UIBlockedUserWindowModel Model { get; set; }

	public static UIBlockedUserWindowView Display(UIBlockedUserWindowModel model, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object> { { "model", model } };
		return (UIBlockedUserWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIBlockedUserWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		Model = (UIBlockedUserWindowModel)supplementalData["Model".ToLower()];
		blockedUsers.SetModel(Model.BlockedUsers, triggerOnModelChanged: true);
	}

	public override void Close()
	{
		base.Close();
		blockedUsers.Clear();
	}
}
