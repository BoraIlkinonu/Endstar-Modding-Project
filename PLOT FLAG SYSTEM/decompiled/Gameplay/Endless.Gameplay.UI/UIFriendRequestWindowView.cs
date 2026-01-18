using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIFriendRequestWindowView : UIBaseWindowView
{
	[Header("UIFriendRequestWindowView")]
	[SerializeField]
	private UISpriteAndEnumTabGroup tabs;

	[SerializeField]
	private UIIEnumerablePresenter receivedFriendRequests;

	[SerializeField]
	private UIIEnumerablePresenter sentFriendRequests;

	public UIFriendRequestWindowModel Model { get; set; }

	public static UIFriendRequestWindowView Display(UIFriendRequestWindowModel model, Transform parent = null)
	{
		Dictionary<string, object> supplementalData = new Dictionary<string, object> { { "model", model } };
		return (UIFriendRequestWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIFriendRequestWindowView>(parent, supplementalData);
	}

	public override void Initialize(Dictionary<string, object> supplementalData)
	{
		base.Initialize(supplementalData);
		Model = (UIFriendRequestWindowModel)supplementalData["Model".ToLower()];
		if (!tabs.PopulatedFromEnum)
		{
			tabs.PopulateFromEnum(UIFriendRequestType.Received, triggerOnValueChanged: true);
		}
		View(Model.ReceivedFriendRequests, Model.SentFriendRequests);
	}

	public override void Close()
	{
		base.Close();
		sentFriendRequests.Clear();
		receivedFriendRequests.Clear();
	}

	public void View(IReadOnlyList<FriendRequest> receivedFriendRequestsList, IReadOnlyList<FriendRequest> sentFriendRequestsList)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", receivedFriendRequestsList.Count, sentFriendRequestsList.Count);
		}
		receivedFriendRequests.SetModel(receivedFriendRequestsList, triggerOnModelChanged: false);
		sentFriendRequests.SetModel(sentFriendRequestsList, triggerOnModelChanged: false);
		tabs.SetTabBadge(0, receivedFriendRequests.Count.ToString());
		tabs.SetTabBadge(1, sentFriendRequestsList.Count.ToString());
	}
}
