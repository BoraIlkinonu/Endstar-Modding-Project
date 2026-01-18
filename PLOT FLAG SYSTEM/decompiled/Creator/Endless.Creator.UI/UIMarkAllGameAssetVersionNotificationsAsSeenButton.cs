using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIMarkAllGameAssetVersionNotificationsAsSeenButton : UIGameObject
{
	[SerializeField]
	private UIButton button;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(Initialize);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(Uninitialize);
		button.onClick.AddListener(MarkAllSeen);
	}

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
		int numberOfNotifications = MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.GetNumberOfNotifications(activeGame);
		HandleButtonInteractability(numberOfNotifications);
	}

	private void Initialize()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize");
		}
		MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.OnNumberOfNotificationsChanged.AddListener(HandleButtonInteractability);
		Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
		int numberOfNotifications = MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.GetNumberOfNotifications(activeGame);
		HandleButtonInteractability(numberOfNotifications);
	}

	private void Uninitialize()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Uninitialize");
		}
		MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.OnNumberOfNotificationsChanged.RemoveListener(HandleButtonInteractability);
	}

	private void HandleButtonInteractability(int count)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "HandleButtonInteractability", "count", count), this);
		}
		button.interactable = count > 0;
	}

	private void MarkAllSeen()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "MarkAllSeen");
		}
		Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
		MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.MarkAllSeen(activeGame);
		button.interactable = false;
	}
}
