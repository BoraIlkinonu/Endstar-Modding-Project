using System;
using System.Runtime.CompilerServices;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Gameplay.LevelEditing;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001B3 RID: 435
	[RequireComponent(typeof(UIGameLibraryAssetReplacementConfirmationModalView))]
	public class UIGameLibraryAssetReplacementConfirmationModalController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000672 RID: 1650 RVA: 0x000216E4 File Offset: 0x0001F8E4
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000673 RID: 1651 RVA: 0x000216EC File Offset: 0x0001F8EC
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000674 RID: 1652 RVA: 0x000216F4 File Offset: 0x0001F8F4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.noButton.onClick.AddListener(new UnityAction(this.Cancel));
			this.yesButton.onClick.AddListener(new UnityAction(this.RemoveAsset));
			base.TryGetComponent<UIGameLibraryAssetReplacementConfirmationModalView>(out this.view);
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x0002175E File Offset: 0x0001F95E
		private void Cancel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Cancel", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x00021784 File Offset: 0x0001F984
		private async void RemoveAsset()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "RemoveAsset", "ToRemove: " + this.view.ToRemove.AssetID + ", ToReplace: " + this.view.ToReplace.AssetID, Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			TaskAwaiter<GameLibrary.RemoveTerrainEntryResult> taskAwaiter = MonoBehaviourSingleton<GameEditor>.Instance.RemoveTerrainEntryFromGameLibrary(this.view.ToRemove.AssetID, this.view.ToReplace.AssetID).GetAwaiter();
			if (!taskAwaiter.IsCompleted)
			{
				await taskAwaiter;
				TaskAwaiter<GameLibrary.RemoveTerrainEntryResult> taskAwaiter2;
				taskAwaiter = taskAwaiter2;
				taskAwaiter2 = default(TaskAwaiter<GameLibrary.RemoveTerrainEntryResult>);
			}
			if (taskAwaiter.GetResult().Success)
			{
				if (this.verboseLogging)
				{
					DebugUtility.Log("Removal success!", this);
				}
			}
			else
			{
				DebugUtility.LogError(string.Concat(new string[]
				{
					"Could not replace ",
					this.view.ToRemove.AssetID,
					" with ",
					this.view.ToReplace.AssetID,
					"!"
				}), this);
			}
			this.OnLoadingEnded.Invoke();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x040005CA RID: 1482
		[SerializeField]
		private UIGameLibraryAssetReplacementConfirmationModalView view;

		// Token: 0x040005CB RID: 1483
		[SerializeField]
		private UIButton noButton;

		// Token: 0x040005CC RID: 1484
		[SerializeField]
		private UIButton yesButton;

		// Token: 0x040005CD RID: 1485
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
