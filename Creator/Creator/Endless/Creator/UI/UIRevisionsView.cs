using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000277 RID: 631
	public class UIRevisionsView : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000A6B RID: 2667 RVA: 0x00030B0B File Offset: 0x0002ED0B
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000A6C RID: 2668 RVA: 0x00030B13 File Offset: 0x0002ED13
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000A6D RID: 2669 RVA: 0x00030B1C File Offset: 0x0002ED1C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.versionListModel.ModelChangedUnityEvent.AddListener(new UnityAction(this.GetRevisions));
			this.versionListModel.ItemSelectedUnityEvent.AddListener(new UnityAction<int>(this.OnVersionSelected));
			this.selectedVersionTimestampText.enabled = false;
		}

		// Token: 0x06000A6E RID: 2670 RVA: 0x00030B88 File Offset: 0x0002ED88
		public void Initialize(LevelState levelState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { levelState.AssetID });
			}
			this.levelState = levelState;
			this.versionListModel.Initialize(levelState.AssetID);
		}

		// Token: 0x06000A6F RID: 2671 RVA: 0x00030BD4 File Offset: 0x0002EDD4
		private void GetRevisions()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetRevisions", Array.Empty<object>());
			}
			if (this.versionListModel.Count == 0)
			{
				if (this.verboseLogging)
				{
					DebugUtility.LogMethodWithAppension(this, "GetRevisions", "There are no versions to get revisions from!", Array.Empty<object>());
				}
				return;
			}
			if (this.versionListModel.SelectedTypedList.Count == 0)
			{
				DebugUtility.LogException(new Exception("versionListModel.SelectedTypedList.Count is 0!"), this);
				return;
			}
			string text = this.versionListModel.SelectedTypedList[0];
			if (this.verboseLogging)
			{
				DebugUtility.Log("version: " + text, this);
			}
			this.revisionListModel.Initialize(this.levelState, text);
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x00030C84 File Offset: 0x0002EE84
		private void OnVersionSelected(int index)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnVersionSelected", new object[] { index });
			}
			this.OnLoadingStarted.Invoke();
			this.selectedVersionTimestampText.enabled = false;
			string text = this.versionListModel.SelectedTypedList[0];
			this.versionListModel.GetTimestampAsync(text, new Action<string, DateTime>(this.ViewTimestamp));
		}

		// Token: 0x06000A71 RID: 2673 RVA: 0x00030CF8 File Offset: 0x0002EEF8
		private void ViewTimestamp(string version, DateTime timestamp)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewTimestamp", new object[] { version, timestamp });
			}
			if (this.versionListModel.ReadOnlySelectedList.Count == 0)
			{
				return;
			}
			if (this.versionListModel.SelectedTypedList[0] != version)
			{
				return;
			}
			this.selectedVersionTimestampText.text = timestamp.ToShortDateString() + " at " + timestamp.ToShortTimeString();
			this.selectedVersionTimestampText.enabled = true;
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x040008A9 RID: 2217
		[SerializeField]
		private UIVersionListModel versionListModel;

		// Token: 0x040008AA RID: 2218
		[SerializeField]
		private TextMeshProUGUI selectedVersionTimestampText;

		// Token: 0x040008AB RID: 2219
		[SerializeField]
		private UIRevisionListModel revisionListModel;

		// Token: 0x040008AC RID: 2220
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040008AD RID: 2221
		private LevelState levelState;
	}
}
