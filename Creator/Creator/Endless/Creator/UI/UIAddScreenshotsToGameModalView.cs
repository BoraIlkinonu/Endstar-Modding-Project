using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000283 RID: 643
	public class UIAddScreenshotsToGameModalView : UIEscapableModalView
	{
		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000A9A RID: 2714 RVA: 0x00031B2E File Offset: 0x0002FD2E
		// (set) Token: 0x06000A9B RID: 2715 RVA: 0x00031B36 File Offset: 0x0002FD36
		public UIAddScreenshotsToGameModalModel Model { get; private set; }

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000A9C RID: 2716 RVA: 0x00031B3F File Offset: 0x0002FD3F
		public UnityEvent<List<ScreenshotFileInstances>> OnScreenshotsToAdded { get; } = new UnityEvent<List<ScreenshotFileInstances>>();

		// Token: 0x06000A9D RID: 2717 RVA: 0x00031B48 File Offset: 0x0002FD48
		protected override void Start()
		{
			base.Start();
			this.Model.SynchronizedUnityEvent.AddListener(new UnityAction(this.OnScreenshotsToAddChangedAction));
			UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction = (Action)Delegate.Combine(UIAddScreenshotsToGameModalModel.ScreenshotsToAddChangedAction, new Action(this.OnScreenshotsToAddChangedAction));
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x00031B98 File Offset: 0x0002FD98
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			UnityAction<List<ScreenshotFileInstances>> unityAction = (UnityAction<List<ScreenshotFileInstances>>)modalData[0];
			this.OnScreenshotsToAdded.AddListener(unityAction);
			this.doneButton.interactable = false;
			this.selectedText.text = "Loading...";
			this.Model.Synchronize();
		}

		// Token: 0x06000A9F RID: 2719 RVA: 0x00031BE8 File Offset: 0x0002FDE8
		public override void Close()
		{
			base.Close();
			this.Model.Clear();
			this.OnScreenshotsToAdded.RemoveAllListeners();
		}

		// Token: 0x06000AA0 RID: 2720 RVA: 0x00031C08 File Offset: 0x0002FE08
		private void OnScreenshotsToAddChangedAction()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScreenshotsToAddChangedAction", Array.Empty<object>());
			}
			int num = Mathf.Clamp(this.Model.Game.Screenshots.Count, 0, this.screenshotLimit.Value);
			int num2 = Mathf.Clamp(this.screenshotLimit.Value - num, 0, this.screenshotLimit.Value);
			List<ScreenshotFileInstances> screenshotsToAdd = this.Model.ScreenshotsToAdd;
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "screenshotCount", num), this);
				DebugUtility.Log(string.Format("{0}: {1}", "unusedScreenshots", num2), this);
				DebugUtility.Log(string.Format("{0}: {1}", "screenshotsToAdd", screenshotsToAdd.Count), this);
			}
			this.doneButton.interactable = screenshotsToAdd.Count <= num2;
			this.selectedText.text = string.Format("{0}/{1}", screenshotsToAdd.Count, num2);
		}

		// Token: 0x040008E5 RID: 2277
		[SerializeField]
		private IntVariable screenshotLimit;

		// Token: 0x040008E6 RID: 2278
		[SerializeField]
		private UIButton doneButton;

		// Token: 0x040008E7 RID: 2279
		[SerializeField]
		private TextMeshProUGUI selectedText;
	}
}
