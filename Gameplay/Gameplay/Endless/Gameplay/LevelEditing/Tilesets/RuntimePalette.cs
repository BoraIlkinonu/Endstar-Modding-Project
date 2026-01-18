using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
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

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x0200051B RID: 1307
	public class RuntimePalette
	{
		// Token: 0x17000610 RID: 1552
		// (get) Token: 0x06001F81 RID: 8065 RVA: 0x0008BA64 File Offset: 0x00089C64
		public List<Tileset> DistinctTilesets
		{
			get
			{
				return this.tilesetList.OrderBy((Tileset tileset) => tileset.Index).Distinct<Tileset>().ToList<Tileset>();
			}
		}

		// Token: 0x06001F82 RID: 8066 RVA: 0x0008BA9C File Offset: 0x00089C9C
		public RuntimePalette(CollisionLibrary collisionLibrary, GameObject terrainFallback, Sprite fallbackDisplayIcon, GameObject loadingTerrain)
		{
			this.collisionLibrary = collisionLibrary;
			this.terrainFallback = terrainFallback;
			this.fallbackDisplayIcon = fallbackDisplayIcon;
			this.loadingTerrain = loadingTerrain;
			this.generatedTerrainTracker = new GeneratedTerrainTracker();
			GameObject gameObject = new GameObject("Terrain Visuals Manager");
			gameObject.transform.SetParent(MonoBehaviourSingleton<StageManager>.Instance.transform);
			this.endlessVisuals = gameObject.AddComponent<EndlessVisuals>();
		}

		// Token: 0x06001F83 RID: 8067 RVA: 0x0008BB24 File Offset: 0x00089D24
		public bool IsRepopulateRequired(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame)
		{
			bool flag;
			if (!(flag = newGame.GameLibrary.TerrainEntries.Count != oldGame.GameLibrary.TerrainEntries.Count))
			{
				int num = 0;
				while (num < newGame.GameLibrary.TerrainEntries.Count && !flag)
				{
					TerrainUsage terrainUsage = newGame.GameLibrary.TerrainEntries[num];
					TerrainUsage terrainUsage2 = oldGame.GameLibrary.TerrainEntries[num];
					if (terrainUsage != terrainUsage2)
					{
						flag = true;
					}
					num++;
				}
			}
			return flag;
		}

		// Token: 0x06001F84 RID: 8068 RVA: 0x0008BBA8 File Offset: 0x00089DA8
		public Task Repopulate(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame, CancellationToken cancellationToken)
		{
			RuntimePalette.<Repopulate>d__14 <Repopulate>d__;
			<Repopulate>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<Repopulate>d__.<>4__this = this;
			<Repopulate>d__.newGame = newGame;
			<Repopulate>d__.oldGame = oldGame;
			<Repopulate>d__.cancellationToken = cancellationToken;
			<Repopulate>d__.<>1__state = -1;
			<Repopulate>d__.<>t__builder.Start<RuntimePalette.<Repopulate>d__14>(ref <Repopulate>d__);
			return <Repopulate>d__.<>t__builder.Task;
		}

		// Token: 0x06001F85 RID: 8069 RVA: 0x0008BC04 File Offset: 0x00089E04
		public async Task Populate(IReadOnlyList<TerrainUsage> usagesToPopulate, GameLibrary gameLibrary, CancellationToken cancelToken, Action<int, int> progressCallback = null)
		{
			RuntimePalette.<>c__DisplayClass15_0 CS$<>8__locals1 = new RuntimePalette.<>c__DisplayClass15_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.gameLibrary = gameLibrary;
			CS$<>8__locals1.cancelToken = cancelToken;
			CS$<>8__locals1.progressCallback = progressCallback;
			MonoBehaviourSingleton<LoadTimeTester>.Instance.StartTracking("RuntimePalette - Populate");
			CS$<>8__locals1.activeCount = usagesToPopulate.Count<TerrainUsage>();
			if (!MonoBehaviourSingleton<StageManager>.Instance.PreloadContent)
			{
				using (Dictionary<AssetReference, LocalAssetBundleInfo>.Enumerator enumerator = this.localAssetBundleInfo.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<AssetReference, LocalAssetBundleInfo> entry2 = enumerator.Current;
						if (!usagesToPopulate.Any((TerrainUsage usage) => usage.TerrainAssetReference == entry2.Key))
						{
							this.ReleaseGeneratedTerrain(entry2.Key, false);
							this.SetTilesetAtIndex(entry2.Value.GeneratedTileset.Index, entry2.Value.GeneratedTileset);
						}
					}
				}
				MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Unload Content", "RuntimePalette - Populate");
			}
			AssetReference[] array = this.localAssetBundleInfo.Keys.Where((AssetReference blah) => !(CS$<>8__locals1.<>4__this.localAssetBundleInfo[blah].GeneratedTileset is LoadingPlaceholderTileset)).ToArray<AssetReference>();
			AssetReference[] assetReferences = usagesToPopulate.Select((TerrainUsage entry) => entry.TerrainAssetReference).Except(array).ToArray<AssetReference>();
			BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<TerrainTilesetCosmeticAsset>(assetReferences);
			CS$<>8__locals1.bulkResult = bulkAssetCacheResult;
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Bulk fetch", "RuntimePalette - Populate");
			await TaskUtilities.ProcessTasksWithSimultaneousCap(CS$<>8__locals1.bulkResult.Assets.Count, 10, (int index) => CS$<>8__locals1.<>4__this.ProcessBulkTerrain(CS$<>8__locals1.gameLibrary, CS$<>8__locals1.cancelToken, new Action<int>(base.<Populate>g__ProgressCallback|3), CS$<>8__locals1.bulkResult.Assets, index), CS$<>8__locals1.cancelToken, false);
			MonoBehaviourSingleton<LoadTimeTester>.Instance.LogTimeDelta("Process Terrain", "RuntimePalette - Populate");
			await Resources.UnloadUnusedAssets();
			if (!CS$<>8__locals1.cancelToken.IsCancellationRequested)
			{
				IEnumerable<AssetReference> enumerable = assetReferences;
				Func<AssetReference, bool> func;
				if ((func = CS$<>8__locals1.<>9__5) == null)
				{
					RuntimePalette.<>c__DisplayClass15_0 CS$<>8__locals3 = CS$<>8__locals1;
					Func<AssetReference, bool> func2 = (AssetReference reference) => !CS$<>8__locals1.bulkResult.Assets.Any((TerrainTilesetCosmeticAsset asset) => asset.AssetID == reference.AssetID);
					CS$<>8__locals3.<>9__5 = func2;
					func = func2;
				}
				foreach (AssetReference assetReference in enumerable.Where(func))
				{
					int terrainIndex = CS$<>8__locals1.gameLibrary.GetTerrainIndex(assetReference.AssetID);
					this.SetTilesetAtIndex(terrainIndex, new FallbackTileset(new Asset
					{
						Name = "Unknown",
						Description = "Unknown Description",
						AssetID = assetReference.AssetID
					}, this.fallbackDisplayIcon, this.terrainFallback, terrainIndex));
				}
				for (int i = 0; i < CS$<>8__locals1.gameLibrary.TerrainEntries.Count; i++)
				{
					TerrainUsage terrainUsage = CS$<>8__locals1.gameLibrary.TerrainEntries[i];
					if (!terrainUsage.IsActive)
					{
						this.ConvertInactiveTerrainUsageToTileset(CS$<>8__locals1.gameLibrary, terrainUsage, i);
					}
				}
				this.LoadFinished();
				MonoBehaviourSingleton<LoadTimeTester>.Instance.StopTracking("RuntimePalette - Populate");
			}
		}

		// Token: 0x06001F86 RID: 8070 RVA: 0x0008BC68 File Offset: 0x00089E68
		private async Task ProcessBulkTerrain(GameLibrary gameLibrary, CancellationToken cancelToken, Action<int> progressCallback, List<TerrainTilesetCosmeticAsset> terrain, int index)
		{
			TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = terrain[index];
			int usageIndex = gameLibrary.GetTerrainIndex(terrainTilesetCosmeticAsset.AssetID);
			Tileset tileset = await this.LoadTileset(gameLibrary, terrainTilesetCosmeticAsset.AssetID, usageIndex, cancelToken);
			if (!cancelToken.IsCancellationRequested)
			{
				this.SetTilesetAtIndex(usageIndex, tileset);
				if (progressCallback != null)
				{
					progressCallback(this.localAssetBundleInfo.Values.Count((LocalAssetBundleInfo bundleInfo) => !(bundleInfo.GeneratedTileset is LoadingPlaceholderTileset)));
				}
			}
		}

		// Token: 0x06001F87 RID: 8071 RVA: 0x0008BCD8 File Offset: 0x00089ED8
		private void ConvertInactiveTerrainUsageToTileset(GameLibrary gameLibrary, TerrainUsage usage, int index)
		{
			HashSet<int> hashSet = new HashSet<int>();
			TerrainUsage terrainUsage = usage;
			int num;
			for (;;)
			{
				num = terrainUsage.RedirectIndex;
				if (num > gameLibrary.TerrainEntries.Count)
				{
					num = 0;
					global::UnityEngine.Debug.LogException(new Exception("Had improper index in terrain redirects"));
				}
				else if (hashSet.Contains(num))
				{
					break;
				}
				hashSet.Add(num);
				terrainUsage = gameLibrary.TerrainEntries[num];
				if (terrainUsage.IsActive)
				{
					goto Block_3;
				}
			}
			global::UnityEngine.Debug.LogException(new Exception(string.Format("Redirect Indexes looped. Can't validate terrain usage at index: {0}", index)));
			return;
			Block_3:
			this.SetTilesetAtIndex(index, this.GetTileset(num));
		}

		// Token: 0x06001F88 RID: 8072 RVA: 0x0008BD64 File Offset: 0x00089F64
		private async Task<Tileset> LoadTileset(GameLibrary gameLibrary, SerializableGuid id, int index, CancellationToken cancellationToken)
		{
			TerrainUsage usage = gameLibrary.TerrainEntries.FirstOrDefault((TerrainUsage terrainUsage) => terrainUsage.TilesetId == id);
			AssetBundle targetAssetBundle = null;
			string empty = string.Empty;
			LocalAssetBundleInfo localAssetBundleInfo;
			Tileset tileset;
			if (this.localAssetBundleInfo.TryGetValue(usage.TerrainAssetReference, out localAssetBundleInfo) && !(localAssetBundleInfo.GeneratedTileset is LoadingPlaceholderTileset))
			{
				tileset = localAssetBundleInfo.GeneratedTileset;
			}
			else
			{
				AssetCacheResult<TerrainTilesetCosmeticAsset> assetCacheResult = await EndlessAssetCache.GetAssetAsync<TerrainTilesetCosmeticAsset>(usage.TerrainAssetReference.AssetID, usage.TerrainAssetReference.AssetVersion);
				AssetCacheResult<TerrainTilesetCosmeticAsset> result = assetCacheResult;
				TilesetCosmeticProfileRequest acquiredAssetBundleRequest;
				if (result.HasErrors)
				{
					acquiredAssetBundleRequest = null;
					global::UnityEngine.Debug.LogException(result.GetErrorMessage());
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
						int fileInstanceIdFromProfile = RuntimePalette.GetFileInstanceIdFromProfile(asset);
						foreach (KeyValuePair<AssetReference, LocalAssetBundleInfo> keyValuePair in this.localAssetBundleInfo)
						{
							if (keyValuePair.Key.AssetID == id && keyValuePair.Value.FileInstanceId == fileInstanceIdFromProfile && !(keyValuePair.Value.GeneratedTileset is LoadingPlaceholderTileset))
							{
								keyValuePair.Key.AssetVersion = usage.TerrainAssetReference.AssetVersion;
								keyValuePair.Value.GeneratedTileset.UpdateToAsset(asset);
								return keyValuePair.Value.GeneratedTileset;
							}
						}
						acquiredAssetBundleRequest = await RuntimePalette.AcquireAssetBundle(asset);
						if (cancellationToken.IsCancellationRequested)
						{
							return null;
						}
						targetAssetBundle = acquiredAssetBundleRequest.AssetBundle;
						string name = acquiredAssetBundleRequest.Asset.Name;
					}
					catch (Exception ex)
					{
						acquiredAssetBundleRequest = null;
						global::UnityEngine.Debug.LogException(ex);
					}
				}
				Sprite displayIcon = this.fallbackDisplayIcon;
				try
				{
					Texture2D texture2D = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetTexture2DAsync(MonoBehaviourSingleton<StageManager>.Instance, result.Asset.DisplayIconFileInstance.AssetFileInstanceId, "png");
					displayIcon = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), Vector2.zero);
				}
				catch (Exception ex2)
				{
					global::UnityEngine.Debug.LogException(ex2);
				}
				if (acquiredAssetBundleRequest == null)
				{
					tileset = new FallbackTileset(new Asset
					{
						Name = "Unknown",
						Description = "Unknown Description",
						AssetID = id
					}, displayIcon, this.terrainFallback, index);
				}
				else if (acquiredAssetBundleRequest.AssetBundle == null)
				{
					tileset = new FallbackTileset(acquiredAssetBundleRequest.Asset, displayIcon, this.terrainFallback, index);
				}
				else
				{
					TilesetCosmeticProfile targetProfile = targetAssetBundle.LoadAllAssets<TilesetCosmeticProfile>().FirstOrDefault<TilesetCosmeticProfile>();
					if (!targetProfile)
					{
						tileset = new FallbackTileset(acquiredAssetBundleRequest.Asset, displayIcon, this.terrainFallback, index);
					}
					else
					{
						if (!this.generatedTerrainRoot)
						{
							this.generatedTerrainRoot = new GameObject("Generated Terrain Root");
							this.generatedTerrainRoot.transform.SetParent(MonoBehaviourSingleton<StageManager>.Instance.transform);
						}
						GameObject gameObject = new GameObject(targetProfile.DisplayName);
						gameObject.SetActive(false);
						Transform currentRoot = gameObject.transform;
						currentRoot.SetParent(this.generatedTerrainRoot.transform);
						this.generatedTerrainTracker.Add(usage.TerrainAssetReference, currentRoot.gameObject);
						Stopwatch frameStopWatch = new Stopwatch();
						frameStopWatch.Start();
						int tileCosmeticIndex = 0;
						while (tileCosmeticIndex < targetProfile.TileCosmetics.Length)
						{
							int visualIndex = 0;
							int i;
							while (visualIndex < targetProfile.TileCosmetics[tileCosmeticIndex].Visuals.Count)
							{
								Transform transform = global::UnityEngine.Object.Instantiate<Transform>(targetProfile.TileCosmetics[tileCosmeticIndex].Visuals[visualIndex], currentRoot);
								this.generatedTerrainTracker.Add(usage.TerrainAssetReference, transform.gameObject);
								this.collisionLibrary.ApplyCollisionToVisual(transform, tileCosmeticIndex, targetProfile.TilesetType);
								targetProfile.TileCosmetics[tileCosmeticIndex].Visuals[visualIndex] = transform;
								if (frameStopWatch.ElapsedMilliseconds > 64L)
								{
									await Task.Yield();
									frameStopWatch.Restart();
									if (cancellationToken.IsCancellationRequested)
									{
										return null;
									}
								}
								i = visualIndex++;
							}
							i = tileCosmeticIndex++;
						}
						if (cancellationToken.IsCancellationRequested)
						{
							tileset = null;
						}
						else
						{
							List<Material> list = new List<Material>();
							MaterialWeightTableAsset[] materialVariantWeightTables = targetProfile.MaterialVariantWeightTables;
							for (int i = 0; i < materialVariantWeightTables.Length; i++)
							{
								foreach (Material material in materialVariantWeightTables[i].Table.Values)
								{
									list.Add(material);
								}
							}
							if (targetProfile.TopDecorationSet)
							{
								foreach (Transform transform2 in targetProfile.TopDecorationSet.Values)
								{
									if (!(transform2 == null))
									{
										RuntimePalette.<LoadTileset>g__AddMaterials|18_1(transform2, list);
									}
								}
							}
							if (targetProfile.SideDecorationSet)
							{
								foreach (Transform transform3 in targetProfile.SideDecorationSet.Values)
								{
									if (!(transform3 == null))
									{
										RuntimePalette.<LoadTileset>g__AddMaterials|18_1(transform3, list);
									}
								}
							}
							if (targetProfile.BottomDecorationSet)
							{
								foreach (Transform transform4 in targetProfile.BottomDecorationSet.Values)
								{
									if (!(transform4 == null))
									{
										RuntimePalette.<LoadTileset>g__AddMaterials|18_1(transform4, list);
									}
								}
							}
							foreach (BaseFringeCosmeticProfile baseFringeCosmeticProfile in targetProfile.BaseFringeSets)
							{
								if (baseFringeCosmeticProfile.CornerFringe)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(baseFringeCosmeticProfile.CornerFringe.transform, list);
								}
								if (baseFringeCosmeticProfile.IslandFringe)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(baseFringeCosmeticProfile.IslandFringe.transform, list);
								}
								if (baseFringeCosmeticProfile.PeninsulaFringe)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(baseFringeCosmeticProfile.PeninsulaFringe.transform, list);
								}
								if (baseFringeCosmeticProfile.SingleFringe)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(baseFringeCosmeticProfile.SingleFringe.transform, list);
								}
								if (baseFringeCosmeticProfile.CornerCapFringe)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(baseFringeCosmeticProfile.CornerCapFringe.transform, list);
								}
							}
							foreach (SlopeFringeCosmeticProfile slopeFringeCosmeticProfile in targetProfile.SlopeFringeSets)
							{
								GameObject[] array = slopeFringeCosmeticProfile.VerticalFringes;
								for (int j = 0; j < array.Length; j++)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(array[j].transform, list);
								}
								array = slopeFringeCosmeticProfile.FlatFringeCorners;
								for (int j = 0; j < array.Length; j++)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(array[j].transform, list);
								}
								array = slopeFringeCosmeticProfile.FlatFringePeninsulas;
								for (int j = 0; j < array.Length; j++)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(array[j].transform, list);
								}
								array = slopeFringeCosmeticProfile.InnerCornerFringePeninsulas;
								for (int j = 0; j < array.Length; j++)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(array[j].transform, list);
								}
								array = slopeFringeCosmeticProfile.OuterCornerFringePeninsulas;
								for (int j = 0; j < array.Length; j++)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(array[j].transform, list);
								}
								array = slopeFringeCosmeticProfile.FlatFringeSingles;
								for (int j = 0; j < array.Length; j++)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(array[j].transform, list);
								}
								if (slopeFringeCosmeticProfile.FlatFringeIsland)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(slopeFringeCosmeticProfile.FlatFringeIsland.transform, list);
								}
								if (slopeFringeCosmeticProfile.InnerCornerFringe)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(slopeFringeCosmeticProfile.InnerCornerFringe.transform, list);
								}
								if (slopeFringeCosmeticProfile.OuterCornerFringe)
								{
									RuntimePalette.<LoadTileset>g__AddMaterials|18_1(slopeFringeCosmeticProfile.OuterCornerFringe.transform, list);
								}
							}
							foreach (Material material2 in list)
							{
								string text = ((material2.shader.name == "Shader Graphs/Endless_Shader") ? "Shader Graphs/Endless_Shader_Terrain" : material2.shader.name);
								EndlessVisuals.SwapShader(material2, Shader.Find(text), true);
							}
							List<MaterialManager> list2 = this.endlessVisuals.ManageMaterials(list);
							Tileset tileset2;
							switch (targetProfile.TilesetType)
							{
							case TilesetType.Base:
								tileset2 = new BaseTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index);
								break;
							case TilesetType.Slope:
								tileset2 = new SlopeTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index);
								break;
							case TilesetType.Pillar:
								tileset2 = new PillarTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index);
								break;
							case TilesetType.Horizontal:
								tileset2 = new HorizontalTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index);
								break;
							default:
								tileset2 = new BaseTileset(targetProfile, acquiredAssetBundleRequest.Asset, displayIcon, index);
								break;
							}
							Tileset tileset3 = tileset2;
							targetAssetBundle.Unload(false);
							LocalAssetBundleInfo localAssetBundleInfo2 = new LocalAssetBundleInfo
							{
								Name = acquiredAssetBundleRequest.Asset.Name,
								FileInstanceId = acquiredAssetBundleRequest.FileInstanceId,
								AssetVersion = acquiredAssetBundleRequest.Asset.AssetVersion,
								GeneratedTileset = tileset3,
								MaterialManagers = list2,
								IconFileInstanceId = acquiredAssetBundleRequest.Asset.DisplayIconFileInstance.AssetFileInstanceId
							};
							AssetIdVersionKey assetIdVersionKey = default(AssetIdVersionKey);
							assetIdVersionKey.AssetId = usage.TilesetId;
							assetIdVersionKey.Version = acquiredAssetBundleRequest.Asset.AssetVersion;
							this.localAssetBundleInfo[usage.TerrainAssetReference] = localAssetBundleInfo2;
							if (cancellationToken.IsCancellationRequested)
							{
								tileset = null;
							}
							else
							{
								tileset = tileset3;
							}
						}
					}
				}
			}
			return tileset;
		}

		// Token: 0x06001F89 RID: 8073 RVA: 0x0008BDC8 File Offset: 0x00089FC8
		public void SetTilesetAtIndex(int index, Tileset tileset)
		{
			try
			{
				int num = index + 1 - this.tilesetList.Count;
				for (int i = 0; i < num; i++)
				{
					this.tilesetList.Add(null);
				}
				this.tilesetList[index] = tileset;
				this.tilesetList[index].Index = index;
			}
			catch (ArgumentOutOfRangeException ex)
			{
				global::UnityEngine.Debug.LogError(string.Format("Index: {0} out of range of {1}", index, this.tilesetList.Count));
				global::UnityEngine.Debug.LogException(ex);
			}
		}

		// Token: 0x06001F8A RID: 8074 RVA: 0x0008BE5C File Offset: 0x0008A05C
		public Tileset GetTileset(int index)
		{
			while (this.tilesetList.Count <= index)
			{
				global::UnityEngine.Debug.LogWarning("Added an unknown fallback");
				FallbackTileset fallbackTileset = new FallbackTileset(new Asset
				{
					Name = "Unknown",
					Description = "Unknown Description",
					AssetID = SerializableGuid.Empty
				}, this.fallbackDisplayIcon, this.terrainFallback, index);
				this.tilesetList.Add(fallbackTileset);
			}
			return this.tilesetList[index];
		}

		// Token: 0x06001F8B RID: 8075 RVA: 0x0008BED9 File Offset: 0x0008A0D9
		public void Release()
		{
			global::UnityEngine.Object.Destroy(this.endlessVisuals.gameObject);
			global::UnityEngine.Object.Destroy(this.generatedTerrainRoot);
		}

		// Token: 0x06001F8C RID: 8076 RVA: 0x0008BEF8 File Offset: 0x0008A0F8
		private static async Task<TilesetCosmeticProfileRequest> AcquireAssetBundle(TerrainTilesetCosmeticAsset profile)
		{
			int targetFileInstanceId = RuntimePalette.GetFileInstanceIdFromProfile(profile);
			TilesetCosmeticProfileRequest tilesetCosmeticProfileRequest = new TilesetCosmeticProfileRequest();
			tilesetCosmeticProfileRequest.Asset = profile;
			TilesetCosmeticProfileRequest tilesetCosmeticProfileRequest2 = tilesetCosmeticProfileRequest;
			AssetBundle assetBundle = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAssetBundleAsync(MonoBehaviourSingleton<StageManager>.Instance, targetFileInstanceId);
			tilesetCosmeticProfileRequest2.AssetBundle = assetBundle;
			tilesetCosmeticProfileRequest.FileInstanceId = targetFileInstanceId;
			TilesetCosmeticProfileRequest tilesetCosmeticProfileRequest3 = tilesetCosmeticProfileRequest;
			tilesetCosmeticProfileRequest2 = null;
			tilesetCosmeticProfileRequest = null;
			return tilesetCosmeticProfileRequest3;
		}

		// Token: 0x06001F8D RID: 8077 RVA: 0x0008BF3B File Offset: 0x0008A13B
		private static int GetFileInstanceIdFromProfile(TerrainTilesetCosmeticAsset profile)
		{
			return profile.WindowsStandaloneBundleFile.AssetFileInstanceId;
		}

		// Token: 0x06001F8E RID: 8078 RVA: 0x0008BF48 File Offset: 0x0008A148
		public int ReplaceWithFallbackPalette(int index)
		{
			FallbackTileset fallbackTileset = new FallbackTileset(new Asset
			{
				Name = "Unknown",
				Description = "Unknown Description",
				AssetID = SerializableGuid.Empty
			}, this.fallbackDisplayIcon, this.terrainFallback, index);
			this.tilesetList[index] = fallbackTileset;
			return index;
		}

		// Token: 0x06001F8F RID: 8079 RVA: 0x0008BFA4 File Offset: 0x0008A1A4
		public void ReleaseGeneratedTerrain(AssetReference assetReference, bool unloadData)
		{
			LocalAssetBundleInfo localAssetBundleInfo;
			if (!this.localAssetBundleInfo.TryGetValue(assetReference, out localAssetBundleInfo))
			{
				return;
			}
			if (unloadData && localAssetBundleInfo.GeneratedTileset is LoadingPlaceholderTileset)
			{
				this.localAssetBundleInfo.Remove(assetReference);
				return;
			}
			this.endlessVisuals.UnmanageMaterials(localAssetBundleInfo.MaterialManagers);
			this.generatedTerrainTracker.ClearGeneratedTerrainForId(assetReference);
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, localAssetBundleInfo.FileInstanceId);
			if (unloadData)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, localAssetBundleInfo.IconFileInstanceId);
				this.localAssetBundleInfo.Remove(assetReference);
				return;
			}
			if (!(localAssetBundleInfo.GeneratedTileset is LoadingPlaceholderTileset))
			{
				Tileset tileset = new LoadingPlaceholderTileset(this.loadingTerrain, localAssetBundleInfo.GeneratedTileset.Asset, localAssetBundleInfo.GeneratedTileset.DisplayIcon, localAssetBundleInfo.GeneratedTileset.Index);
				localAssetBundleInfo.GeneratedTileset = tileset;
			}
		}

		// Token: 0x06001F90 RID: 8080 RVA: 0x0008C07C File Offset: 0x0008A27C
		public async Task PreloadData(GameLibrary gameLibrary, CancellationToken cancelToken, Action<int, int> terrainLoadingUpdate)
		{
			RuntimePalette.<>c__DisplayClass26_0 CS$<>8__locals1 = new RuntimePalette.<>c__DisplayClass26_0();
			this.tilesetList.Clear();
			List<TerrainUsage> list = gameLibrary.TerrainEntries.Where((TerrainUsage usage) => usage.IsActive).ToList<TerrainUsage>();
			int activeCount = list.Count<TerrainUsage>();
			IEnumerable<TerrainUsage> enumerable = gameLibrary.TerrainEntries.Where((TerrainUsage entry) => entry.IsActive);
			AssetReference[] array = this.localAssetBundleInfo.Keys.ToArray<AssetReference>();
			foreach (AssetReference assetReference in list.Select((TerrainUsage entry) => entry.TerrainAssetReference))
			{
				LocalAssetBundleInfo localAssetBundleInfo;
				if (this.localAssetBundleInfo.TryGetValue(assetReference, out localAssetBundleInfo))
				{
					int terrainIndex = gameLibrary.GetTerrainIndex(assetReference.AssetID);
					this.SetTilesetAtIndex(terrainIndex, localAssetBundleInfo.GeneratedTileset);
				}
			}
			AssetReference[] assetReferencesToFetch = enumerable.Select((TerrainUsage entry) => entry.TerrainAssetReference).Except(array).ToArray<AssetReference>();
			BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<TerrainTilesetCosmeticAsset>(assetReferencesToFetch);
			CS$<>8__locals1.bulkResult = bulkAssetCacheResult;
			if (!cancelToken.IsCancellationRequested)
			{
				CS$<>8__locals1.iconTextures = new Texture2D[CS$<>8__locals1.bulkResult.Assets.Count];
				await TaskUtilities.ProcessTasksWithSimultaneousCap(CS$<>8__locals1.bulkResult.Assets.Count, 10, new Func<int, Task>(CS$<>8__locals1.<PreloadData>g__LoadTexture|3), cancelToken, false);
				if (!cancelToken.IsCancellationRequested)
				{
					for (int i = 0; i < CS$<>8__locals1.bulkResult.Assets.Count; i++)
					{
						TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset = CS$<>8__locals1.bulkResult.Assets[i];
						int terrainIndex2 = gameLibrary.GetTerrainIndex(terrainTilesetCosmeticAsset.AssetID);
						Texture2D texture2D = CS$<>8__locals1.iconTextures[i];
						if (cancelToken.IsCancellationRequested)
						{
							foreach (TerrainTilesetCosmeticAsset terrainTilesetCosmeticAsset2 in CS$<>8__locals1.bulkResult.Assets)
							{
								MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, terrainTilesetCosmeticAsset2.DisplayIconFileInstance.AssetFileInstanceId);
							}
							return;
						}
						Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), Vector2.zero);
						Tileset tileset2 = new LoadingPlaceholderTileset(this.loadingTerrain, terrainTilesetCosmeticAsset, sprite, terrainIndex2);
						LocalAssetBundleInfo localAssetBundleInfo2 = new LocalAssetBundleInfo();
						localAssetBundleInfo2.Name = terrainTilesetCosmeticAsset.Name;
						localAssetBundleInfo2.FileInstanceId = terrainTilesetCosmeticAsset.BundleFileAsset.AssetFileInstanceId;
						localAssetBundleInfo2.AssetVersion = terrainTilesetCosmeticAsset.AssetVersion;
						localAssetBundleInfo2.GeneratedTileset = tileset2;
						localAssetBundleInfo2.MaterialManagers = new List<MaterialManager>();
						localAssetBundleInfo2.IconFileInstanceId = terrainTilesetCosmeticAsset.DisplayIconFileInstance.AssetFileInstanceId;
						this.localAssetBundleInfo.Add(terrainTilesetCosmeticAsset.ToAssetReference(), localAssetBundleInfo2);
						this.SetTilesetAtIndex(terrainIndex2, tileset2);
						int num = this.tilesetList.Count((Tileset tileset) => tileset != null);
						if (terrainLoadingUpdate != null)
						{
							terrainLoadingUpdate(num, activeCount);
						}
					}
					if (CS$<>8__locals1.bulkResult.HasErrors)
					{
						global::UnityEngine.Debug.LogException(CS$<>8__locals1.bulkResult.GetErrorMessage());
					}
					IEnumerable<AssetReference> enumerable2 = assetReferencesToFetch;
					Func<AssetReference, bool> func;
					if ((func = CS$<>8__locals1.<>9__6) == null)
					{
						RuntimePalette.<>c__DisplayClass26_0 CS$<>8__locals2 = CS$<>8__locals1;
						Func<AssetReference, bool> func2 = (AssetReference reference) => !CS$<>8__locals1.bulkResult.Assets.Any((TerrainTilesetCosmeticAsset asset) => asset.AssetID == reference.AssetID);
						CS$<>8__locals2.<>9__6 = func2;
						func = func2;
					}
					foreach (AssetReference assetReference2 in enumerable2.Where(func))
					{
					}
					if (!cancelToken.IsCancellationRequested)
					{
						for (int j = 0; j < gameLibrary.TerrainEntries.Count<TerrainUsage>(); j++)
						{
							TerrainUsage terrainUsage = gameLibrary.TerrainEntries[j];
							if (!terrainUsage.IsActive)
							{
								this.ConvertInactiveTerrainUsageToTileset(gameLibrary, terrainUsage, j);
							}
						}
					}
				}
			}
		}

		// Token: 0x06001F91 RID: 8081 RVA: 0x0008C0D8 File Offset: 0x0008A2D8
		public void UnloadExcept(List<TerrainUsage> gameLibraryTerrainEntries)
		{
			List<AssetReference> list = new List<AssetReference>();
			using (Dictionary<AssetReference, LocalAssetBundleInfo>.Enumerator enumerator = this.localAssetBundleInfo.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<AssetReference, LocalAssetBundleInfo> kvp = enumerator.Current;
					if (!gameLibraryTerrainEntries.Any((TerrainUsage entry) => entry.TerrainAssetReference == kvp.Key))
					{
						list.Add(kvp.Key);
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				this.ReleaseGeneratedTerrain(list[i], true);
			}
		}

		// Token: 0x06001F92 RID: 8082 RVA: 0x0008C17C File Offset: 0x0008A37C
		public void LoadFinished()
		{
			foreach (LocalAssetBundleInfo localAssetBundleInfo in this.localAssetBundleInfo.Values)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, localAssetBundleInfo.FileInstanceId);
			}
		}

		// Token: 0x06001F93 RID: 8083 RVA: 0x0008C1E4 File Offset: 0x0008A3E4
		public bool IsTilesetLoaded(int tilesetIndex)
		{
			return tilesetIndex < this.tilesetList.Count && !(this.tilesetList[tilesetIndex] is LoadingPlaceholderTileset);
		}

		// Token: 0x06001F94 RID: 8084 RVA: 0x0008C210 File Offset: 0x0008A410
		public async Task LoadTerrain(TerrainUsage usage, int index, CancellationToken cancelToken)
		{
			if (!this.inflightLoadRequests.Add(index))
			{
				while (this.inflightLoadRequests.Contains(index) && !cancelToken.IsCancellationRequested)
				{
					await Task.Yield();
				}
			}
			else
			{
				Tileset tileset = await this.LoadTileset(MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary, usage.TilesetId, index, cancelToken);
				this.inflightLoadRequests.Remove(index);
				if (!cancelToken.IsCancellationRequested)
				{
					this.SetTilesetAtIndex(index, tileset);
				}
			}
		}

		// Token: 0x06001F95 RID: 8085 RVA: 0x0008C26B File Offset: 0x0008A46B
		public bool IsTilesetIndexInFlight(int tilesetIndex)
		{
			return this.inflightLoadRequests.Contains(tilesetIndex);
		}

		// Token: 0x06001F96 RID: 8086 RVA: 0x0008C27C File Offset: 0x0008A47C
		public void UnloadAll()
		{
			foreach (AssetReference assetReference in this.localAssetBundleInfo.Keys.ToArray<AssetReference>())
			{
				this.ReleaseGeneratedTerrain(assetReference, true);
			}
		}

		// Token: 0x06001F97 RID: 8087 RVA: 0x0008C2B4 File Offset: 0x0008A4B4
		public int GetIndexOfFirstNonRedirect()
		{
			for (int i = 0; i < MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.TerrainEntries.Count<TerrainUsage>(); i++)
			{
				if (MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.TerrainEntries[i].IsActive)
				{
					return i;
				}
			}
			global::UnityEngine.Debug.LogError("Somehow all tilesets are set to inactive?");
			return -1;
		}

		// Token: 0x06001F98 RID: 8088 RVA: 0x0008C314 File Offset: 0x0008A514
		[CompilerGenerated]
		internal static void <LoadTileset>g__AddMaterials|18_1(Transform transform, List<Material> targetMaterialList)
		{
			MeshRenderer[] componentsInChildren = transform.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					Material material = sharedMaterials[j];
					if (!targetMaterialList.Any((Material trackedMaterial) => trackedMaterial.GetInstanceID() == material.GetInstanceID()))
					{
						targetMaterialList.Add(material);
					}
				}
			}
		}

		// Token: 0x04001902 RID: 6402
		private List<Tileset> tilesetList = new List<Tileset>();

		// Token: 0x04001903 RID: 6403
		private readonly CollisionLibrary collisionLibrary;

		// Token: 0x04001904 RID: 6404
		private readonly GameObject terrainFallback;

		// Token: 0x04001905 RID: 6405
		private readonly GameObject loadingTerrain;

		// Token: 0x04001906 RID: 6406
		private Dictionary<AssetReference, LocalAssetBundleInfo> localAssetBundleInfo = new Dictionary<AssetReference, LocalAssetBundleInfo>();

		// Token: 0x04001907 RID: 6407
		private readonly Sprite fallbackDisplayIcon;

		// Token: 0x04001908 RID: 6408
		private GameObject generatedTerrainRoot;

		// Token: 0x04001909 RID: 6409
		private GeneratedTerrainTracker generatedTerrainTracker;

		// Token: 0x0400190A RID: 6410
		private EndlessVisuals endlessVisuals;

		// Token: 0x0400190B RID: 6411
		private HashSet<int> inflightLoadRequests = new HashSet<int>();
	}
}
