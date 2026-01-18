using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Assets;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.Screenshotting;

public class ScreenshotAPI : MonoBehaviourSingleton<ScreenshotAPI>
{
	public class InMemoryScreenShot
	{
		public Texture2D Original;

		public Texture2D MainImage;

		public string FileName;

		public InMemoryScreenShot(Texture2D original, Texture2D mainImage)
		{
			Original = original;
			MainImage = mainImage;
			FileName = $"Screenshot_{DateTime.Now}.png";
		}
	}

	public UnityEvent<bool> OnToggleCharacterVisibilty;

	public UnityEvent<bool> OnToggleUiVisibility;

	public UnityEvent OnBeforeScreenshot;

	public UnityEvent OnAfterScreenshot;

	public UnityEvent<FileAsset> ScreenshotResultAvailable;

	private Coroutine simpleScreenshotCoroutine;

	private readonly List<InMemoryScreenShot> inMemoryScreenshots = new List<InMemoryScreenShot>();

	private ScreenshotOptions activeScreenshotModeOptions;

	public List<InMemoryScreenShot> InMemoryScreenshots => inMemoryScreenshots;

	public void StartScreenshotMode(ScreenshotOptions screenshotOptions = null)
	{
		activeScreenshotModeOptions = screenshotOptions;
		if (simpleScreenshotCoroutine == null)
		{
			SetupScreenshotOptions(activeScreenshotModeOptions);
		}
	}

	public void StopScreenshotMode()
	{
		OnToggleCharacterVisibilty.Invoke(arg0: false);
		OnToggleUiVisibility.Invoke(arg0: false);
		if (activeScreenshotModeOptions != null)
		{
			activeScreenshotModeOptions = null;
			if (simpleScreenshotCoroutine == null)
			{
				SetupScreenshotOptions(null);
			}
		}
	}

	public void SetupScreenshotOptions(ScreenshotOptions screenshotOptions)
	{
		if (screenshotOptions == null)
		{
			OnToggleCharacterVisibilty?.Invoke(arg0: false);
			OnToggleUiVisibility?.Invoke(arg0: false);
		}
		else
		{
			OnToggleCharacterVisibilty?.Invoke(screenshotOptions.HideCharacter);
			OnToggleUiVisibility?.Invoke(screenshotOptions.HideUi);
			activeScreenshotModeOptions = screenshotOptions;
		}
	}

	public bool RequestSimpleScreenshot(ScreenshotOptions options = null)
	{
		if (simpleScreenshotCoroutine != null)
		{
			Debug.LogWarning("Attempted to request multiple screenshots in the same frame!");
			return false;
		}
		simpleScreenshotCoroutine = StartCoroutine(RecordSimpleScreenshotToFile(options));
		return true;
	}

	private IEnumerator RecordSimpleScreenshotToFile(ScreenshotOptions options)
	{
		SetupScreenshotOptions(options);
		OnBeforeScreenshot.Invoke();
		yield return new WaitForEndOfFrame();
		try
		{
			Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();
			Texture2D texture2D2 = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGB24, mipChain: false);
			texture2D2.SetPixels32(texture2D.GetPixels32(), 0);
			texture2D2.Apply();
			Texture2D mainImage = ScaleToWidth(CropTo16_9(texture2D2), 1920);
			inMemoryScreenshots.Add(new InMemoryScreenShot(texture2D2, mainImage));
			simpleScreenshotCoroutine = null;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		finally
		{
			SetupScreenshotOptions(activeScreenshotModeOptions);
		}
		OnAfterScreenshot.Invoke();
	}

	public static Texture2D CropTo16_9(Texture2D source)
	{
		float num = (float)source.width / (float)source.height;
		if (num == 1.7777778f)
		{
			return source;
		}
		int x = 0;
		int y = 0;
		int num2 = source.width;
		int num3 = source.height;
		if (num > 1.7777778f)
		{
			num2 = Mathf.RoundToInt((float)source.height * 1.7777778f);
			x = (source.width - num2) / 2;
		}
		else if (num < 1.7777778f)
		{
			num3 = Mathf.RoundToInt((float)source.width / 1.7777778f);
			y = (source.height - num3) / 2;
		}
		Color[] pixels = source.GetPixels(x, y, num2, num3);
		Texture2D texture2D = new Texture2D(num2, num3, source.format, source.mipmapCount, source.isDataSRGB);
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D ScaleToWidth(Texture2D source, int width)
	{
		float num = (float)width / (float)source.width;
		int num2 = Mathf.RoundToInt((float)source.height * num);
		Texture2D texture2D = new Texture2D(width, num2);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int x = Mathf.RoundToInt((float)j / num);
				int y = Mathf.RoundToInt((float)i / num);
				Color pixel = source.GetPixel(x, y);
				texture2D.SetPixel(j, i, pixel);
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public void PurgeInMemoryScreenshots()
	{
		inMemoryScreenshots.Clear();
	}
}
