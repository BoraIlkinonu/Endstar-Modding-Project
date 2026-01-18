using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIFriendRequestView : UIBaseSocialView<FriendRequest>
{
	[Header("UIFriendRequestView")]
	[SerializeField]
	private UIUserView userView;

	[SerializeField]
	private UIRectTransformDictionary userNameRectTransformDictionary;

	[SerializeField]
	private UIButton acceptFriendRequestButton;

	[SerializeField]
	private UIButton rejectFriendRequestButton;

	[SerializeField]
	private UIButton cancelFriendRequestButton;

	public event Action AcceptFriendRequest;

	public event Action RejectFriendRequest;

	public event Action CancelFriendRequest;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		acceptFriendRequestButton.onClick.AddListener(InvokeAcceptFriendRequest);
		rejectFriendRequestButton.onClick.AddListener(InvokeRejectFriendRequest);
		cancelFriendRequestButton.onClick.AddListener(InvokeCancelFriendRequest);
	}

	public override void View(FriendRequest model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		User model2 = UISocialUtility.ExtractNonActiveUser(model);
		userView.View(model2);
		UIFriendRequestType uIFriendRequestType = ((model.Sender.Id == EndlessServices.Instance.CloudService.ActiveUserId) ? UIFriendRequestType.Sent : UIFriendRequestType.Received);
		userNameRectTransformDictionary.Apply(uIFriendRequestType.ToString());
		acceptFriendRequestButton.gameObject.SetActive(uIFriendRequestType == UIFriendRequestType.Received);
		rejectFriendRequestButton.gameObject.SetActive(uIFriendRequestType == UIFriendRequestType.Received);
		cancelFriendRequestButton.gameObject.SetActive(uIFriendRequestType == UIFriendRequestType.Sent);
	}

	private void InvokeAcceptFriendRequest()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeAcceptFriendRequest");
		}
		this.AcceptFriendRequest?.Invoke();
	}

	private void InvokeRejectFriendRequest()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeRejectFriendRequest");
		}
		this.RejectFriendRequest?.Invoke();
	}

	private void InvokeCancelFriendRequest()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeCancelFriendRequest");
		}
		this.CancelFriendRequest?.Invoke();
	}
}
