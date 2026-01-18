using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000211 RID: 529
	public class UIAudioReferenceReadOnlyView : UIBaseView<AudioReference, UIAudioReferenceView.Styles>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000876 RID: 2166 RVA: 0x00029621 File Offset: 0x00027821
		// (set) Token: 0x06000877 RID: 2167 RVA: 0x00029629 File Offset: 0x00027829
		public override UIAudioReferenceView.Styles Style { get; protected set; } = UIAudioReferenceView.Styles.ReadOnly;

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000878 RID: 2168 RVA: 0x00029632 File Offset: 0x00027832
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06000879 RID: 2169 RVA: 0x0002963A File Offset: 0x0002783A
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x0600087A RID: 2170 RVA: 0x00029644 File Offset: 0x00027844
		public override void View(AudioReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			if (model == null || model.IsReferenceEmpty())
			{
				this.ShowSkeleton();
				return;
			}
			this.HideSkeleton();
			this.ViewAudioInformation(model);
		}

		// Token: 0x0600087B RID: 2171 RVA: 0x00029694 File Offset: 0x00027894
		private void ViewAudioInformation(AudioReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewAudioInformation", new object[] { model });
			}
			this.displayNameText.text = string.Empty;
			this.fileInstanceTexture2D.enabled = false;
			SerializableGuid id = InspectorReferenceUtility.GetId(model);
			RuntimeAudioInfo runtimeAudioInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(id, out runtimeAudioInfo))
			{
				this.displayNameText.text = "Missing";
				DebugUtility.LogWarning(string.Format("Could not find audio info for {0}", id), this);
				return;
			}
			this.assetId = runtimeAudioInfo.AudioAsset.AssetID;
			this.displayNameText.text = runtimeAudioInfo.AudioAsset.Name;
			this.fileInstanceTexture2D.enabled = true;
			if (runtimeAudioInfo.Icon)
			{
				this.fileInstanceTexture2D.View(runtimeAudioInfo.Icon);
				return;
			}
			this.fileInstanceTexture2D.View(runtimeAudioInfo.AudioAsset.IconFileInstanceId);
		}

		// Token: 0x0600087C RID: 2172 RVA: 0x00029784 File Offset: 0x00027984
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.fileInstanceTexture2D.enabled = false;
			this.fileInstanceTexture2D.Clear();
			this.UntrackAllRequests();
		}

		// Token: 0x0600087D RID: 2173 RVA: 0x000297BC File Offset: 0x000279BC
		protected void TrackRequest(UIAudioReferenceReadOnlyView.RequestType request)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TrackRequest", new object[] { request });
			}
			if (!this.requestsInProgress.Add(request))
			{
				DebugUtility.LogWarning(string.Format("{0} is already in progress.", request), this);
				return;
			}
			if (this.requestsInProgress.Count == 1)
			{
				this.OnLoadingStarted.Invoke();
			}
		}

		// Token: 0x0600087E RID: 2174 RVA: 0x0002982C File Offset: 0x00027A2C
		protected void UntrackRequest(UIAudioReferenceReadOnlyView.RequestType request)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UntrackRequest", new object[] { request });
			}
			if (!this.requestsInProgress.Remove(request))
			{
				return;
			}
			if (this.requestsInProgress.Count == 0)
			{
				this.OnLoadingEnded.Invoke();
			}
		}

		// Token: 0x0600087F RID: 2175 RVA: 0x00029884 File Offset: 0x00027A84
		private void UntrackAllRequests()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UntrackAllRequests", Array.Empty<object>());
			}
			foreach (UIAudioReferenceReadOnlyView.RequestType requestType in this.requestsInProgress.ToList<UIAudioReferenceReadOnlyView.RequestType>())
			{
				this.UntrackRequest(requestType);
			}
		}

		// Token: 0x06000880 RID: 2176 RVA: 0x000298F4 File Offset: 0x00027AF4
		private void ShowSkeleton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ShowSkeleton", Array.Empty<object>());
			}
			this.TrackRequest(UIAudioReferenceReadOnlyView.RequestType.SkeletonLoading);
			this.skeletonLoadingVisual.SetActive(true);
		}

		// Token: 0x06000881 RID: 2177 RVA: 0x00029921 File Offset: 0x00027B21
		private void HideSkeleton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideSkeleton", Array.Empty<object>());
			}
			if (!this.skeletonLoadingVisual.activeSelf)
			{
				return;
			}
			this.skeletonLoadingVisual.SetActive(false);
			this.UntrackRequest(UIAudioReferenceReadOnlyView.RequestType.SkeletonLoading);
		}

		// Token: 0x04000758 RID: 1880
		[Header("Visuals")]
		[SerializeField]
		private GameObject skeletonLoadingVisual;

		// Token: 0x04000759 RID: 1881
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x0400075A RID: 1882
		[Header("Icon")]
		[SerializeField]
		private UIFileInstanceTexture2DView fileInstanceTexture2D;

		// Token: 0x0400075B RID: 1883
		private readonly HashSet<UIAudioReferenceReadOnlyView.RequestType> requestsInProgress = new HashSet<UIAudioReferenceReadOnlyView.RequestType>();

		// Token: 0x0400075C RID: 1884
		private string assetId;

		// Token: 0x02000212 RID: 530
		protected enum RequestType
		{
			// Token: 0x04000761 RID: 1889
			SkeletonLoading
		}
	}
}
