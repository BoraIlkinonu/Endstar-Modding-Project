using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Gameplay.LevelEditing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000CD RID: 205
	public class UIGameAssetDetailView : UIBaseGameAssetView
	{
		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000335 RID: 821 RVA: 0x0001506E File Offset: 0x0001326E
		// (set) Token: 0x06000336 RID: 822 RVA: 0x00015076 File Offset: 0x00013276
		public UIUserRolesModel UserRolesModel { get; private set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x06000337 RID: 823 RVA: 0x0001507F File Offset: 0x0001327F
		public IReadOnlyList<string> Versions
		{
			get
			{
				return this.versions;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000338 RID: 824 RVA: 0x00015087 File Offset: 0x00013287
		// (set) Token: 0x06000339 RID: 825 RVA: 0x0001508F File Offset: 0x0001328F
		public string ActiveAssetVersion { get; private set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x0600033A RID: 826 RVA: 0x00015098 File Offset: 0x00013298
		// (set) Token: 0x0600033B RID: 827 RVA: 0x000150A0 File Offset: 0x000132A0
		public bool Writeable { get; private set; }

		// Token: 0x0600033C RID: 828 RVA: 0x000150A9 File Offset: 0x000132A9
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!this.listenersAdded)
			{
				this.AddListeners();
			}
		}

		// Token: 0x0600033D RID: 829 RVA: 0x000150D4 File Offset: 0x000132D4
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.Clear();
			if (this.listenersAdded)
			{
				this.UserRolesModel.OnUserRolesSet.RemoveListener(new UnityAction<List<UserRole>>(base.OnUserRolesChanged));
				this.UserRolesModel.OnLocalClientRoleSet.RemoveListener(new UnityAction<Roles>(this.OnLocalClientRoleSet));
			}
		}

		// Token: 0x0600033E RID: 830 RVA: 0x00015140 File Offset: 0x00013340
		public override void View(UIGameAsset model)
		{
			base.View(model);
			if (UIGameAsset.IsNullOrEmpty(model))
			{
				return;
			}
			DebugUtility.Log("Viewing asset: " + model.AssetID + ", " + model.AssetVersion, this);
			if (!this.listenersAdded)
			{
				this.AddListeners();
			}
			Game activeGame = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame;
			if (activeGame != null)
			{
				MonoBehaviourSingleton<GameEditorAssetVersionManager>.Instance.SetAssetUpdateSeen(activeGame.AssetID, base.Model.AssetID);
			}
			this.SetWriteable(false);
			this.nameInputField.text = model.Name;
			this.descriptionInputField.text = model.Description;
			this.moderatorateButton.gameObject.SetActive(this.assetContext == AssetContexts.UserReported);
			if (model.Type.IsAudio())
			{
				this.ViewAudioButton(false);
			}
			else
			{
				this.playAudioClipButton.gameObject.SetActive(false);
				this.stopAudioClipButton.gameObject.SetActive(false);
			}
			if (model.Tileset == null)
			{
				this.SetWriteable(false);
				this.SetActiveAssetVersionAndHandleRelatedVisibility();
			}
			else
			{
				this.assetVersion = this.GetTerrainUsageVersion(model.AssetID);
			}
			this.UserRolesModel.Initialize(model.AssetID, model.Name, SerializableGuid.Empty, this.assetContext);
			UIUserRoleWizard.AssetTypes assetTypes;
			if (this.assetTypeDictionary.TryGetValue(model.Type, out assetTypes))
			{
				this.UserRolesModel.SetAssetType(assetTypes);
			}
			else
			{
				DebugUtility.LogError(string.Format("{0} has not entry for a key of {1}", "assetTypeDictionary", model.Type), this);
			}
			base.GetAsset(this.assetVersion, new Action(this.ViewDescriptionAndSetActiveAssetVersionAndHandleRelatedVisibility));
			this.LoadVersionsAsync();
		}

		// Token: 0x0600033F RID: 831 RVA: 0x000152F4 File Offset: 0x000134F4
		private void AddListeners()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AddListeners", Array.Empty<object>());
			}
			if (this.listenersAdded)
			{
				return;
			}
			this.listenersAdded = true;
			this.UserRolesModel.OnUserRolesSet.AddListener(new UnityAction<List<UserRole>>(base.OnUserRolesChanged));
			this.UserRolesModel.OnLocalClientRoleSet.AddListener(new UnityAction<Roles>(this.OnLocalClientRoleSet));
		}

		// Token: 0x06000340 RID: 832 RVA: 0x00015364 File Offset: 0x00013564
		private Task LoadVersionsAsync()
		{
			UIGameAssetDetailView.<LoadVersionsAsync>d__38 <LoadVersionsAsync>d__;
			<LoadVersionsAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<LoadVersionsAsync>d__.<>4__this = this;
			<LoadVersionsAsync>d__.<>1__state = -1;
			<LoadVersionsAsync>d__.<>t__builder.Start<UIGameAssetDetailView.<LoadVersionsAsync>d__38>(ref <LoadVersionsAsync>d__);
			return <LoadVersionsAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000341 RID: 833 RVA: 0x000153A8 File Offset: 0x000135A8
		public void ViewVersionInDropdown(string currentVersion)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewVersionInDropdown", new object[] { currentVersion });
			}
			if (this.versions == null || this.versions.Count == 0)
			{
				this.versionDropdown.gameObject.SetActive(false);
				return;
			}
			List<string> list = this.versions.OrderByDescending(new Func<string, Version>(Version.Parse)).ToList<string>();
			if (list.IndexOf(currentVersion) < 0)
			{
				DebugUtility.LogException(new Exception("Version '" + currentVersion + "' not found!"), this);
				this.versionDropdown.gameObject.SetActive(false);
				return;
			}
			this.versionDropdown.SetOptionsAndValue(list, currentVersion, false);
			this.versionDropdown.gameObject.SetActive(true);
		}

		// Token: 0x06000342 RID: 834 RVA: 0x0001546C File Offset: 0x0001366C
		public void SetContext(AssetContexts context)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetContext", new object[] { context });
			}
			this.assetContext = context;
			bool flag = context == AssetContexts.GameLibraryAddition;
			this.userRolesView.SetBlockActivationOfAddUserRoleButton(flag);
			this.versionDropdown.SetIsInteractable(!flag);
			this.versionInLibraryText.gameObject.SetActive(context == AssetContexts.GameLibrary);
			if (!flag)
			{
				this.addButton.gameObject.SetActive(false);
			}
		}

		// Token: 0x06000343 RID: 835 RVA: 0x000154EC File Offset: 0x000136EC
		public void ViewAssetNameAndDescription()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewAssetNameAndDescription", Array.Empty<object>());
			}
			if (base.Model.Asset == null)
			{
				DebugUtility.LogError(this, "ViewAssetNameAndDescription", "This should not be called on a Tileset!", Array.Empty<object>());
				return;
			}
			Asset asset = base.Model.Asset;
			this.nameInputField.text = asset.Name;
			this.descriptionInputField.text = asset.Description;
		}

		// Token: 0x06000344 RID: 836 RVA: 0x00015564 File Offset: 0x00013764
		public void SetWriteable(bool newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "SetWriteable", string.Format("{0}: {1}", "assetContext", this.assetContext), new object[] { newValue });
			}
			this.Writeable = newValue;
			this.versionDropdown.gameObject.SetActive(!this.Writeable);
			bool flag = this.assetContext == AssetContexts.GameLibraryAddition;
			bool flag2 = this.assetContext == AssetContexts.MainMenu;
			this.publishButton.gameObject.SetActive(this.Writeable && flag2);
			this.nameInputField.gameObject.SetActive(this.Writeable && !flag);
			this.descriptionInputField.gameObject.SetActive(this.Writeable && !flag);
			this.saveAsNewVersionButton.gameObject.SetActive(this.Writeable && !flag);
			this.discardButton.gameObject.SetActive(this.Writeable && !flag);
		}

		// Token: 0x06000345 RID: 837 RVA: 0x00015678 File Offset: 0x00013878
		public void SetActiveAssetVersionAndHandleRelatedVisibility()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetActiveAssetVersionAndHandleRelatedVisibility", Array.Empty<object>());
			}
			this.ActiveAssetVersion = null;
			bool flag = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame != null;
			if (flag)
			{
				GameLibrary gameLibrary = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary;
				UIGameAssetTypes type = base.Model.Type;
				switch (type)
				{
				case UIGameAssetTypes.Terrain:
				{
					using (List<TerrainUsage>.Enumerator enumerator = gameLibrary.TerrainEntries.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							TerrainUsage terrainUsage = enumerator.Current;
							if (terrainUsage.IsActive && terrainUsage.TerrainAssetReference.AssetID == base.Model.AssetID)
							{
								this.ActiveAssetVersion = terrainUsage.TerrainAssetReference.AssetVersion;
								break;
							}
						}
						goto IL_01C1;
					}
					break;
				}
				case UIGameAssetTypes.Prop:
					break;
				case UIGameAssetTypes.Terrain | UIGameAssetTypes.Prop:
					goto IL_01A6;
				case UIGameAssetTypes.SFX:
					goto IL_014E;
				default:
					if (type != UIGameAssetTypes.Ambient && type != UIGameAssetTypes.Music)
					{
						goto IL_01A6;
					}
					goto IL_014E;
				}
				using (IEnumerator<AssetReference> enumerator2 = gameLibrary.PropReferences.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						AssetReference assetReference = enumerator2.Current;
						if (assetReference.AssetID == base.Model.AssetID)
						{
							this.ActiveAssetVersion = assetReference.AssetVersion;
							break;
						}
					}
					goto IL_01C1;
				}
				IL_014E:
				using (IEnumerator<AssetReference> enumerator2 = gameLibrary.AudioReferences.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						AssetReference assetReference2 = enumerator2.Current;
						if (assetReference2.AssetID == base.Model.AssetID)
						{
							this.ActiveAssetVersion = assetReference2.AssetVersion;
							break;
						}
					}
					goto IL_01C1;
				}
				IL_01A6:
				DebugUtility.LogNoEnumSupportError<UIGameAssetTypes>(this, "SetActiveAssetVersionAndHandleRelatedVisibility", base.Model.Type, Array.Empty<object>());
			}
			IL_01C1:
			bool flag2 = this.ActiveAssetVersion != null;
			bool flag3 = this.assetContext == AssetContexts.GameLibraryAddition;
			bool flag4 = flag && !flag2 && flag3;
			this.addButton.gameObject.SetActive(flag4);
			bool flag5 = flag2 && base.Model.Asset.AssetVersion != this.ActiveAssetVersion && this.assetContext == AssetContexts.GameLibrary;
			this.applyVersionButton.gameObject.SetActive(flag5);
		}

		// Token: 0x06000346 RID: 838 RVA: 0x000158E0 File Offset: 0x00013AE0
		public override void Clear()
		{
			base.Clear();
			this.UserRolesModel.Clear();
			this.versionDropdown.SetValueText("Loading...");
			this.nameInputField.text = string.Empty;
			this.descriptionInputField.text = string.Empty;
			this.descriptionText.text = string.Empty;
			this.addButton.gameObject.SetActive(false);
			this.applyVersionButton.gameObject.SetActive(false);
			this.publishButton.gameObject.SetActive(false);
			this.saveAsNewVersionButton.gameObject.SetActive(false);
			this.discardButton.gameObject.SetActive(false);
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.loadVersionsCancellationTokenSource);
		}

		// Token: 0x06000347 RID: 839 RVA: 0x000159A0 File Offset: 0x00013BA0
		public void ViewAudioButton(bool isPlaying)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewAudioButton", new object[] { isPlaying });
			}
			this.playAudioClipButton.gameObject.SetActive(!isPlaying);
			this.stopAudioClipButton.gameObject.SetActive(isPlaying);
		}

		// Token: 0x06000348 RID: 840 RVA: 0x000159F4 File Offset: 0x00013BF4
		private void ViewDescriptionAndSetActiveAssetVersionAndHandleRelatedVisibility()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewDescriptionAndSetActiveAssetVersionAndHandleRelatedVisibility", Array.Empty<object>());
			}
			this.descriptionText.text = base.Model.Description;
			this.SetActiveAssetVersionAndHandleRelatedVisibility();
		}

		// Token: 0x06000349 RID: 841 RVA: 0x00015A2C File Offset: 0x00013C2C
		private void OnLocalClientRoleSet(Roles localClientRole)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLocalClientRoleSet", new object[] { localClientRole });
			}
			bool flag = this.assetContext == AssetContexts.MainMenu && localClientRole.IsGreaterThanOrEqualTo(Roles.Publisher);
			this.publishButton.gameObject.SetActive(flag);
		}

		// Token: 0x0600034A RID: 842 RVA: 0x00015A84 File Offset: 0x00013C84
		private string GetTerrainUsageVersion(SerializableGuid tilesetId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetTerrainUsageVersion", new object[] { tilesetId });
			}
			if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame == null)
			{
				Debug.LogException(new Exception("You can't add an asset while not in a game!"), this);
				return null;
			}
			TerrainUsage terrainUsage = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.TerrainEntries.FirstOrDefault((TerrainUsage entry) => entry.TilesetId == tilesetId);
			if (terrainUsage != null)
			{
				return terrainUsage.TerrainAssetReference.AssetVersion;
			}
			DebugUtility.LogException(new Exception(string.Format("Could not find a {0} in {1}.{2} with a {3} of {4}!", new object[] { "TerrainUsage", "GameLibrary", "TerrainEntries", "tilesetId", tilesetId })), this);
			return null;
		}

		// Token: 0x0400036A RID: 874
		[Header("UIGameAssetDetailView")]
		[SerializeField]
		private UIDropdownVersion versionDropdown;

		// Token: 0x0400036B RID: 875
		[SerializeField]
		private TextMeshProUGUI versionInLibraryText;

		// Token: 0x0400036C RID: 876
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x0400036D RID: 877
		[Header("Actions")]
		[SerializeField]
		private UIButton playAudioClipButton;

		// Token: 0x0400036E RID: 878
		[SerializeField]
		private UIButton stopAudioClipButton;

		// Token: 0x0400036F RID: 879
		[SerializeField]
		private UIButton moderatorateButton;

		// Token: 0x04000370 RID: 880
		[SerializeField]
		private UIButton addButton;

		// Token: 0x04000371 RID: 881
		[SerializeField]
		private UIButton applyVersionButton;

		// Token: 0x04000372 RID: 882
		[SerializeField]
		private UIButton publishButton;

		// Token: 0x04000373 RID: 883
		[Header("Editable")]
		[SerializeField]
		private UIInputField nameInputField;

		// Token: 0x04000374 RID: 884
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x04000375 RID: 885
		[SerializeField]
		private UIButton saveAsNewVersionButton;

		// Token: 0x04000376 RID: 886
		[SerializeField]
		private UIButton discardButton;

		// Token: 0x04000377 RID: 887
		[SerializeField]
		private UIUserRolesView userRolesView;

		// Token: 0x04000378 RID: 888
		private readonly string[] versionsValues = new string[1];

		// Token: 0x04000379 RID: 889
		private readonly Dictionary<UIGameAssetTypes, UIUserRoleWizard.AssetTypes> assetTypeDictionary = new Dictionary<UIGameAssetTypes, UIUserRoleWizard.AssetTypes>
		{
			{
				UIGameAssetTypes.Terrain,
				UIUserRoleWizard.AssetTypes.Tileset
			},
			{
				UIGameAssetTypes.Prop,
				UIUserRoleWizard.AssetTypes.Prop
			},
			{
				UIGameAssetTypes.SFX,
				UIUserRoleWizard.AssetTypes.Audio
			},
			{
				UIGameAssetTypes.Ambient,
				UIUserRoleWizard.AssetTypes.Audio
			},
			{
				UIGameAssetTypes.Music,
				UIUserRoleWizard.AssetTypes.Audio
			}
		};

		// Token: 0x0400037A RID: 890
		private AssetContexts assetContext;

		// Token: 0x0400037B RID: 891
		private List<string> versions = new List<string>();

		// Token: 0x0400037C RID: 892
		private bool listenersAdded;

		// Token: 0x0400037D RID: 893
		private CancellationTokenSource loadVersionsCancellationTokenSource;
	}
}
