using System;
using System.Collections.Generic;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000288 RID: 648
	public class UIAddScreenshotsToLevelModalView : UIBaseModalView
	{
		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000ABA RID: 2746 RVA: 0x0003260A File Offset: 0x0003080A
		public UnityEvent BackUnityEvent { get; } = new UnityEvent();

		// Token: 0x06000ABB RID: 2747 RVA: 0x00032614 File Offset: 0x00030814
		protected override void Start()
		{
			base.Start();
			UIAddScreenshotsToLevelModalModel.SynchronizedAction = (Action)Delegate.Combine(UIAddScreenshotsToLevelModalModel.SynchronizedAction, new Action(this.OnSynchronized));
			this.inMemoryScreenshotListModel.SelectionChangedUnityEvent.AddListener(new UnityAction<int, bool>(this.OnSelectionChange));
			LayoutRebuilder.MarkLayoutForRebuild(base.RectTransform);
		}

		// Token: 0x06000ABC RID: 2748 RVA: 0x00032670 File Offset: 0x00030870
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			List<ScreenshotAPI.InMemoryScreenShot> list = new List<ScreenshotAPI.InMemoryScreenShot>(MonoBehaviourSingleton<ScreenshotAPI>.Instance.InMemoryScreenshots);
			this.inMemoryScreenshotListModel.Set(list, true);
			this.doneButton.interactable = false;
			this.selectedText.text = "Loading...";
			this.model.Synchronize();
		}

		// Token: 0x06000ABD RID: 2749 RVA: 0x000326C8 File Offset: 0x000308C8
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			this.BackUnityEvent.Invoke();
		}

		// Token: 0x06000ABE RID: 2750 RVA: 0x000326ED File Offset: 0x000308ED
		private void OnSynchronized()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSynchronized", Array.Empty<object>());
			}
			this.ViewSelection();
		}

		// Token: 0x06000ABF RID: 2751 RVA: 0x0003270D File Offset: 0x0003090D
		private void OnSelectionChange(int dataIndex, bool selected)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelectionChange", new object[] { dataIndex, selected });
			}
			this.ViewSelection();
		}

		// Token: 0x06000AC0 RID: 2752 RVA: 0x00032740 File Offset: 0x00030940
		private void ViewSelection()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewSelection", Array.Empty<object>());
			}
			int num = Mathf.Clamp(this.model.LevelState.Screenshots.Count, 0, this.screenshotLimit.Value);
			int num2 = Mathf.Clamp(this.screenshotLimit.Value - num, 0, this.screenshotLimit.Value);
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "screenshotCount", num), this);
				DebugUtility.Log(string.Format("{0}: {1}", "unusedScreenshots", num2), this);
			}
			this.doneButton.interactable = this.inMemoryScreenshotListModel.SelectedTypedList.Count <= num2;
			this.selectedText.text = string.Format("{0}/{1}", this.inMemoryScreenshotListModel.SelectedTypedList.Count, num2);
		}

		// Token: 0x04000905 RID: 2309
		[Header("UIAddScreenshotsToLevelModalView")]
		[SerializeField]
		private UIAddScreenshotsToLevelModalModel model;

		// Token: 0x04000906 RID: 2310
		[SerializeField]
		private UIInMemoryScreenshotListModel inMemoryScreenshotListModel;

		// Token: 0x04000907 RID: 2311
		[SerializeField]
		private IntVariable screenshotLimit;

		// Token: 0x04000908 RID: 2312
		[SerializeField]
		private UIButton doneButton;

		// Token: 0x04000909 RID: 2313
		[SerializeField]
		private TextMeshProUGUI selectedText;
	}
}
