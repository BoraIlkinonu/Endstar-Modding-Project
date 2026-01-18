using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

[DisallowMultipleComponent]
public class UIModalMatchCloseHandler : UIGameObject
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		MatchSession.OnMatchSessionClose += OnMatchClosed;
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		MatchSession.OnMatchSessionClose -= OnMatchClosed;
	}

	private void OnMatchClosed(MatchSession _)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnMatchClosed");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
	}
}
