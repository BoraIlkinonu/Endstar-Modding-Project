using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000B4 RID: 180
	public class UIMarkAllGameAssetVersionNotificationsAsSeenButton : UIGameObject
	{
		// Token: 0x060002CC RID: 716 RVA: 0x000128E4 File Offset: 0x00010AE4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.Initialize));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(this.Uninitialize));
			this.button.onClick.AddListener(new UnityAction(this.MarkAllSeen));
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0001295C File Offset: 0x00010B5C
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			int numberOfNotifications = MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.GetNumberOfNotifications(activeGame);
			this.HandleButtonInteractability(numberOfNotifications);
		}

		// Token: 0x060002CE RID: 718 RVA: 0x000129A0 File Offset: 0x00010BA0
		private void Initialize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.OnNumberOfNotificationsChanged.AddListener(new UnityAction<int>(this.HandleButtonInteractability));
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			int numberOfNotifications = MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.GetNumberOfNotifications(activeGame);
			this.HandleButtonInteractability(numberOfNotifications);
		}

		// Token: 0x060002CF RID: 719 RVA: 0x000129FE File Offset: 0x00010BFE
		private void Uninitialize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Uninitialize", Array.Empty<object>());
			}
			MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.OnNumberOfNotificationsChanged.RemoveListener(new UnityAction<int>(this.HandleButtonInteractability));
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x00012A33 File Offset: 0x00010C33
		private void HandleButtonInteractability(int count)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "HandleButtonInteractability", "count", count), this);
			}
			this.button.interactable = count > 0;
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x00012A6C File Offset: 0x00010C6C
		private void MarkAllSeen()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "MarkAllSeen", Array.Empty<object>());
			}
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.MarkAllSeen(activeGame);
			this.button.interactable = false;
		}

		// Token: 0x04000301 RID: 769
		[SerializeField]
		private UIButton button;

		// Token: 0x04000302 RID: 770
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
