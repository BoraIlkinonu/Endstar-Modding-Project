using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001BE RID: 446
	public class UILevelStateSelectionModalView : UIEscapableModalView, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x0600069F RID: 1695 RVA: 0x00021FF2 File Offset: 0x000201F2
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x060006A0 RID: 1696 RVA: 0x00021FFA File Offset: 0x000201FA
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x060006A1 RID: 1697 RVA: 0x00022002 File Offset: 0x00020202
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.levelStateListModel.Clear(true);
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x00022018 File Offset: 0x00020218
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.mainMenuGameModel = modalData[0] as MainMenuGameModel;
			this.openLevelInAdminMode = (bool)modalData[1];
			if (base.VerboseLogging)
			{
				DebugUtility.Log(JsonUtility.ToJson(this.mainMenuGameModel), this);
			}
			this.levelStateListModel.SetGame(this.mainMenuGameModel);
			this.levelStateListModel.Set(new List<LevelAsset>(), true);
			this.levelStateListModel.SetOpenLevelInAdminMode(this.openLevelInAdminMode);
			this.FetchLevels();
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x0002209C File Offset: 0x0002029C
		private async void FetchLevels()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "FetchLevels", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			SerializableGuid serializableGuid = this.mainMenuGameModel.AssetID;
			string empty = string.Empty;
			AssetParams assetParams = new AssetParams(LevelAsset.QueryString, true, null);
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "assetId", serializableGuid), this);
				DebugUtility.Log("version: " + empty, this);
				DebugUtility.Log(string.Format("{0}: {1}", "assetParams", assetParams), this);
			}
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(serializableGuid, empty, assetParams, false, 10);
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.LevelStateSelectionModalView_FetchLevelsFailure, graphQlResult.GetErrorMessage(0), true, false);
			}
			else
			{
				var <>f__AnonymousType = new
				{
					levels = new List<LevelAsset>()
				};
				var <>f__AnonymousType2 = JsonConvert.DeserializeAnonymousType(graphQlResult.GetDataMember().ToString(), <>f__AnonymousType);
				this.levelStateListModel.Set(<>f__AnonymousType2.levels, true);
				this.OnLoadingEnded.Invoke();
			}
		}

		// Token: 0x040005EE RID: 1518
		[Header("UILevelStateSelectionModalView")]
		[SerializeField]
		private UILevelAssetListModel levelStateListModel;

		// Token: 0x040005EF RID: 1519
		private MainMenuGameModel mainMenuGameModel;

		// Token: 0x040005F0 RID: 1520
		private bool openLevelInAdminMode;
	}
}
