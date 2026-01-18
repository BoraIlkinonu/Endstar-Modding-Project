using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAssetVersionNotificationCountView : UIGameObject
{
	[SerializeField]
	private TextMeshProUGUI countText;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.OnNumberOfNotificationsChanged.AddListener(View);
		Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
		if (activeGame == null)
		{
			View(0);
			return;
		}
		int numberOfNotifications = MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.GetNumberOfNotifications(activeGame);
		View(numberOfNotifications);
	}

	private void View(int count)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "count", count), this);
		}
		countText.text = ((count > 99) ? $"{count}+" : count.ToString());
		base.gameObject.SetActive(count > 0);
	}
}
