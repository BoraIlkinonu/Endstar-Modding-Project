using System;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI;

public class UIFriendRequestPresenter : UIBasePresenter<FriendRequest>
{
	private UIFriendRequestView typedView;

	public static event Action<FriendRequest> OnAcceptFriendRequest;

	public static event Action<FriendRequest> OnRejectFriendRequest;

	public static event Action<FriendRequest> OnCancelFriendRequest;

	protected override void Start()
	{
		base.Start();
		typedView = base.View.Interface as UIFriendRequestView;
		typedView.AcceptFriendRequest += AcceptFriendRequest;
		typedView.RejectFriendRequest += RejectFriendRequest;
		typedView.CancelFriendRequest += CancelFriendRequest;
	}

	private void OnDestroy()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		typedView.AcceptFriendRequest -= AcceptFriendRequest;
		typedView.RejectFriendRequest -= RejectFriendRequest;
		typedView.CancelFriendRequest -= CancelFriendRequest;
	}

	private void AcceptFriendRequest()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "AcceptFriendRequest");
		}
		EndlessServices.Instance.CloudService.AcceptFriendRequestAsync(base.Model.RequestId, base.VerboseLogging);
		UIFriendRequestPresenter.OnAcceptFriendRequest?.Invoke(base.Model);
	}

	private void RejectFriendRequest()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "RejectFriendRequest");
		}
		EndlessServices.Instance.CloudService.RejectFriendRequestAsync(base.Model.RequestId, base.VerboseLogging);
		UIFriendRequestPresenter.OnRejectFriendRequest?.Invoke(base.Model);
	}

	private void CancelFriendRequest()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CancelFriendRequest");
		}
		EndlessServices.Instance.CloudService.CancelFriendRequestAsync(base.Model.RequestId, base.VerboseLogging);
		UIFriendRequestPresenter.OnCancelFriendRequest?.Invoke(base.Model);
	}
}
