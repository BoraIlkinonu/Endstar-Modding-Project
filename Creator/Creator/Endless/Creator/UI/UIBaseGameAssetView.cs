using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.RightsManagement;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.TerrainCosmetics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020000D2 RID: 210
	public abstract class UIBaseGameAssetView : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x0600035A RID: 858 RVA: 0x000161FB File Offset: 0x000143FB
		// (set) Token: 0x0600035B RID: 859 RVA: 0x00016203 File Offset: 0x00014403
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x0600035C RID: 860 RVA: 0x0001620C File Offset: 0x0001440C
		// (set) Token: 0x0600035D RID: 861 RVA: 0x00016214 File Offset: 0x00014414
		public UIGameAsset Model { get; private set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x0600035E RID: 862 RVA: 0x0001621D File Offset: 0x0001441D
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x0600035F RID: 863 RVA: 0x00016225 File Offset: 0x00014425
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000360 RID: 864 RVA: 0x0001622D File Offset: 0x0001442D
		public virtual void View(UIGameAsset model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model.Name });
			}
			this.ViewAsync(model);
		}

		// Token: 0x06000361 RID: 865 RVA: 0x0001625C File Offset: 0x0001445C
		private async Task ViewAsync(UIGameAsset model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewAsync", new object[] { model.Name });
			}
			if (model != this.Model)
			{
				this.Clear();
			}
			this.Model = model;
			this.assetId = model.AssetID;
			this.assetVersion = model.AssetVersion;
			if (UIGameAsset.IsNullOrEmpty(model))
			{
				this.ShowSkeleton();
			}
			else
			{
				this.HideSkeleton();
				this.ViewAssetInformation(model);
				await this.LoadCreatorAsync();
			}
		}

		// Token: 0x06000362 RID: 866 RVA: 0x000162A8 File Offset: 0x000144A8
		private void ViewAssetInformation(UIGameAsset model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewAssetInformation", new object[] { model.Name });
			}
			this.displayNameText.text = string.Empty;
			this.iconImage.enabled = false;
			this.fileInstanceTexture2D.enabled = false;
			UIGameAssetTypes type = model.Type;
			if (type <= UIGameAssetTypes.SFX)
			{
				if (type - UIGameAssetTypes.Terrain <= 1)
				{
					this.ViewTerrainOrProp(model);
					return;
				}
				if (type != UIGameAssetTypes.SFX)
				{
					goto IL_007C;
				}
			}
			else if (type != UIGameAssetTypes.Ambient && type != UIGameAssetTypes.Music)
			{
				goto IL_007C;
			}
			this.ViewAudio(model);
			return;
			IL_007C:
			DebugUtility.LogNoEnumSupportError<UIGameAssetTypes>(this, "ViewAssetInformation", model.Type, Array.Empty<object>());
		}

		// Token: 0x06000363 RID: 867 RVA: 0x00016348 File Offset: 0x00014548
		private void ViewTerrainOrProp(UIGameAsset model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewTerrainOrProp", new object[] { model.Name });
			}
			if (model.Tileset != null)
			{
				this.assetId = model.Tileset.Asset.AssetID;
				this.displayNameText.text = model.Tileset.DisplayName;
				if (this.visualizeIcon)
				{
					this.iconImage.sprite = model.Tileset.DisplayIcon;
					this.iconImage.enabled = true;
					return;
				}
			}
			else
			{
				this.displayNameText.text = model.Asset.Name;
				if (!this.visualizeIcon)
				{
					return;
				}
				if (model.Icon || model.IconFileInstance != -1)
				{
					this.fileInstanceTexture2D.enabled = true;
					if (model.Icon)
					{
						this.fileInstanceTexture2D.View(model.Icon);
						return;
					}
					this.fileInstanceTexture2D.View(model.IconFileInstance);
					return;
				}
				else
				{
					DebugUtility.LogWarning(string.Format("No icon available on model {0}", model), this);
				}
			}
		}

		// Token: 0x06000364 RID: 868 RVA: 0x00016460 File Offset: 0x00014660
		private void ViewAudio(UIGameAsset model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewAudio", new object[] { model.Name });
			}
			AudioAsset audioAsset = (AudioAsset)model.Asset;
			this.displayNameText.text = audioAsset.Name;
			if (!this.visualizeIcon)
			{
				return;
			}
			this.fileInstanceTexture2D.enabled = true;
			if (model.Icon)
			{
				this.fileInstanceTexture2D.View(model.Icon);
				return;
			}
			this.fileInstanceTexture2D.View(model.IconFileInstance);
		}

		// Token: 0x06000365 RID: 869 RVA: 0x000164F4 File Offset: 0x000146F4
		private async Task LoadCreatorAsync()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "LoadCreatorAsync", Array.Empty<object>());
			}
			this.TrackRequest(UIBaseGameAssetView.RequestType.GetCreator);
			this.creatorProfileImage.sprite = null;
			this.creatorNameText.text = "Loading...";
			this.creatorOutlineImage.color = this.creatorTypeColorDictionary[UIGameAssetCreatorTypes.Official];
			try
			{
				GetAllRolesResult getAllRolesResult = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(this.assetId, new Action<IReadOnlyList<UserRole>>(this.OnUserRolesChanged), false);
				this.OnUserRolesChanged(getAllRolesResult.Roles);
			}
			catch (Exception ex)
			{
				DebugUtility.LogException(ex, this);
			}
			finally
			{
				this.UntrackRequest(UIBaseGameAssetView.RequestType.GetCreator);
			}
		}

		// Token: 0x06000366 RID: 870 RVA: 0x00016538 File Offset: 0x00014738
		public async Task GetAsset(string version, Action onComplete = null)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[]
				{
					"GetAsset ( version: ",
					version,
					", onComplete: ",
					onComplete.DebugIsNull(),
					" )"
				}), this);
			}
			this.TrackRequest(UIBaseGameAssetView.RequestType.GetAsset);
			try
			{
				UIGameAssetTypes type = this.Model.Type;
				switch (type)
				{
				case UIGameAssetTypes.Terrain:
					await this.LoadModel<TerrainTilesetCosmeticAsset>(version);
					goto IL_0200;
				case UIGameAssetTypes.Prop:
					await this.LoadModel<Prop>(version);
					goto IL_0200;
				case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
					goto IL_01DB;
				case UIGameAssetTypes.SFX:
					break;
				default:
					if (type != UIGameAssetTypes.Ambient && type != UIGameAssetTypes.Music)
					{
						goto IL_01DB;
					}
					break;
				}
				await this.LoadModel<AudioAsset>(version);
				goto IL_0200;
				IL_01DB:
				DebugUtility.LogNoEnumSupportError<UIGameAssetTypes>(this, "GetAsset", this.Model.Type, new object[] { version });
				IL_0200:;
			}
			catch (Exception ex)
			{
				DebugUtility.LogException(ex, this);
			}
			finally
			{
				this.UntrackRequest(UIBaseGameAssetView.RequestType.GetAsset);
			}
			if (onComplete != null)
			{
				onComplete();
			}
		}

		// Token: 0x06000367 RID: 871 RVA: 0x0001658C File Offset: 0x0001478C
		public virtual void Clear()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			if (this.visualizeIcon)
			{
				this.iconImage.enabled = false;
				this.iconImage.sprite = null;
				this.fileInstanceTexture2D.enabled = false;
				this.fileInstanceTexture2D.Clear();
			}
			this.Model = null;
			this.UntrackAllRequests();
		}

		// Token: 0x06000368 RID: 872 RVA: 0x000165F8 File Offset: 0x000147F8
		protected void TrackRequest(UIBaseGameAssetView.RequestType request)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "TrackRequest", new object[] { request });
			}
			if (!this.requestsInProgress.Add(request))
			{
				DebugUtility.LogWarning(string.Format("{0} is already in progress.", request), this);
				return;
			}
			if (this.requestsInProgress.Count == 1)
			{
				this.OnLoadingStarted.Invoke();
			}
		}

		// Token: 0x06000369 RID: 873 RVA: 0x00016668 File Offset: 0x00014868
		protected void UntrackRequest(UIBaseGameAssetView.RequestType request)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UntrackRequest", new object[] { request });
			}
			if (!this.requestsInProgress.Remove(request))
			{
				return;
			}
			if (this.requestsInProgress.Count == 0)
			{
				this.OnLoadingEnded.Invoke();
			}
		}

		// Token: 0x0600036A RID: 874 RVA: 0x000166C0 File Offset: 0x000148C0
		protected void OnUserRolesChanged(IReadOnlyList<UserRole> roles)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserRolesChanged", new object[] { roles.Count });
			}
			bool flag = roles.Any((UserRole r) => r.UserId == this.endlessStudiosUserId.InternalId);
			this.ViewCreatorType(flag);
		}

		// Token: 0x0600036B RID: 875 RVA: 0x00016710 File Offset: 0x00014910
		private void UntrackAllRequests()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UntrackAllRequests", Array.Empty<object>());
			}
			foreach (UIBaseGameAssetView.RequestType requestType in this.requestsInProgress.ToList<UIBaseGameAssetView.RequestType>())
			{
				this.UntrackRequest(requestType);
			}
		}

		// Token: 0x0600036C RID: 876 RVA: 0x00016780 File Offset: 0x00014980
		private void ViewCreatorType(bool isOfficial)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewCreatorType", new object[] { isOfficial });
			}
			this.creatorProfileImage.sprite = (isOfficial ? this.officialCreatorSprite : this.communityCreatorSprite);
			this.creatorNameText.text = (isOfficial ? "Endless Studios" : "Community");
			UIGameAssetCreatorTypes uigameAssetCreatorTypes = (isOfficial ? UIGameAssetCreatorTypes.Official : UIGameAssetCreatorTypes.Community);
			this.creatorOutlineImage.color = this.creatorTypeColorDictionary[uigameAssetCreatorTypes];
		}

		// Token: 0x0600036D RID: 877 RVA: 0x00016804 File Offset: 0x00014A04
		private async Task LoadModel<T>(string version) where T : Asset
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "LoadModel", new object[] { version });
			}
			AssetCacheResult<T> assetCacheResult = await EndlessAssetCache.GetAssetAsync<T>(this.assetId, version);
			if (assetCacheResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIBaseGameAssetVew_GetAsset, assetCacheResult.GetErrorMessage(), true, false);
			}
			else
			{
				UIGameAssetTypes type = this.Model.Type;
				switch (type)
				{
				case UIGameAssetTypes.Terrain:
					this.Model = new UIGameAsset(assetCacheResult.Asset as TerrainTilesetCosmeticAsset);
					goto IL_01C6;
				case UIGameAssetTypes.Prop:
					this.Model = new UIGameAsset(assetCacheResult.Asset as Prop);
					goto IL_01C6;
				case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
					goto IL_01AB;
				case UIGameAssetTypes.SFX:
					break;
				default:
					if (type != UIGameAssetTypes.Ambient && type != UIGameAssetTypes.Music)
					{
						goto IL_01AB;
					}
					break;
				}
				AudioAsset audioAsset = assetCacheResult.Asset as AudioAsset;
				switch (audioAsset.AudioCategory)
				{
				case AudioCategory.Music:
					this.Model = new UIGameAsset(audioAsset, UIGameAssetTypes.Music);
					goto IL_01C6;
				case AudioCategory.SFX:
					this.Model = new UIGameAsset(audioAsset, UIGameAssetTypes.SFX);
					goto IL_01C6;
				case AudioCategory.Ambient:
					this.Model = new UIGameAsset(audioAsset, UIGameAssetTypes.Ambient);
					goto IL_01C6;
				default:
					DebugUtility.LogNoEnumSupportError<AudioCategory>(this, "LoadModel", audioAsset.AudioCategory, Array.Empty<object>());
					goto IL_01C6;
				}
				IL_01AB:
				DebugUtility.LogNoEnumSupportError<UIGameAssetTypes>(this, "LoadModel", this.Model.Type, Array.Empty<object>());
				IL_01C6:;
			}
		}

		// Token: 0x0600036E RID: 878 RVA: 0x0001684F File Offset: 0x00014A4F
		private void ShowSkeleton()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ShowSkeleton", Array.Empty<object>());
			}
			this.TrackRequest(UIBaseGameAssetView.RequestType.SkeletonLoading);
			this.skeletonLoadingVisual.SetActive(true);
		}

		// Token: 0x0600036F RID: 879 RVA: 0x0001687C File Offset: 0x00014A7C
		private void HideSkeleton()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideSkeleton", Array.Empty<object>());
			}
			if (!this.skeletonLoadingVisual.activeSelf)
			{
				return;
			}
			this.skeletonLoadingVisual.SetActive(false);
			this.UntrackRequest(UIBaseGameAssetView.RequestType.SkeletonLoading);
		}

		// Token: 0x04000394 RID: 916
		[SerializeField]
		protected UIGameAssetTypeStyleDictionary gameAssetTypeStyleDictionary;

		// Token: 0x04000395 RID: 917
		[SerializeField]
		private EndlessStudiosUserId endlessStudiosUserId;

		// Token: 0x04000396 RID: 918
		[Header("Visuals")]
		[SerializeField]
		private GameObject skeletonLoadingVisual;

		// Token: 0x04000397 RID: 919
		[SerializeField]
		private bool visualizeIcon = true;

		// Token: 0x04000398 RID: 920
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x04000399 RID: 921
		[Header("Icon")]
		[SerializeField]
		private Image iconImage;

		// Token: 0x0400039A RID: 922
		[SerializeField]
		private UIFileInstanceTexture2DView fileInstanceTexture2D;

		// Token: 0x0400039B RID: 923
		[Header("Creator Profile")]
		[SerializeField]
		private TextMeshProUGUI creatorNameText;

		// Token: 0x0400039C RID: 924
		[SerializeField]
		private Image creatorProfileImage;

		// Token: 0x0400039D RID: 925
		[SerializeField]
		private Sprite officialCreatorSprite;

		// Token: 0x0400039E RID: 926
		[SerializeField]
		private Sprite communityCreatorSprite;

		// Token: 0x0400039F RID: 927
		[SerializeField]
		private Image creatorOutlineImage;

		// Token: 0x040003A0 RID: 928
		[SerializeField]
		private UIGameAssetCreatorTypesColorDictionary creatorTypeColorDictionary;

		// Token: 0x040003A1 RID: 929
		private readonly HashSet<UIBaseGameAssetView.RequestType> requestsInProgress = new HashSet<UIBaseGameAssetView.RequestType>();

		// Token: 0x040003A2 RID: 930
		protected string assetId;

		// Token: 0x040003A3 RID: 931
		protected string assetVersion;

		// Token: 0x020000D3 RID: 211
		protected enum RequestType
		{
			// Token: 0x040003A9 RID: 937
			SkeletonLoading,
			// Token: 0x040003AA RID: 938
			GetVersions,
			// Token: 0x040003AB RID: 939
			GetAsset,
			// Token: 0x040003AC RID: 940
			GetCreator
		}
	}
}
