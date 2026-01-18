using System;
using System.Threading.Tasks;
using Endless.Creator.UI;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Shared.Matchmaking;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000387 RID: 903
	public class SaveLoadManager
	{
		// Token: 0x17000294 RID: 660
		// (get) Token: 0x0600116F RID: 4463 RVA: 0x0005518A File Offset: 0x0005338A
		// (set) Token: 0x06001170 RID: 4464 RVA: 0x00055192 File Offset: 0x00053392
		public SerializableGuid CachedLevelStateId { get; private set; }

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06001171 RID: 4465 RVA: 0x0005519B File Offset: 0x0005339B
		// (set) Token: 0x06001172 RID: 4466 RVA: 0x000551A3 File Offset: 0x000533A3
		public string CachedLevelStateVersion { get; private set; }

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x06001173 RID: 4467 RVA: 0x000551AC File Offset: 0x000533AC
		public bool PendingSave
		{
			get
			{
				return this.pendingLevelSave;
			}
		}

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x06001174 RID: 4468 RVA: 0x000551B4 File Offset: 0x000533B4
		public bool SaveInProgress
		{
			get
			{
				return this.saveInProgress;
			}
		}

		// Token: 0x06001175 RID: 4469 RVA: 0x000551BC File Offset: 0x000533BC
		public void SetCachedLevelState(string levelId, string assetVersion)
		{
			Stage activeStage = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
			if (activeStage && activeStage.LevelState.AssetID == levelId)
			{
				activeStage.UpdateVersion(assetVersion);
				NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateLevelVersion(assetVersion, this.PendingSave);
			}
			this.CachedLevelStateId = levelId;
			this.CachedLevelStateVersion = assetVersion;
		}

		// Token: 0x06001176 RID: 4470 RVA: 0x0005521C File Offset: 0x0005341C
		public void UpdateSaveLoad()
		{
			if (NetworkManager.Singleton.IsServer)
			{
				float num = Time.realtimeSinceStartup - this.lastSaveTimeRequest;
				float num2 = Time.realtimeSinceStartup - this.initialSaveTimeRequest;
				if (this.pendingLevelSave && !this.saveInProgress && (num > 60f || num2 > 300f))
				{
					this.pendingLevelSave = false;
					this.SubmitSave();
				}
			}
		}

		// Token: 0x06001177 RID: 4471 RVA: 0x0005527C File Offset: 0x0005347C
		private async void SubmitSave()
		{
			await this.SubmitSaveAsync();
		}

		// Token: 0x06001178 RID: 4472 RVA: 0x000552B4 File Offset: 0x000534B4
		private async Task SubmitSaveAsync()
		{
			if (MonoBehaviourSingleton<StageManager>.Instance && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState != null)
			{
				this.saveInProgress = true;
				LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Copy();
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.RevisionMetaData.Changes.Clear();
				levelState.AssetVersion = string.Empty;
				if (MatchmakingClientController.Instance != null)
				{
					SaveLoadManager.<>c__DisplayClass20_0 CS$<>8__locals1 = new SaveLoadManager.<>c__DisplayClass20_0();
					NetworkBehaviourSingleton<UISaveStatusManager>.Instance.SetSaveIndicator(true);
					GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(levelState, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, true, false);
					CS$<>8__locals1.response = graphQlResult;
					if (!CS$<>8__locals1.response.HasErrors)
					{
						ValueTuple<string, string> valueTuple = await Task.Run<ValueTuple<string, string>>(delegate
						{
							LevelState levelState2 = LevelStateLoader.Load(CS$<>8__locals1.response.GetDataMember().ToString());
							return new ValueTuple<string, string>(levelState2.AssetID, levelState2.AssetVersion);
						});
						this.SetCachedLevelState(valueTuple.Item1, valueTuple.Item2);
					}
					else
					{
						Debug.LogException(CS$<>8__locals1.response.GetErrorMessage(0));
					}
					CS$<>8__locals1 = null;
				}
				this.saveInProgress = false;
				NetworkBehaviourSingleton<UISaveStatusManager>.Instance.SetSaveIndicator(false);
			}
		}

		// Token: 0x06001179 RID: 4473 RVA: 0x000552F8 File Offset: 0x000534F8
		public void SaveLevel()
		{
			if (!this.pendingLevelSave)
			{
				this.initialSaveTimeRequest = Time.realtimeSinceStartup;
				this.lastSaveTimeRequest = Time.realtimeSinceStartup;
				this.pendingLevelSave = true;
				NetworkBehaviourSingleton<UISaveStatusManager>.Instance.UpdateLevelVersion(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.AssetVersion, true);
				Debug.Log("Setting to save a level!");
				return;
			}
			this.lastSaveTimeRequest = Time.realtimeSinceStartup;
		}

		// Token: 0x0600117A RID: 4474 RVA: 0x00055360 File Offset: 0x00053560
		public async void ForceSaveIfNeeded()
		{
			await this.ForceSaveIfNeededAsync();
		}

		// Token: 0x0600117B RID: 4475 RVA: 0x00055398 File Offset: 0x00053598
		public async Task ForceSaveIfNeededAsync()
		{
			if (this.pendingLevelSave)
			{
				this.pendingLevelSave = false;
				Debug.Log("Forcing Save!");
				await this.SubmitSaveAsync();
			}
		}

		// Token: 0x04000E49 RID: 3657
		private const float MINIMUM_DELAY_IN_SECONDS_BEFORE_SAVE = 60f;

		// Token: 0x04000E4A RID: 3658
		private bool pendingLevelSave;

		// Token: 0x04000E4B RID: 3659
		private bool saveInProgress;

		// Token: 0x04000E4C RID: 3660
		private float initialSaveTimeRequest;

		// Token: 0x04000E4D RID: 3661
		private float lastSaveTimeRequest;
	}
}
