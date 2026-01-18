using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIAddScreenshotsToLevelModalController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	private const string DISCARD_TITLE = "Discard All Screenshots?";

	private const string DISCARD_BODY = "Do you want to close without applying your selection?";

	[SerializeField]
	private UIAddScreenshotsToLevelModalView view;

	[SerializeField]
	private UIButton closeButton;

	[SerializeField]
	private UIInMemoryScreenshotListModel inMemoryScreenshotListModel;

	[SerializeField]
	private UIButton doneButton;

	[Header("Lost Screenshot Confirmation Modal")]
	[SerializeField]
	private Sprite noScreenshotConfirmModalIcon;

	[SerializeField]
	private Color noButtonColor = Color.green;

	[SerializeField]
	private Color yesButtonColor = Color.red;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private ScreenshotTool screenshotTool;

	private int requests;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		closeButton.onClick.AddListener(Close);
		doneButton.onClick.AddListener(Done);
		screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
		view.BackUnityEvent.AddListener(OnBack);
	}

	private void OnBack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		ConfirmNoPhotoSelection("Discard All Screenshots?", "Do you want to close without applying your selection?");
	}

	private void Close()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Close");
		}
		ConfirmNoPhotoSelection("Discard All Screenshots?", "Do you want to close without applying your selection?");
	}

	private void ConfirmNoPhotoSelection(string title, string body)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ConfirmNoPhotoSelection", title, body);
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal(title, noScreenshotConfirmModalIcon, body, UIModalManagerStackActions.MaintainStack, new UIModalGenericViewAction(noButtonColor, "No", delegate
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}), new UIModalGenericViewAction(yesButtonColor, "Yes", delegate
		{
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.PurgeInMemoryScreenshots();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}));
	}

	private void Done()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Done");
		}
		IReadOnlyList<ScreenshotAPI.InMemoryScreenShot> selectedTypedList = inMemoryScreenshotListModel.SelectedTypedList;
		if (selectedTypedList.Count == 0)
		{
			ConfirmNoPhotoSelection("Confirm Photo Selection", "You have selected no screenshots! These will be lost if you close the menu, are you sure you want to leave?");
			return;
		}
		OnLoadingStarted.Invoke();
		requests = selectedTypedList.Count;
		foreach (ScreenshotAPI.InMemoryScreenShot item in selectedTypedList)
		{
			FileUploadData[] array = new FileUploadData[3];
			byte[] bytes = ScreenshotAPI.ScaleToWidth(item.MainImage, 640).EncodeToPNG();
			byte[] bytes2 = item.MainImage.EncodeToPNG();
			byte[] bytes3 = item.Original.EncodeToPNG();
			string text = RemoveSymbols(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Name);
			string text2 = RemoveSymbols(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name);
			string text3 = $"Endstar {text} {text2} {DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";
			FileUploadData fileUploadData = new FileUploadData
			{
				Bytes = bytes,
				Filename = text3 + " Thumbnail",
				MimeType = "image/png"
			};
			FileUploadData fileUploadData2 = new FileUploadData
			{
				Bytes = bytes2,
				Filename = text3 + " MainImage",
				MimeType = "image/png"
			};
			FileUploadData fileUploadData3 = new FileUploadData
			{
				Bytes = bytes3,
				Filename = text3 + " OriginalRes",
				MimeType = "image/png"
			};
			array[0] = fileUploadData;
			array[1] = fileUploadData2;
			array[2] = fileUploadData3;
			CloudUploader.BatchUploadFileBytes(EndlessServices.Instance.CloudService, array, "endstar", OnUploadSuccess, OnUploadedFailed);
		}
	}

	private void OnUploadSuccess(int[] fileInstanceIds)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnUploadSuccess", fileInstanceIds.Length);
		}
		ScreenshotFileInstances screenshotFileInstance = new ScreenshotFileInstances
		{
			Thumbnail = fileInstanceIds[0],
			MainImage = fileInstanceIds[1],
			OriginalRes = fileInstanceIds[2]
		};
		screenshotTool.AddScreenshotsToLevel_ServerRPC(screenshotFileInstance);
		OnRequestComplete();
	}

	private void OnUploadedFailed(Exception[] exceptions)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnUploadedFailed", exceptions.Length);
		}
		OnRequestComplete();
	}

	private string RemoveSymbols(string inputString)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveSymbols", inputString);
		}
		return Regex.Replace(inputString, "[^\\w\\s]", string.Empty);
	}

	private void OnRequestComplete()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnRequestComplete");
		}
		requests--;
		if (requests <= 0)
		{
			OnLoadingEnded.Invoke();
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.PurgeInMemoryScreenshots();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}
	}
}
