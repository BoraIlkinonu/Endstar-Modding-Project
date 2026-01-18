using System;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using Runtime.Shared.Matchmaking;
using TMPro;
using Unity.Cloud.UserReporting;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000070 RID: 112
	[RequireComponent(typeof(UIUserReportWindowView))]
	public class UIUserReportWindowController : UIGameObject, IUILoadingSpinnerViewCompatible, IValidatable
	{
		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000208 RID: 520 RVA: 0x0000B74C File Offset: 0x0000994C
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000209 RID: 521 RVA: 0x0000B754 File Offset: 0x00009954
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x0600020A RID: 522 RVA: 0x0000B75C File Offset: 0x0000995C
		// (set) Token: 0x0600020B RID: 523 RVA: 0x0000B764 File Offset: 0x00009964
		public bool IsReporting { get; private set; }

		// Token: 0x0600020C RID: 524 RVA: 0x0000B770 File Offset: 0x00009970
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.aLevelSubmitButton.onClick.AddListener(new UnityAction(this.SubmitBugLevelA));
			this.bLevelSubmitButton.onClick.AddListener(new UnityAction(this.SubmitBugLevelB));
			this.cLevelSubmitButton.onClick.AddListener(new UnityAction(this.SubmitBugLevelC));
			this.submitFeedbackButton.onClick.AddListener(new UnityAction(this.SubmitFeedback));
			this.closeButton.onClick.AddListener(new UnityAction(this.Close));
			base.TryGetComponent<UIUserReportWindowView>(out this.view);
		}

		// Token: 0x0600020D RID: 525 RVA: 0x0000B830 File Offset: 0x00009A30
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.categoryTabs.OptionsLength != Enum.GetValues(typeof(UIUserReportWindowController.UserReportTypes)).Length)
			{
				Debug.LogException(new Exception(string.Format("{0} ({1}) must have as many values as {2} ({3})!", new object[]
				{
					"categoryTabs",
					this.categoryTabs.OptionsLength,
					"UserReportTypes",
					Enum.GetValues(typeof(UIUserReportWindowController.UserReportTypes)).Length
				})), this);
			}
		}

		// Token: 0x0600020E RID: 526 RVA: 0x0000B8D0 File Offset: 0x00009AD0
		public void TakeScreenshots()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TakeScreenshots", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			UnityUserReporting.CurrentClient.ClearScreenshots();
			UnityUserReporting.CurrentClient.TakeScreenshot(this.mainScreenshotSize.x, this.mainScreenshotSize.y, new Action<UserReportScreenshot>(this.OnMainScreenshotComplete));
			UnityUserReporting.CurrentClient.TakeScreenshot(this.thumbnailScreenshotSize.x, this.thumbnailScreenshotSize.y, new Action<UserReportScreenshot>(this.OnThumbnailScreenshotComplete));
		}

		// Token: 0x0600020F RID: 527 RVA: 0x0000B96E File Offset: 0x00009B6E
		private void SubmitBugLevelA()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SubmitBugLevelA", Array.Empty<object>());
			}
			this.bugLevel = UIUserReportWindowController.BugLevels.A;
			this.Submit(UIUserReportWindowController.UserReportTypes.Bug);
		}

		// Token: 0x06000210 RID: 528 RVA: 0x0000B996 File Offset: 0x00009B96
		private void SubmitBugLevelB()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SubmitBugLevelB", Array.Empty<object>());
			}
			this.bugLevel = UIUserReportWindowController.BugLevels.B;
			this.Submit(UIUserReportWindowController.UserReportTypes.Bug);
		}

		// Token: 0x06000211 RID: 529 RVA: 0x0000B9BE File Offset: 0x00009BBE
		private void SubmitBugLevelC()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SubmitBugLevelC", Array.Empty<object>());
			}
			this.bugLevel = UIUserReportWindowController.BugLevels.C;
			this.Submit(UIUserReportWindowController.UserReportTypes.Bug);
		}

		// Token: 0x06000212 RID: 530 RVA: 0x0000B9E6 File Offset: 0x00009BE6
		private void SubmitFeedback()
		{
			this.Submit(UIUserReportWindowController.UserReportTypes.Feedback);
		}

		// Token: 0x06000213 RID: 531 RVA: 0x0000B9F0 File Offset: 0x00009BF0
		private void Submit(UIUserReportWindowController.UserReportTypes reportType)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Submit", Array.Empty<object>());
			}
			UIUserReportWindowController.UserReportTypes reportType2 = reportType;
			if (reportType2 != UIUserReportWindowController.UserReportTypes.Bug)
			{
				if (reportType2 == UIUserReportWindowController.UserReportTypes.Feedback)
				{
					if (this.feedbackInputField.IsNullOrEmptyOrWhiteSpace(true))
					{
						return;
					}
				}
			}
			else if (this.bugInputField.IsNullOrEmptyOrWhiteSpace(true))
			{
				return;
			}
			this.OnLoadingStarted.Invoke();
			UnityUserReporting.CurrentClient.CreateUserReport(delegate(UserReport report)
			{
				this.SendUserReport(report, reportType);
			});
		}

		// Token: 0x06000214 RID: 532 RVA: 0x0000BA78 File Offset: 0x00009C78
		private void OnMainScreenshotComplete(UserReportScreenshot userReportScreenshot)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMainScreenshotComplete", new object[] { userReportScreenshot });
			}
			this.mainScreenshotComplete = true;
			this.OnScreenshotComplete();
		}

		// Token: 0x06000215 RID: 533 RVA: 0x0000BAA9 File Offset: 0x00009CA9
		private void OnThumbnailScreenshotComplete(UserReportScreenshot userReportScreenshot)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnThumbnailScreenshotComplete", new object[] { userReportScreenshot });
			}
			this.thumbnailScreenshotComplete = true;
			this.OnScreenshotComplete();
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000BADC File Offset: 0x00009CDC
		private void OnScreenshotComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScreenshotComplete", Array.Empty<object>());
			}
			if (!this.mainScreenshotComplete || !this.thumbnailScreenshotComplete)
			{
				if (this.verboseLogging)
				{
					Debug.Log(string.Format("{0} returned out. {1}: {2}, {3}: {4}", new object[] { "OnScreenshotComplete", "mainScreenshotComplete", this.mainScreenshotComplete, "thumbnailScreenshotComplete", this.thumbnailScreenshotComplete }), this);
				}
				return;
			}
			this.mainScreenshotComplete = false;
			this.thumbnailScreenshotComplete = false;
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x06000217 RID: 535 RVA: 0x0000BB80 File Offset: 0x00009D80
		private void SendUserReport(UserReport userReport, UIUserReportWindowController.UserReportTypes reportType)
		{
			UIUserReportWindowController.<>c__DisplayClass43_0 CS$<>8__locals1 = new UIUserReportWindowController.<>c__DisplayClass43_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.userReport = userReport;
			CS$<>8__locals1.reportType = reportType;
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SendUserReport", new object[] { CS$<>8__locals1.userReport });
			}
			if (CS$<>8__locals1.userReport.ProjectIdentifier.IsNullOrEmptyOrWhiteSpace())
			{
				Debug.LogWarning("The UserReport's ProjectIdentifier is not set. Please setup cloud services using the Services tab or manually specify a project identifier when calling UnityUserReporting.Configure().");
				return;
			}
			CS$<>8__locals1.userReport = this.AddDimensionAndField("Category", CS$<>8__locals1.reportType.ToString(), CS$<>8__locals1.userReport);
			CS$<>8__locals1.userReport = this.AddDimensionAndField("Device", SystemInfo.deviceType.ToString(), CS$<>8__locals1.userReport);
			CS$<>8__locals1.userReport = this.AddDimensionAndField("Version", Application.version, CS$<>8__locals1.userReport);
			UIUserReportWindowController.UserReportTypes reportType2 = CS$<>8__locals1.reportType;
			if (reportType2 != UIUserReportWindowController.UserReportTypes.Bug)
			{
				if (reportType2 == UIUserReportWindowController.UserReportTypes.Feedback)
				{
					CS$<>8__locals1.userReport.Summary = this.feedbackInputField.text;
				}
			}
			else
			{
				CS$<>8__locals1.userReport.Summary = this.bugInputField.text;
				CS$<>8__locals1.userReport = this.AddField("Repro", this.reproStepsInputField.text, CS$<>8__locals1.userReport);
				CS$<>8__locals1.userReport = this.AddField("Expected", this.expectedInputField.text, CS$<>8__locals1.userReport);
				CS$<>8__locals1.userReport = this.AddField("Severity", this.bugLevel.ToString(), CS$<>8__locals1.userReport);
				CS$<>8__locals1.userReport = this.AddDimension("Severity", this.bugLevel.ToString(), CS$<>8__locals1.userReport);
			}
			EndlessCloudService endlessCloudService = null;
			if (EndlessServices.Instance)
			{
				endlessCloudService = EndlessServices.Instance.CloudService;
			}
			CS$<>8__locals1.userReport = this.AddField("UserId", (endlessCloudService != null) ? endlessCloudService.ActiveUserId.ToString() : "null", CS$<>8__locals1.userReport);
			CS$<>8__locals1.userReport = this.AddField("UserName", (endlessCloudService != null) ? endlessCloudService.ActiveUserName : "null", CS$<>8__locals1.userReport);
			CS$<>8__locals1.userReport = this.AddField("CurrentGameState", (NetworkBehaviourSingleton<GameStateManager>.Instance != null) ? NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState.ToString() : "null", CS$<>8__locals1.userReport);
			CS$<>8__locals1.userReport = this.AddField("CurrentGame", (NetworkBehaviourSingleton<GameStateManager>.Instance != null) ? NetworkBehaviourSingleton<GameStateManager>.Instance.GameId.ToString() : "null", CS$<>8__locals1.userReport);
			CS$<>8__locals1.userReport = this.AddField("CurrentLevel", (NetworkBehaviourSingleton<GameStateManager>.Instance != null) ? NetworkBehaviourSingleton<GameStateManager>.Instance.LevelId.ToString() : "null", CS$<>8__locals1.userReport);
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)Time.realtimeSinceStartup);
			CS$<>8__locals1.userReport = this.AddField("TimeSinceStartup", string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds }), CS$<>8__locals1.userReport);
			this.progressText.gameObject.SetActive(true);
			this.DisplayProgress(0f, 0f);
			this.IsReporting = true;
			CS$<>8__locals1.sendUserReportAttempts = 0;
			UnityUserReporting.CurrentClient.SendUserReport(CS$<>8__locals1.userReport, new Action<float, float>(this.DisplayProgress), new Action<bool, UserReport>(CS$<>8__locals1.<SendUserReport>g__HandleReportCompleted|0));
		}

		// Token: 0x06000218 RID: 536 RVA: 0x0000BF34 File Offset: 0x0000A134
		private UserReport AddDimensionAndField(string itemName, string itemValue, UserReport userReport)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddDimensionAndField", new object[] { itemName, itemValue, userReport.Identifier });
			}
			userReport = this.AddDimension(itemName, itemValue, userReport);
			userReport = this.AddField(itemName, itemValue, userReport);
			return userReport;
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0000BF84 File Offset: 0x0000A184
		private UserReport AddField(string fieldName, string fieldValue, UserReport userReport)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddField", new object[] { fieldName, fieldValue, userReport.Identifier });
			}
			UserReportNamedValue userReportNamedValue = new UserReportNamedValue
			{
				Name = fieldName,
				Value = fieldValue
			};
			userReport.Fields.Add(userReportNamedValue);
			return userReport;
		}

		// Token: 0x0600021A RID: 538 RVA: 0x0000BFE4 File Offset: 0x0000A1E4
		private UserReport AddDimension(string dimensionName, string dimensionValue, UserReport userReport)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddDimension", new object[] { dimensionName, dimensionValue, userReport.Identifier });
			}
			UserReportNamedValue userReportNamedValue = new UserReportNamedValue
			{
				Name = dimensionName,
				Value = dimensionValue
			};
			userReport.Dimensions.Add(userReportNamedValue);
			return userReport;
		}

		// Token: 0x0600021B RID: 539 RVA: 0x0000C044 File Offset: 0x0000A244
		private void DisplayProgress(float uploadProgress, float downloadProgress)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayProgress", new object[] { uploadProgress, downloadProgress });
			}
			this.progressText.text = string.Format("{0:P}", uploadProgress);
		}

		// Token: 0x0600021C RID: 540 RVA: 0x0000C098 File Offset: 0x0000A298
		private void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			this.OnLoadingEnded.Invoke();
			this.progressText.gameObject.SetActive(false);
			this.view.Hide();
		}

		// Token: 0x04000173 RID: 371
		[SerializeField]
		private UISpriteAndStringTabGroup categoryTabs;

		// Token: 0x04000174 RID: 372
		[Header("Bug")]
		[SerializeField]
		private UIInputField bugInputField;

		// Token: 0x04000175 RID: 373
		[SerializeField]
		private UIInputField reproStepsInputField;

		// Token: 0x04000176 RID: 374
		[SerializeField]
		private UIInputField expectedInputField;

		// Token: 0x04000177 RID: 375
		[SerializeField]
		private UIButton aLevelSubmitButton;

		// Token: 0x04000178 RID: 376
		[SerializeField]
		private UIButton bLevelSubmitButton;

		// Token: 0x04000179 RID: 377
		[SerializeField]
		private UIButton cLevelSubmitButton;

		// Token: 0x0400017A RID: 378
		[Header("Feedback")]
		[SerializeField]
		private UIInputField feedbackInputField;

		// Token: 0x0400017B RID: 379
		[SerializeField]
		private UIButton submitFeedbackButton;

		// Token: 0x0400017C RID: 380
		[Header("Misc")]
		[SerializeField]
		private UIButton closeButton;

		// Token: 0x0400017D RID: 381
		[SerializeField]
		private TextMeshProUGUI progressText;

		// Token: 0x0400017E RID: 382
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400017F RID: 383
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000180 RID: 384
		private readonly Vector2Int mainScreenshotSize = Vector2Int.one * 2048;

		// Token: 0x04000181 RID: 385
		private readonly Vector2Int thumbnailScreenshotSize = Vector2Int.one * 512;

		// Token: 0x04000182 RID: 386
		private UIUserReportWindowView view;

		// Token: 0x04000183 RID: 387
		private UIUserReportWindowController.BugLevels bugLevel;

		// Token: 0x04000184 RID: 388
		private bool mainScreenshotComplete;

		// Token: 0x04000185 RID: 389
		private bool thumbnailScreenshotComplete;

		// Token: 0x04000186 RID: 390
		private const int MAX_SEND_USER_REPORT_ATTEMPTS = 3;

		// Token: 0x02000071 RID: 113
		private enum BugLevels
		{
			// Token: 0x0400018B RID: 395
			A,
			// Token: 0x0400018C RID: 396
			B,
			// Token: 0x0400018D RID: 397
			C
		}

		// Token: 0x02000072 RID: 114
		private enum UserReportTypes
		{
			// Token: 0x0400018F RID: 399
			Bug,
			// Token: 0x04000190 RID: 400
			Feedback
		}
	}
}
