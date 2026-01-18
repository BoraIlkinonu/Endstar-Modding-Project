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
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000284 RID: 644
	public class UIAddScreenshotsToLevelModalController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000AA2 RID: 2722 RVA: 0x00031D2D File Offset: 0x0002FF2D
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x06000AA3 RID: 2723 RVA: 0x00031D35 File Offset: 0x0002FF35
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000AA4 RID: 2724 RVA: 0x00031D40 File Offset: 0x0002FF40
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.closeButton.onClick.AddListener(new UnityAction(this.Close));
			this.doneButton.onClick.AddListener(new UnityAction(this.Done));
			this.screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
			this.view.BackUnityEvent.AddListener(new UnityAction(this.OnBack));
		}

		// Token: 0x06000AA5 RID: 2725 RVA: 0x00031DC9 File Offset: 0x0002FFC9
		private void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			this.ConfirmNoPhotoSelection("Discard All Screenshots?", "Do you want to close without applying your selection?");
		}

		// Token: 0x06000AA6 RID: 2726 RVA: 0x00031DF3 File Offset: 0x0002FFF3
		private void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			this.ConfirmNoPhotoSelection("Discard All Screenshots?", "Do you want to close without applying your selection?");
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x00031E20 File Offset: 0x00030020
		private void ConfirmNoPhotoSelection(string title, string body)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmNoPhotoSelection", new object[] { title, body });
			}
			UIModalManager instance = MonoBehaviourSingleton<UIModalManager>.Instance;
			Sprite sprite = this.noScreenshotConfirmModalIcon;
			UIModalManagerStackActions uimodalManagerStackActions = UIModalManagerStackActions.MaintainStack;
			UIModalGenericViewAction[] array = new UIModalGenericViewAction[2];
			array[0] = new UIModalGenericViewAction(this.noButtonColor, "No", delegate
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
			});
			array[1] = new UIModalGenericViewAction(this.yesButtonColor, "Yes", delegate
			{
				MonoBehaviourSingleton<ScreenshotAPI>.Instance.PurgeInMemoryScreenshots();
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			});
			instance.DisplayGenericModal(title, sprite, body, uimodalManagerStackActions, array);
		}

		// Token: 0x06000AA8 RID: 2728 RVA: 0x00031ED4 File Offset: 0x000300D4
		private void Done()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Done", Array.Empty<object>());
			}
			IReadOnlyList<ScreenshotAPI.InMemoryScreenShot> selectedTypedList = this.inMemoryScreenshotListModel.SelectedTypedList;
			if (selectedTypedList.Count == 0)
			{
				this.ConfirmNoPhotoSelection("Confirm Photo Selection", "You have selected no screenshots! These will be lost if you close the menu, are you sure you want to leave?");
				return;
			}
			this.OnLoadingStarted.Invoke();
			this.requests = selectedTypedList.Count;
			foreach (ScreenshotAPI.InMemoryScreenShot inMemoryScreenShot in selectedTypedList)
			{
				FileUploadData[] array = new FileUploadData[3];
				byte[] array2 = ScreenshotAPI.ScaleToWidth(inMemoryScreenShot.MainImage, 640).EncodeToPNG();
				byte[] array3 = inMemoryScreenShot.MainImage.EncodeToPNG();
				byte[] array4 = inMemoryScreenShot.Original.EncodeToPNG();
				string text = this.RemoveSymbols(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Name);
				string text2 = this.RemoveSymbols(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name);
				string text3 = string.Format("Endstar {0} {1} {2}-{3}-{4} {5}-{6}-{7}", new object[]
				{
					text,
					text2,
					DateTime.Now.Year,
					DateTime.Now.Month,
					DateTime.Now.Day,
					DateTime.Now.Hour,
					DateTime.Now.Minute,
					DateTime.Now.Second
				});
				FileUploadData fileUploadData = new FileUploadData
				{
					Bytes = array2,
					Filename = text3 + " Thumbnail",
					MimeType = "image/png"
				};
				FileUploadData fileUploadData2 = new FileUploadData
				{
					Bytes = array3,
					Filename = text3 + " MainImage",
					MimeType = "image/png"
				};
				FileUploadData fileUploadData3 = new FileUploadData
				{
					Bytes = array4,
					Filename = text3 + " OriginalRes",
					MimeType = "image/png"
				};
				array[0] = fileUploadData;
				array[1] = fileUploadData2;
				array[2] = fileUploadData3;
				CloudUploader.BatchUploadFileBytes(EndlessServices.Instance.CloudService, array, "endstar", new Action<int[]>(this.OnUploadSuccess), new Action<Exception[]>(this.OnUploadedFailed));
			}
		}

		// Token: 0x06000AA9 RID: 2729 RVA: 0x00032144 File Offset: 0x00030344
		private void OnUploadSuccess(int[] fileInstanceIds)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUploadSuccess", new object[] { fileInstanceIds.Length });
			}
			ScreenshotFileInstances screenshotFileInstances = new ScreenshotFileInstances
			{
				Thumbnail = fileInstanceIds[0],
				MainImage = fileInstanceIds[1],
				OriginalRes = fileInstanceIds[2]
			};
			this.screenshotTool.AddScreenshotsToLevel_ServerRPC(screenshotFileInstances, default(ServerRpcParams));
			this.OnRequestComplete();
		}

		// Token: 0x06000AAA RID: 2730 RVA: 0x000321B1 File Offset: 0x000303B1
		private void OnUploadedFailed(Exception[] exceptions)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUploadedFailed", new object[] { exceptions.Length });
			}
			this.OnRequestComplete();
		}

		// Token: 0x06000AAB RID: 2731 RVA: 0x000321DD File Offset: 0x000303DD
		private string RemoveSymbols(string inputString)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveSymbols", new object[] { inputString });
			}
			return Regex.Replace(inputString, "[^\\w\\s]", string.Empty);
		}

		// Token: 0x06000AAC RID: 2732 RVA: 0x0003220C File Offset: 0x0003040C
		private void OnRequestComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnRequestComplete", Array.Empty<object>());
			}
			this.requests--;
			if (this.requests > 0)
			{
				return;
			}
			this.OnLoadingEnded.Invoke();
			MonoBehaviourSingleton<ScreenshotAPI>.Instance.PurgeInMemoryScreenshots();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}

		// Token: 0x040008E9 RID: 2281
		private const string DISCARD_TITLE = "Discard All Screenshots?";

		// Token: 0x040008EA RID: 2282
		private const string DISCARD_BODY = "Do you want to close without applying your selection?";

		// Token: 0x040008EB RID: 2283
		[SerializeField]
		private UIAddScreenshotsToLevelModalView view;

		// Token: 0x040008EC RID: 2284
		[SerializeField]
		private UIButton closeButton;

		// Token: 0x040008ED RID: 2285
		[SerializeField]
		private UIInMemoryScreenshotListModel inMemoryScreenshotListModel;

		// Token: 0x040008EE RID: 2286
		[SerializeField]
		private UIButton doneButton;

		// Token: 0x040008EF RID: 2287
		[Header("Lost Screenshot Confirmation Modal")]
		[SerializeField]
		private Sprite noScreenshotConfirmModalIcon;

		// Token: 0x040008F0 RID: 2288
		[SerializeField]
		private Color noButtonColor = Color.green;

		// Token: 0x040008F1 RID: 2289
		[SerializeField]
		private Color yesButtonColor = Color.red;

		// Token: 0x040008F2 RID: 2290
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040008F3 RID: 2291
		private ScreenshotTool screenshotTool;

		// Token: 0x040008F4 RID: 2292
		private int requests;
	}
}
