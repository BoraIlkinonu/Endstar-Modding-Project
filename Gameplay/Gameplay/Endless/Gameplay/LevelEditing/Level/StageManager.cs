using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Gameplay.Scripting;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Runtime.Gameplay.LevelEditing;
using Runtime.Gameplay.LevelEditing.Levels;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200057D RID: 1405
	public class StageManager : MonoBehaviourSingleton<StageManager>
	{
		// Token: 0x1700066E RID: 1646
		// (get) Token: 0x060021F0 RID: 8688 RVA: 0x0009BE86 File Offset: 0x0009A086
		public PropLibrary ActivePropLibrary
		{
			get
			{
				return this.activePropLibrary;
			}
		}

		// Token: 0x1700066F RID: 1647
		// (get) Token: 0x060021F1 RID: 8689 RVA: 0x0009BE8E File Offset: 0x0009A08E
		public RuntimePalette ActiveTerrainPalette
		{
			get
			{
				return this.activeTerrainPalette;
			}
		}

		// Token: 0x17000670 RID: 1648
		// (get) Token: 0x060021F2 RID: 8690 RVA: 0x0009BE96 File Offset: 0x0009A096
		public AudioLibrary ActiveAudioLibrary
		{
			get
			{
				return this.activeAudioLibrary;
			}
		}

		// Token: 0x17000671 RID: 1649
		// (get) Token: 0x060021F3 RID: 8691 RVA: 0x0009BE9E File Offset: 0x0009A09E
		public IReadOnlyDictionary<SerializableGuid, PropRequirement> PropRequirementLookup
		{
			get
			{
				if (this.propRequirementLookup == null)
				{
					this.BuildPropRequirementsLookup();
				}
				return this.propRequirementLookup;
			}
		}

		// Token: 0x17000672 RID: 1650
		// (get) Token: 0x060021F4 RID: 8692 RVA: 0x0009BEB4 File Offset: 0x0009A0B4
		public IReadOnlyDictionary<SerializableGuid, BaseTypeRequirement> BaseTypeRequirementLookup
		{
			get
			{
				if (this.baseTypeRequirementLookup == null)
				{
					this.BuildBaseTypeRequirementsLookup();
				}
				return this.baseTypeRequirementLookup;
			}
		}

		// Token: 0x17000673 RID: 1651
		// (get) Token: 0x060021F5 RID: 8693 RVA: 0x0009BECA File Offset: 0x0009A0CA
		public Stage ActiveStage
		{
			get
			{
				if (this.spawnedLevels.ContainsKey(this.activeLevelGuid))
				{
					return this.spawnedLevels[this.activeLevelGuid];
				}
				return null;
			}
		}

		// Token: 0x17000674 RID: 1652
		// (get) Token: 0x060021F6 RID: 8694 RVA: 0x0009BEF2 File Offset: 0x0009A0F2
		public BaseTypeList BaseTypeList
		{
			get
			{
				return this.baseTypeList;
			}
		}

		// Token: 0x17000675 RID: 1653
		// (get) Token: 0x060021F7 RID: 8695 RVA: 0x0009BEFA File Offset: 0x0009A0FA
		public ComponentList ComponentList
		{
			get
			{
				return this.componentList;
			}
		}

		// Token: 0x17000676 RID: 1654
		// (get) Token: 0x060021F8 RID: 8696 RVA: 0x0009BF02 File Offset: 0x0009A102
		public SerializableGuid ActiveLevelGuid
		{
			get
			{
				return this.activeLevelGuid;
			}
		}

		// Token: 0x17000677 RID: 1655
		// (get) Token: 0x060021F9 RID: 8697 RVA: 0x0001965C File Offset: 0x0001785C
		public bool PreloadContent
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060021FA RID: 8698 RVA: 0x0009BF0C File Offset: 0x0009A10C
		protected override void Awake()
		{
			base.Awake();
			this.activeTerrainPalette = new RuntimePalette(this.collisionLibrary, this.fallbackTerrain, this.fallbackDisplayIcon, this.loadingTerrain);
			this.activePropLibrary = new PropLibrary(this.prefabSpawnTransform, this.loadingPropObject, this.basePropPrefab, this.missingObjectPrefab);
			this.activeAudioLibrary = new AudioLibrary(base.gameObject);
		}

		// Token: 0x060021FB RID: 8699 RVA: 0x0009BF76 File Offset: 0x0009A176
		public void Start()
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.HandleGameplayCleanup));
			base.transform.SetParent(null);
		}

		// Token: 0x060021FC RID: 8700 RVA: 0x0009BFA0 File Offset: 0x0009A1A0
		public bool TryGetOfflineStage(AssetReference reference, out OfflineStage offlineStage)
		{
			bool flag = this.spawnedOfflineStages.TryGetValue(reference, out offlineStage);
			if (!flag)
			{
				foreach (AssetReference assetReference in this.spawnedOfflineStages.Keys.ToArray<AssetReference>())
				{
					if (assetReference.AssetID == reference.AssetID)
					{
						global::UnityEngine.Object.Destroy(this.spawnedOfflineStages[assetReference].gameObject);
						this.spawnedOfflineStages.Remove(assetReference);
					}
				}
			}
			return flag;
		}

		// Token: 0x060021FD RID: 8701 RVA: 0x0009C018 File Offset: 0x0009A218
		public void RemoveOfflineStage(AssetReference reference)
		{
			OfflineStage offlineStage;
			if (!this.spawnedOfflineStages.TryGetValue(reference, out offlineStage))
			{
				return;
			}
			global::UnityEngine.Object.Destroy(offlineStage.gameObject);
			this.spawnedOfflineStages.Remove(reference);
		}

		// Token: 0x060021FE RID: 8702 RVA: 0x0009C050 File Offset: 0x0009A250
		public OfflineStage GetNewOfflineStage(AssetReference reference, string stageName)
		{
			OfflineStage offlineStage = global::UnityEngine.Object.Instantiate<OfflineStage>(this.offlineStagePrefab, base.transform);
			offlineStage.gameObject.name = string.Concat(new string[] { "OfflineStage: ", base.name, " - ", reference.AssetID, " ", reference.AssetVersion });
			this.spawnedOfflineStages.Add(reference, offlineStage);
			return offlineStage;
		}

		// Token: 0x060021FF RID: 8703 RVA: 0x0009C0C8 File Offset: 0x0009A2C8
		public bool TryGetCachedLevel(SerializableGuid levelId, string versionNumber, out LevelState levelState)
		{
			AssetReference assetReference = new AssetReference
			{
				AssetID = levelId,
				AssetVersion = versionNumber,
				AssetType = "level"
			};
			return this.cachedLevelStates.TryGetValue(assetReference, out levelState);
		}

		// Token: 0x06002200 RID: 8704 RVA: 0x0009C108 File Offset: 0x0009A308
		public void UpdateStageVersion(AssetReference oldReference, AssetReference newReference)
		{
			OfflineStage offlineStage = this.spawnedOfflineStages[oldReference];
			this.spawnedOfflineStages.Remove(oldReference);
			this.spawnedOfflineStages[newReference] = offlineStage;
			LevelState levelState = this.cachedLevelStates[oldReference];
			this.cachedLevelStates[newReference] = levelState;
			this.cachedLevelStates.Remove(oldReference);
		}

		// Token: 0x06002201 RID: 8705 RVA: 0x0009C164 File Offset: 0x0009A364
		private void PurgeOfflineStages()
		{
			foreach (AssetReference assetReference in this.spawnedOfflineStages.Keys.ToArray<AssetReference>())
			{
				global::UnityEngine.Object.Destroy(this.spawnedOfflineStages[assetReference].gameObject);
				this.spawnedOfflineStages.Remove(assetReference);
			}
			this.spawnedOfflineStages.Clear();
		}

		// Token: 0x06002202 RID: 8706 RVA: 0x0009C1C2 File Offset: 0x0009A3C2
		private void PurgeCachedLevels()
		{
			this.cachedLevelStates.Clear();
		}

		// Token: 0x06002203 RID: 8707 RVA: 0x0009C1CF File Offset: 0x0009A3CF
		public void SetJoinLevelId(SerializableGuid levelId)
		{
			this.activeLevelGuid = levelId;
		}

		// Token: 0x06002204 RID: 8708 RVA: 0x0009C1D8 File Offset: 0x0009A3D8
		private void HandleGameplayCleanup()
		{
			this.ClearPersistantPropStates();
			this.ClearDestroyedObjectMap();
		}

		// Token: 0x06002205 RID: 8709 RVA: 0x0009C1E8 File Offset: 0x0009A3E8
		public object GetPropState(SerializableGuid instanceId, string componentName)
		{
			if (!this.persistantPropStateMap.ContainsKey(this.activeLevelGuid))
			{
				return null;
			}
			if (!this.persistantPropStateMap[this.activeLevelGuid].ContainsKey(instanceId))
			{
				return null;
			}
			if (!this.persistantPropStateMap[this.activeLevelGuid][instanceId].ContainsKey(componentName))
			{
				return null;
			}
			return this.persistantPropStateMap[this.activeLevelGuid][instanceId][componentName];
		}

		// Token: 0x06002206 RID: 8710 RVA: 0x0009C264 File Offset: 0x0009A464
		public void SavePropState(SerializableGuid instanceId, string componentName, object newState)
		{
			if (!this.persistantPropStateMap.ContainsKey(this.activeLevelGuid))
			{
				this.persistantPropStateMap.Add(this.activeLevelGuid, new Dictionary<SerializableGuid, Dictionary<string, object>>());
			}
			if (!this.persistantPropStateMap[this.activeLevelGuid].ContainsKey(instanceId))
			{
				this.persistantPropStateMap[this.activeLevelGuid].Add(instanceId, new Dictionary<string, object>());
			}
			if (!this.persistantPropStateMap[this.activeLevelGuid][instanceId].ContainsKey(componentName))
			{
				this.persistantPropStateMap[this.activeLevelGuid][instanceId].Add(componentName, newState);
				return;
			}
			this.persistantPropStateMap[this.activeLevelGuid][instanceId][componentName] = newState;
		}

		// Token: 0x06002207 RID: 8711 RVA: 0x0009C32B File Offset: 0x0009A52B
		public void ClearPersistantPropStates()
		{
			this.persistantPropStateMap.Clear();
		}

		// Token: 0x06002208 RID: 8712 RVA: 0x0009C338 File Offset: 0x0009A538
		public void ClearDestroyedObjectMap()
		{
			this.destroyedObjectMapByStage.Clear();
		}

		// Token: 0x06002209 RID: 8713 RVA: 0x0009C348 File Offset: 0x0009A548
		public void PropDestroyed(SerializableGuid instanceId)
		{
			SerializableGuid mapId = this.ActiveStage.MapId;
			if (this.destroyedObjectMapByStage.ContainsKey(mapId))
			{
				this.destroyedObjectMapByStage[mapId].Add(instanceId);
			}
			else
			{
				this.destroyedObjectMapByStage.Add(mapId, new List<SerializableGuid> { instanceId });
			}
			this.ActiveStage.PropDestroyed(instanceId);
		}

		// Token: 0x0600220A RID: 8714 RVA: 0x0009C3A8 File Offset: 0x0009A5A8
		public bool IsPropDestroyed(SerializableGuid instanceId)
		{
			SerializableGuid mapId = this.ActiveStage.MapId;
			return this.destroyedObjectMapByStage.ContainsKey(mapId) && this.destroyedObjectMapByStage[mapId].Contains(instanceId);
		}

		// Token: 0x0600220B RID: 8715 RVA: 0x0009C3E4 File Offset: 0x0009A5E4
		private void BuildPropRequirementsLookup()
		{
			this.propRequirementLookup = new Dictionary<SerializableGuid, PropRequirement>();
			foreach (PropRequirement propRequirement in this.propRequirements)
			{
				foreach (SerializableGuid serializableGuid in propRequirement.Guids)
				{
					this.propRequirementLookup.Add(serializableGuid, propRequirement);
				}
			}
		}

		// Token: 0x0600220C RID: 8716 RVA: 0x0009C484 File Offset: 0x0009A684
		private void BuildBaseTypeRequirementsLookup()
		{
			this.baseTypeRequirementLookup = new Dictionary<SerializableGuid, BaseTypeRequirement>();
			foreach (BaseTypeRequirement baseTypeRequirement in this.baseTypeRequirements)
			{
				foreach (SerializableGuid serializableGuid in baseTypeRequirement.Guids)
				{
					this.baseTypeRequirementLookup.Add(serializableGuid, baseTypeRequirement);
				}
			}
		}

		// Token: 0x0600220D RID: 8717 RVA: 0x0009C524 File Offset: 0x0009A724
		public void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon)
		{
			this.injectedProps.Add(new InjectedProps
			{
				Prop = prop,
				TestPrefab = testPrefab,
				TestScript = testScript,
				Icon = icon
			});
		}

		// Token: 0x0600220E RID: 8718 RVA: 0x0009C554 File Offset: 0x0009A754
		public async Task RepopulateAudioLibrary(CancellationToken cancellationToken)
		{
			this.activeAudioLibrary.UnloadAssetsNotInGameLibrary();
			await this.activeAudioLibrary.PreloadData(cancellationToken, null);
		}

		// Token: 0x0600220F RID: 8719 RVA: 0x0009C5A0 File Offset: 0x0009A7A0
		public void PrepareForLevelChange(SerializableGuid levelToChangeTo)
		{
			if (levelToChangeTo == this.activeLevelGuid || levelToChangeTo == SerializableGuid.Empty)
			{
				return;
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.MapId != levelToChangeTo)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.CleanupProps();
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.gameObject.SetActive(false);
			}
			this.activeLevelGuid = levelToChangeTo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.gameObject.SetActive(true);
			}
		}

		// Token: 0x06002210 RID: 8720 RVA: 0x0009C644 File Offset: 0x0009A844
		public async Task LoadLevel(LevelState levelState, bool loadLibraryPrefabs, CancellationToken cancelToken, Action<string> progressCallback = null)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("StageManager");
			SerializableGuid serializableGuid = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID;
			if (this.lastGameGuid != serializableGuid)
			{
				this.lastGameGuid = serializableGuid;
				this.PurgeOfflineStages();
				this.PurgeCachedLevels();
			}
			AssetReference assetReference = levelState.ToAssetReference();
			if (!this.cachedLevelStates.TryAdd(assetReference, levelState))
			{
				this.cachedLevelStates[assetReference] = levelState;
			}
			Debug.Log("StageManager.LoadLevel - Loading Level: " + levelState.AssetID);
			if (loadLibraryPrefabs)
			{
				await this.LoadLibraryPrefabs(levelState, cancelToken, progressCallback);
				MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("LoadLibraryPrefabs", "StageManager");
				if (cancelToken.IsCancellationRequested)
				{
					return;
				}
			}
			if (NetworkManager.Singleton.IsServer)
			{
				Stage stage2;
				if (!this.spawnedLevels.TryGetValue(levelState.AssetID, out stage2) || stage2 == null)
				{
					Stage stage = global::UnityEngine.Object.Instantiate<Stage>(this.stageTemplate);
					stage.SetMapId(levelState.AssetID);
					stage.RuntimePalette = this.activeTerrainPalette;
					stage.GetComponent<NetworkObject>().Spawn(false);
					this.RegisterStage(stage);
					this.activeLevelGuid = levelState.AssetID;
					await stage.LoadLevelIfNecessary(levelState, cancelToken, progressCallback);
					Debug.Log("Adding MapId: " + levelState.AssetID + ", to Loaded Levels.");
					foreach (ulong num in NetworkManager.Singleton.ConnectedClientsIds)
					{
						if (num != 0UL)
						{
							stage.HandleNewPlayer(num);
						}
					}
					this.HandleLevelLoaded(levelState.AssetID);
					stage = null;
				}
				else
				{
					this.HandleLevelLoaded(levelState.AssetID);
				}
				if (this.spawnedLevels.Keys.Count == 1)
				{
					this.activeLevelGuid = levelState.AssetID;
					this.OnActiveStageChanged.Invoke(this.ActiveStage);
				}
			}
			else
			{
				Stage stage3;
				while (!this.spawnedLevels.TryGetValue(levelState.AssetID, out stage3))
				{
					await Task.Yield();
					if (cancelToken.IsCancellationRequested)
					{
						return;
					}
				}
				Debug.Log("Client -- StageManager.LoadLevel " + levelState.AssetID);
				Stage stage4 = stage3;
				if (stage4.RuntimePalette == null)
				{
					stage4.RuntimePalette = this.activeTerrainPalette;
				}
				if (!stage3.IsLoading)
				{
					await this.spawnedLevels[levelState.AssetID].LoadLevelIfNecessary(levelState, cancelToken, progressCallback);
					Debug.Log("Adding MapId: " + levelState.AssetID + ", to Loaded Levels.");
					if (!cancelToken.IsCancellationRequested)
					{
						this.OnActiveStageChanged.Invoke(this.ActiveStage);
						this.HandleLevelLoaded(levelState.AssetID);
					}
				}
			}
		}

		// Token: 0x06002211 RID: 8721 RVA: 0x0009C6A8 File Offset: 0x0009A8A8
		public async Task LoadLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action<string> progressCallback)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("StageManager - LoadLibraryPrefabs");
			progressCallback("Loading terrain for level...");
			await this.LoadTerrainLibraryPrefabs(levelState, cancelToken, progressCallback);
			if (!cancelToken.IsCancellationRequested)
			{
				MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load Terrain", "StageManager - LoadLibraryPrefabs");
				progressCallback("Loading props for level...");
				await this.LoadPropLibraryReferences(levelState, cancelToken, progressCallback);
				if (!cancelToken.IsCancellationRequested)
				{
					MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Load props", "StageManager - LoadLibraryPrefabs");
					await Resources.UnloadUnusedAssets();
					MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("StageManager - LoadLibraryPrefabs");
				}
			}
		}

		// Token: 0x06002212 RID: 8722 RVA: 0x0009C704 File Offset: 0x0009A904
		private async Task LoadPropLibraryReferences(LevelState levelState, CancellationToken cancelToken, Action<string> loadingProgressCallback)
		{
			StageManager.<>c__DisplayClass74_0 CS$<>8__locals1 = new StageManager.<>c__DisplayClass74_0();
			CS$<>8__locals1.loadingProgressCallback = loadingProgressCallback;
			foreach (InjectedProps injectedProps in this.injectedProps)
			{
				this.activePropLibrary.InjectProp(injectedProps.Prop, injectedProps.TestPrefab, injectedProps.TestScript, injectedProps.Icon, this.prefabSpawnTransform, this.basePropPrefab);
			}
			await this.activePropLibrary.LoadPropPrefabs(levelState, this.prefabSpawnTransform, this.basePropPrefab, this.missingObjectPrefab, cancelToken, new Action<int, int>(CS$<>8__locals1.<LoadPropLibraryReferences>g__ProgressCallback|0));
		}

		// Token: 0x06002213 RID: 8723 RVA: 0x0009C760 File Offset: 0x0009A960
		private async Task LoadTerrainLibraryPrefabs(LevelState levelState, CancellationToken cancelToken, Action<string> loadingProgressCallback)
		{
			StageManager.<>c__DisplayClass75_0 CS$<>8__locals1 = new StageManager.<>c__DisplayClass75_0();
			CS$<>8__locals1.loadingProgressCallback = loadingProgressCallback;
			HashSet<int> usedTileSetIds = levelState.GetUsedTileSetIds(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary);
			IReadOnlyList<TerrainUsage> terrainUsagesInLevel = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.GetTerrainUsagesInLevel(usedTileSetIds);
			await this.activeTerrainPalette.Populate(terrainUsagesInLevel, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary, cancelToken, new Action<int, int>(CS$<>8__locals1.<LoadTerrainLibraryPrefabs>g__ProgressCallback|0));
		}

		// Token: 0x06002214 RID: 8724 RVA: 0x0009C7BC File Offset: 0x0009A9BC
		public bool LevelIsLoaded(SerializableGuid levelId)
		{
			Stage stage;
			return this.spawnedLevels.TryGetValue(levelId, out stage) && stage.TerrainLoaded;
		}

		// Token: 0x06002215 RID: 8725 RVA: 0x0009C7E1 File Offset: 0x0009A9E1
		private void HandleLevelLoaded(SerializableGuid mapId)
		{
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("StageManager");
			this.OnLevelLoaded.Invoke(mapId);
		}

		// Token: 0x06002216 RID: 8726 RVA: 0x0009C7FE File Offset: 0x0009A9FE
		public void RegisterStage(Stage stage)
		{
			this.spawnedLevels[stage.MapId] = stage;
		}

		// Token: 0x06002217 RID: 8727 RVA: 0x0009C814 File Offset: 0x0009AA14
		public void FlushLoadedAndSpawnedStages(bool destroyStageObjects)
		{
			if (destroyStageObjects)
			{
				foreach (Stage stage in this.spawnedLevels.Values)
				{
					global::UnityEngine.Object.Destroy(stage.gameObject);
				}
			}
			foreach (OfflineStage offlineStage in this.spawnedOfflineStages.Values)
			{
				offlineStage.gameObject.SetActive(false);
			}
			this.spawnedLevels.Clear();
		}

		// Token: 0x06002218 RID: 8728 RVA: 0x0009C8C8 File Offset: 0x0009AAC8
		public void LeavingSession()
		{
			if (this.ActiveStage)
			{
				this.ActiveStage.CleanupProps();
			}
			this.spawnedLevels.Clear();
			this.activeLevelGuid = SerializableGuid.Empty;
		}

		// Token: 0x06002219 RID: 8729 RVA: 0x0009C8F8 File Offset: 0x0009AAF8
		public void PropFailedToLoad(PropEntry propEntry)
		{
			Prop prop = new Prop
			{
				Name = "Missing Object",
				AssetID = propEntry.AssetId,
				AssetVersion = "0.0.0"
			};
			prop.AddLocationOffset(new PropLocationOffset
			{
				Offset = Vector3Int.zero
			});
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!this.activePropLibrary.TryGetRuntimePropInfo(prop.ToAssetReference(), out runtimePropInfo))
			{
				runtimePropInfo = new PropLibrary.RuntimePropInfo
				{
					EndlessProp = global::UnityEngine.Object.Instantiate<EndlessProp>(this.missingObjectPrefab, this.prefabSpawnTransform),
					Icon = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon,
					PropData = prop,
					IsMissingObject = true,
					IsLoading = false
				};
				runtimePropInfo.EndlessProp.gameObject.name = prop.Name + " (Missing)";
				this.activePropLibrary.AddMissingObjectRuntimePropInfo(runtimePropInfo);
			}
			Debug.Log(string.Format("{0} spawning missing prop for {1}", "PropFailedToLoad", propEntry.AssetId));
			this.ActiveStage.ReplacePropWithMissingObject(propEntry.InstanceId, runtimePropInfo, runtimePropInfo);
		}

		// Token: 0x0600221A RID: 8730 RVA: 0x0009CA00 File Offset: 0x0009AC00
		public BaseTypeDefinition GetBaseTypeDefinition(Type type)
		{
			BaseTypeDefinition baseTypeDefinition;
			if (!this.baseTypeList.TryGetDefinition(type, out baseTypeDefinition))
			{
				return null;
			}
			return baseTypeDefinition;
		}

		// Token: 0x0600221B RID: 8731 RVA: 0x0009CA20 File Offset: 0x0009AC20
		public ComponentDefinition GetComponentDefinition(Type type)
		{
			ComponentDefinition componentDefinition;
			if (!this.componentList.TryGetDefinition(type, out componentDefinition))
			{
				return null;
			}
			return componentDefinition;
		}

		// Token: 0x0600221C RID: 8732 RVA: 0x0009CA40 File Offset: 0x0009AC40
		public async Task FetchAndSpawnPropPrefab(AssetReference assetReference)
		{
			await this.activePropLibrary.FetchAndSpawnPropPrefab(assetReference, CancellationToken.None, this.basePropPrefab, this.missingObjectPrefab, null);
		}

		// Token: 0x0600221D RID: 8733 RVA: 0x0009CA8C File Offset: 0x0009AC8C
		public async void LoadTilesetByIndex(int tilesetIndex)
		{
			if (this.activeTerrainPalette.IsTilesetIndexInFlight(tilesetIndex))
			{
				Debug.Log(string.Format("Request for Index ({0}) is already in flight. Aborting", tilesetIndex));
			}
			else
			{
				GameLibrary gameLibrary = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary;
				HashSet<int> hashSet = new HashSet<int>();
				TerrainUsage terrainUsage = gameLibrary.TerrainEntries[tilesetIndex];
				for (;;)
				{
					int num = terrainUsage.RedirectIndex;
					if (num > gameLibrary.TerrainEntries.Count)
					{
						num = 0;
						Debug.LogException(new Exception("Had improper index in terrain redirects"));
					}
					else if (hashSet.Contains(num))
					{
						break;
					}
					hashSet.Add(num);
					terrainUsage = gameLibrary.TerrainEntries[num];
					if (terrainUsage.IsActive)
					{
						goto Block_4;
					}
				}
				Debug.LogException(new Exception(string.Format("Redirect Indexes looped. Can't validate terrain usage at index: {0}", tilesetIndex)));
				return;
				Block_4:
				await this.ActiveTerrainPalette.LoadTerrain(terrainUsage, tilesetIndex, CancellationToken.None);
				await Resources.UnloadUnusedAssets();
				base.StartCoroutine(this.ActiveStage.RespawnTerrainCellsWithTilesetIndex(tilesetIndex, null));
			}
		}

		// Token: 0x0600221E RID: 8734 RVA: 0x0009CACC File Offset: 0x0009ACCC
		public PropLibrary.RuntimePropInfo GetMissingObjectInfo(Prop propData)
		{
			Prop prop = new Prop
			{
				Name = propData.Name + " (Missing)",
				AssetType = "prop",
				AssetID = propData.AssetID,
				AssetVersion = propData.AssetVersion,
				InternalVersion = Prop.INTERNAL_VERSION.ToString()
			};
			prop.AddLocationOffset(new PropLocationOffset
			{
				Offset = Vector3Int.zero
			});
			return new PropLibrary.RuntimePropInfo
			{
				EndlessProp = global::UnityEngine.Object.Instantiate<EndlessProp>(this.missingObjectPrefab, this.prefabSpawnTransform),
				Icon = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon,
				PropData = prop,
				IsMissingObject = true,
				IsLoading = false,
				EndlessProp = 
				{
					gameObject = 
					{
						name = propData.Name + " (Missing)"
					}
				}
			};
		}

		// Token: 0x0600221F RID: 8735 RVA: 0x0009CBA0 File Offset: 0x0009ADA0
		public bool TryGetComponentDefinition(Type type, out ComponentDefinition definition)
		{
			definition = null;
			if (!typeof(IBaseType).IsAssignableFrom(type))
			{
				return typeof(IComponentBase).IsAssignableFrom(type) && MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(type, out definition);
			}
			BaseTypeDefinition baseTypeDefinition;
			if (!MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(type, out baseTypeDefinition))
			{
				return false;
			}
			definition = baseTypeDefinition;
			return true;
		}

		// Token: 0x06002220 RID: 8736 RVA: 0x0009CC08 File Offset: 0x0009AE08
		public bool TryGetDataTypeSignature(List<EndlessEventInfo> eventInfos, string memberName, out int[] signature)
		{
			EndlessEventInfo endlessEventInfo = eventInfos.FirstOrDefault((EndlessEventInfo eventInfo) => eventInfo.MemberName == memberName);
			if (endlessEventInfo == null)
			{
				signature = Array.Empty<int>();
				return false;
			}
			signature = endlessEventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray<int>();
			return true;
		}

		// Token: 0x06002221 RID: 8737 RVA: 0x0009CC74 File Offset: 0x0009AE74
		public bool SignaturesMatch(int[] signatureOne, int[] signatureTwo)
		{
			return signatureOne.Length == signatureTwo.Length && !signatureOne.Where((int t, int index) => t != signatureTwo[index]).Any<int>();
		}

		// Token: 0x06002222 RID: 8738 RVA: 0x0009CCB7 File Offset: 0x0009AEB7
		public void UnloadAll()
		{
			this.activeTerrainPalette.UnloadAll();
			this.activePropLibrary.UnloadAll();
			this.activeAudioLibrary.UnloadAll();
		}

		// Token: 0x04001AF8 RID: 6904
		[SerializeField]
		private Stage stageTemplate;

		// Token: 0x04001AF9 RID: 6905
		[SerializeField]
		private OfflineStage offlineStagePrefab;

		// Token: 0x04001AFA RID: 6906
		[SerializeField]
		private List<PropRequirement> propRequirements = new List<PropRequirement>();

		// Token: 0x04001AFB RID: 6907
		[SerializeField]
		private List<BaseTypeRequirement> baseTypeRequirements = new List<BaseTypeRequirement>();

		// Token: 0x04001AFC RID: 6908
		[SerializeField]
		private BaseTypeList baseTypeList;

		// Token: 0x04001AFD RID: 6909
		[SerializeField]
		private ComponentList componentList;

		// Token: 0x04001AFE RID: 6910
		[SerializeField]
		private Transform prefabSpawnTransform;

		// Token: 0x04001AFF RID: 6911
		[SerializeField]
		private EndlessProp basePropPrefab;

		// Token: 0x04001B00 RID: 6912
		[Header("Terrain")]
		[SerializeField]
		private CollisionLibrary collisionLibrary;

		// Token: 0x04001B01 RID: 6913
		[SerializeField]
		private GameObject fallbackTerrain;

		// Token: 0x04001B02 RID: 6914
		[SerializeField]
		private Sprite fallbackDisplayIcon;

		// Token: 0x04001B03 RID: 6915
		[SerializeField]
		private GameObject loadingTerrain;

		// Token: 0x04001B04 RID: 6916
		[Header("Props")]
		[SerializeField]
		private EndlessProp loadingPropObject;

		// Token: 0x04001B05 RID: 6917
		[SerializeField]
		private EndlessProp missingObjectPrefab;

		// Token: 0x04001B06 RID: 6918
		private readonly Dictionary<SerializableGuid, Stage> spawnedLevels = new Dictionary<SerializableGuid, Stage>();

		// Token: 0x04001B07 RID: 6919
		private SerializableGuid lastGameGuid = SerializableGuid.Empty;

		// Token: 0x04001B08 RID: 6920
		private readonly Dictionary<AssetReference, OfflineStage> spawnedOfflineStages = new Dictionary<AssetReference, OfflineStage>();

		// Token: 0x04001B09 RID: 6921
		private readonly Dictionary<AssetReference, LevelState> cachedLevelStates = new Dictionary<AssetReference, LevelState>();

		// Token: 0x04001B0A RID: 6922
		private List<InjectedProps> injectedProps = new List<InjectedProps>();

		// Token: 0x04001B0B RID: 6923
		private PropLibrary activePropLibrary;

		// Token: 0x04001B0C RID: 6924
		private RuntimePalette activeTerrainPalette;

		// Token: 0x04001B0D RID: 6925
		private AudioLibrary activeAudioLibrary;

		// Token: 0x04001B0E RID: 6926
		private Dictionary<SerializableGuid, Dictionary<SerializableGuid, Dictionary<string, object>>> persistantPropStateMap = new Dictionary<SerializableGuid, Dictionary<SerializableGuid, Dictionary<string, object>>>();

		// Token: 0x04001B0F RID: 6927
		private Dictionary<SerializableGuid, List<SerializableGuid>> destroyedObjectMapByStage = new Dictionary<SerializableGuid, List<SerializableGuid>>();

		// Token: 0x04001B10 RID: 6928
		private SerializableGuid activeLevelGuid = SerializableGuid.Empty;

		// Token: 0x04001B11 RID: 6929
		private Dictionary<SerializableGuid, PropRequirement> propRequirementLookup;

		// Token: 0x04001B12 RID: 6930
		private Dictionary<SerializableGuid, BaseTypeRequirement> baseTypeRequirementLookup;

		// Token: 0x04001B13 RID: 6931
		public UnityEvent<Stage> OnActiveStageChanged = new UnityEvent<Stage>();

		// Token: 0x04001B14 RID: 6932
		public UnityEvent<Stage> TerrainAndPropsLoaded;

		// Token: 0x04001B15 RID: 6933
		public UnityEvent<SerializableGuid> OnLevelLoaded = new UnityEvent<SerializableGuid>();
	}
}
