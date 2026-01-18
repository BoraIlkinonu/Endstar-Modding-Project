using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Core.UI
{
	// Token: 0x0200007C RID: 124
	public sealed class UIGameInspectorScreenView : UIBaseScreenView, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700003E RID: 62
		// (get) Token: 0x06000271 RID: 625 RVA: 0x0000D7A1 File Offset: 0x0000B9A1
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000272 RID: 626 RVA: 0x0000D7A9 File Offset: 0x0000B9A9
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000273 RID: 627 RVA: 0x0000D7B1 File Offset: 0x0000B9B1
		// (set) Token: 0x06000274 RID: 628 RVA: 0x0000D7B9 File Offset: 0x0000B9B9
		public UIGameInspectorScreenModel Model { get; private set; }

		// Token: 0x06000275 RID: 629 RVA: 0x0000D7C4 File Offset: 0x0000B9C4
		protected override void Start()
		{
			base.Start();
			this.userRolesModel.OnLoadingStarted.AddListener(new UnityAction(this.OnLoadingStarted.Invoke));
			this.userRolesModel.OnLoadingEnded.AddListener(new UnityAction(this.OnLoadingEnded.Invoke));
			this.screenshotFileInstancesListView.SnappedUnityEvent.AddListener(new UnityAction<int, int, UIBaseListItemView<ScreenshotFileInstances>>(this.OnScreenshotSnapped));
		}

		// Token: 0x06000276 RID: 630 RVA: 0x0000D838 File Offset: 0x0000BA38
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.userRolesModel.OnLoadingStarted.RemoveListener(new UnityAction(this.OnLoadingStarted.Invoke));
			this.userRolesModel.OnLoadingEnded.RemoveListener(new UnityAction(this.OnLoadingEnded.Invoke));
			this.screenshotFileInstancesListView.SnappedUnityEvent.RemoveListener(new UnityAction<int, int, UIBaseListItemView<ScreenshotFileInstances>>(this.OnScreenshotSnapped));
		}

		// Token: 0x06000277 RID: 631 RVA: 0x0000D8BC File Offset: 0x0000BABC
		public static UIGameInspectorScreenView Display(UIScreenManager.DisplayStackActions displayStackAction, UIGameInspectorScreenModel model)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object> { { "model", model } };
			return (UIGameInspectorScreenView)MonoBehaviourSingleton<UIScreenManager>.Instance.Display<UIGameInspectorScreenView>(displayStackAction, dictionary);
		}

		// Token: 0x06000278 RID: 632 RVA: 0x0000D8EC File Offset: 0x0000BAEC
		public override async void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			UIGameInspectorScreenModel uigameInspectorScreenModel = (UIGameInspectorScreenModel)supplementalData["model"];
			this.Model = uigameInspectorScreenModel;
			this.InitializeScreenshots();
			this.InitializeGameName();
			this.InitializeDescription();
			this.InitializeButtons();
			this.OnLoadingStarted.Invoke();
			await this.SetupCreatorAsync();
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x06000279 RID: 633 RVA: 0x0000D92B File Offset: 0x0000BB2B
		public override void OnBack()
		{
			base.OnBack();
			UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x0600027A RID: 634 RVA: 0x0000D93A File Offset: 0x0000BB3A
		public override void Close(Action onCloseTweenComplete)
		{
			base.Close(onCloseTweenComplete);
			this.screenshotFileInstancesListModel.Clear(true);
			this.tabs.SetValue(0, true);
			this.userRolesModel.Clear();
		}

		// Token: 0x0600027B RID: 635 RVA: 0x0000D968 File Offset: 0x0000BB68
		private void InitializeScreenshots()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeScreenshots", Array.Empty<object>());
			}
			bool flag = this.Model.MainMenuGameModel.Screenshots.Count > 0;
			List<ScreenshotFileInstances> list = new List<ScreenshotFileInstances>(this.Model.MainMenuGameModel.Screenshots);
			if (flag)
			{
				this.screenshotFileInstancesListModel.Set(list, true);
			}
			this.screenshotFileInstancesListModel.gameObject.SetActive(flag);
			this.defaultScreenshotImage.enabled = !flag;
			this.UpdateScreenshotIndicators(0);
		}

		// Token: 0x0600027C RID: 636 RVA: 0x0000D9F4 File Offset: 0x0000BBF4
		private void InitializeGameName()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeGameName", Array.Empty<object>());
			}
			string name = this.Model.MainMenuGameModel.Name;
			this.gameNameMeasureText.text = name;
			this.gameNameText.text = name;
			this.gameNameMeasureText.ForceMeshUpdate(false, false);
			float num = (float)Mathf.Clamp(this.gameNameMeasureText.textInfo.lineCount, 1, this.maxVisibleLines) * this.lineHeight;
			this.gameNameLayoutElement.PreferredHeightLayoutDimension.ExplicitValue = num;
			LayoutRebuilder.MarkLayoutForRebuild(this.gameNameLayoutElement.RectTransform);
		}

		// Token: 0x0600027D RID: 637 RVA: 0x0000DA98 File Offset: 0x0000BC98
		private void InitializeDescription()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeDescription", Array.Empty<object>());
			}
			this.descriptionText.text = this.Model.MainMenuGameModel.Description;
			long revisionTimestamp = this.Model.MainMenuGameModel.RevisionMetaData.RevisionTimestamp;
			DateTime dateTime = new DateTime(revisionTimestamp, DateTimeKind.Utc);
			DateTime dateTime2 = dateTime.ToLocalTime();
			this.lastUpdatedText.text = "Last Updated: " + dateTime2.ToShortDateString();
		}

		// Token: 0x0600027E RID: 638 RVA: 0x0000DB1C File Offset: 0x0000BD1C
		private void InitializeButtons()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeButtons", Array.Empty<object>());
			}
			this.adminButton.gameObject.SetActive(this.Model.MainMenuGameContext == MainMenuGameContext.Admin);
			this.editButton.gameObject.SetActive(this.Model.MainMenuGameContext == MainMenuGameContext.Edit);
			this.playButton.gameObject.SetActive(this.Model.MainMenuGameContext == MainMenuGameContext.Play);
			this.copyShareLinkButton.gameObject.SetActive(this.Model.MainMenuGameContext == MainMenuGameContext.Play);
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0000DBBC File Offset: 0x0000BDBC
		private async Task SetupCreatorAsync()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetupCreatorAsync", Array.Empty<object>());
			}
			this.creatorProfileImage.sprite = null;
			this.creatorNameText.text = "Loading...";
			this.creatorOutlineImage.color = this.creatorTypeColorDictionary[UIGameAssetCreatorTypes.Official];
			AssetContexts assetContexts = ((this.Model.MainMenuGameContext == MainMenuGameContext.Edit) ? AssetContexts.GameInspectorCreate : AssetContexts.GameInspectorPlay);
			this.userRolesModel.Initialize(this.Model.MainMenuGameModel.AssetID, this.Model.MainMenuGameModel.Name, SerializableGuid.Empty, assetContexts);
			ref GetAllRolesResult ptr = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(this.Model.MainMenuGameModel.AssetID, null, false);
			bool flag = false;
			using (IEnumerator<UserRole> enumerator = ptr.Roles.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.UserId == this.endlessStudiosUserId.InternalId)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				this.creatorProfileImage.sprite = this.officialCreatorSprite;
				this.creatorNameText.text = "Endless Studios";
				this.creatorOutlineImage.color = this.creatorTypeColorDictionary[UIGameAssetCreatorTypes.Official];
			}
			else
			{
				this.creatorProfileImage.sprite = this.communityCreatorSprite;
				this.creatorNameText.text = "Community";
				this.creatorOutlineImage.color = this.creatorTypeColorDictionary[UIGameAssetCreatorTypes.Community];
			}
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0000DBFF File Offset: 0x0000BDFF
		private void OnScreenshotSnapped(int snapCellViewIndex, int snapDataIndex, UIBaseListItemView<ScreenshotFileInstances> cellView)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScreenshotSnapped", new object[] { snapCellViewIndex, snapDataIndex, cellView });
			}
			this.UpdateScreenshotIndicators(snapDataIndex);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0000DC38 File Offset: 0x0000BE38
		private void UpdateScreenshotIndicators(int activeIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateScreenshotIndicators", new object[] { activeIndex });
			}
			int count = this.screenshotFileInstancesListModel.Count;
			for (int i = 0; i < this.screenshotIndicators.Length; i++)
			{
				UIDisplayAndHideHandler uidisplayAndHideHandler = this.screenshotIndicators[i];
				if (!uidisplayAndHideHandler)
				{
					DebugUtility.LogException(new NullReferenceException(string.Format("Null item in {0} at index {1}", "screenshotIndicators", i)), this);
				}
				else
				{
					bool flag = i < count;
					uidisplayAndHideHandler.gameObject.SetActive(flag);
					if (flag)
					{
						if (i == activeIndex)
						{
							uidisplayAndHideHandler.Display();
						}
						else
						{
							uidisplayAndHideHandler.Hide();
						}
					}
				}
			}
		}

		// Token: 0x040001B6 RID: 438
		[Header("Screenshots")]
		[SerializeField]
		private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

		// Token: 0x040001B7 RID: 439
		[SerializeField]
		private UIScreenshotFileInstancesListView screenshotFileInstancesListView;

		// Token: 0x040001B8 RID: 440
		[SerializeField]
		private Image defaultScreenshotImage;

		// Token: 0x040001B9 RID: 441
		[SerializeField]
		private UIDisplayAndHideHandler[] screenshotIndicators = new UIDisplayAndHideHandler[10];

		// Token: 0x040001BA RID: 442
		[Header("Game Name")]
		[SerializeField]
		private TextMeshProUGUI gameNameText;

		// Token: 0x040001BB RID: 443
		[SerializeField]
		private UILayoutElement gameNameLayoutElement;

		// Token: 0x040001BC RID: 444
		[SerializeField]
		private TextMeshProUGUI gameNameMeasureText;

		// Token: 0x040001BD RID: 445
		[SerializeField]
		[Min(1f)]
		private int maxVisibleLines = 2;

		// Token: 0x040001BE RID: 446
		[SerializeField]
		private float lineHeight = 46.8322f;

		// Token: 0x040001BF RID: 447
		[Header("Creator")]
		[SerializeField]
		private TextMeshProUGUI creatorNameText;

		// Token: 0x040001C0 RID: 448
		[SerializeField]
		private Image creatorProfileImage;

		// Token: 0x040001C1 RID: 449
		[SerializeField]
		private Sprite officialCreatorSprite;

		// Token: 0x040001C2 RID: 450
		[SerializeField]
		private Sprite communityCreatorSprite;

		// Token: 0x040001C3 RID: 451
		[SerializeField]
		private Image creatorOutlineImage;

		// Token: 0x040001C4 RID: 452
		[SerializeField]
		private UIGameAssetCreatorTypesColorDictionary creatorTypeColorDictionary;

		// Token: 0x040001C5 RID: 453
		[SerializeField]
		private EndlessStudiosUserId endlessStudiosUserId;

		// Token: 0x040001C6 RID: 454
		[Header("Buttons")]
		[SerializeField]
		private UIButton adminButton;

		// Token: 0x040001C7 RID: 455
		[SerializeField]
		private UIButton editButton;

		// Token: 0x040001C8 RID: 456
		[SerializeField]
		private UIButton playButton;

		// Token: 0x040001C9 RID: 457
		[SerializeField]
		private UIButton copyShareLinkButton;

		// Token: 0x040001CA RID: 458
		[Header("Body")]
		[SerializeField]
		private UIStringTabGroup tabs;

		// Token: 0x040001CB RID: 459
		[Header("Description")]
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x040001CC RID: 460
		[SerializeField]
		private TextMeshProUGUI lastUpdatedText;

		// Token: 0x040001CD RID: 461
		[Header("Roles")]
		[SerializeField]
		private UIUserRolesModel userRolesModel;
	}
}
