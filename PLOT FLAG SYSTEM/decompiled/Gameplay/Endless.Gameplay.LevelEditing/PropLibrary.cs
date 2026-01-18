using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Gameplay.Scripting;
using Endless.ParticleSystems.Assets;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing;

public class PropLibrary
{
	public class RuntimePropInfo
	{
		public Prop PropData;

		public Sprite Icon;

		public EndlessProp EndlessProp;

		public bool IsLoading { get; set; } = true;

		public bool IsMissingObject { get; set; }

		public List<ComponentDefinition> GetAllDefinitions()
		{
			List<ComponentDefinition> list = new List<ComponentDefinition>();
			if ((SerializableGuid)PropData.BaseTypeId != SerializableGuid.Empty)
			{
				list.Add(GetBaseTypeDefinition());
			}
			list.AddRange(GetComponentDefinitions());
			return list;
		}

		public ComponentDefinition GetBaseTypeDefinition()
		{
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(PropData.BaseTypeId, out var componentDefinition))
			{
				return componentDefinition;
			}
			return null;
		}

		public List<ComponentDefinition> GetComponentDefinitions()
		{
			List<ComponentDefinition> list = new List<ComponentDefinition>();
			foreach (string componentId in PropData.ComponentIds)
			{
				if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentId, out var componentDefinition))
				{
					list.Add(componentDefinition);
				}
			}
			return list;
		}
	}

	private readonly ReferenceFilter[] dynamicFilters = new ReferenceFilter[2]
	{
		ReferenceFilter.Npc,
		ReferenceFilter.InventoryItem
	};

	private Dictionary<AssetReference, RuntimePropInfo> loadedPropMap = new Dictionary<AssetReference, RuntimePropInfo>();

	private List<SerializableGuid> injectedPropIds = new List<SerializableGuid>();

	private HashSet<AssetReference> inflightLoadRequests = new HashSet<AssetReference>();

	private Dictionary<ReferenceFilter, List<RuntimePropInfo>> _referenceFilterMap;

	private readonly EndlessProp loadingObjectProp;

	private readonly Transform prefabSpawnRoot;

	private readonly EndlessProp basePropPrefab;

	private readonly EndlessProp missingObjectPrefab;

	private Dictionary<ReferenceFilter, List<RuntimePropInfo>> ReferenceFilterMap
	{
		get
		{
			if (_referenceFilterMap == null)
			{
				PopulateReferenceFilterMap();
			}
			return _referenceFilterMap;
		}
	}

	public RuntimePropInfo this[AssetReference assetReference] => GetRuntimePropInfo(assetReference);

	public RuntimePropInfo this[SerializableGuid assetId] => GetRuntimePropInfo(assetId);

	public IReadOnlyList<RuntimePropInfo> GetReferenceFilteredDefinitionList(ReferenceFilter filter)
	{
		return ReferenceFilterMap[filter];
	}

	public PropLibrary(Transform prefabSpawnRoot, EndlessProp loadingObjectProp, EndlessProp basePropPrefab, EndlessProp missingObjectPrefab)
	{
		this.prefabSpawnRoot = prefabSpawnRoot;
		this.loadingObjectProp = loadingObjectProp;
		this.basePropPrefab = basePropPrefab;
		this.missingObjectPrefab = missingObjectPrefab;
	}

	public async void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
	{
		if (!loadedPropMap.TryGetValue(prop.ToAssetReference(), out var value) || value.IsMissingObject)
		{
			_referenceFilterMap = null;
			EndlessProp newProp = UnityEngine.Object.Instantiate(propPrefab, prefabSpawnTransform);
			newProp.gameObject.name = prop.Name;
			await newProp.BuildPrefab(prop, testPrefab, testScript, CancellationToken.None);
			RuntimePropInfo value2 = new RuntimePropInfo
			{
				PropData = prop,
				Icon = icon,
				EndlessProp = newProp,
				IsLoading = false,
				IsMissingObject = false
			};
			loadedPropMap.Add(prop.ToAssetReference(), value2);
			injectedPropIds.Add(prop.AssetID);
		}
	}

	public async Task<PropPopulateResult> LoadPropPrefabs(LevelState levelState, Transform prefabSpawnTransform, EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action<int, int> progressCallback = null)
	{
		List<AssetReference> propReferences = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.GetPropAssetReferencesForLoad(levelState.GetUsedPropIds()).ToList();
		_referenceFilterMap = null;
		List<AssetIdVersionKey> modifiedIds = new List<AssetIdVersionKey>();
		foreach (KeyValuePair<AssetReference, RuntimePropInfo> item2 in loadedPropMap)
		{
			if (propReferences.Contains(item2.Key) || !MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(item2.Value.PropData.BaseTypeId, out var componentDefinition))
			{
				continue;
			}
			ReferenceFilter[] array = dynamicFilters;
			foreach (ReferenceFilter referenceFilter in array)
			{
				if (componentDefinition.ComponentBase.Filter.HasFlag(referenceFilter))
				{
					propReferences.Add(item2.Key);
				}
			}
		}
		IReadOnlyList<AssetReference> propReferences2 = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences;
		foreach (KeyValuePair<AssetReference, RuntimePropInfo> item3 in loadedPropMap)
		{
			if (!injectedPropIds.Contains(item3.Key.AssetID))
			{
				if (!propReferences2.Contains(item3.Key))
				{
					UnloadProp(item3.Value, dataUnload: false);
				}
				else if (!MonoBehaviourSingleton<StageManager>.Instance.PreloadContent && (!item3.Value.IsLoading || item3.Value.IsMissingObject) && !propReferences.Contains(item3.Key))
				{
					UnloadProp(item3.Value, dataUnload: false);
				}
			}
		}
		AssetReference[] second = loadedPropMap.Keys.Where((AssetReference key) => !loadedPropMap[key].IsLoading && !loadedPropMap[key].IsMissingObject).ToArray();
		AssetReference[] array2 = propReferences.Except(inflightLoadRequests).Except(second).ToArray();
		AssetReference[] array3 = array2;
		foreach (AssetReference item in array3)
		{
			inflightLoadRequests.Add(item);
		}
		BulkAssetCacheResult<Prop> bulkResult = await EndlessAssetCache.GetBulkAssetsAsync<Prop>(array2);
		List<AssetReference> list = new List<AssetReference>();
		List<AssetReference> list2 = new List<AssetReference>();
		List<AssetReference> list3 = new List<AssetReference>();
		foreach (Prop asset in bulkResult.Assets)
		{
			if (asset.ScriptAsset != null)
			{
				list.Add(asset.ScriptAsset);
			}
			if (asset.PrefabAsset != null)
			{
				list2.Add(asset.PrefabAsset);
			}
			if (asset.VisualAssets != null)
			{
				list3.AddRange(asset.VisualAssets.Where((AssetReference visualAsset) => visualAsset != null));
			}
		}
		Task bulkAssetsAsync = EndlessAssetCache.GetBulkAssetsAsync<ParticleSystemAsset>(list3);
		Task bulkAssetsAsync2 = EndlessAssetCache.GetBulkAssetsAsync<Script>(list);
		Task bulkAssetsAsync3 = EndlessAssetCache.GetBulkAssetsAsync<EndlessPrefabAsset>(list2);
		await Task.WhenAll(new List<Task> { bulkAssetsAsync, bulkAssetsAsync2, bulkAssetsAsync3 });
		await TaskUtilities.ProcessTasksWithSimultaneousCap(bulkResult.Assets.Count, 10, (int index) => LoadPropPrefab(propPrefab, missingObjectPrefab, cancelToken, ProgressUpdated, bulkResult.Assets[index], modifiedIds), cancelToken);
		if (cancelToken.IsCancellationRequested)
		{
			foreach (Prop asset2 in bulkResult.Assets)
			{
				inflightLoadRequests.Remove(asset2.ToAssetReference());
			}
		}
		await Resources.UnloadUnusedAssets();
		if (cancelToken.IsCancellationRequested)
		{
			return null;
		}
		PropPopulateResult result = new PropPopulateResult(modifiedIds);
		PopulateReferenceFilterMap();
		return result;
		void ProgressUpdated(int progress)
		{
			progressCallback?.Invoke(progress, propReferences.Count);
		}
	}

	private async Task LoadPropPrefab(EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action<int> progressCallback, Prop prop, List<AssetIdVersionKey> modifiedIds)
	{
		AssetReference propReference = prop.ToAssetReference();
		if (!loadedPropMap.ContainsKey(propReference) && TryGetRuntimePropInfo(propReference.AssetID, out var metadata))
		{
			modifiedIds.Add(new AssetIdVersionKey
			{
				AssetId = propReference.AssetID,
				Version = propReference.AssetVersion
			});
			UnloadProp(metadata, dataUnload: true);
		}
		await SpawnPropPrefab(propReference, cancelToken, propPrefab, missingObjectPrefab, modifiedIds, prop);
		inflightLoadRequests.Remove(propReference);
		if (!cancelToken.IsCancellationRequested)
		{
			progressCallback?.Invoke(loadedPropMap.Values.Count((RuntimePropInfo value) => !value.IsLoading));
		}
	}

	private void UnloadProp(RuntimePropInfo info, bool dataUnload)
	{
		if (dataUnload)
		{
			if ((bool)info.EndlessProp)
			{
				info.EndlessProp.Cleanup();
				UnityEngine.Object.Destroy(info.EndlessProp.gameObject);
				loadedPropMap.Remove(info.PropData.ToAssetReference());
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, info.PropData.IconFileInstanceId);
			}
		}
		else if (!info.IsMissingObject && !info.IsLoading && (bool)info.EndlessProp)
		{
			info.EndlessProp.Cleanup();
			UnityEngine.Object.Destroy(info.EndlessProp.gameObject);
			info.IsLoading = true;
			info.IsMissingObject = false;
			info.EndlessProp = UnityEngine.Object.Instantiate(loadingObjectProp, prefabSpawnRoot);
			info.EndlessProp.gameObject.name = info.PropData.Name + " (Loading)";
		}
	}

	private void PopulateReferenceFilterMap()
	{
		IEnumerable<ReferenceFilter> enumerable = Enum.GetValues(typeof(ReferenceFilter)).Cast<ReferenceFilter>();
		_referenceFilterMap = new Dictionary<ReferenceFilter, List<RuntimePropInfo>>();
		foreach (ReferenceFilter item in enumerable)
		{
			if (item != ReferenceFilter.None)
			{
				_referenceFilterMap.Add(item, new List<RuntimePropInfo>());
			}
		}
		foreach (RuntimePropInfo value in loadedPropMap.Values)
		{
			foreach (ReferenceFilter item2 in enumerable)
			{
				if (item2 != ReferenceFilter.None && value.EndlessProp.ReferenceFilter.HasFlag(item2))
				{
					_referenceFilterMap[item2].Add(value);
				}
			}
		}
	}

	public AssetReference[] GetAssetReferences()
	{
		return loadedPropMap.Keys.ToArray();
	}

	public RuntimePropInfo GetRuntimePropInfo(AssetReference assetReference)
	{
		return loadedPropMap[assetReference];
	}

	public RuntimePropInfo GetRuntimePropInfo(SerializableGuid assetId)
	{
		AssetReference key = loadedPropMap.Keys.First((AssetReference reference) => (SerializableGuid)reference.AssetID == assetId);
		return loadedPropMap[key];
	}

	public RuntimePropInfo[] GetAllRuntimeProps()
	{
		return loadedPropMap.Values.ToArray();
	}

	public bool TryGetRuntimePropInfo(AssetReference assetReference, out RuntimePropInfo metadata)
	{
		return loadedPropMap.TryGetValue(assetReference, out metadata);
	}

	public bool TryGetRuntimePropInfo(SerializableGuid assetId, out RuntimePropInfo metadata)
	{
		AssetReference assetReference = loadedPropMap.Keys.FirstOrDefault((AssetReference reference) => (SerializableGuid)reference.AssetID == assetId);
		if (assetReference == null)
		{
			metadata = null;
			return false;
		}
		return loadedPropMap.TryGetValue(assetReference, out metadata);
	}

	public void AddMissingObjectRuntimePropInfo(RuntimePropInfo missingObjectInfo)
	{
		loadedPropMap[missingObjectInfo.PropData.ToAssetReference()] = missingObjectInfo;
	}

	public async Task PreloadData(CancellationToken cancelToken, List<SerializableGuid> previouslyMissingProps, Action<int, int> propLoadingUpdate = null)
	{
		IReadOnlyList<AssetReference> propsToPreload = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences;
		BulkAssetCacheResult<Prop> bulkResult = await EndlessAssetCache.GetBulkAssetsAsync<Prop>(propsToPreload.ToArray());
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		Texture2D[] iconTextures = new Texture2D[bulkResult.Assets.Count];
		await TaskUtilities.ProcessTasksWithSimultaneousCap(bulkResult.Assets.Count, 10, LoadTexture, cancelToken);
		if (cancelToken.IsCancellationRequested)
		{
			foreach (Prop asset in bulkResult.Assets)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, asset.IconFileInstanceId);
			}
			return;
		}
		for (int i = 0; i < bulkResult.Assets.Count; i++)
		{
			Prop prop = bulkResult.Assets[i];
			AssetReference assetReference = prop.ToAssetReference();
			RuntimePropInfo metadata = null;
			if (loadedPropMap.TryGetValue(assetReference, out var value))
			{
				if (!value.IsMissingObject)
				{
					propLoadingUpdate?.Invoke(loadedPropMap.Count, propsToPreload.Count);
					continue;
				}
			}
			else
			{
				TryGetRuntimePropInfo(assetReference.AssetID, out metadata);
			}
			Texture2D texture2D = iconTextures[i];
			if (cancelToken.IsCancellationRequested)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, prop.IconFileInstanceId);
				return;
			}
			Sprite icon = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
			EndlessProp endlessProp = UnityEngine.Object.Instantiate(loadingObjectProp, prefabSpawnRoot);
			endlessProp.name = prop.Name + " (Loading)";
			endlessProp.CalculateReferenceFilter(prop);
			RuntimePropInfo value2 = new RuntimePropInfo
			{
				PropData = prop,
				Icon = icon,
				EndlessProp = endlessProp,
				IsLoading = true,
				IsMissingObject = false
			};
			loadedPropMap[assetReference] = value2;
			if (value != null && value.IsMissingObject)
			{
				previouslyMissingProps?.Add(value.PropData.AssetID);
			}
			if (metadata != null)
			{
				UnloadProp(metadata, dataUnload: true);
			}
			propLoadingUpdate?.Invoke(loadedPropMap.Count, propsToPreload.Count);
		}
		foreach (AssetReference item in propsToPreload.Where((AssetReference propToPreload) => !bulkResult.Assets.Any((Prop entry) => entry.AssetID == propToPreload.AssetID)).ToList())
		{
			Prop propData = new Prop
			{
				Name = "Missing Prop",
				AssetID = item.AssetID,
				AssetVersion = item.AssetVersion,
				AssetType = "prop"
			};
			RuntimePropInfo missingObjectInfo = MonoBehaviourSingleton<StageManager>.Instance.GetMissingObjectInfo(propData);
			loadedPropMap.Add(item, missingObjectInfo);
		}
		async Task LoadTexture(int index)
		{
			Texture2D texture2D2 = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2DAsync(MonoBehaviourSingleton<StageManager>.Instance, bulkResult.Assets[index].IconFileInstanceId, "png");
			iconTextures[index] = texture2D2;
		}
	}

	public List<RuntimePropInfo> UnloadPropsNotInGameLibrary()
	{
		List<RuntimePropInfo> list = new List<RuntimePropInfo>();
		foreach (KeyValuePair<AssetReference, RuntimePropInfo> entry in loadedPropMap)
		{
			if (!MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences.Any((AssetReference reference) => reference.AssetID == entry.Value.PropData.AssetID) && !injectedPropIds.Contains(entry.Value.PropData.AssetID))
			{
				list.Add(entry.Value);
			}
		}
		for (int num = 0; num < list.Count; num++)
		{
			UnloadProp(list[num], dataUnload: true);
		}
		return list;
	}

	public async Task FetchAndSpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List<AssetIdVersionKey> modifiedIds = null)
	{
		if (loadedPropMap.TryGetValue(assetReference, out var value) && !value.IsLoading)
		{
			return;
		}
		if (!inflightLoadRequests.Add(assetReference))
		{
			while (inflightLoadRequests.Contains(assetReference) && !cancelToken.IsCancellationRequested)
			{
				await Task.Yield();
			}
			return;
		}
		if (modifiedIds == null)
		{
			modifiedIds = new List<AssetIdVersionKey>();
		}
		AssetCacheResult<Prop> assetCacheResult = await EndlessAssetCache.GetAssetAsync<Prop>(assetReference.AssetID, assetReference.AssetVersion);
		if (!cancelToken.IsCancellationRequested)
		{
			if (assetCacheResult.HasErrors)
			{
				inflightLoadRequests.Remove(assetReference);
				Prop prop = new Prop
				{
					AssetID = assetReference.AssetID,
					AssetVersion = assetReference.AssetVersion,
					AssetType = "prop"
				};
				RuntimePropInfo runtimePropInfo = new RuntimePropInfo
				{
					PropData = prop,
					Icon = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon,
					EndlessProp = UnityEngine.Object.Instantiate(missingObjectPrefab, prefabSpawnRoot),
					IsLoading = false,
					IsMissingObject = true
				};
				runtimePropInfo.EndlessProp.name = "Missing Object from result.HasErrors";
				loadedPropMap[prop.ToAssetReference()] = runtimePropInfo;
				Debug.LogException(new Exception("Failed to fetch prop from library: " + assetReference.AssetID + ": " + assetReference.AssetVersion, assetCacheResult.GetErrorMessage()));
			}
			else
			{
				Prop asset = assetCacheResult.Asset;
				await SpawnPropPrefab(assetReference, cancelToken, propPrefab, missingObjectPrefab, modifiedIds, asset);
				inflightLoadRequests.Remove(assetReference);
			}
		}
	}

	private async Task SpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List<AssetIdVersionKey> modifiedIds, Prop prop)
	{
		try
		{
			EndlessProp newProp = UnityEngine.Object.Instantiate(propPrefab, prefabSpawnRoot);
			newProp.gameObject.name = prop.Name;
			await newProp.BuildPrefab(prop, null, null, cancelToken);
			if (cancelToken.IsCancellationRequested)
			{
				newProp.Cleanup();
				inflightLoadRequests.Remove(assetReference);
				UnityEngine.Object.Destroy(newProp.gameObject);
				return;
			}
			if (loadedPropMap.TryGetValue(assetReference, out var currentInfo))
			{
				if (currentInfo.EndlessProp != null)
				{
					UnityEngine.Object.Destroy(currentInfo.EndlessProp.gameObject);
				}
				if (currentInfo.PropData.PropLocationOffsets == null || currentInfo.PropData.PropLocationOffsets.Count == 0)
				{
					currentInfo.PropData.AddLocationOffset(new PropLocationOffset
					{
						Offset = Vector3Int.zero
					});
				}
				currentInfo.IsLoading = false;
				if (currentInfo.IsMissingObject)
				{
					Texture2D texture2D = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2DAsync(MonoBehaviourSingleton<StageManager>.Instance, prop.IconFileInstanceId, ".png");
					Sprite icon = ((texture2D != null) ? Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero) : null);
					currentInfo.Icon = icon;
				}
				currentInfo.IsMissingObject = false;
				currentInfo.EndlessProp = newProp;
			}
			else
			{
				Texture2D texture2D2 = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2DAsync(MonoBehaviourSingleton<StageManager>.Instance, prop.IconFileInstanceId, ".png");
				if (cancelToken.IsCancellationRequested)
				{
					return;
				}
				Sprite icon2 = ((texture2D2 != null) ? Sprite.Create(texture2D2, new Rect(0f, 0f, texture2D2.width, texture2D2.height), Vector2.zero) : null);
				RuntimePropInfo value = new RuntimePropInfo
				{
					PropData = prop,
					Icon = icon2,
					EndlessProp = newProp,
					IsLoading = false,
					IsMissingObject = false
				};
				loadedPropMap[assetReference] = value;
			}
			if (!modifiedIds.Any((AssetIdVersionKey modified) => modified.AssetId == prop.AssetID))
			{
				modifiedIds.Add(new AssetIdVersionKey
				{
					AssetId = prop.AssetID,
					Version = prop.AssetVersion
				});
			}
		}
		catch (Exception innerException)
		{
			RuntimePropInfo runtimePropInfo = new RuntimePropInfo
			{
				PropData = prop,
				Icon = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon,
				EndlessProp = UnityEngine.Object.Instantiate(missingObjectPrefab, prefabSpawnRoot),
				IsLoading = false,
				IsMissingObject = true
			};
			runtimePropInfo.EndlessProp.gameObject.name = prop.Name + " (Missing)";
			loadedPropMap[assetReference] = runtimePropInfo;
			Debug.LogException(new Exception("Failed to load prop from library: " + assetReference.AssetID + ": " + assetReference.AssetVersion, innerException));
		}
	}

	public void UnloadAll()
	{
		AssetReference[] array = loadedPropMap.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			UnloadProp(loadedPropMap[array[i]], dataUnload: true);
		}
	}

	public bool IsRepopulateRequired(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame)
	{
		bool flag = newGame.GameLibrary.PropReferences.Count != oldGame.GameLibrary.PropReferences.Count;
		if (!flag)
		{
			for (int i = 0; i < newGame.GameLibrary.PropReferences.Count; i++)
			{
				AssetReference currentReference = newGame.GameLibrary.PropReferences[i];
				AssetReference assetReference = oldGame.GameLibrary.PropReferences.FirstOrDefault((AssetReference propRef) => propRef.AssetID == currentReference.AssetID);
				if ((object)assetReference == null || currentReference.AssetVersion != assetReference.AssetVersion)
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
	}

	public async Task<bool> Repopulate(Stage activeStage, CancellationToken cancellationToken)
	{
		foreach (RuntimePropInfo item in UnloadPropsNotInGameLibrary())
		{
			foreach (PropEntry propEntry in activeStage.LevelState.PropEntries)
			{
				if (!(propEntry.AssetId != item.PropData.AssetID))
				{
					RuntimePropInfo missingObjectInfo = MonoBehaviourSingleton<StageManager>.Instance.GetMissingObjectInfo(item.PropData);
					AddMissingObjectRuntimePropInfo(missingObjectInfo);
					Debug.Log(string.Format("{0}.{1} spawning missing prop for {2}", "PropLibrary", "Repopulate", propEntry.AssetId));
					activeStage.ReplacePropWithMissingObject(propEntry.InstanceId, item, missingObjectInfo);
				}
			}
		}
		List<SerializableGuid> previouslyMissingProps = new List<SerializableGuid>();
		await PreloadData(cancellationToken, previouslyMissingProps);
		if (cancellationToken.IsCancellationRequested)
		{
			return false;
		}
		PropPopulateResult propPopulateResult = await LoadPropPrefabs(activeStage.LevelState, prefabSpawnRoot, basePropPrefab, missingObjectPrefab, cancellationToken);
		if (cancellationToken.IsCancellationRequested)
		{
			return false;
		}
		if (!(await AssetFlagCache.AreAssetsAllowed(propPopulateResult.ModifiedProps)))
		{
			ErrorHandler.HandleError(ErrorCodes.RepopulatingGame_PropLibrary_ContentRestricted, new Exception("Unable to repopulate game library. Reason: Content restricted."), displayModal: true, leaveMatch: true);
		}
		bool flag = false;
		foreach (AssetIdVersionKey asset in propPopulateResult.ModifiedProps)
		{
			activeStage.RespawnPropsWithAssetId(asset.AssetId, previouslyMissingProps.Contains(asset.AssetId));
			foreach (PropEntry item2 in activeStage.LevelState.PropEntries.Where((PropEntry entry) => entry.AssetId == asset.AssetId))
			{
				flag |= SanitizeMemberChanges(item2);
				flag |= SanitizeWireBundles(activeStage.LevelState, item2);
			}
		}
		activeStage.ApplyMemberChanges();
		activeStage.LoadExistingWires();
		return flag;
	}

	private bool SanitizeMemberChanges(PropEntry propEntry)
	{
		bool flag = false;
		foreach (ComponentEntry componentEntry in propEntry.ComponentEntries)
		{
			for (int num = componentEntry.Changes.Count - 1; num >= 0; num--)
			{
				MemberChange memberChange = componentEntry.Changes[num];
				if (Type.GetType(componentEntry.AssemblyQualifiedName).GetMember(memberChange.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0)
				{
					flag = true;
					componentEntry.Changes.RemoveAt(num);
				}
			}
		}
		if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out var metadata) || metadata.IsMissingObject)
		{
			return false;
		}
		EndlessScriptComponent scriptComponent = metadata.EndlessProp.ScriptComponent;
		if (!scriptComponent)
		{
			return false;
		}
		for (int num2 = propEntry.LuaMemberChanges.Count - 1; num2 >= 0; num2--)
		{
			MemberChange memberChange2 = propEntry.LuaMemberChanges[num2];
			if (scriptComponent.Script.InspectorValues.FirstOrDefault((InspectorScriptValue inspectorValue) => inspectorValue.Name == memberChange2.MemberName && memberChange2.DataType == inspectorValue.DataType) == null)
			{
				propEntry.LuaMemberChanges.RemoveAt(num2);
				flag = true;
			}
		}
		if (flag)
		{
			Debug.Log($"Member Change Sanitization was required for prop: {GetRuntimePropInfo(propEntry.AssetId).PropData.Name}: {propEntry.InstanceId}.");
		}
		return flag;
	}

	private bool SanitizeWireBundles(LevelState levelState, PropEntry propEntry)
	{
		bool flag = false;
		for (int num = levelState.WireBundles.Count - 1; num >= 0; num--)
		{
			WireBundle wireBundle = levelState.WireBundles[num];
			PropEntry propEntry2 = levelState.GetPropEntry(wireBundle.EmitterInstanceId);
			PropEntry propEntry3 = levelState.GetPropEntry(wireBundle.ReceiverInstanceId);
			if (!(propEntry2.InstanceId != propEntry.InstanceId) || !(propEntry3.InstanceId != propEntry.InstanceId))
			{
				SerializableGuid assetId = propEntry2.AssetId;
				SerializableGuid assetId2 = propEntry3.AssetId;
				RuntimePropInfo metadata2;
				if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out var metadata) || metadata.IsMissingObject)
				{
					levelState.RemoveBundle(levelState.WireBundles[num]);
				}
				else if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId2, out metadata2) || metadata2.IsMissingObject)
				{
					levelState.RemoveBundle(levelState.WireBundles[num]);
				}
				else
				{
					for (int num2 = wireBundle.Wires.Count - 1; num2 >= 0; num2--)
					{
						WireEntry wireEntry = wireBundle.Wires[num2];
						bool num3 = string.IsNullOrEmpty(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
						bool flag2 = string.IsNullOrEmpty(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
						Type type = ((!num3) ? Type.GetType(wireEntry.EmitterComponentAssemblyQualifiedTypeName) : typeof(EndlessScriptComponent));
						Type type2 = ((!flag2) ? Type.GetType(wireEntry.ReceiverComponentAssemblyQualifiedTypeName) : typeof(EndlessScriptComponent));
						int[] signature;
						if (num3)
						{
							EndlessScriptComponent scriptComponent = metadata.EndlessProp.ScriptComponent;
							if (!scriptComponent)
							{
								Debug.LogWarning("Emitter Component isn't a script component");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
								continue;
							}
							EndlessEventInfo eventInfo = scriptComponent.GetEventInfo(wireEntry.EmitterMemberName);
							if (eventInfo == null)
							{
								Debug.LogWarning("luaEvent " + wireEntry.EmitterMemberName + " no longer exists in the script");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
								continue;
							}
							signature = eventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray();
						}
						else
						{
							if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type, out var definition))
							{
								Debug.LogWarning("The entire component definition could not be found " + type.Name + " " + wireEntry.EmitterComponentAssemblyQualifiedTypeName);
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
								continue;
							}
							if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetDataTypeSignature(definition.AvailableEvents, wireEntry.EmitterMemberName, out signature))
							{
								Debug.LogWarning("The signature of the emitter couldn't be found");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
								continue;
							}
						}
						int[] signature2;
						if (flag2)
						{
							EndlessScriptComponent scriptComponent2 = metadata2.EndlessProp.ScriptComponent;
							if (!scriptComponent2)
							{
								Debug.LogWarning("Receiver Component isn't a script component");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
								continue;
							}
							EndlessEventInfo endlessEventInfo = scriptComponent2.Script.Receivers.FirstOrDefault((EndlessEventInfo r) => r.MemberName == wireEntry.ReceiverMemberName);
							if (endlessEventInfo == null)
							{
								Debug.LogWarning("luaEvent no longer exists in the script");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
								continue;
							}
							signature2 = endlessEventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray();
						}
						else
						{
							if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type2, out var definition2))
							{
								Debug.LogWarning("The entire component definition could not be found");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
								continue;
							}
							if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetDataTypeSignature(definition2.AvailableReceivers, wireEntry.ReceiverMemberName, out signature2))
							{
								Debug.LogWarning("The signature of the receiver couldn't be found");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
								continue;
							}
						}
						if (signature2.Length == 0)
						{
							if (wireEntry.StaticParameters.Length != 0)
							{
								Debug.LogWarning("Receiver receives no parameters but we had leftover static parameters");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
							}
						}
						else if (wireEntry.StaticParameters.Length != 0)
						{
							int[] signatureTwo = wireEntry.StaticParameters.Select((StoredParameter staticParam) => staticParam.DataType).ToArray();
							if (!MonoBehaviourSingleton<StageManager>.Instance.SignaturesMatch(signature2, signatureTwo))
							{
								Debug.LogWarning("Receiver did not match the static parameter data type signature");
								flag = true;
								wireBundle.Wires.RemoveAt(num2);
							}
						}
						else if (!MonoBehaviourSingleton<StageManager>.Instance.SignaturesMatch(signature2, signature))
						{
							Debug.LogWarning("Receiver did not match the emitter's parameter data type signature");
							flag = true;
							wireBundle.Wires.RemoveAt(num2);
						}
					}
					if (wireBundle.Wires.Count == 0)
					{
						flag = true;
						levelState.RemoveBundle(levelState.WireBundles[num]);
					}
				}
			}
		}
		if (flag)
		{
			Debug.Log($"Wire Sanitization was required for prop: {GetRuntimePropInfo(propEntry.AssetId).PropData.Name}: {propEntry.InstanceId}.");
		}
		return flag;
	}

	public bool IsInjectedProp(string propDataAssetID)
	{
		return injectedPropIds.Contains(propDataAssetID);
	}

	public List<SerializableGuid> GetBaseTypeList(string propDataBaseTypeId)
	{
		return GetBaseTypeList(new SerializableGuid[1] { propDataBaseTypeId });
	}

	public List<SerializableGuid> GetBaseTypeList(SerializableGuid[] propDataBaseTypeId)
	{
		return loadedPropMap.Values.Where((RuntimePropInfo info) => propDataBaseTypeId.Contains<SerializableGuid>(info.PropData.BaseTypeId)).Select((Func<RuntimePropInfo, SerializableGuid>)((RuntimePropInfo info) => info.PropData.AssetID)).ToList();
	}
}
