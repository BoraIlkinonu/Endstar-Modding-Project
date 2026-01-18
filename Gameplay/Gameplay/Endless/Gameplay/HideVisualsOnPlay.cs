using System;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000366 RID: 870
	public class HideVisualsOnPlay : EndlessBehaviour, IGameEndSubscriber, IStartSubscriber
	{
		// Token: 0x06001659 RID: 5721 RVA: 0x0006926A File Offset: 0x0006746A
		protected override void Start()
		{
			base.Start();
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.AddListener(new UnityAction(this.ScreenshotStarted));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(new UnityAction(this.ScreenshotFinished));
		}

		// Token: 0x0600165A RID: 5722 RVA: 0x000692A8 File Offset: 0x000674A8
		protected override void OnDestroy()
		{
			base.OnDestroy();
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.RemoveListener(new UnityAction(this.ScreenshotStarted));
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.RemoveListener(new UnityAction(this.ScreenshotFinished));
		}

		// Token: 0x0600165B RID: 5723 RVA: 0x000692E6 File Offset: 0x000674E6
		private void ScreenshotStarted()
		{
			this.SetVisuals(false);
		}

		// Token: 0x0600165C RID: 5724 RVA: 0x000692EF File Offset: 0x000674EF
		private void ScreenshotFinished()
		{
			this.SetVisuals(true);
		}

		// Token: 0x0600165D RID: 5725 RVA: 0x000692F8 File Offset: 0x000674F8
		private void Reset()
		{
			if (this.renderersToManage == null || this.renderersToManage.Length == 0)
			{
				this.renderersToManage = base.GetComponentsInChildren<Renderer>();
			}
		}

		// Token: 0x0600165E RID: 5726 RVA: 0x000692E6 File Offset: 0x000674E6
		public void EndlessStart()
		{
			this.SetVisuals(false);
		}

		// Token: 0x0600165F RID: 5727 RVA: 0x000692EF File Offset: 0x000674EF
		public void EndlessGameEnd()
		{
			this.SetVisuals(true);
		}

		// Token: 0x06001660 RID: 5728 RVA: 0x00069318 File Offset: 0x00067518
		private void SetVisuals(bool enabled)
		{
			Renderer[] array = this.renderersToManage;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = enabled;
			}
		}

		// Token: 0x04001213 RID: 4627
		[SerializeField]
		private Renderer[] renderersToManage;
	}
}
