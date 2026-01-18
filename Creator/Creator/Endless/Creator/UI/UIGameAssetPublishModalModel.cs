using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001AA RID: 426
	public class UIGameAssetPublishModalModel : IUILoadingSpinnerViewCompatible
	{
		// Token: 0x06000640 RID: 1600 RVA: 0x000203F9 File Offset: 0x0001E5F9
		public UIGameAssetPublishModalModel(UIGameAsset gameAsset)
		{
			this.GameAsset = gameAsset;
			this.LoadVersions();
		}

		// Token: 0x170000A6 RID: 166
		// (get) Token: 0x06000641 RID: 1601 RVA: 0x0002042F File Offset: 0x0001E62F
		public UIGameAsset GameAsset { get; }

		// Token: 0x170000A7 RID: 167
		// (get) Token: 0x06000642 RID: 1602 RVA: 0x00020437 File Offset: 0x0001E637
		// (set) Token: 0x06000643 RID: 1603 RVA: 0x0002043F File Offset: 0x0001E63F
		public string VersionBeta { get; private set; }

		// Token: 0x170000A8 RID: 168
		// (get) Token: 0x06000644 RID: 1604 RVA: 0x00020448 File Offset: 0x0001E648
		// (set) Token: 0x06000645 RID: 1605 RVA: 0x00020450 File Offset: 0x0001E650
		public string VersionPublic { get; private set; }

		// Token: 0x170000A9 RID: 169
		// (get) Token: 0x06000646 RID: 1606 RVA: 0x00020459 File Offset: 0x0001E659
		// (set) Token: 0x06000647 RID: 1607 RVA: 0x00020461 File Offset: 0x0001E661
		public bool IsLoading { get; private set; }

		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000648 RID: 1608 RVA: 0x0002046A File Offset: 0x0001E66A
		public UnityEvent<List<string>, List<PublishedVersion>> OnModelChanged { get; } = new UnityEvent<List<string>, List<PublishedVersion>>();

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000649 RID: 1609 RVA: 0x00020472 File Offset: 0x0001E672
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x0600064A RID: 1610 RVA: 0x0002047A File Offset: 0x0001E67A
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x0600064B RID: 1611 RVA: 0x00020482 File Offset: 0x0001E682
		public void LoadVersions()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "LoadVersions", Array.Empty<object>());
			}
			this.LoadVersionsAsync();
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x000204A4 File Offset: 0x0001E6A4
		public void ChangePublishState(string assetId, string version, UIPublishStates targetState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ChangePublishState", new object[] { assetId, version, targetState });
			}
			bool flag = false;
			string text = string.Empty;
			switch (targetState)
			{
			case UIPublishStates.Beta:
				flag = !this.VersionBeta.IsNullOrEmptyOrWhiteSpace();
				text = this.VersionBeta;
				break;
			case UIPublishStates.Public:
				flag = !this.VersionPublic.IsNullOrEmptyOrWhiteSpace();
				text = this.VersionPublic;
				break;
			case UIPublishStates.Unpublished:
				break;
			default:
				throw new ArgumentOutOfRangeException("targetState", targetState, null);
			}
			if (text == version)
			{
				return;
			}
			this.ChangePublishStateAsync(assetId, version, targetState, flag, text);
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x00020550 File Offset: 0x0001E750
		public void Unpublish(UIPublishStates state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Unpublish", new object[] { state });
			}
			if (state == UIPublishStates.Beta)
			{
				this.UnpublishAsync(this.VersionBeta);
				return;
			}
			if (state != UIPublishStates.Public)
			{
				throw new ArgumentOutOfRangeException("state", state, null);
			}
			this.UnpublishAsync(this.VersionPublic);
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x000205B8 File Offset: 0x0001E7B8
		private async Task LoadVersionsAsync()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "LoadVersionsAsync", Array.Empty<object>());
			}
			this.SetLoading(true);
			try
			{
				EndlessCloudService cloudService = EndlessServices.Instance.CloudService;
				SerializableGuid serializableGuid = this.GameAsset.AssetID;
				Task<GraphQlResult> getVersionsTask = cloudService.GetVersionsAsync(serializableGuid, false);
				Task<GraphQlResult> getPublishedVersionsOfAssetTask = cloudService.GetPublishedVersionsOfAssetAsync(serializableGuid, false);
				await Task.WhenAll<GraphQlResult>(new Task<GraphQlResult>[] { getVersionsTask, getPublishedVersionsOfAssetTask });
				GraphQlResult result = getVersionsTask.Result;
				if (result.HasErrors)
				{
					ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalView_RetrievingAssetVersions, result.GetErrorMessage(0), true, false);
				}
				else
				{
					GraphQlResult result2 = getPublishedVersionsOfAssetTask.Result;
					if (result2.HasErrors)
					{
						ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalView_GetPublishedVersionsOfAsset, result2.GetErrorMessage(0), true, false);
					}
					else
					{
						List<string> list;
						try
						{
							list = VersionUtilities.GetParsedAndOrderedVersions(result.GetDataMember()).ToList<string>();
						}
						catch (Exception ex)
						{
							ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalView_ParsingVersions, ex, true, false);
							return;
						}
						List<PublishedVersion> list2 = JsonConvert.DeserializeObject<List<PublishedVersion>>(result2.GetDataMember().ToString());
						foreach (PublishedVersion publishedVersion in list2)
						{
							if (publishedVersion.State == UIGameAssetPublishModalModel.betaState)
							{
								this.VersionBeta = publishedVersion.AssetVersion;
							}
							if (publishedVersion.State == UIGameAssetPublishModalModel.publicState)
							{
								this.VersionPublic = publishedVersion.AssetVersion;
							}
						}
						if (this.verboseLogging)
						{
							DebugUtility.Log(string.Format("{0}: {1}", "versions", list.Count<string>()), null);
							DebugUtility.DebugEnumerable<PublishedVersion>("publishedVersions", list2, null);
						}
						this.OnModelChanged.Invoke(list, list2);
						getVersionsTask = null;
						getPublishedVersionsOfAssetTask = null;
					}
				}
			}
			catch (Exception ex2)
			{
				DebugUtility.LogException(ex2, null);
			}
			finally
			{
				this.SetLoading(false);
			}
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x000205FC File Offset: 0x0001E7FC
		private async Task ChangePublishStateAsync(string assetId, string version, UIPublishStates targetState, bool needsUnpublish, string previousVersion)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ChangePublishStateAsync", new object[] { assetId, version, targetState, needsUnpublish, previousVersion });
			}
			if (needsUnpublish)
			{
				await this.UnpublishAsync(previousVersion);
			}
			this.SetLoading(true);
			try
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetPublishStateOnAssetAsync(assetId, version, targetState.ToEndlessCloudServicesCompatibleString(), false);
				if (graphQlResult.HasErrors)
				{
					ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalController_PublishGame, graphQlResult.GetErrorMessage(0), true, false);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalController_PublishGame, ex, true, false);
			}
			finally
			{
				this.SetLoading(false);
				this.LoadVersions();
			}
			DebugUtility.LogMethodWithAppension(this, "ChangePublishStateAsync", "COMPLETE", new object[] { assetId, version, targetState });
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x0002066C File Offset: 0x0001E86C
		private async Task UnpublishAsync(string versionToUnpublish)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UnpublishAsync", new object[] { versionToUnpublish });
			}
			this.SetLoading(true);
			try
			{
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.SetPublishStateOnAssetAsync(this.GameAsset.AssetID, versionToUnpublish, UIGameAssetPublishModalModel.unpublishedState, false);
				if (graphQlResult.HasErrors)
				{
					ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalController_UnpublishGame, graphQlResult.GetErrorMessage(0), true, false);
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetPublishModalController_UnpublishGame, ex, true, false);
			}
			finally
			{
				this.SetLoading(false);
				this.LoadVersions();
			}
		}

		// Token: 0x06000651 RID: 1617 RVA: 0x000206B8 File Offset: 0x0001E8B8
		private void SetLoading(bool loading)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLoading", new object[] { loading });
			}
			this.IsLoading = loading;
			if (loading)
			{
				this.OnLoadingStarted.Invoke();
				return;
			}
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x0400058D RID: 1421
		private static readonly string betaState = UIPublishStates.Beta.ToEndlessCloudServicesCompatibleString();

		// Token: 0x0400058E RID: 1422
		private static readonly string publicState = UIPublishStates.Public.ToEndlessCloudServicesCompatibleString();

		// Token: 0x0400058F RID: 1423
		private static readonly string unpublishedState = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();

		// Token: 0x04000590 RID: 1424
		private readonly bool verboseLogging;
	}
}
