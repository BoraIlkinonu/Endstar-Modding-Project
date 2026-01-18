using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002D0 RID: 720
	public class UIGameEditorWindowView : UIBaseWindowView, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000C2C RID: 3116 RVA: 0x0003A426 File Offset: 0x00038626
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x06000C2D RID: 3117 RVA: 0x0003A42E File Offset: 0x0003862E
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x06000C2E RID: 3118 RVA: 0x0003A436 File Offset: 0x00038636
		// (set) Token: 0x06000C2F RID: 3119 RVA: 0x0003A43E File Offset: 0x0003863E
		public Game Game { get; private set; }

		// Token: 0x17000194 RID: 404
		// (get) Token: 0x06000C30 RID: 3120 RVA: 0x0003A447 File Offset: 0x00038647
		// (set) Token: 0x06000C31 RID: 3121 RVA: 0x0003A44F File Offset: 0x0003864F
		public Roles LocalClientRole { get; private set; } = Roles.None;

		// Token: 0x06000C32 RID: 3122 RVA: 0x0003A458 File Offset: 0x00038658
		protected override void Start()
		{
			base.Start();
			this.layoutables.RequestLayout();
		}

		// Token: 0x06000C33 RID: 3123 RVA: 0x0003A46B File Offset: 0x0003866B
		public static UIGameEditorWindowView Display(Transform parent = null)
		{
			return (UIGameEditorWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIGameEditorWindowView>(parent, null);
		}

		// Token: 0x06000C34 RID: 3124 RVA: 0x0003A47E File Offset: 0x0003867E
		public override void OnSpawn()
		{
			base.OnSpawn();
			MonoBehaviourSingleton<UIStartMatchHelper>.Instance.EndMatchAndStartNewMatchWithCachedDataUnityEvent.AddListener(new UnityAction(this.DisplayLoadingSpinner));
			MatchmakingClientController.MatchLeave += this.OnMatchLeave;
		}

		// Token: 0x06000C35 RID: 3125 RVA: 0x0003A4B4 File Offset: 0x000386B4
		public override void OnDespawn()
		{
			base.OnDespawn();
			MonoBehaviourSingleton<UIStartMatchHelper>.Instance.EndMatchAndStartNewMatchWithCachedDataUnityEvent.RemoveListener(new UnityAction(this.DisplayLoadingSpinner));
			MatchmakingClientController.MatchLeave -= this.OnMatchLeave;
			this.HideLoadingSpinner();
			this.LocalClientRole = Roles.None;
			this.Game = null;
			this.levelStateListModel.Clear(true);
			this.userRolesModel.Clear();
			UIScreenObserver.OnSizeChange = (Action)Delegate.Remove(UIScreenObserver.OnSizeChange, new Action(this.layoutables.RequestLayout));
		}

		// Token: 0x06000C36 RID: 3126 RVA: 0x0003A544 File Offset: 0x00038744
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.Game = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			this.gameModelHandler.Set(this.Game);
			this.levelStateListModel.Clear(true);
			this.userRolesModel.Initialize(this.Game.AssetID, this.Game.Name, SerializableGuid.Empty, AssetContexts.GameOrLevelEditor);
			this.LoadLevelData();
			UIScreenObserver.OnSizeChange = (Action)Delegate.Combine(UIScreenObserver.OnSizeChange, new Action(this.layoutables.RequestLayout));
		}

		// Token: 0x06000C37 RID: 3127 RVA: 0x0003A5DC File Offset: 0x000387DC
		private async void LoadLevelData()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "LoadLevelData", Array.Empty<object>());
			}
			this.DisplayLoadingSpinner();
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(this.Game.AssetID, this.Game.AssetVersion, new AssetParams(LevelAsset.QueryString, true, null), false, 10);
			this.OnLoadingEnded.Invoke();
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameEditorWindowView_GetLevels, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				var <>f__AnonymousType = new
				{
					Levels = Array.Empty<LevelAsset>()
				};
				var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), <>f__AnonymousType);
				this.levelStateListModel.SetGame(this.Game);
				List<LevelAsset> list = <>f__AnonymousType2.Levels.ToList<LevelAsset>();
				this.levelStateListModel.Set(list, true);
			}
		}

		// Token: 0x06000C38 RID: 3128 RVA: 0x0003A614 File Offset: 0x00038814
		public void OnLevelAdded(LevelState levelState)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLevelAdded", new object[] { levelState });
			}
			LevelAsset levelAsset = new LevelAsset(levelState);
			this.levelStateListModel.Add(levelAsset, true);
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x0003A652 File Offset: 0x00038852
		private void DisplayLoadingSpinner()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayLoadingSpinner", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
		}

		// Token: 0x06000C3A RID: 3130 RVA: 0x0003A677 File Offset: 0x00038877
		private void OnMatchLeave(string reason)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnMatchLeave", new object[] { reason });
			}
			this.HideLoadingSpinner();
		}

		// Token: 0x06000C3B RID: 3131 RVA: 0x0003A69C File Offset: 0x0003889C
		private void HideLoadingSpinner()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideLoadingSpinner", Array.Empty<object>());
			}
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x04000A84 RID: 2692
		[Header("UIGameEditorWindowView")]
		[SerializeField]
		private UIGameModelHandler gameModelHandler;

		// Token: 0x04000A85 RID: 2693
		[SerializeField]
		private UILevelAssetListModel levelStateListModel;

		// Token: 0x04000A86 RID: 2694
		[SerializeField]
		private UIUserRolesModel userRolesModel;

		// Token: 0x04000A87 RID: 2695
		[SerializeField]
		private InterfaceReference<IUILayoutable>[] layoutables = Array.Empty<InterfaceReference<IUILayoutable>>();
	}
}
