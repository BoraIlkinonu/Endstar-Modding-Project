using Endless.Gameplay.UI;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UILevelLoaderModalView))]
public class UILevelLoaderModalController : UIGameObject
{
	[SerializeField]
	private UIButton loadLevelButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UILevelLoaderModalView view;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		loadLevelButton.onClick.AddListener(LoadLevel);
		TryGetComponent<UILevelLoaderModalView>(out view);
	}

	private void LoadLevel()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "LoadLevel");
		}
		string gameId = MatchmakingClientController.Instance.ActiveGameId.ToString();
		MonoBehaviourSingleton<UIStartMatchHelper>.Instance.TryToStartMatch(gameId, null, view.LevelId, MainMenuGameContext.Edit);
	}
}
