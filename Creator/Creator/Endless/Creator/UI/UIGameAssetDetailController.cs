using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Data;
using Endless.FileManagement;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.GraphQl;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.TerrainCosmetics;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000C2 RID: 194
	public class UIGameAssetDetailController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600030B RID: 779 RVA: 0x000135AA File Offset: 0x000117AA
		private UIGameAsset Model
		{
			get
			{
				return this.view.Model;
			}
		}

		// Token: 0x0600030C RID: 780 RVA: 0x000135B8 File Offset: 0x000117B8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.versionDropdown.OnValueChanged.AddListener(new UnityAction(this.ViewVersionAsync));
			this.playAudioClipButton.onClick.AddListener(new UnityAction(this.PlayAudioClip));
			this.stopAudioClipButton.onClick.AddListener(new UnityAction(this.StopAudioClip));
			this.moderatorateButton.onClick.AddListener(new UnityAction(this.DisplayAssetModerationModal));
			this.addButton.onClick.AddListener(new UnityAction(this.Add));
			this.applyVersionButton.onClick.AddListener(new UnityAction(this.ApplyVersion));
			this.publishButton.onClick.AddListener(new UnityAction(this.Publish));
			this.saveAsNewVersionButton.onClick.AddListener(new UnityAction(this.SaveAsNewVersion));
			this.discardButton.onClick.AddListener(new UnityAction(this.Discard));
		}

		// Token: 0x0600030D RID: 781 RVA: 0x000136DC File Offset: 0x000118DC
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.audioSource.isPlaying)
			{
				this.audioSource.Stop();
				this.audioSource.clip = null;
			}
			if (this.waitForAudioClipAndViewPlayButtonCoroutine != null)
			{
				base.StopCoroutine(this.waitForAudioClipAndViewPlayButtonCoroutine);
				this.waitForAudioClipAndViewPlayButtonCoroutine = null;
			}
			if (this.audioClip)
			{
				this.audioClip = null;
				AudioAsset audioAsset = (AudioAsset)this.Model.Asset;
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(base.gameObject, audioAsset.AudioFileInstanceId);
			}
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.viewVersionCancellationTokenSource);
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.playAudioClipCancellationTokenSource);
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.addCancellationTokenSource);
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.applyVersionCancellationTokenSource);
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.saveAsNewVersionCts);
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600030E RID: 782 RVA: 0x000137B2 File Offset: 0x000119B2
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600030F RID: 783 RVA: 0x000137BA File Offset: 0x000119BA
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000310 RID: 784 RVA: 0x000137C4 File Offset: 0x000119C4
		private void ViewVersionAsync()
		{
			UIGameAssetDetailController.<ViewVersionAsync>d__35 <ViewVersionAsync>d__;
			<ViewVersionAsync>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<ViewVersionAsync>d__.<>4__this = this;
			<ViewVersionAsync>d__.<>1__state = -1;
			<ViewVersionAsync>d__.<>t__builder.Start<UIGameAssetDetailController.<ViewVersionAsync>d__35>(ref <ViewVersionAsync>d__);
		}

		// Token: 0x06000311 RID: 785 RVA: 0x000137FC File Offset: 0x000119FC
		private void PlayAudioClip()
		{
			UIGameAssetDetailController.<PlayAudioClip>d__36 <PlayAudioClip>d__;
			<PlayAudioClip>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<PlayAudioClip>d__.<>4__this = this;
			<PlayAudioClip>d__.<>1__state = -1;
			<PlayAudioClip>d__.<>t__builder.Start<UIGameAssetDetailController.<PlayAudioClip>d__36>(ref <PlayAudioClip>d__);
		}

		// Token: 0x06000312 RID: 786 RVA: 0x00013833 File Offset: 0x00011A33
		private IEnumerator WaitForAudioClipAndViewPlayButton()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitForAudioClipAndViewPlayButton", Array.Empty<object>());
			}
			yield return new WaitForSeconds(this.audioSource.clip.length);
			this.view.ViewAudioButton(false);
			this.waitForAudioClipAndViewPlayButtonCoroutine = null;
			yield break;
		}

		// Token: 0x06000313 RID: 787 RVA: 0x00013844 File Offset: 0x00011A44
		private void StopAudioClip()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "StopAudioClip", Array.Empty<object>());
			}
			if (this.waitForAudioClipAndViewPlayButtonCoroutine != null)
			{
				base.StopCoroutine(this.waitForAudioClipAndViewPlayButtonCoroutine);
				this.waitForAudioClipAndViewPlayButtonCoroutine = null;
			}
			this.view.ViewAudioButton(false);
			this.audioSource.Stop();
		}

		// Token: 0x06000314 RID: 788 RVA: 0x0001389B File Offset: 0x00011A9B
		private void DisplayAssetModerationModal()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayAssetModerationModal", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.assetModerationModalSource, UIModalManagerStackActions.MaintainStack, new object[] { this.view.Model });
		}

		// Token: 0x06000315 RID: 789 RVA: 0x000138DC File Offset: 0x00011ADC
		private async void Add()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Add", Array.Empty<object>());
			}
			CancellationTokenSourceUtility.RecreateTokenSource(ref this.addCancellationTokenSource);
			CancellationToken token = this.addCancellationTokenSource.Token;
			this.OnLoadingStarted.Invoke();
			try
			{
				string value = this.versionDropdown.Value;
				token.ThrowIfCancellationRequested();
				UIGameAssetTypes type = this.Model.Type;
				switch (type)
				{
				case UIGameAssetTypes.Terrain:
				{
					await MonoBehaviourSingleton<GameEditor>.Instance.AddTerrainUsageToGameLibrary(this.Model.AssetID, value);
					int terrainIndex = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.GetTerrainIndex(this.Model.AssetID);
					PaintingTool paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PaintingTool>();
					if (paintingTool.IsActive)
					{
						paintingTool.SetActiveTilesetIndex(terrainIndex);
						goto IL_02DB;
					}
					goto IL_02DB;
				}
				case UIGameAssetTypes.Prop:
				{
					AssetReference assetReference = new AssetReference
					{
						AssetID = this.Model.AssetID,
						AssetVersion = value,
						AssetType = "prop"
					};
					TaskAwaiter<bool> taskAwaiter = MonoBehaviourSingleton<GameEditor>.Instance.AddPropToGameLibrary(assetReference).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<bool> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (!taskAwaiter.GetResult())
					{
						goto IL_02DB;
					}
					PropTool propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
					if (propTool.IsActive)
					{
						propTool.UpdateSelectedAssetId(this.Model.AssetID);
						goto IL_02DB;
					}
					goto IL_02DB;
				}
				case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
					goto IL_02C0;
				case UIGameAssetTypes.SFX:
					break;
				default:
					if (type != UIGameAssetTypes.Ambient && type != UIGameAssetTypes.Music)
					{
						goto IL_02C0;
					}
					break;
				}
				AssetReference assetReference2 = new AssetReference
				{
					AssetID = this.Model.AssetID,
					AssetVersion = value,
					AssetType = "audio"
				};
				await MonoBehaviourSingleton<GameEditor>.Instance.AddAudioToGameLibrary(assetReference2);
				goto IL_02DB;
				IL_02C0:
				DebugUtility.LogNoEnumSupportError<UIGameAssetTypes>(this, "Add", this.Model.Type, Array.Empty<object>());
				IL_02DB:;
			}
			catch (OperationCanceledException)
			{
				if (this.verboseLogging)
				{
					Debug.Log("Add was cancelled", this);
				}
				this.OnLoadingEnded.Invoke();
				return;
			}
			catch (Exception ex)
			{
				DebugUtility.LogException(ex, this);
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_AddAsset, ex, true, false);
			}
			this.OnLoadingEnded.Invoke();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			Action assetAdded = UIGameAssetDetailController.AssetAdded;
			if (assetAdded != null)
			{
				assetAdded();
			}
		}

		// Token: 0x06000316 RID: 790 RVA: 0x00013914 File Offset: 0x00011B14
		private void Publish()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Publish", Array.Empty<object>());
			}
			UIGameAssetPublishModalModel uigameAssetPublishModalModel = new UIGameAssetPublishModalModel(this.Model);
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.gameAssetPublishModalSource, UIModalManagerStackActions.MaintainStack, new object[] { uigameAssetPublishModalModel });
		}

		// Token: 0x06000317 RID: 791 RVA: 0x00013960 File Offset: 0x00011B60
		private async void ApplyVersion()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyVersion", Array.Empty<object>());
			}
			if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame == null)
			{
				Debug.LogException(new Exception("You can't add an asset while not in a game!"), this);
			}
			else
			{
				CancellationTokenSourceUtility.RecreateTokenSource(ref this.applyVersionCancellationTokenSource);
				CancellationToken token = this.applyVersionCancellationTokenSource.Token;
				string value = this.versionDropdown.Value;
				string text = this.view.Versions[0];
				if (value == text)
				{
					Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
					MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.MarkAssetVersionAsLatest(activeGame.AssetID, this.Model.AssetID);
				}
				this.OnLoadingStarted.Invoke();
				string assetId = this.Model.AssetID;
				UIGameAsset uigameAsset;
				try
				{
					token.ThrowIfCancellationRequested();
					uigameAsset = await this.UpdateGameAsset(this.Model, value);
				}
				catch (OperationCanceledException)
				{
					if (this.verboseLogging)
					{
						Debug.Log("ApplyVersion was cancelled", this);
					}
					this.OnLoadingEnded.Invoke();
					return;
				}
				catch (Exception ex)
				{
					ErrorHandler.HandleError(ErrorCodes.GameLibrary_VersionChangeFailed, ex, true, false);
					this.OnLoadingEnded.Invoke();
					return;
				}
				this.OnLoadingEnded.Invoke();
				if (base.gameObject.activeInHierarchy)
				{
					if (!(assetId != this.Model.AssetID))
					{
						this.view.SetActiveAssetVersionAndHandleRelatedVisibility();
						this.view.View(uigameAsset);
					}
				}
			}
		}

		// Token: 0x06000318 RID: 792 RVA: 0x00013998 File Offset: 0x00011B98
		private async Task<UIGameAsset> UpdateGameAsset(UIGameAsset gameAsset, string version)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateGameAsset", new object[] { gameAsset.AssetID, version });
			}
			string assetId = gameAsset.AssetID;
			try
			{
				UIGameAssetTypes type = this.Model.Type;
				switch (type)
				{
				case UIGameAssetTypes.Terrain:
					await MonoBehaviourSingleton<GameEditor>.Instance.SetTerrainUsageVersionInGameLibrary(assetId, version);
					return new UIGameAsset(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette.DistinctTilesets.Find((Tileset tileset) => tileset.Asset.AssetID == assetId));
				case UIGameAssetTypes.Prop:
				{
					await MonoBehaviourSingleton<GameEditor>.Instance.SetPropVersionInGameLibrary(assetId, version);
					PropLibrary.RuntimePropInfo runtimePropInfo;
					if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out runtimePropInfo))
					{
						return new UIGameAsset(runtimePropInfo);
					}
					throw new Exception("Failed to set prop version");
				}
				case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
					goto IL_02E5;
				case UIGameAssetTypes.SFX:
					break;
				default:
					if (type != UIGameAssetTypes.Ambient && type != UIGameAssetTypes.Music)
					{
						goto IL_02E5;
					}
					break;
				}
				bool flag = await MonoBehaviourSingleton<GameEditor>.Instance.SetAudioVersionInGameLibrary(assetId, version);
				RuntimeAudioInfo runtimeAudioInfo;
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(assetId, out runtimeAudioInfo))
				{
					Debug.Log("Got new version " + runtimeAudioInfo.AudioAsset.AssetVersion);
					return new UIGameAsset(runtimeAudioInfo);
				}
				throw new Exception(string.Format("Failed to set audio version: {0}", flag));
				IL_02E5:
				DebugUtility.LogException(new ArgumentOutOfRangeException(string.Format("No support for a {0} of {1}!", this.Model.Type.GetType().Name, this.Model.Type)), this);
			}
			catch (Exception ex)
			{
				ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_UpdateGameAsset, ex, true, false);
			}
			return gameAsset;
		}

		// Token: 0x06000319 RID: 793 RVA: 0x000139EC File Offset: 0x00011BEC
		private void Discard()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Discard", Array.Empty<object>());
			}
			this.nameInputField.text = this.Model.Name;
			this.descriptionInputField.text = this.Model.Asset.Description;
			this.view.SetWriteable(false);
		}

		// Token: 0x0600031A RID: 794 RVA: 0x00013A50 File Offset: 0x00011C50
		private async void SaveAsNewVersion()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SaveAsNewVersion", Array.Empty<object>());
			}
			CancellationTokenSourceUtility.RecreateTokenSource(ref this.saveAsNewVersionCts);
			CancellationToken token = this.saveAsNewVersionCts.Token;
			if (this.Model.Type == UIGameAssetTypes.Terrain && this.Model.Tileset != null)
			{
				DebugUtility.LogException(new Exception("A Tileset is not an asset and therefore can not be updated!"), this);
			}
			this.Model.Asset.Name = this.nameInputField.text;
			this.Model.Asset.Description = this.descriptionInputField.text;
			Version version = new Version(this.view.Versions[0]);
			Version version2 = new Version(version.Major, version.Minor, version.Build + 1);
			this.Model.Asset.AssetVersion = version2.ToString();
			this.OnLoadingStarted.Invoke();
			GraphQlResult graphQlResult = null;
			try
			{
				token.ThrowIfCancellationRequested();
				UIGameAssetTypes uigameAssetTypes = this.Model.Type;
				switch (uigameAssetTypes)
				{
				case UIGameAssetTypes.Terrain:
				{
					TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = (TerrainTilesetCosmeticAsset)this.Model.Asset;
					graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(terrainTilesetCosmeticAsset, false, false);
					goto IL_0317;
				}
				case UIGameAssetTypes.Prop:
				{
					Prop prop = ((Prop)this.Model.Asset).Clone();
					graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync(prop, false, false);
					goto IL_0317;
				}
				case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
					goto IL_02FC;
				case UIGameAssetTypes.SFX:
					break;
				default:
					if (uigameAssetTypes != UIGameAssetTypes.Ambient && uigameAssetTypes != UIGameAssetTypes.Music)
					{
						goto IL_02FC;
					}
					break;
				}
				graphQlResult = await EndlessServices.Instance.CloudService.UpdateAssetAsync((AudioAsset)this.Model.Asset, false, false);
				goto IL_0317;
				IL_02FC:
				DebugUtility.LogNoEnumSupportError<UIGameAssetTypes>(this, "SaveAsNewVersion", this.Model.Type, Array.Empty<object>());
				IL_0317:
				this.OnLoadingEnded.Invoke();
				if (graphQlResult != null)
				{
					if (graphQlResult.HasErrors)
					{
						Exception exception = graphQlResult.GetErrorMessage(0);
						if (exception.Message.Contains("already exists"))
						{
							this.OnLoadingStarted.Invoke();
							GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.GetVersionsAsync(this.Model.AssetID, false);
							this.OnLoadingEnded.Invoke();
							if (graphQlResult2.HasErrors)
							{
								ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_RetrievingAssetVersions, exception, true, false);
							}
							else
							{
								string[] versions = JsonConvert.DeserializeObject<string[]>(graphQlResult2.GetDataMember().ToString());
								if (versions.Length == 0)
								{
									this.OnLoadingEnded.Invoke();
									ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_NoVersionsOfAsset, new Exception("There are no versions in the result!"), true, false);
									return;
								}
								MonoBehaviourSingleton<UIModalManager>.Instance.Confirm("Save Conflict\nA version of " + this.Model.Asset.AssetVersion + " already been saved. Do you wish to save what you have as a newer version?", delegate
								{
									DebugUtility.DebugEnumerable<string>("versions", versions, this);
									string text = versions.Select((string item) => new Version(item)).Max<Version>().ToString();
									this.Model.Asset.AssetVersion = text;
									MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
									this.SaveAsNewVersion();
								}, new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack), UIModalManagerStackActions.MaintainStack);
							}
						}
						else
						{
							ErrorHandler.HandleError(ErrorCodes.UIGameAssetDetailController_UpdateAsset, exception, true, false);
						}
						exception = null;
					}
					else
					{
						object dataMember = graphQlResult.GetDataMember();
						uigameAssetTypes = this.Model.Type;
						switch (uigameAssetTypes)
						{
						case UIGameAssetTypes.Terrain:
						{
							TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset2 = JsonConvert.DeserializeObject<TerrainTilesetCosmeticAsset>(dataMember.ToString());
							EndlessAssetCache.AddNewVersionToCache<TerrainTilesetCosmeticAsset>(terrainTilesetCosmeticAsset2);
							this.Model.SetAsset(terrainTilesetCosmeticAsset2);
							goto IL_0585;
						}
						case UIGameAssetTypes.Prop:
						{
							Prop prop2 = JsonConvert.DeserializeObject<Prop>(dataMember.ToString());
							EndlessAssetCache.AddNewVersionToCache<Prop>(prop2);
							this.Model.SetAsset(prop2);
							goto IL_0585;
						}
						case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
							goto IL_056A;
						case UIGameAssetTypes.SFX:
							break;
						default:
							if (uigameAssetTypes != UIGameAssetTypes.Ambient && uigameAssetTypes != UIGameAssetTypes.Music)
							{
								goto IL_056A;
							}
							break;
						}
						AudioAsset audioAsset = JsonConvert.DeserializeObject<AudioAsset>(dataMember.ToString());
						EndlessAssetCache.AddNewVersionToCache<AudioAsset>(audioAsset);
						this.Model.SetAsset(audioAsset);
						goto IL_0585;
						IL_056A:
						DebugUtility.LogNoEnumSupportError<UIGameAssetTypes>(this, "SaveAsNewVersion", this.Model.Type, Array.Empty<object>());
						IL_0585:
						this.view.View(this.Model);
					}
				}
				this.view.ViewAssetNameAndDescription();
				this.view.SetWriteable(false);
			}
			catch (OperationCanceledException)
			{
				if (this.verboseLogging)
				{
					Debug.Log("SaveAsNewVersion was cancelled", this);
				}
				this.OnLoadingEnded.Invoke();
			}
		}

		// Token: 0x04000326 RID: 806
		public static Action AssetAdded;

		// Token: 0x04000327 RID: 807
		[SerializeField]
		private UIGameAssetDetailView view;

		// Token: 0x04000328 RID: 808
		[SerializeField]
		private UIDropdownVersion versionDropdown;

		// Token: 0x04000329 RID: 809
		[Header("Actions")]
		[SerializeField]
		private UIButton playAudioClipButton;

		// Token: 0x0400032A RID: 810
		[SerializeField]
		private UIButton stopAudioClipButton;

		// Token: 0x0400032B RID: 811
		[SerializeField]
		private AudioSource audioSource;

		// Token: 0x0400032C RID: 812
		[SerializeField]
		private UIAudioCategoryAudioMixerGroupDictionary audioCategoryAudioMixerGroupDictionary;

		// Token: 0x0400032D RID: 813
		[SerializeField]
		private UIButton moderatorateButton;

		// Token: 0x0400032E RID: 814
		[SerializeField]
		private UIAssetModerationModalView assetModerationModalSource;

		// Token: 0x0400032F RID: 815
		[SerializeField]
		private UIButton addButton;

		// Token: 0x04000330 RID: 816
		[SerializeField]
		private UIButton applyVersionButton;

		// Token: 0x04000331 RID: 817
		[SerializeField]
		private UIButton publishButton;

		// Token: 0x04000332 RID: 818
		[SerializeField]
		private UIGameAssetPublishModalView gameAssetPublishModalSource;

		// Token: 0x04000333 RID: 819
		[Header("Editable")]
		[SerializeField]
		private UIInputField nameInputField;

		// Token: 0x04000334 RID: 820
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x04000335 RID: 821
		[SerializeField]
		private UIButton saveAsNewVersionButton;

		// Token: 0x04000336 RID: 822
		[SerializeField]
		private UIButton discardButton;

		// Token: 0x04000337 RID: 823
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000338 RID: 824
		private AudioClip audioClip;

		// Token: 0x04000339 RID: 825
		private Coroutine waitForAudioClipAndViewPlayButtonCoroutine;

		// Token: 0x0400033A RID: 826
		private CancellationTokenSource viewVersionCancellationTokenSource;

		// Token: 0x0400033B RID: 827
		private CancellationTokenSource playAudioClipCancellationTokenSource;

		// Token: 0x0400033C RID: 828
		private CancellationTokenSource addCancellationTokenSource;

		// Token: 0x0400033D RID: 829
		private CancellationTokenSource applyVersionCancellationTokenSource;

		// Token: 0x0400033E RID: 830
		private CancellationTokenSource saveAsNewVersionCts;
	}
}
