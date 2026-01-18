using Endless.FileManagement;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIScreenshotView : UIGameObject, IUILoadingSpinnerViewCompatible
{
	private enum States
	{
		Empty,
		Loading,
		Loaded
	}

	[SerializeField]
	private ScreenshotTypes screenshotType;

	[SerializeField]
	private Color colorWhenClear = Color.gray;

	[SerializeField]
	private RawImage rawImage;

	[SerializeField]
	private bool canSaveToDisk;

	[SerializeField]
	private UIButton saveToDiskButton;

	[SerializeField]
	private RectTransform spinnerBackgroundImageRectTransform;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private int? activeFileInstanceId;

	private States state;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public Texture2D Texture2D => (Texture2D)rawImage.texture;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		saveToDiskButton.gameObject.SetActive(!MobileUtility.IsMobile && canSaveToDisk);
		if (state == States.Empty)
		{
			Clear();
		}
	}

	public void SetScreenshotType(ScreenshotTypes newValue)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetScreenshotType", newValue);
		}
		screenshotType = newValue;
	}

	public void Display(Texture2D texture2D)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}", "Display", "texture2D", texture2D != null, base.transform.parent.parent.name), this);
		}
		if (!texture2D)
		{
			Clear();
			return;
		}
		if (state != States.Empty)
		{
			Clear();
		}
		Apply(texture2D);
	}

	public void Display(ScreenshotFileInstances screenshotFileInstances)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Display", "screenshotFileInstances", screenshotFileInstances), this);
		}
		int num;
		switch (screenshotType)
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
			DebugUtility.LogNoEnumSupportError(this, "Display", screenshotType, screenshotType);
			num = screenshotFileInstances.Thumbnail;
			break;
		}
		if (activeFileInstanceId != num)
		{
			if (state == States.Loading || state == States.Loaded)
			{
				Clear();
			}
			state = States.Loading;
			activeFileInstanceId = num;
			if (spinnerBackgroundImageRectTransform.rect.width > base.RectTransform.rect.width || spinnerBackgroundImageRectTransform.rect.height > base.RectTransform.rect.height)
			{
				spinnerBackgroundImageRectTransform.SetAnchor(AnchorPresets.StretchAll);
			}
			OnLoadingStarted.Invoke();
			MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2D(this, activeFileInstanceId.Value, "png", OnGetTexture2dCompleted);
		}
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		OnLoadingEnded.Invoke();
		rawImage.texture = null;
		rawImage.color = colorWhenClear;
		saveToDiskButton.gameObject.SetActive(value: false);
		state = States.Empty;
		if (activeFileInstanceId.HasValue)
		{
			if (verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}.{1}: {2}", "activeFileInstanceId", "Value", activeFileInstanceId.Value), this);
			}
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, activeFileInstanceId.Value);
		}
		activeFileInstanceId = null;
	}

	private void OnGetTexture2dCompleted(int fileInstanceId, Texture2D texture2D)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnGetTexture2dCompleted", fileInstanceId, texture2D.DebugSafeName());
		}
		OnLoadingEnded.Invoke();
		if (state == States.Empty)
		{
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, fileInstanceId);
		}
		else
		{
			Apply(texture2D);
		}
	}

	private void Apply(Texture2D texture2D)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}", "Apply", "texture2D", texture2D != null, base.transform.parent.parent.name), this);
		}
		state = States.Loaded;
		rawImage.texture = texture2D;
		rawImage.color = Color.white;
		rawImage.rectTransform.SetAnchor(AnchorPresets.StretchAll);
		saveToDiskButton.gameObject.SetActive(!MobileUtility.IsMobile && canSaveToDisk);
	}
}
