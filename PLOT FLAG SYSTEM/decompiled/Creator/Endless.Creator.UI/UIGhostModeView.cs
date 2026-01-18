using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGhostModeView : UIGameObject
{
	[SerializeField]
	private TweenCollection trueTweenCollection;

	[SerializeField]
	private TweenCollection falseTweenCollection;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private PlayerNetworkController playerNetworkController;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		MonoBehaviourSingleton<PlayerManager>.Instance.OnOwnerRegistered.AddListener(OnOwnerRegistered);
	}

	private void OnOwnerRegistered(ulong clientId, PlayerReferenceManager playerReferenceManager)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnOwnerRegistered", clientId, playerReferenceManager.IsOwner);
		}
		playerNetworkController = playerReferenceManager.PlayerNetworkController;
		playerNetworkController.GhostChangedUnityEvent.AddListener(OnGhostModeChanged);
		View();
	}

	private void View()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View");
		}
		if (playerNetworkController.Ghost)
		{
			trueTweenCollection.Tween();
		}
		else
		{
			falseTweenCollection.Tween();
		}
	}

	private void OnGhostModeChanged(bool state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGhostModeChanged", state);
		}
		View();
	}
}
