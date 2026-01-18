using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000260 RID: 608
	public class UIGameAssetVersionNotificationCountView : UIGameObject
	{
		// Token: 0x060009E3 RID: 2531 RVA: 0x0002D894 File Offset: 0x0002BA94
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.OnNumberOfNotificationsChanged.AddListener(new UnityAction<int>(this.View));
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			if (activeGame == null)
			{
				this.View(0);
				return;
			}
			int numberOfNotifications = MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.GetNumberOfNotifications(activeGame);
			this.View(numberOfNotifications);
		}

		// Token: 0x060009E4 RID: 2532 RVA: 0x0002D900 File Offset: 0x0002BB00
		private void View(int count)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "count", count), this);
			}
			this.countText.text = ((count > 99) ? string.Format("{0}+", count) : count.ToString());
			base.gameObject.SetActive(count > 0);
		}

		// Token: 0x0400081B RID: 2075
		[SerializeField]
		private TextMeshProUGUI countText;

		// Token: 0x0400081C RID: 2076
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
