using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200027E RID: 638
	public class UIAddScreenshotsToGameModalController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000A83 RID: 2691 RVA: 0x0003138A File Offset: 0x0002F58A
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000A84 RID: 2692 RVA: 0x00031392 File Offset: 0x0002F592
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000A85 RID: 2693 RVA: 0x0003139A File Offset: 0x0002F59A
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.doneButton.onClick.AddListener(new UnityAction(this.Done));
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x000313D0 File Offset: 0x0002F5D0
		private async void Done()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Done", Array.Empty<object>());
			}
			if (this.model.ScreenshotsToAdd.Count == 0)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			}
			else
			{
				this.OnLoadingStarted.Invoke();
				bool flag = await MonoBehaviourSingleton<GameEditor>.Instance.AddScreenshotsToGame(this.model.ScreenshotsToAdd);
				this.OnLoadingEnded.Invoke();
				if (flag)
				{
					this.view.OnScreenshotsToAdded.Invoke(this.model.ScreenshotsToAdd);
					MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				}
			}
		}

		// Token: 0x040008C8 RID: 2248
		[SerializeField]
		private UIAddScreenshotsToGameModalModel model;

		// Token: 0x040008C9 RID: 2249
		[SerializeField]
		private UIAddScreenshotsToGameModalView view;

		// Token: 0x040008CA RID: 2250
		[SerializeField]
		private UIButton doneButton;

		// Token: 0x040008CB RID: 2251
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
