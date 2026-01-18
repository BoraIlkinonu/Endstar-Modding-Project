using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.Screenshotting
{
	// Token: 0x02000429 RID: 1065
	public class ScreenshotAPI : MonoBehaviourSingleton<ScreenshotAPI>
	{
		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x06001A6B RID: 6763 RVA: 0x000794AE File Offset: 0x000776AE
		public List<ScreenshotAPI.InMemoryScreenShot> InMemoryScreenshots
		{
			get
			{
				return this.inMemoryScreenshots;
			}
		}

		// Token: 0x06001A6C RID: 6764 RVA: 0x000794B6 File Offset: 0x000776B6
		public void StartScreenshotMode(ScreenshotOptions screenshotOptions = null)
		{
			this.activeScreenshotModeOptions = screenshotOptions;
			if (this.simpleScreenshotCoroutine == null)
			{
				this.SetupScreenshotOptions(this.activeScreenshotModeOptions);
			}
		}

		// Token: 0x06001A6D RID: 6765 RVA: 0x000794D3 File Offset: 0x000776D3
		public void StopScreenshotMode()
		{
			this.OnToggleCharacterVisibilty.Invoke(false);
			this.OnToggleUiVisibility.Invoke(false);
			if (this.activeScreenshotModeOptions == null)
			{
				return;
			}
			this.activeScreenshotModeOptions = null;
			if (this.simpleScreenshotCoroutine == null)
			{
				this.SetupScreenshotOptions(null);
			}
		}

		// Token: 0x06001A6E RID: 6766 RVA: 0x0007950C File Offset: 0x0007770C
		public void SetupScreenshotOptions(ScreenshotOptions screenshotOptions)
		{
			if (screenshotOptions != null)
			{
				UnityEvent<bool> onToggleCharacterVisibilty = this.OnToggleCharacterVisibilty;
				if (onToggleCharacterVisibilty != null)
				{
					onToggleCharacterVisibilty.Invoke(screenshotOptions.HideCharacter);
				}
				UnityEvent<bool> onToggleUiVisibility = this.OnToggleUiVisibility;
				if (onToggleUiVisibility != null)
				{
					onToggleUiVisibility.Invoke(screenshotOptions.HideUi);
				}
				this.activeScreenshotModeOptions = screenshotOptions;
				return;
			}
			UnityEvent<bool> onToggleCharacterVisibilty2 = this.OnToggleCharacterVisibilty;
			if (onToggleCharacterVisibilty2 != null)
			{
				onToggleCharacterVisibilty2.Invoke(false);
			}
			UnityEvent<bool> onToggleUiVisibility2 = this.OnToggleUiVisibility;
			if (onToggleUiVisibility2 == null)
			{
				return;
			}
			onToggleUiVisibility2.Invoke(false);
		}

		// Token: 0x06001A6F RID: 6767 RVA: 0x00079575 File Offset: 0x00077775
		public bool RequestSimpleScreenshot(ScreenshotOptions options = null)
		{
			if (this.simpleScreenshotCoroutine != null)
			{
				Debug.LogWarning("Attempted to request multiple screenshots in the same frame!");
				return false;
			}
			this.simpleScreenshotCoroutine = base.StartCoroutine(this.RecordSimpleScreenshotToFile(options));
			return true;
		}

		// Token: 0x06001A70 RID: 6768 RVA: 0x0007959F File Offset: 0x0007779F
		private IEnumerator RecordSimpleScreenshotToFile(ScreenshotOptions options)
		{
			this.SetupScreenshotOptions(options);
			this.OnBeforeScreenshot.Invoke();
			yield return new WaitForEndOfFrame();
			try
			{
				Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();
				Texture2D texture2D2 = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGB24, false);
				texture2D2.SetPixels32(texture2D.GetPixels32(), 0);
				texture2D2.Apply();
				Texture2D texture2D3 = ScreenshotAPI.ScaleToWidth(ScreenshotAPI.CropTo16_9(texture2D2), 1920);
				this.inMemoryScreenshots.Add(new ScreenshotAPI.InMemoryScreenShot(texture2D2, texture2D3));
				this.simpleScreenshotCoroutine = null;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			finally
			{
				this.SetupScreenshotOptions(this.activeScreenshotModeOptions);
			}
			this.OnAfterScreenshot.Invoke();
			yield break;
		}

		// Token: 0x06001A71 RID: 6769 RVA: 0x000795B8 File Offset: 0x000777B8
		public static Texture2D CropTo16_9(Texture2D source)
		{
			float num = (float)source.width / (float)source.height;
			if (num == 1.7777778f)
			{
				return source;
			}
			int num2 = 0;
			int num3 = 0;
			int num4 = source.width;
			int num5 = source.height;
			if (num > 1.7777778f)
			{
				num4 = Mathf.RoundToInt((float)source.height * 1.7777778f);
				num2 = (source.width - num4) / 2;
			}
			else if (num < 1.7777778f)
			{
				num5 = Mathf.RoundToInt((float)source.width / 1.7777778f);
				num3 = (source.height - num5) / 2;
			}
			Color[] pixels = source.GetPixels(num2, num3, num4, num5);
			Texture2D texture2D = new Texture2D(num4, num5, source.format, source.mipmapCount, source.isDataSRGB);
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			return texture2D;
		}

		// Token: 0x06001A72 RID: 6770 RVA: 0x00079678 File Offset: 0x00077878
		public static Texture2D ScaleToWidth(Texture2D source, int width)
		{
			float num = (float)width / (float)source.width;
			int num2 = Mathf.RoundToInt((float)source.height * num);
			Texture2D texture2D = new Texture2D(width, num2);
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < width; j++)
				{
					int num3 = Mathf.RoundToInt((float)j / num);
					int num4 = Mathf.RoundToInt((float)i / num);
					Color pixel = source.GetPixel(num3, num4);
					texture2D.SetPixel(j, i, pixel);
				}
			}
			texture2D.Apply();
			return texture2D;
		}

		// Token: 0x06001A73 RID: 6771 RVA: 0x000796F8 File Offset: 0x000778F8
		public void PurgeInMemoryScreenshots()
		{
			this.inMemoryScreenshots.Clear();
		}

		// Token: 0x04001521 RID: 5409
		public UnityEvent<bool> OnToggleCharacterVisibilty;

		// Token: 0x04001522 RID: 5410
		public UnityEvent<bool> OnToggleUiVisibility;

		// Token: 0x04001523 RID: 5411
		public UnityEvent OnBeforeScreenshot;

		// Token: 0x04001524 RID: 5412
		public UnityEvent OnAfterScreenshot;

		// Token: 0x04001525 RID: 5413
		public UnityEvent<FileAsset> ScreenshotResultAvailable;

		// Token: 0x04001526 RID: 5414
		private Coroutine simpleScreenshotCoroutine;

		// Token: 0x04001527 RID: 5415
		private readonly List<ScreenshotAPI.InMemoryScreenShot> inMemoryScreenshots = new List<ScreenshotAPI.InMemoryScreenShot>();

		// Token: 0x04001528 RID: 5416
		private ScreenshotOptions activeScreenshotModeOptions;

		// Token: 0x0200042A RID: 1066
		public class InMemoryScreenShot
		{
			// Token: 0x06001A75 RID: 6773 RVA: 0x00079718 File Offset: 0x00077918
			public InMemoryScreenShot(Texture2D original, Texture2D mainImage)
			{
				this.Original = original;
				this.MainImage = mainImage;
				this.FileName = string.Format("Screenshot_{0}.png", DateTime.Now);
			}

			// Token: 0x04001529 RID: 5417
			public Texture2D Original;

			// Token: 0x0400152A RID: 5418
			public Texture2D MainImage;

			// Token: 0x0400152B RID: 5419
			public string FileName;
		}
	}
}
