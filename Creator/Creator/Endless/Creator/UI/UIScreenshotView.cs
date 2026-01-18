using System;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200028B RID: 651
	public class UIScreenshotView : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700015D RID: 349
		// (get) Token: 0x06000ACA RID: 2762 RVA: 0x00032B2C File Offset: 0x00030D2C
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000ACB RID: 2763 RVA: 0x00032B34 File Offset: 0x00030D34
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000ACC RID: 2764 RVA: 0x00032B3C File Offset: 0x00030D3C
		public Texture2D Texture2D
		{
			get
			{
				return (Texture2D)this.rawImage.texture;
			}
		}

		// Token: 0x06000ACD RID: 2765 RVA: 0x00032B50 File Offset: 0x00030D50
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.saveToDiskButton.gameObject.SetActive(!MobileUtility.IsMobile && this.canSaveToDisk);
			if (this.state == UIScreenshotView.States.Empty)
			{
				this.Clear();
			}
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x00032BA3 File Offset: 0x00030DA3
		public void SetScreenshotType(ScreenshotTypes newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetScreenshotType", new object[] { newValue });
			}
			this.screenshotType = newValue;
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x00032BD0 File Offset: 0x00030DD0
		public void Display(Texture2D texture2D)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}", new object[]
				{
					"Display",
					"texture2D",
					texture2D != null,
					base.transform.parent.parent.name
				}), this);
			}
			if (!texture2D)
			{
				this.Clear();
				return;
			}
			if (this.state != UIScreenshotView.States.Empty)
			{
				this.Clear();
			}
			this.Apply(texture2D);
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x00032C58 File Offset: 0x00030E58
		public void Display(ScreenshotFileInstances screenshotFileInstances)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Display", "screenshotFileInstances", screenshotFileInstances), this);
			}
			int num;
			switch (this.screenshotType)
			{
			case ScreenshotTypes.Thumbnail:
				num = screenshotFileInstances.Thumbnail;
				break;
			case ScreenshotTypes.MainImage:
				num = screenshotFileInstances.MainImage;
				break;
			case ScreenshotTypes.OriginalRes:
				num = screenshotFileInstances.OriginalRes;
				break;
			default:
				DebugUtility.LogNoEnumSupportError<ScreenshotTypes>(this, "Display", this.screenshotType, new object[] { this.screenshotType });
				num = screenshotFileInstances.Thumbnail;
				break;
			}
			int? num2 = this.activeFileInstanceId;
			int num3 = num;
			if ((num2.GetValueOrDefault() == num3) & (num2 != null))
			{
				return;
			}
			if (this.state == UIScreenshotView.States.Loading || this.state == UIScreenshotView.States.Loaded)
			{
				this.Clear();
			}
			this.state = UIScreenshotView.States.Loading;
			this.activeFileInstanceId = new int?(num);
			if (this.spinnerBackgroundImageRectTransform.rect.width > base.RectTransform.rect.width || this.spinnerBackgroundImageRectTransform.rect.height > base.RectTransform.rect.height)
			{
				this.spinnerBackgroundImageRectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			}
			this.OnLoadingStarted.Invoke();
			MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2D(this, this.activeFileInstanceId.Value, "png", new Action<int, Texture2D>(this.OnGetTexture2dCompleted));
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x00032DDC File Offset: 0x00030FDC
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.OnLoadingEnded.Invoke();
			this.rawImage.texture = null;
			this.rawImage.color = this.colorWhenClear;
			this.saveToDiskButton.gameObject.SetActive(false);
			this.state = UIScreenshotView.States.Empty;
			if (this.activeFileInstanceId != null)
			{
				if (this.verboseLogging)
				{
					DebugUtility.Log(string.Format("{0}.{1}: {2}", "activeFileInstanceId", "Value", this.activeFileInstanceId.Value), this);
				}
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, this.activeFileInstanceId.Value);
			}
			this.activeFileInstanceId = null;
		}

		// Token: 0x06000AD2 RID: 2770 RVA: 0x00032EA4 File Offset: 0x000310A4
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
			if (this.state == UIScreenshotView.States.Empty)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, fileInstanceId);
				return;
			}
			this.Apply(texture2D);
		}

		// Token: 0x06000AD3 RID: 2771 RVA: 0x00032F04 File Offset: 0x00031104
		private void Apply(Texture2D texture2D)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}", new object[]
				{
					"Apply",
					"texture2D",
					texture2D != null,
					base.transform.parent.parent.name
				}), this);
			}
			this.state = UIScreenshotView.States.Loaded;
			this.rawImage.texture = texture2D;
			this.rawImage.color = Color.white;
			this.rawImage.rectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			this.saveToDiskButton.gameObject.SetActive(!MobileUtility.IsMobile && this.canSaveToDisk);
		}

		// Token: 0x04000910 RID: 2320
		[SerializeField]
		private ScreenshotTypes screenshotType;

		// Token: 0x04000911 RID: 2321
		[SerializeField]
		private Color colorWhenClear = Color.gray;

		// Token: 0x04000912 RID: 2322
		[SerializeField]
		private RawImage rawImage;

		// Token: 0x04000913 RID: 2323
		[SerializeField]
		private bool canSaveToDisk;

		// Token: 0x04000914 RID: 2324
		[SerializeField]
		private UIButton saveToDiskButton;

		// Token: 0x04000915 RID: 2325
		[SerializeField]
		private RectTransform spinnerBackgroundImageRectTransform;

		// Token: 0x04000916 RID: 2326
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000917 RID: 2327
		private int? activeFileInstanceId;

		// Token: 0x04000918 RID: 2328
		private UIScreenshotView.States state;

		// Token: 0x0200028C RID: 652
		private enum States
		{
			// Token: 0x0400091C RID: 2332
			Empty,
			// Token: 0x0400091D RID: 2333
			Loading,
			// Token: 0x0400091E RID: 2334
			Loaded
		}
	}
}
