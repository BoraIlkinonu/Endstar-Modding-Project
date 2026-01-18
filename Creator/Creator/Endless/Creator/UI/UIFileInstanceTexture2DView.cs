using System;
using Endless.FileManagement;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020000BA RID: 186
	public class UIFileInstanceTexture2DView : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060002FE RID: 766 RVA: 0x0001332F File Offset: 0x0001152F
		// (set) Token: 0x060002FF RID: 767 RVA: 0x00013337 File Offset: 0x00011537
		public int FileInstanceId { get; private set; } = -1;

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000300 RID: 768 RVA: 0x00013340 File Offset: 0x00011540
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000301 RID: 769 RVA: 0x00013348 File Offset: 0x00011548
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000302 RID: 770 RVA: 0x00013350 File Offset: 0x00011550
		public void View(Sprite sprite)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("View ( sprite: " + sprite.DebugSafeName(true) + " )", this);
			}
			UIFileInstanceTexture2DView.States states = this.state;
			if (states == UIFileInstanceTexture2DView.States.Loading || states == UIFileInstanceTexture2DView.States.Loaded)
			{
				this.Clear();
			}
			this.state = UIFileInstanceTexture2DView.States.Loaded;
			if (sprite != null)
			{
				this.rawImage.texture = sprite.texture;
				this.rawImage.enabled = true;
			}
		}

		// Token: 0x06000303 RID: 771 RVA: 0x000133C4 File Offset: 0x000115C4
		public void View(int fileInstanceId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "fileInstanceId", fileInstanceId), this);
			}
			UIFileInstanceTexture2DView.States states = this.state;
			if (states == UIFileInstanceTexture2DView.States.Loading || states == UIFileInstanceTexture2DView.States.Loaded)
			{
				this.Clear();
			}
			this.FileInstanceId = fileInstanceId;
			if (this.FileInstanceId <= 0)
			{
				return;
			}
			this.state = UIFileInstanceTexture2DView.States.Loading;
			this.OnLoadingStarted.Invoke();
			try
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2D(this, fileInstanceId, "png", new Action<int, Texture2D>(this.OnGetTexture2dCompleted));
			}
			catch (Exception ex)
			{
				this.Clear();
				DebugUtility.LogException(ex, this);
			}
		}

		// Token: 0x06000304 RID: 772 RVA: 0x00013470 File Offset: 0x00011670
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Clear", this.state.ToString(), Array.Empty<object>());
			}
			this.OnLoadingEnded.Invoke();
			this.rawImage.enabled = false;
			this.rawImage.texture = null;
			if (this.state != UIFileInstanceTexture2DView.States.Empty)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, this.FileInstanceId);
				this.state = UIFileInstanceTexture2DView.States.Empty;
			}
			this.FileInstanceId = -1;
		}

		// Token: 0x06000305 RID: 773 RVA: 0x000134F0 File Offset: 0x000116F0
		private void OnGetTexture2dCompleted(int fileInstanceId, Texture2D texture2D)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGetTexture2dCompleted", new object[]
				{
					fileInstanceId,
					texture2D.DebugSafeName(true)
				});
			}
			this.OnLoadingEnded.Invoke();
			this.rawImage.texture = texture2D;
			this.rawImage.enabled = true;
		}

		// Token: 0x0400030F RID: 783
		[SerializeField]
		private RawImage rawImage;

		// Token: 0x04000310 RID: 784
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000311 RID: 785
		private UIFileInstanceTexture2DView.States state;

		// Token: 0x020000BB RID: 187
		private enum States
		{
			// Token: 0x04000316 RID: 790
			Empty,
			// Token: 0x04000317 RID: 791
			Loading,
			// Token: 0x04000318 RID: 792
			Loaded
		}
	}
}
