using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000A8 RID: 168
	public class UIGameController : UIAssetWithScreenshotsController
	{
		// Token: 0x060002A8 RID: 680 RVA: 0x00011CC4 File Offset: 0x0000FEC4
		protected override void Start()
		{
			base.Start();
			UIButton[] array = this.addScreenshotButtons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].onClick.AddListener(new UnityAction(this.OpenAddLevelScreenshotsToGameModal));
			}
			this.screenshotFileInstancesListModel.OnItemMovedUnityEvent.AddListener(new UnityAction<int, int>(this.RearrangeScreenshots));
			this.removeScreenshotDropHandler.DropWithGameObjectUnityEvent.AddListener(new UnityAction<GameObject>(this.OnDroppedScreenshot));
			this.onScreenshotsToAddedUnityAction = new UnityAction<List<ScreenshotFileInstances>>(this.gameView.AddScreenshots);
		}

		// Token: 0x060002A9 RID: 681 RVA: 0x00011D58 File Offset: 0x0000FF58
		protected override async void SetName(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetName", new object[] { newValue });
			}
			if (!(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Name == newValue))
			{
				if (newValue.Replace(" ", string.Empty).IsNullOrEmptyOrWhiteSpace())
				{
					base.NameInputField.PlayInvalidInputTweens();
				}
				else
				{
					await MonoBehaviourSingleton<GameEditor>.Instance.UpdateGameName(newValue);
				}
			}
		}

		// Token: 0x060002AA RID: 682 RVA: 0x00011D98 File Offset: 0x0000FF98
		protected override async void SetDescription(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDescription", new object[] { newValue });
			}
			if (!(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Description == newValue))
			{
				await MonoBehaviourSingleton<GameEditor>.Instance.UpdateDescription(newValue, null, null);
			}
		}

		// Token: 0x060002AB RID: 683 RVA: 0x00011DD8 File Offset: 0x0000FFD8
		protected override async void RemoveScreenshot(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveScreenshot", new object[] { index });
			}
			base.OnLoadingStarted.Invoke();
			bool flag = await MonoBehaviourSingleton<GameEditor>.Instance.RemoveGameScreenshotAt(index);
			base.OnLoadingEnded.Invoke();
			if (flag)
			{
				bool flag2 = index > 0;
				this.screenshotFileInstancesListModel.RemoveAt(index, flag2);
				if (index == 0)
				{
					this.gameView.DisplayScreenshots(this.screenshotFileInstancesListModel.ReadOnlyList.ToList<ScreenshotFileInstances>());
				}
			}
		}

		// Token: 0x060002AC RID: 684 RVA: 0x00011E18 File Offset: 0x00010018
		protected override async void RearrangeScreenshots(int oldIndex, int newIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RearrangeScreenshots", new object[] { oldIndex, newIndex });
			}
			base.OnLoadingStarted.Invoke();
			await MonoBehaviourSingleton<GameEditor>.Instance.ReorderGameScreenshot(this.screenshotFileInstancesListModel.ReadOnlyList.ToList<ScreenshotFileInstances>());
			base.OnLoadingEnded.Invoke();
			this.modelHandler.GetAsset();
		}

		// Token: 0x060002AD RID: 685 RVA: 0x00011E60 File Offset: 0x00010060
		private void OnDroppedScreenshot(GameObject droppedGameObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDroppedScreenshot", new object[] { droppedGameObject.DebugSafeName(true) });
			}
			UIScreenshotFileInstancesListCellView uiscreenshotFileInstancesListCellView;
			if (!droppedGameObject.TryGetComponent<UIScreenshotFileInstancesListCellView>(out uiscreenshotFileInstancesListCellView))
			{
				throw new NullReferenceException("Could not get UIScreenshotFileInstancesListCellView from droppedGameObject!");
			}
			int num = this.screenshotFileInstancesListModel.ReadOnlyList.IndexOf(uiscreenshotFileInstancesListCellView.Model);
			if (num < 0)
			{
				throw new IndexOutOfRangeException("Could not get index of " + uiscreenshotFileInstancesListCellView.Model.GetType().Namespace + " from screenshotFileInstancesListModel!");
			}
			this.RemoveScreenshot(num);
		}

		// Token: 0x060002AE RID: 686 RVA: 0x00011EEC File Offset: 0x000100EC
		private void OpenAddLevelScreenshotsToGameModal()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenAddLevelScreenshotsToGameModal", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.addLevelScreenshotsToGameModalSource, UIModalManagerStackActions.ClearStack, new object[] { this.onScreenshotsToAddedUnityAction });
		}

		// Token: 0x040002DC RID: 732
		[Header("UIGameController")]
		[SerializeField]
		private UIGameView gameView;

		// Token: 0x040002DD RID: 733
		[SerializeField]
		private UIGameModelHandler modelHandler;

		// Token: 0x040002DE RID: 734
		[SerializeField]
		private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

		// Token: 0x040002DF RID: 735
		[SerializeField]
		private UIButton[] addScreenshotButtons = Array.Empty<UIButton>();

		// Token: 0x040002E0 RID: 736
		[SerializeField]
		private UIAddScreenshotsToGameModalView addLevelScreenshotsToGameModalSource;

		// Token: 0x040002E1 RID: 737
		[SerializeField]
		private UIDropHandler removeScreenshotDropHandler;

		// Token: 0x040002E2 RID: 738
		private UnityAction<List<ScreenshotFileInstances>> onScreenshotsToAddedUnityAction;
	}
}
