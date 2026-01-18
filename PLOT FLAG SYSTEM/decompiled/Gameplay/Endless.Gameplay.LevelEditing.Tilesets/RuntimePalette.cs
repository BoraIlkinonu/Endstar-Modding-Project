using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Data;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.VisualManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using Runtime.Gameplay.LevelEditing;
using Runtime.Gameplay.LevelEditing.Levels;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class RuntimePalette
{
	private List<Tileset> tilesetList = new List<Tileset>();

	private readonly CollisionLibrary collisionLibrary;

	private readonly GameObject terrainFallback;

	private readonly GameObject loadingTerrain;

	private Dictionary<AssetReference, LocalAssetBundleInfo> localAssetBundleInfo = new Dictionary<AssetReference, LocalAssetBundleInfo>();

	private readonly Sprite fallbackDisplayIcon;

	private GameObject generatedTerrainRoot;

	private GeneratedTerrainTracker generatedTerrainTracker;

	private EndlessVisuals endlessVisuals;

	private HashSet<int> inflightLoadRequests = new HashSet<int>();

	public List<Tileset> DistinctTilesets => tilesetList.OrderBy((Tileset tileset) => tileset.Index).Distinct().ToList();

	public RuntimePalette(CollisionLibrary collisionLibrary, GameObject terrainFallback, Sprite fallbackDisplayIcon, GameObject loadingTerrain)
	{
		this.collisionLibrary = collisionLibrary;
		this.terrainFallback = terrainFallback;
		this.fallbackDisplayIcon = fallbackDisplayIcon;
		this.loadingTerrain = loadingTerrain;
		generatedTerrainTracker = new GeneratedTerrainTracker();
		GameObject gameObject = new GameObject("Terrain Visuals Manager");
		gameObject.transform.SetParent(MonoBehaviourSingleton<StageManager>.Instance.transform);
		endlessVisuals = gameObject.AddComponent<EndlessVisuals>();
	}

	public bool IsRepopulateRequired(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame)
	{
		bool flag;
		if (!(flag = newGame.GameLibrary.TerrainEntries.Count != oldGame.GameLibrary.TerrainEntries.Count))
		{
			for (int i = 0; i < newGame.GameLibrary.TerrainEntries.Count; i++)
			{
				if (flag)
				{
					break;
				}
				TerrainUsage terrainUsage = newGame.GameLibrary.TerrainEntries[i];
				TerrainUsage terrainUsage2 = oldGame.GameLibrary.TerrainEntries[i];
				if (terrainUsage != terrainUsage2)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	public async Task Repopulate(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame, CancellationToken cancellationToken)
	{
		HashSet<SerializableGuid> activeGuids = new HashSet<SerializableGuid>();
		for (int i = 0; i < oldGame.GameLibrary.TerrainEntries.Count; i++)
		{
			TerrainUsage terrainUsage = newGame.GameLibrary.TerrainEntries[i];
			TerrainUsage terrainUsage2 = oldGame.GameLibrary.TerrainEntries[i];
			if (terrainUsage.IsActive && !(terrainUsage.TerrainAssetReference.AssetVersion != terrainUsage2.TerrainAssetReference.AssetVersion))
			{
				activeGuids.Add(terrainUsage.TerrainAssetReference.AssetID);
			}
		}
		List<int> setsToRespawn = new List<int>();
		List<int> fallbackTilsetIndexes = new List<int>();
		CancellationToken cancelToken = cancellationToken;
		List<Tileset> oldTilesets = new List<Tileset>(tilesetList);
		for (int j = 0; j < tilesetList.Count; j++)
		{
			if (tilesetList[j] is FallbackTileset)
			{
				fallbackTilsetIndexes.Add(j);
			}
		}
		await PreloadData(newGame.GameLibrary, cancelToken, delegate
		{
		});
		if (cancellationToken.IsCancellationRequested)
		{
			return;
		}
		HashSet<int> usedTileSetIds = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.GetUsedTileSetIds(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary);
		IReadOnlyList<TerrainUsage> terrainsReferencesForLevel = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.GetTerrainUsagesInLevel(usedTileSetIds);
		await Populate(terrainsReferencesForLevel, newGame.GameLibrary, cancelToken);
		List<AssetIdVersionKey> list = new List<AssetIdVersionKey>();
		foreach (TerrainUsage item in terrainsReferencesForLevel.Where((TerrainUsage reference) => reference.IsActive))
		{
			list.Add(new AssetIdVersionKey
			{
				AssetId = item.TerrainAssetReference.AssetID,
				Version = item.TerrainAssetReference.AssetVersion
			});
		}
		if (!(await AssetFlagCache.AreAssetsAllowed(list)))
		{
			ErrorHandler.HandleError(ErrorCodes.RepopulatingGame_RuntimePalette_ContentRestricted, new Exception("Unable to repopulate game library. Reason: Content restricted."), displayModal: true, leaveMatch: true);
		}
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		for (int num = 0; num < tilesetList.Count; num++)
		{
			if (fallbackTilsetIndexes.Contains(num) && !(tilesetList[num] is FallbackTileset))
			{
				setsToRespawn.Add(num);
			}
			if (oldTilesets.Count <= num)
			{
				setsToRespawn.Add(num);
			}
			else if (oldTilesets[num].Asset.AssetID != tilesetList[num].Asset.AssetID || oldTilesets[num].Asset.AssetVersion != tilesetList[num].Asset.AssetVersion)
			{
				setsToRespawn.Add(num);
			}
		}
		List<int> tilesetsToRespawn = setsToRespawn.Distinct().ToList();
		await MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RespawnTerrainCellsWithTilesetIndexes(tilesetsToRespawn, cancelToken);
		for (int num2 = 0; num2 < oldGame.GameLibrary.TerrainEntries.Count; num2++)
		{
			TerrainUsage terrainUsage3 = oldGame.GameLibrary.TerrainEntries[num2];
			if (terrainUsage3.IsActive && !activeGuids.Contains(terrainUsage3.TerrainAssetReference.AssetID))
			{
				UnityEngine.Debug.Log("Releasing Terrain Id: " + terrainUsage3.TerrainAssetReference.AssetID + ", Version: " + terrainUsage3.TerrainAssetReference.AssetVersion);
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.RuntimePalette.ReleaseGeneratedTerrain(terrainUsage3.TerrainAssetReference, unloadData: true);
			}
		}
	}

	public async Task Populate(IReadOnlyList<TerrainUsage> usagesToPopulate, GameLibrary gameLibrary, CancellationToken cancelToken, Action<int, int> progressCallback = null)
	{
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("RuntimePalette - Populate");
		int activeCount = usagesToPopulate.Count();
		if (!MonoBehaviourSingleton<StageManager>.Instance.PreloadContent)
		{
			foreach (KeyValuePair<AssetReference, LocalAssetBundleInfo> entry in localAssetBundleInfo)
			{
				if (!usagesToPopulate.Any((TerrainUsage usage) => usage.TerrainAssetReference == entry.Key))
				{
					ReleaseGeneratedTerrain(entry.Key, unloadData: false);
					SetTilesetAtIndex(entry.Value.GeneratedTileset.Index, entry.Value.GeneratedTileset);
				}
			}
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Unload Content", "RuntimePalette - Populate");
		}
		AssetReference[] second = localAssetBundleInfo.Keys.Where((AssetReference blah) => !(localAssetBundleInfo[blah].GeneratedTileset is LoadingPlaceholderTileset)).ToArray();
		AssetReference[] assetReferences = usagesToPopulate.Select((TerrainUsage terrainUsage2) => terrainUsage2.TerrainAssetReference).Except(second).ToArray();
		BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkResult = await EndlessAssetCache.GetBulkAssetsAsync<TerrainTilesetCosmeticAsset>(assetReferences);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Bulk fetch", "RuntimePalette - Populate");
		await TaskUtilities.ProcessTasksWithSimultaneousCap(bulkResult.Assets.Count, 10, (int index) => ProcessBulkTerrain(gameLibrary, cancelToken, ProgressCallback, bulkResult.Assets, index), cancelToken);
		MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Process Terrain", "RuntimePalette - Populate");
		await Resources.UnloadUnusedAssets();
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		foreach (AssetReference item in assetReferences.Where((AssetReference reference) => !bulkResult.Assets.Any((TerrainTilesetCosmeticAsset asset) => asset.AssetID == reference.AssetID)))
		{
			int terrainIndex = gameLibrary.GetTerrainIndex(item.AssetID);
			Tileset tileset = new FallbackTileset(new Asset
			{
				Name = "Unknown",
				Description = "Unknown Description",
				AssetID = item.AssetID
			}, fallbackDisplayIcon, terrainFallback, terrainIndex);
			SetTilesetAtIndex(terrainIndex, tileset);
		}
		for (int num = 0; num < gameLibrary.TerrainEntries.Count; num++)
		{
			TerrainUsage terrainUsage = gameLibrary.TerrainEntries[num];
			if (!terrainUsage.IsActive)
			{
				ConvertInactiveTerrainUsageToTileset(gameLibrary, terrainUsage, num);
			}
		}
		LoadFinished();
		MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("RuntimePalette - Populate");
		void ProgressCallback(int loadedCount)
		{
			progressCallback?.Invoke(loadedCount, activeCount);
		}
	}

	private async Task ProcessBulkTerrain(GameLibrary gameLibrary, CancellationToken cancelToken, Action<int> progressCallback, List<TerrainTilesetCosmeticAsset> terrain, int index)
	{
		TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = terrain[index];
		int usageIndex = gameLibrary.GetTerrainIndex(terrainTilesetCosmeticAsset.AssetID);
		Tileset tileset = await LoadTileset(gameLibrary, terrainTilesetCosmeticAsset.AssetID, usageIndex, cancelToken);
		if (!cancelToken.IsCancellationRequested)
		{
			SetTilesetAtIndex(usageIndex, tileset);
			progressCallback?.Invoke(localAssetBundleInfo.Values.Count((LocalAssetBundleInfo bundleInfo) => !(bundleInfo.GeneratedTileset is LoadingPlaceholderTileset)));
		}
	}

	private void ConvertInactiveTerrainUsageToTileset(GameLibrary gameLibrary, TerrainUsage usage, int index)
	{
		HashSet<int> hashSet = new HashSet<int>();
		TerrainUsage terrainUsage = usage;
		int num;
		do
		{
			num = terrainUsage.RedirectIndex;
			if (num > gameLibrary.TerrainEntries.Count)
			{
				num = 0;
				UnityEngine.Debug.LogException(new Exception("Had improper index in terrain redirects"));
			}
			else if (hashSet.Contains(num))
			{
				UnityEngine.Debug.LogException(new Exception($"Redirect Indexes looped. Can't validate terrain usage at index: {index}"));
				return;
			}
			hashSet.Add(num);
			terrainUsage = gameLibrary.TerrainEntries[num];
		}
		while (!terrainUsage.IsActive);
		SetTilesetAtIndex(index, GetTileset(num));
	}

	private async Task<Tileset> LoadTileset(GameLibrary gameLibrary, SerializableGuid id, int index, CancellationToken cancellationToken)
	{
		TerrainUsage usage = gameLibrary.TerrainEntries.FirstOrDefault((TerrainUsage terrainUsage) => terrainUsage.TilesetId == id);
		AssetBundle targetAssetBundle = null;
		_ = string.Empty;
		if (localAssetBundleInfo.TryGetValue(usage.TerrainAssetReference, out var value) && !(value.GeneratedTileset is LoadingPlaceholderTileset))
		{
			return value.GeneratedTileset;
		}
		AssetCacheResult<TerrainTilesetCosmeticAsset> result = await EndlessAssetCache.GetAssetAsync<TerrainTilesetCosmeticAsset>(usage.TerrainAssetReference.AssetID, usage.TerrainAssetReference.AssetVersion);
		TilesetCosmeticProfileRequest acquiredAssetBundleRequest;
		if (result.HasErrors)
		{
			acquiredAssetBundleRequest = null;
			UnityEngine.Debug.LogException(result.GetErrorMessage());
		}
		else
		{
			try
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return null;
				}
				TerrainTilesetCosmeticAsset asset = result.Asset;
				int fileInstanceIdFromProfile = GetFileInstanceIdFromProfile(asset);
				foreach (KeyValuePair<AssetReference, LocalAssetBundleInfo> item in localAssetBundleInfo)
				{
					if ((SerializableGuid)item.Key.AssetID == id && item.Value.FileInstanceId == fileInstanceIdFromProfile && !(item.Value.GeneratedTileset is LoadingPlaceholderTileset))
					{
						item.Key.AssetVersion = usage.TerrainAssetReference.AssetVersion;
						item.Value.GeneratedTileset.UpdateToAsset(asset);
						return item.Value.GeneratedTileset;
					}
				}
				acquiredAssetBundleRequest = await AcquireAssetBundle(asset);
				if (cancellationToken.IsCancellationRequested)
				{
					return null;
				}
				targetAssetBundle = acquiredAssetBundleRequest.AssetBundle;
				_ = acquiredAssetBundleRequest.Asset.Name;
			}
			catch (Exception exception)
			{
				acquiredAssetBundleRequest = null;
				UnityEngine.Debug.LogException(exception);
			}
		}
		Sprite displayIcon = fallbackDisplayIcon;
		try
		{
			Texture2D texture2D = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2DAsync(MonoBehaviourSingleton<StageManager>.Instance, result.Asset.DisplayIconFileInstance.AssetFileInstanceId, "png");
			displayIcon = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogException(exception2);
		}
		if (acquiredAssetBundleRequest == null)
		{
			return new FallbackTileset(new Asset
			{
				Name = "Unknown",
				Description = "Unknown Description",
				AssetID = id
			}, displayIcon, terrainFallback, index);
		}
		if ((object)acquiredAssetBundleRequest.AssetBundle == null)
		{
			return new FallbackTileset(acquiredAssetBundleRequest.Asset, displayIcon, terrainFallback, index);
		}
		TilesetCosmeticProfile targetProfile = targetAssetBundle.LoadAllAssets<TilesetCosmeticProfile>().FirstOrDefault();
		if (!targetProfile)
		{
			return new FallbackTileset(acquiredAssetBundleRequest.Asset, displayIcon, terrainFallback, index);
		}
		if (!generatedTerrainRoot)
		{
			generatedTerrainRoot = new GameObject("Generated Terrain Root");
			generatedTerrainRoot.transform.SetParent(MonoBehaviourSingleton<StageManager>.Instance.transform);
		}
		GameObject gameObject = new GameObject(targetProfile.DisplayName);
		gameObject.SetActive(value: false);
		Transform currentRoot = gameObject.transform;
		currentRoot.SetParent(generatedTerrainRoot.transform);
		generatedTerrainTracker.Add(usage.TerrainAssetReference, currentRoot.gameObject);
		Stopwatch frameStopWatch = new Stopwatch();
		frameStopWatch.Start();
		for (int tileCosmeticIndex = 0; tileCosmeticIndex < targetProfile.TileCosmetics.Length; tileCosmeticIndex++)
		{
			for (int visualIndex = 0; visualIndex < targetProfile.TileCosmetics[tileCosmeticIndex].Visuals.Count; visualIndex++)
			{
				Transform transform = UnityEngine.Object.Instantiate(targetProfile.TileCosmetics[tileCosmeticIndex].Visuals[visualIndex], currentRoot);
				generatedTerrainTracker.Add(usage.TerrainAssetReference, transform.gameObject);
				collisionLibrary.ApplyCollisionToVisual(transform, tileCosmeticIndex, targetProfile.TilesetType);
				targetProfile.TileCosmetics[tileCosmeticIndex].Visuals[visualIndex] = transform;
				if (frameStopWatch.ElapsedMilliseconds > 64)
				{
					await Task.Yield();
					frameStopWatch.Restart();
					if (cancellationToken.IsCancellationRequested)
					{
						return null;
					}
				}
			}
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return null;
		}
		List<Material> list = new List<Material>();
		MaterialWeightTableAsset[] materialVariantWeightTables = targetProfile.MaterialVariantWeightTables;
		for (int num = 0; num < materialVariantWeightTables.Length; num++)
		{
			foreach (Material value3 in materialVariantWeightTables[num].Table.Values)
			{
				list.Add(value3);
			}
		}
		if ((bool)targetProfile.TopDecorationSet)
		{
			foreach (Transform value4 in targetProfile.TopDecorationSet.Values)
			{
				if (!(value4 == null))
				{
					AddMaterials(value4, list);
				}
			}
		}
		if ((bool)targetProfile.SideDecorationSet)
		{
			foreach (Transform value5 in targetProfile.SideDecorationSet.Values)
			{
				if (!(value5 == null))
				{
					AddMaterials(value5, list);
				}
			}
		}
		if ((bool)targetProfile.BottomDecorationSet)
		{
			foreach (Transform value6 in targetProfile.BottomDecorationSet.Values)
			{
				if (!(value6 == null))
				{
					AddMaterials(value6, list);
				}
			}
		}
		BaseFringeCosmeticProfile[] baseFringeSets = targetProfile.BaseFringeSets;
		foreach (BaseFringeCosmeticProfile baseFringeCosmeticProfile in baseFringeSets)
		{
			if ((bool)baseFringeCosmeticProfile.CornerFringe)
			{
				AddMaterials(baseFringeCosmeticProfile.CornerFringe.transform, list);
			}
			if ((bool)baseFringeCosmeticProfile.IslandFringe)
			{
				AddMaterials(baseFringeCosmeticProfile.IslandFringe.transform, list);
			}
			if ((bool)baseFringeCosmeticProfile.PeninsulaFringe)
			{
				AddMaterials(baseFringeCosmeticProfile.PeninsulaFringe.transform, list);
			}
			if ((bool)baseFringeCosmeticProfile.SingleFringe)
			{
				AddMaterials(baseFringeCosmeticProfile.SingleFringe.transform, list);
			}
			if ((bool)baseFringeCosmeticProfile.CornerCapFringe)
			{
				AddMaterials(baseFringeCosmeticProfile.CornerCapFringe.transform, list);
			}
		}
		SlopeFringeCosmeticProfile[] slopeFringeSets = targetProfile.SlopeFringeSets;
		foreach (SlopeFringeCosmeticProfile slopeFringeCosmeticProfile in slopeFringeSets)
		{
			GameObject[] verticalFringes = slopeFringeCosmeticProfile.VerticalFringes;
			for (int num2 = 0; num2 < verticalFringes.Length; num2++)
			{
				AddMaterials(verticalFringes[num2].transform, list);
			}
			verticalFringes = slopeFringeCosmeticProfile.FlatFringeCorners;
			for (int num2 = 0; num2 < verticalFringes.Length; num2++)
			{
				AddMaterials(verticalFringes[num2].transform, list);
			}
			verticalFringes = slopeFringeCosmeticProfile.FlatFringePeninsulas;
			for (int num2 = 0; num2 < verticalFringes.Length; num2++)
			{
				AddMaterials(verticalFringes[num2].transform, list);
			}
			verticalFringes = slopeFringeCosmeticProfile.InnerCornerFringePeninsulas;
			for (int num2 = 0; num2 < verticalFringes.Length; num2++)
			{
				AddMaterials(verticalFringes[num2].transform, list);
			}
			verticalFringes = slopeFringeCosmeticProfile.OuterCornerFringePeninsulas;
			for (int num2 = 0; num2 < verticalFringes.Length; num2++)
			{
				AddMaterials(verticalFringes[num2].transform, list);
			}
			verticalFringes = slopeFringeCosmeticProfile.FlatFringeSingles;
			for (int num2 = 0; num2 < verticalFringes.Length; num2++)
			{
				AddMaterials(verticalFringes[num2].transform, list);
			}
			if ((bool)slopeFringeCosmeticProfile.FlatFringeIsland)
			{
				AddMaterials(slopeFringeCosmeticProfile.FlatFringeIsland.transform, list);
			}
			if ((bool)slopeFringeCosmeticProfile.InnerCornerFringe)
			{
				AddMaterials(slopeFringeCosmeticProfile.InnerCornerFringe.transform, list);
			}
			if ((bool)slopeFringeCosmeticProfile.OuterCornerFringe)
			{
				AddMaterials(slopeFringeCosmeticProfile.OuterCornerFringe.transform, list);
			}
		}
		foreach (Material item2 in list)
		{
			string name = ((item2.shader.name == "Shader Graphs/Endless_Shader") ? "Shader Graphs/Endless_Shader_Terrain" : item2.shader.name);
			EndlessVisuals.SwapShader(item2, Shader.Find(name));
		}
		List<MaterialManager> materialManagers = endlessVisuals.ManageMaterials(list);
		Tileset tileset = targetProfile.TilesetType switch
		{
			TilesetType.Base => new BaseTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index), 
			TilesetType.Horizontal => new HorizontalTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index), 
			TilesetType.Pillar => new PillarTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index), 
			TilesetType.Slope => new SlopeTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index), 
			_ => new BaseTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index), 
		};
		targetAssetBundle.Unload(unloadAllLoadedObjects: false);
		LocalAssetBundleInfo value2 = new LocalAssetBundleInfo
		{
			Name = acquiredAssetBundleRequest.Asset.Name,
			FileInstanceId = acquiredAssetBundleRequest.FileInstanceId,
			AssetVersion = acquiredAssetBundleRequest.Asset.AssetVersion,
			GeneratedTileset = tileset,
			MaterialManagers = materialManagers,
			IconFileInstanceId = acquiredAssetBundleRequest.Asset.DisplayIconFileInstance.AssetFileInstanceId
		};
		AssetIdVersionKey assetIdVersionKey = new AssetIdVersionKey
		{
			AssetId = usage.TilesetId,
			Version = acquiredAssetBundleRequest.Asset.AssetVersion
		};
		localAssetBundleInfo[usage.TerrainAssetReference] = value2;
		if (cancellationToken.IsCancellationRequested)
		{
			return null;
		}
		return tileset;
		static void AddMaterials(Transform transform2, List<Material> targetMaterialList)
		{
			MeshRenderer[] componentsInChildren = transform2.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
				foreach (Material material in sharedMaterials)
				{
					if (!targetMaterialList.Any((Material trackedMaterial) => trackedMaterial.GetInstanceID() == material.GetInstanceID()))
					{
						targetMaterialList.Add(material);
					}
				}
			}
		}
	}

	public void SetTilesetAtIndex(int index, Tileset tileset)
	{
		try
		{
			int num = index + 1 - tilesetList.Count;
			for (int i = 0; i < num; i++)
			{
				tilesetList.Add(null);
			}
			tilesetList[index] = tileset;
			tilesetList[index].Index = index;
		}
		catch (ArgumentOutOfRangeException exception)
		{
			UnityEngine.Debug.LogError($"Index: {index} out of range of {tilesetList.Count}");
			UnityEngine.Debug.LogException(exception);
		}
	}

	public Tileset GetTileset(int index)
	{
		while (tilesetList.Count <= index)
		{
			UnityEngine.Debug.LogWarning("Added an unknown fallback");
			FallbackTileset item = new FallbackTileset(new Asset
			{
				Name = "Unknown",
				Description = "Unknown Description",
				AssetID = SerializableGuid.Empty
			}, fallbackDisplayIcon, terrainFallback, index);
			tilesetList.Add(item);
		}
		return tilesetList[index];
	}

	public void Release()
	{
		UnityEngine.Object.Destroy(endlessVisuals.gameObject);
		UnityEngine.Object.Destroy(generatedTerrainRoot);
	}

	private static async Task<TilesetCosmeticProfileRequest> AcquireAssetBundle(TerrainTilesetCosmeticAsset profile)
	{
		int targetFileInstanceId = GetFileInstanceIdFromProfile(profile);
		TilesetCosmeticProfileRequest tilesetCosmeticProfileRequest = new TilesetCosmeticProfileRequest
		{
			Asset = profile
		};
		TilesetCosmeticProfileRequest tilesetCosmeticProfileRequest2 = tilesetCosmeticProfileRequest;
		tilesetCosmeticProfileRequest2.AssetBundle = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAssetBundleAsync(MonoBehaviourSingleton<StageManager>.Instance, targetFileInstanceId);
		tilesetCosmeticProfileRequest.FileInstanceId = targetFileInstanceId;
		return tilesetCosmeticProfileRequest;
	}

	private static int GetFileInstanceIdFromProfile(TerrainTilesetCosmeticAsset profile)
	{
		return profile.WindowsStandaloneBundleFile.AssetFileInstanceId;
	}

	public int ReplaceWithFallbackPalette(int index)
	{
		FallbackTileset value = new FallbackTileset(new Asset
		{
			Name = "Unknown",
			Description = "Unknown Description",
			AssetID = SerializableGuid.Empty
		}, fallbackDisplayIcon, terrainFallback, index);
		tilesetList[index] = value;
		return index;
	}

	public void ReleaseGeneratedTerrain(AssetReference assetReference, bool unloadData)
	{
		if (!localAssetBundleInfo.TryGetValue(assetReference, out var value))
		{
			return;
		}
		if (unloadData && value.GeneratedTileset is LoadingPlaceholderTileset)
		{
			localAssetBundleInfo.Remove(assetReference);
			return;
		}
		endlessVisuals.UnmanageMaterials(value.MaterialManagers);
		generatedTerrainTracker.ClearGeneratedTerrainForId(assetReference);
		MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, value.FileInstanceId);
		if (unloadData)
		{
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, value.IconFileInstanceId);
			localAssetBundleInfo.Remove(assetReference);
		}
		else if (!(value.GeneratedTileset is LoadingPlaceholderTileset))
		{
			Tileset generatedTileset = new LoadingPlaceholderTileset(loadingTerrain, value.GeneratedTileset.Asset, value.GeneratedTileset.DisplayIcon, value.GeneratedTileset.Index);
			value.GeneratedTileset = generatedTileset;
		}
	}

	public async Task PreloadData(GameLibrary gameLibrary, CancellationToken cancelToken, Action<int, int> terrainLoadingUpdate)
	{
		tilesetList.Clear();
		List<TerrainUsage> source = gameLibrary.TerrainEntries.Where((TerrainUsage usage) => usage.IsActive).ToList();
		int activeCount = source.Count();
		IEnumerable<TerrainUsage> source2 = gameLibrary.TerrainEntries.Where((TerrainUsage entry) => entry.IsActive);
		AssetReference[] second = localAssetBundleInfo.Keys.ToArray();
		foreach (AssetReference item in source.Select((TerrainUsage entry) => entry.TerrainAssetReference))
		{
			if (localAssetBundleInfo.TryGetValue(item, out var value))
			{
				int terrainIndex = gameLibrary.GetTerrainIndex(item.AssetID);
				SetTilesetAtIndex(terrainIndex, value.GeneratedTileset);
			}
		}
		AssetReference[] assetReferencesToFetch = source2.Select((TerrainUsage entry) => entry.TerrainAssetReference).Except(second).ToArray();
		BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkResult = await EndlessAssetCache.GetBulkAssetsAsync<TerrainTilesetCosmeticAsset>(assetReferencesToFetch);
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		Texture2D[] iconTextures = new Texture2D[bulkResult.Assets.Count];
		await TaskUtilities.ProcessTasksWithSimultaneousCap(bulkResult.Assets.Count, 10, LoadTexture, cancelToken);
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		for (int num = 0; num < bulkResult.Assets.Count; num++)
		{
			TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = bulkResult.Assets[num];
			int terrainIndex2 = gameLibrary.GetTerrainIndex(terrainTilesetCosmeticAsset.AssetID);
			Texture2D texture2D = iconTextures[num];
			if (cancelToken.IsCancellationRequested)
			{
				foreach (TerrainTilesetCosmeticAsset asset in bulkResult.Assets)
				{
					MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, asset.DisplayIconFileInstance.AssetFileInstanceId);
				}
				return;
			}
			Sprite displayIcon = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
			Tileset tileset = new LoadingPlaceholderTileset(loadingTerrain, terrainTilesetCosmeticAsset, displayIcon, terrainIndex2);
			LocalAssetBundleInfo value2 = new LocalAssetBundleInfo
			{
				Name = terrainTilesetCosmeticAsset.Name,
				FileInstanceId = terrainTilesetCosmeticAsset.BundleFileAsset.AssetFileInstanceId,
				AssetVersion = terrainTilesetCosmeticAsset.AssetVersion,
				GeneratedTileset = tileset,
				MaterialManagers = new List<MaterialManager>(),
				IconFileInstanceId = terrainTilesetCosmeticAsset.DisplayIconFileInstance.AssetFileInstanceId
			};
			localAssetBundleInfo.Add(terrainTilesetCosmeticAsset.ToAssetReference(), value2);
			SetTilesetAtIndex(terrainIndex2, tileset);
			int arg = tilesetList.Count((Tileset tileset2) => tileset2 != null);
			terrainLoadingUpdate?.Invoke(arg, activeCount);
		}
		if (bulkResult.HasErrors)
		{
			UnityEngine.Debug.LogException(bulkResult.GetErrorMessage());
		}
		foreach (AssetReference item2 in assetReferencesToFetch.Where((AssetReference reference) => !bulkResult.Assets.Any((TerrainTilesetCosmeticAsset asset) => asset.AssetID == reference.AssetID)))
		{
			_ = item2;
		}
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		for (int num2 = 0; num2 < gameLibrary.TerrainEntries.Count(); num2++)
		{
			TerrainUsage terrainUsage = gameLibrary.TerrainEntries[num2];
			if (!terrainUsage.IsActive)
			{
				ConvertInactiveTerrainUsageToTileset(gameLibrary, terrainUsage, num2);
			}
		}
		async Task LoadTexture(int index)
		{
			Texture2D texture2D2 = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2DAsync(MonoBehaviourSingleton<StageManager>.Instance, bulkResult.Assets[index].DisplayIconFileInstance.AssetFileInstanceId, "png");
			iconTextures[index] = texture2D2;
		}
	}

	public void UnloadExcept(List<TerrainUsage> gameLibraryTerrainEntries)
	{
		List<AssetReference> list = new List<AssetReference>();
		foreach (KeyValuePair<AssetReference, LocalAssetBundleInfo> kvp in localAssetBundleInfo)
		{
			if (!gameLibraryTerrainEntries.Any((TerrainUsage entry) => entry.TerrainAssetReference == kvp.Key))
			{
				list.Add(kvp.Key);
			}
		}
		for (int num = 0; num < list.Count; num++)
		{
			ReleaseGeneratedTerrain(list[num], unloadData: true);
		}
	}

	public void LoadFinished()
	{
		foreach (LocalAssetBundleInfo value in localAssetBundleInfo.Values)
		{
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, value.FileInstanceId);
		}
	}

	public bool IsTilesetLoaded(int tilesetIndex)
	{
		if (tilesetIndex >= tilesetList.Count)
		{
			return false;
		}
		return !(tilesetList[tilesetIndex] is LoadingPlaceholderTileset);
	}

	public async Task LoadTerrain(TerrainUsage usage, int index, CancellationToken cancelToken)
	{
		if (!inflightLoadRequests.Add(index))
		{
			while (inflightLoadRequests.Contains(index) && !cancelToken.IsCancellationRequested)
			{
				await Task.Yield();
			}
			return;
		}
		Tileset tileset = await LoadTileset(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary, usage.TilesetId, index, cancelToken);
		inflightLoadRequests.Remove(index);
		if (!cancelToken.IsCancellationRequested)
		{
			SetTilesetAtIndex(index, tileset);
		}
	}

	public bool IsTilesetIndexInFlight(int tilesetIndex)
	{
		return inflightLoadRequests.Contains(tilesetIndex);
	}

	public void UnloadAll()
	{
		AssetReference[] array = localAssetBundleInfo.Keys.ToArray();
		foreach (AssetReference assetReference in array)
		{
			ReleaseGeneratedTerrain(assetReference, unloadData: true);
		}
	}

	public int GetIndexOfFirstNonRedirect()
	{
		for (int i = 0; i < MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.TerrainEntries.Count(); i++)
		{
			if (MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.TerrainEntries[i].IsActive)
			{
				return i;
			}
		}
		UnityEngine.Debug.LogError("Somehow all tilesets are set to inactive?");
		return -1;
	}
}
