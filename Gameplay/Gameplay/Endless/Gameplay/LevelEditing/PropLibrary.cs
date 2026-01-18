using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

namespace Endless.Gameplay.LevelEditing
{
	// Token: 0x020004EA RID: 1258
	public class PropLibrary
	{
		// Token: 0x170005FB RID: 1531
		// (get) Token: 0x06001ECB RID: 7883 RVA: 0x00086C85 File Offset: 0x00084E85
		private Dictionary<ReferenceFilter, List<PropLibrary.RuntimePropInfo>> ReferenceFilterMap
		{
			get
			{
				if (this._referenceFilterMap == null)
				{
					this.PopulateReferenceFilterMap();
				}
				return this._referenceFilterMap;
			}
		}

		// Token: 0x06001ECC RID: 7884 RVA: 0x00086C9B File Offset: 0x00084E9B
		public IReadOnlyList<PropLibrary.RuntimePropInfo> GetReferenceFilteredDefinitionList(ReferenceFilter filter)
		{
			return this.ReferenceFilterMap[filter];
		}

		// Token: 0x06001ECD RID: 7885 RVA: 0x00086CAC File Offset: 0x00084EAC
		public PropLibrary(Transform prefabSpawnRoot, EndlessProp loadingObjectProp, EndlessProp basePropPrefab, EndlessProp missingObjectPrefab)
		{
			this.prefabSpawnRoot = prefabSpawnRoot;
			this.loadingObjectProp = loadingObjectProp;
			this.basePropPrefab = basePropPrefab;
			this.missingObjectPrefab = missingObjectPrefab;
		}

		// Token: 0x06001ECE RID: 7886 RVA: 0x00086D14 File Offset: 0x00084F14
		public async void InjectProp(Prop prop, GameObject testPrefab, Script testScript, Sprite icon, Transform prefabSpawnTransform, EndlessProp propPrefab)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!this.loadedPropMap.TryGetValue(prop.ToAssetReference(), out runtimePropInfo) || runtimePropInfo.IsMissingObject)
			{
				this._referenceFilterMap = null;
				EndlessProp newProp = global::UnityEngine.Object.Instantiate<EndlessProp>(propPrefab, prefabSpawnTransform);
				newProp.gameObject.name = prop.Name;
				await newProp.BuildPrefab(prop, testPrefab, testScript, CancellationToken.None);
				PropLibrary.RuntimePropInfo runtimePropInfo2 = new PropLibrary.RuntimePropInfo
				{
					PropData = prop,
					Icon = icon,
					EndlessProp = newProp,
					IsLoading = false,
					IsMissingObject = false
				};
				this.loadedPropMap.Add(prop.ToAssetReference(), runtimePropInfo2);
				this.injectedPropIds.Add(prop.AssetID);
			}
		}

		// Token: 0x06001ECF RID: 7887 RVA: 0x00086D80 File Offset: 0x00084F80
		public async Task<PropPopulateResult> LoadPropPrefabs(LevelState levelState, Transform prefabSpawnTransform, EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action<int, int> progressCallback = null)
		{
			List<AssetReference> propReferences = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.GetPropAssetReferencesForLoad(levelState.GetUsedPropIds()).ToList<AssetReference>();
			this._referenceFilterMap = null;
			List<AssetIdVersionKey> modifiedIds = new List<AssetIdVersionKey>();
			foreach (KeyValuePair<AssetReference, PropLibrary.RuntimePropInfo> keyValuePair in this.loadedPropMap)
			{
				BaseTypeDefinition baseTypeDefinition;
				if (!propReferences.Contains(keyValuePair.Key) && MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(keyValuePair.Value.PropData.BaseTypeId, out baseTypeDefinition))
				{
					foreach (ReferenceFilter referenceFilter in this.dynamicFilters)
					{
						if (baseTypeDefinition.ComponentBase.Filter.HasFlag(referenceFilter))
						{
							propReferences.Add(keyValuePair.Key);
						}
					}
				}
			}
			IReadOnlyList<AssetReference> propReferences2 = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences;
			foreach (KeyValuePair<AssetReference, PropLibrary.RuntimePropInfo> keyValuePair2 in this.loadedPropMap)
			{
				if (!this.injectedPropIds.Contains(keyValuePair2.Key.AssetID))
				{
					if (!propReferences2.Contains(keyValuePair2.Key))
					{
						this.UnloadProp(keyValuePair2.Value, false);
					}
					else if (!MonoBehaviourSingleton<StageManager>.Instance.PreloadContent && (!keyValuePair2.Value.IsLoading || keyValuePair2.Value.IsMissingObject) && !propReferences.Contains(keyValuePair2.Key))
					{
						this.UnloadProp(keyValuePair2.Value, false);
					}
				}
			}
			AssetReference[] array2 = this.loadedPropMap.Keys.Where((AssetReference key) => !this.loadedPropMap[key].IsLoading && !this.loadedPropMap[key].IsMissingObject).ToArray<AssetReference>();
			AssetReference[] array3 = propReferences.Except(this.inflightLoadRequests).Except(array2).ToArray<AssetReference>();
			foreach (AssetReference assetReference in array3)
			{
				this.inflightLoadRequests.Add(assetReference);
			}
			BulkAssetCacheResult<Prop> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<Prop>(array3);
			BulkAssetCacheResult<Prop> bulkResult = bulkAssetCacheResult;
			List<AssetReference> list = new List<AssetReference>();
			List<AssetReference> list2 = new List<AssetReference>();
			List<AssetReference> list3 = new List<AssetReference>();
			foreach (Prop prop in bulkResult.Assets)
			{
				if (prop.ScriptAsset != null)
				{
					list.Add(prop.ScriptAsset);
				}
				if (prop.PrefabAsset != null)
				{
					list2.Add(prop.PrefabAsset);
				}
				if (prop.VisualAssets != null)
				{
					list3.AddRange(prop.VisualAssets.Where((AssetReference visualAsset) => visualAsset != null));
				}
			}
			Task bulkAssetsAsync = EndlessAssetCache.GetBulkAssetsAsync<ParticleSystemAsset>(list3);
			Task bulkAssetsAsync2 = EndlessAssetCache.GetBulkAssetsAsync<Script>(list);
			Task bulkAssetsAsync3 = EndlessAssetCache.GetBulkAssetsAsync<EndlessPrefabAsset>(list2);
			await Task.WhenAll(new List<Task> { bulkAssetsAsync, bulkAssetsAsync2, bulkAssetsAsync3 });
			await TaskUtilities.ProcessTasksWithSimultaneousCap(bulkResult.Assets.Count, 10, (int index) => this.LoadPropPrefab(propPrefab, missingObjectPrefab, cancelToken, new Action<int>(base.<LoadPropPrefabs>g__ProgressUpdated|2), bulkResult.Assets[index], modifiedIds), cancelToken, false);
			if (cancelToken.IsCancellationRequested)
			{
				foreach (Prop prop2 in bulkResult.Assets)
				{
					this.inflightLoadRequests.Remove(prop2.ToAssetReference());
				}
			}
			await Resources.UnloadUnusedAssets();
			PropPopulateResult propPopulateResult;
			if (cancelToken.IsCancellationRequested)
			{
				propPopulateResult = null;
			}
			else
			{
				PropPopulateResult propPopulateResult2 = new PropPopulateResult(modifiedIds);
				this.PopulateReferenceFilterMap();
				propPopulateResult = propPopulateResult2;
			}
			return propPopulateResult;
		}

		// Token: 0x06001ED0 RID: 7888 RVA: 0x00086DF0 File Offset: 0x00084FF0
		private async Task LoadPropPrefab(EndlessProp propPrefab, EndlessProp missingObjectPrefab, CancellationToken cancelToken, Action<int> progressCallback, Prop prop, List<AssetIdVersionKey> modifiedIds)
		{
			AssetReference propReference = prop.ToAssetReference();
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!this.loadedPropMap.ContainsKey(propReference) && this.TryGetRuntimePropInfo(propReference.AssetID, out runtimePropInfo))
			{
				modifiedIds.Add(new AssetIdVersionKey
				{
					AssetId = propReference.AssetID,
					Version = propReference.AssetVersion
				});
				this.UnloadProp(runtimePropInfo, true);
			}
			await this.SpawnPropPrefab(propReference, cancelToken, propPrefab, missingObjectPrefab, modifiedIds, prop);
			this.inflightLoadRequests.Remove(propReference);
			if (!cancelToken.IsCancellationRequested)
			{
				if (progressCallback != null)
				{
					progressCallback(this.loadedPropMap.Values.Count((PropLibrary.RuntimePropInfo value) => !value.IsLoading));
				}
			}
		}

		// Token: 0x06001ED1 RID: 7889 RVA: 0x00086E68 File Offset: 0x00085068
		private void UnloadProp(PropLibrary.RuntimePropInfo info, bool dataUnload)
		{
			if (dataUnload)
			{
				if (info.EndlessProp)
				{
					info.EndlessProp.Cleanup();
					global::UnityEngine.Object.Destroy(info.EndlessProp.gameObject);
					this.loadedPropMap.Remove(info.PropData.ToAssetReference());
					MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, info.PropData.IconFileInstanceId);
					return;
				}
			}
			else if (!info.IsMissingObject && !info.IsLoading && info.EndlessProp)
			{
				info.EndlessProp.Cleanup();
				global::UnityEngine.Object.Destroy(info.EndlessProp.gameObject);
				info.IsLoading = true;
				info.IsMissingObject = false;
				info.EndlessProp = global::UnityEngine.Object.Instantiate<EndlessProp>(this.loadingObjectProp, this.prefabSpawnRoot);
				info.EndlessProp.gameObject.name = info.PropData.Name + " (Loading)";
			}
		}

		// Token: 0x06001ED2 RID: 7890 RVA: 0x00086F58 File Offset: 0x00085158
		private void PopulateReferenceFilterMap()
		{
			IEnumerable<ReferenceFilter> enumerable = Enum.GetValues(typeof(ReferenceFilter)).Cast<ReferenceFilter>();
			this._referenceFilterMap = new Dictionary<ReferenceFilter, List<PropLibrary.RuntimePropInfo>>();
			foreach (ReferenceFilter referenceFilter in enumerable)
			{
				if (referenceFilter != ReferenceFilter.None)
				{
					this._referenceFilterMap.Add(referenceFilter, new List<PropLibrary.RuntimePropInfo>());
				}
			}
			foreach (PropLibrary.RuntimePropInfo runtimePropInfo in this.loadedPropMap.Values)
			{
				foreach (ReferenceFilter referenceFilter2 in enumerable)
				{
					if (referenceFilter2 != ReferenceFilter.None && runtimePropInfo.EndlessProp.ReferenceFilter.HasFlag(referenceFilter2))
					{
						this._referenceFilterMap[referenceFilter2].Add(runtimePropInfo);
					}
				}
			}
		}

		// Token: 0x170005FC RID: 1532
		public PropLibrary.RuntimePropInfo this[AssetReference assetReference]
		{
			get
			{
				return this.GetRuntimePropInfo(assetReference);
			}
		}

		// Token: 0x170005FD RID: 1533
		public PropLibrary.RuntimePropInfo this[SerializableGuid assetId]
		{
			get
			{
				return this.GetRuntimePropInfo(assetId);
			}
		}

		// Token: 0x06001ED5 RID: 7893 RVA: 0x0008708A File Offset: 0x0008528A
		public AssetReference[] GetAssetReferences()
		{
			return this.loadedPropMap.Keys.ToArray<AssetReference>();
		}

		// Token: 0x06001ED6 RID: 7894 RVA: 0x0008709C File Offset: 0x0008529C
		public PropLibrary.RuntimePropInfo GetRuntimePropInfo(AssetReference assetReference)
		{
			return this.loadedPropMap[assetReference];
		}

		// Token: 0x06001ED7 RID: 7895 RVA: 0x000870AC File Offset: 0x000852AC
		public PropLibrary.RuntimePropInfo GetRuntimePropInfo(SerializableGuid assetId)
		{
			AssetReference assetReference = this.loadedPropMap.Keys.First((AssetReference reference) => reference.AssetID == assetId);
			return this.loadedPropMap[assetReference];
		}

		// Token: 0x06001ED8 RID: 7896 RVA: 0x000870EF File Offset: 0x000852EF
		public PropLibrary.RuntimePropInfo[] GetAllRuntimeProps()
		{
			return this.loadedPropMap.Values.ToArray<PropLibrary.RuntimePropInfo>();
		}

		// Token: 0x06001ED9 RID: 7897 RVA: 0x00087101 File Offset: 0x00085301
		public bool TryGetRuntimePropInfo(AssetReference assetReference, out PropLibrary.RuntimePropInfo metadata)
		{
			return this.loadedPropMap.TryGetValue(assetReference, out metadata);
		}

		// Token: 0x06001EDA RID: 7898 RVA: 0x00087110 File Offset: 0x00085310
		public bool TryGetRuntimePropInfo(SerializableGuid assetId, out PropLibrary.RuntimePropInfo metadata)
		{
			AssetReference assetReference = this.loadedPropMap.Keys.FirstOrDefault((AssetReference reference) => reference.AssetID == assetId);
			if (assetReference == null)
			{
				metadata = null;
				return false;
			}
			return this.loadedPropMap.TryGetValue(assetReference, out metadata);
		}

		// Token: 0x06001EDB RID: 7899 RVA: 0x00087162 File Offset: 0x00085362
		public void AddMissingObjectRuntimePropInfo(PropLibrary.RuntimePropInfo missingObjectInfo)
		{
			this.loadedPropMap[missingObjectInfo.PropData.ToAssetReference()] = missingObjectInfo;
		}

		// Token: 0x06001EDC RID: 7900 RVA: 0x0008717C File Offset: 0x0008537C
		public async Task PreloadData(CancellationToken cancelToken, List<SerializableGuid> previouslyMissingProps, Action<int, int> propLoadingUpdate = null)
		{
			PropLibrary.<>c__DisplayClass30_0 CS$<>8__locals1 = new PropLibrary.<>c__DisplayClass30_0();
			IReadOnlyList<AssetReference> propsToPreload = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences;
			BulkAssetCacheResult<Prop> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<Prop>(propsToPreload.ToArray<AssetReference>());
			CS$<>8__locals1.bulkResult = bulkAssetCacheResult;
			if (!cancelToken.IsCancellationRequested)
			{
				CS$<>8__locals1.iconTextures = new Texture2D[CS$<>8__locals1.bulkResult.Assets.Count];
				await TaskUtilities.ProcessTasksWithSimultaneousCap(CS$<>8__locals1.bulkResult.Assets.Count, 10, new Func<int, Task>(CS$<>8__locals1.<PreloadData>g__LoadTexture|0), cancelToken, false);
				if (cancelToken.IsCancellationRequested)
				{
					foreach (Prop prop in CS$<>8__locals1.bulkResult.Assets)
					{
						MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, prop.IconFileInstanceId);
					}
				}
				else
				{
					int i = 0;
					while (i < CS$<>8__locals1.bulkResult.Assets.Count)
					{
						Prop prop2 = CS$<>8__locals1.bulkResult.Assets[i];
						AssetReference assetReference = prop2.ToAssetReference();
						PropLibrary.RuntimePropInfo runtimePropInfo = null;
						PropLibrary.RuntimePropInfo runtimePropInfo2;
						if (!this.loadedPropMap.TryGetValue(assetReference, out runtimePropInfo2))
						{
							this.TryGetRuntimePropInfo(assetReference.AssetID, out runtimePropInfo);
							goto IL_0264;
						}
						if (runtimePropInfo2.IsMissingObject)
						{
							goto IL_0264;
						}
						if (propLoadingUpdate != null)
						{
							propLoadingUpdate(this.loadedPropMap.Count, propsToPreload.Count);
						}
						IL_039C:
						i++;
						continue;
						IL_0264:
						Texture2D texture2D = CS$<>8__locals1.iconTextures[i];
						if (cancelToken.IsCancellationRequested)
						{
							MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(MonoBehaviourSingleton<StageManager>.Instance, prop2.IconFileInstanceId);
							return;
						}
						Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), Vector2.zero);
						EndlessProp endlessProp = global::UnityEngine.Object.Instantiate<EndlessProp>(this.loadingObjectProp, this.prefabSpawnRoot);
						endlessProp.name = prop2.Name + " (Loading)";
						endlessProp.CalculateReferenceFilter(prop2);
						PropLibrary.RuntimePropInfo runtimePropInfo3 = new PropLibrary.RuntimePropInfo();
						runtimePropInfo3.PropData = prop2;
						runtimePropInfo3.Icon = sprite;
						runtimePropInfo3.EndlessProp = endlessProp;
						runtimePropInfo3.IsLoading = true;
						runtimePropInfo3.IsMissingObject = false;
						this.loadedPropMap[assetReference] = runtimePropInfo3;
						if (runtimePropInfo2 != null && runtimePropInfo2.IsMissingObject)
						{
							if (previouslyMissingProps != null)
							{
								previouslyMissingProps.Add(runtimePropInfo2.PropData.AssetID);
							}
						}
						if (runtimePropInfo != null)
						{
							this.UnloadProp(runtimePropInfo, true);
						}
						if (propLoadingUpdate == null)
						{
							goto IL_039C;
						}
						propLoadingUpdate(this.loadedPropMap.Count, propsToPreload.Count);
						goto IL_039C;
					}
					foreach (AssetReference assetReference2 in propsToPreload.Where((AssetReference propToPreload) => !CS$<>8__locals1.bulkResult.Assets.Any((Prop entry) => entry.AssetID == propToPreload.AssetID)).ToList<AssetReference>())
					{
						Prop prop3 = new Prop();
						prop3.Name = "Missing Prop";
						prop3.AssetID = assetReference2.AssetID;
						prop3.AssetVersion = assetReference2.AssetVersion;
						prop3.AssetType = "prop";
						PropLibrary.RuntimePropInfo missingObjectInfo = MonoBehaviourSingleton<StageManager>.Instance.GetMissingObjectInfo(prop3);
						this.loadedPropMap.Add(assetReference2, missingObjectInfo);
					}
				}
			}
		}

		// Token: 0x06001EDD RID: 7901 RVA: 0x000871D8 File Offset: 0x000853D8
		public List<PropLibrary.RuntimePropInfo> UnloadPropsNotInGameLibrary()
		{
			List<PropLibrary.RuntimePropInfo> list = new List<PropLibrary.RuntimePropInfo>();
			using (Dictionary<AssetReference, PropLibrary.RuntimePropInfo>.Enumerator enumerator = this.loadedPropMap.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<AssetReference, PropLibrary.RuntimePropInfo> entry = enumerator.Current;
					if (!MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.GameLibrary.PropReferences.Any((AssetReference reference) => reference.AssetID == entry.Value.PropData.AssetID) && !this.injectedPropIds.Contains(entry.Value.PropData.AssetID))
					{
						list.Add(entry.Value);
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				this.UnloadProp(list[i], true);
			}
			return list;
		}

		// Token: 0x06001EDE RID: 7902 RVA: 0x000872B4 File Offset: 0x000854B4
		public async Task FetchAndSpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List<AssetIdVersionKey> modifiedIds = null)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!this.loadedPropMap.TryGetValue(assetReference, out runtimePropInfo) || runtimePropInfo.IsLoading)
			{
				if (!this.inflightLoadRequests.Add(assetReference))
				{
					while (this.inflightLoadRequests.Contains(assetReference) && !cancelToken.IsCancellationRequested)
					{
						await Task.Yield();
					}
				}
				else
				{
					if (modifiedIds == null)
					{
						modifiedIds = new List<AssetIdVersionKey>();
					}
					AssetCacheResult<Prop> assetCacheResult = await EndlessAssetCache.GetAssetAsync<Prop>(assetReference.AssetID, assetReference.AssetVersion);
					if (!cancelToken.IsCancellationRequested)
					{
						if (assetCacheResult.HasErrors)
						{
							this.inflightLoadRequests.Remove(assetReference);
							Prop prop = new Prop();
							prop.AssetID = assetReference.AssetID;
							prop.AssetVersion = assetReference.AssetVersion;
							prop.AssetType = "prop";
							PropLibrary.RuntimePropInfo runtimePropInfo2 = new PropLibrary.RuntimePropInfo();
							runtimePropInfo2.PropData = prop;
							runtimePropInfo2.Icon = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon;
							runtimePropInfo2.EndlessProp = global::UnityEngine.Object.Instantiate<EndlessProp>(missingObjectPrefab, this.prefabSpawnRoot);
							runtimePropInfo2.IsLoading = false;
							runtimePropInfo2.IsMissingObject = true;
							runtimePropInfo2.EndlessProp.name = "Missing Object from result.HasErrors";
							this.loadedPropMap[prop.ToAssetReference()] = runtimePropInfo2;
							Debug.LogException(new Exception("Failed to fetch prop from library: " + assetReference.AssetID + ": " + assetReference.AssetVersion, assetCacheResult.GetErrorMessage()));
						}
						else
						{
							Prop asset = assetCacheResult.Asset;
							await this.SpawnPropPrefab(assetReference, cancelToken, propPrefab, missingObjectPrefab, modifiedIds, asset);
							this.inflightLoadRequests.Remove(assetReference);
						}
					}
				}
			}
		}

		// Token: 0x06001EDF RID: 7903 RVA: 0x00087324 File Offset: 0x00085524
		private async Task SpawnPropPrefab(AssetReference assetReference, CancellationToken cancelToken, EndlessProp propPrefab, EndlessProp missingObjectPrefab, List<AssetIdVersionKey> modifiedIds, Prop prop)
		{
			try
			{
				EndlessProp newProp = global::UnityEngine.Object.Instantiate<EndlessProp>(propPrefab, this.prefabSpawnRoot);
				newProp.gameObject.name = prop.Name;
				await newProp.BuildPrefab(prop, null, null, cancelToken);
				if (cancelToken.IsCancellationRequested)
				{
					newProp.Cleanup();
					this.inflightLoadRequests.Remove(assetReference);
					global::UnityEngine.Object.Destroy(newProp.gameObject);
				}
				else
				{
					PropLibrary.RuntimePropInfo currentInfo;
					if (this.loadedPropMap.TryGetValue(assetReference, out currentInfo))
					{
						if (currentInfo.EndlessProp != null)
						{
							global::UnityEngine.Object.Destroy(currentInfo.EndlessProp.gameObject);
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
							Sprite sprite = ((texture2D != null) ? Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), Vector2.zero) : null);
							currentInfo.Icon = sprite;
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
						Sprite sprite2 = ((texture2D2 != null) ? Sprite.Create(texture2D2, new Rect(0f, 0f, (float)texture2D2.width, (float)texture2D2.height), Vector2.zero) : null);
						PropLibrary.RuntimePropInfo runtimePropInfo = new PropLibrary.RuntimePropInfo();
						runtimePropInfo.PropData = prop;
						runtimePropInfo.Icon = sprite2;
						runtimePropInfo.EndlessProp = newProp;
						runtimePropInfo.IsLoading = false;
						runtimePropInfo.IsMissingObject = false;
						this.loadedPropMap[assetReference] = runtimePropInfo;
					}
					if (!modifiedIds.Any((AssetIdVersionKey modified) => modified.AssetId == prop.AssetID))
					{
						modifiedIds.Add(new AssetIdVersionKey
						{
							AssetId = prop.AssetID,
							Version = prop.AssetVersion
						});
					}
					newProp = null;
					currentInfo = null;
				}
			}
			catch (Exception ex)
			{
				PropLibrary.RuntimePropInfo runtimePropInfo2 = new PropLibrary.RuntimePropInfo();
				runtimePropInfo2.PropData = prop;
				runtimePropInfo2.Icon = MonoBehaviourSingleton<DefaultContentManager>.Instance.MissingPropDisplayIcon;
				runtimePropInfo2.EndlessProp = global::UnityEngine.Object.Instantiate<EndlessProp>(missingObjectPrefab, this.prefabSpawnRoot);
				runtimePropInfo2.IsLoading = false;
				runtimePropInfo2.IsMissingObject = true;
				runtimePropInfo2.EndlessProp.gameObject.name = prop.Name + " (Missing)";
				this.loadedPropMap[assetReference] = runtimePropInfo2;
				Debug.LogException(new Exception("Failed to load prop from library: " + assetReference.AssetID + ": " + assetReference.AssetVersion, ex));
			}
		}

		// Token: 0x06001EE0 RID: 7904 RVA: 0x0008739C File Offset: 0x0008559C
		public void UnloadAll()
		{
			AssetReference[] array = this.loadedPropMap.Keys.ToArray<AssetReference>();
			for (int i = 0; i < array.Length; i++)
			{
				this.UnloadProp(this.loadedPropMap[array[i]], true);
			}
		}

		// Token: 0x06001EE1 RID: 7905 RVA: 0x000873E0 File Offset: 0x000855E0
		public bool IsRepopulateRequired(Endless.Gameplay.LevelEditing.Level.Game newGame, Endless.Gameplay.LevelEditing.Level.Game oldGame)
		{
			bool flag = newGame.GameLibrary.PropReferences.Count != oldGame.GameLibrary.PropReferences.Count;
			if (!flag)
			{
				for (int i = 0; i < newGame.GameLibrary.PropReferences.Count; i++)
				{
					AssetReference currentReference = newGame.GameLibrary.PropReferences[i];
					AssetReference assetReference = oldGame.GameLibrary.PropReferences.FirstOrDefault((AssetReference propRef) => propRef.AssetID == currentReference.AssetID);
					if (assetReference == null || currentReference.AssetVersion != assetReference.AssetVersion)
					{
						flag = true;
						break;
					}
				}
			}
			return flag;
		}

		// Token: 0x06001EE2 RID: 7906 RVA: 0x0008748C File Offset: 0x0008568C
		public async Task<bool> Repopulate(Stage activeStage, CancellationToken cancellationToken)
		{
			foreach (PropLibrary.RuntimePropInfo runtimePropInfo in this.UnloadPropsNotInGameLibrary())
			{
				foreach (PropEntry propEntry in activeStage.LevelState.PropEntries)
				{
					if (!(propEntry.AssetId != runtimePropInfo.PropData.AssetID))
					{
						PropLibrary.RuntimePropInfo missingObjectInfo = MonoBehaviourSingleton<StageManager>.Instance.GetMissingObjectInfo(runtimePropInfo.PropData);
						this.AddMissingObjectRuntimePropInfo(missingObjectInfo);
						Debug.Log(string.Format("{0}.{1} spawning missing prop for {2}", "PropLibrary", "Repopulate", propEntry.AssetId));
						activeStage.ReplacePropWithMissingObject(propEntry.InstanceId, runtimePropInfo, missingObjectInfo);
					}
				}
			}
			List<SerializableGuid> previouslyMissingProps = new List<SerializableGuid>();
			await this.PreloadData(cancellationToken, previouslyMissingProps, null);
			bool flag;
			if (cancellationToken.IsCancellationRequested)
			{
				flag = false;
			}
			else
			{
				PropPopulateResult propPopulateResult = await this.LoadPropPrefabs(activeStage.LevelState, this.prefabSpawnRoot, this.basePropPrefab, this.missingObjectPrefab, cancellationToken, null);
				if (cancellationToken.IsCancellationRequested)
				{
					flag = false;
				}
				else
				{
					TaskAwaiter<bool> taskAwaiter = AssetFlagCache.AreAssetsAllowed(propPopulateResult.ModifiedProps).GetAwaiter();
					if (!taskAwaiter.IsCompleted)
					{
						await taskAwaiter;
						TaskAwaiter<bool> taskAwaiter2;
						taskAwaiter = taskAwaiter2;
						taskAwaiter2 = default(TaskAwaiter<bool>);
					}
					if (!taskAwaiter.GetResult())
					{
						ErrorHandler.HandleError(ErrorCodes.RepopulatingGame_PropLibrary_ContentRestricted, new Exception("Unable to repopulate game library. Reason: Content restricted."), true, true);
					}
					bool flag2 = false;
					using (List<AssetIdVersionKey>.Enumerator enumerator3 = propPopulateResult.ModifiedProps.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							AssetIdVersionKey asset = enumerator3.Current;
							activeStage.RespawnPropsWithAssetId(asset.AssetId, previouslyMissingProps.Contains(asset.AssetId));
							IEnumerable<PropEntry> propEntries = activeStage.LevelState.PropEntries;
							Func<PropEntry, bool> func;
							Func<PropEntry, bool> <>9__0;
							if ((func = <>9__0) == null)
							{
								Func<PropEntry, bool> func2 = (PropEntry entry) => entry.AssetId == asset.AssetId;
								<>9__0 = func2;
								func = func2;
							}
							foreach (PropEntry propEntry2 in propEntries.Where(func))
							{
								flag2 |= this.SanitizeMemberChanges(propEntry2);
								flag2 |= this.SanitizeWireBundles(activeStage.LevelState, propEntry2);
							}
						}
					}
					activeStage.ApplyMemberChanges();
					activeStage.LoadExistingWires(false);
					flag = flag2;
				}
			}
			return flag;
		}

		// Token: 0x06001EE3 RID: 7907 RVA: 0x000874E0 File Offset: 0x000856E0
		private bool SanitizeMemberChanges(PropEntry propEntry)
		{
			bool flag = false;
			foreach (ComponentEntry componentEntry in propEntry.ComponentEntries)
			{
				for (int i = componentEntry.Changes.Count - 1; i >= 0; i--)
				{
					MemberChange memberChange2 = componentEntry.Changes[i];
					if (Type.GetType(componentEntry.AssemblyQualifiedName).GetMember(memberChange2.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length == 0)
					{
						flag = true;
						componentEntry.Changes.RemoveAt(i);
					}
				}
			}
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(propEntry.AssetId, out runtimePropInfo) || runtimePropInfo.IsMissingObject)
			{
				return false;
			}
			EndlessScriptComponent scriptComponent = runtimePropInfo.EndlessProp.ScriptComponent;
			if (!scriptComponent)
			{
				return false;
			}
			for (int j = propEntry.LuaMemberChanges.Count - 1; j >= 0; j--)
			{
				MemberChange memberChange = propEntry.LuaMemberChanges[j];
				if (scriptComponent.Script.InspectorValues.FirstOrDefault((InspectorScriptValue inspectorValue) => inspectorValue.Name == memberChange.MemberName && memberChange.DataType == inspectorValue.DataType) == null)
				{
					propEntry.LuaMemberChanges.RemoveAt(j);
					flag = true;
				}
			}
			if (flag)
			{
				Debug.Log(string.Format("Member Change Sanitization was required for prop: {0}: {1}.", this.GetRuntimePropInfo(propEntry.AssetId).PropData.Name, propEntry.InstanceId));
			}
			return flag;
		}

		// Token: 0x06001EE4 RID: 7908 RVA: 0x0008765C File Offset: 0x0008585C
		private bool SanitizeWireBundles(LevelState levelState, PropEntry propEntry)
		{
			bool flag = false;
			for (int i = levelState.WireBundles.Count - 1; i >= 0; i--)
			{
				WireBundle wireBundle = levelState.WireBundles[i];
				PropEntry propEntry2 = levelState.GetPropEntry(wireBundle.EmitterInstanceId);
				PropEntry propEntry3 = levelState.GetPropEntry(wireBundle.ReceiverInstanceId);
				if (!(propEntry2.InstanceId != propEntry.InstanceId) || !(propEntry3.InstanceId != propEntry.InstanceId))
				{
					SerializableGuid assetId = propEntry2.AssetId;
					SerializableGuid assetId2 = propEntry3.AssetId;
					PropLibrary.RuntimePropInfo runtimePropInfo;
					PropLibrary.RuntimePropInfo runtimePropInfo2;
					if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId, out runtimePropInfo) || runtimePropInfo.IsMissingObject)
					{
						levelState.RemoveBundle(levelState.WireBundles[i]);
					}
					else if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetId2, out runtimePropInfo2) || runtimePropInfo2.IsMissingObject)
					{
						levelState.RemoveBundle(levelState.WireBundles[i]);
					}
					else
					{
						int j = wireBundle.Wires.Count - 1;
						while (j >= 0)
						{
							WireEntry wireEntry = wireBundle.Wires[j];
							bool flag2 = string.IsNullOrEmpty(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
							bool flag3 = string.IsNullOrEmpty(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
							Type type;
							if (flag2)
							{
								type = typeof(EndlessScriptComponent);
							}
							else
							{
								type = Type.GetType(wireEntry.EmitterComponentAssemblyQualifiedTypeName);
							}
							Type type2;
							if (flag3)
							{
								type2 = typeof(EndlessScriptComponent);
							}
							else
							{
								type2 = Type.GetType(wireEntry.ReceiverComponentAssemblyQualifiedTypeName);
							}
							ComponentDefinition componentDefinition;
							if (flag2)
							{
								EndlessScriptComponent scriptComponent = runtimePropInfo.EndlessProp.ScriptComponent;
								if (!scriptComponent)
								{
									Debug.LogWarning("Emitter Component isn't a script component");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
								}
								else
								{
									EndlessEventInfo eventInfo = scriptComponent.GetEventInfo(wireEntry.EmitterMemberName);
									if (eventInfo != null)
									{
										int[] array = eventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray<int>();
										goto IL_02C6;
									}
									Debug.LogWarning("luaEvent " + wireEntry.EmitterMemberName + " no longer exists in the script");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
								}
							}
							else if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type, out componentDefinition))
							{
								Debug.LogWarning("The entire component definition could not be found " + type.Name + " " + wireEntry.EmitterComponentAssemblyQualifiedTypeName);
								flag = true;
								wireBundle.Wires.RemoveAt(j);
							}
							else
							{
								int[] array;
								if (MonoBehaviourSingleton<StageManager>.Instance.TryGetDataTypeSignature(componentDefinition.AvailableEvents, wireEntry.EmitterMemberName, out array))
								{
									goto IL_02C6;
								}
								Debug.LogWarning("The signature of the emitter couldn't be found");
								flag = true;
								wireBundle.Wires.RemoveAt(j);
							}
							IL_04B4:
							j--;
							continue;
							IL_02C6:
							int[] array2;
							if (flag3)
							{
								EndlessScriptComponent scriptComponent2 = runtimePropInfo2.EndlessProp.ScriptComponent;
								if (!scriptComponent2)
								{
									Debug.LogWarning("Receiver Component isn't a script component");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
									goto IL_04B4;
								}
								EndlessEventInfo endlessEventInfo = scriptComponent2.Script.Receivers.FirstOrDefault((EndlessEventInfo r) => r.MemberName == wireEntry.ReceiverMemberName);
								if (endlessEventInfo == null)
								{
									Debug.LogWarning("luaEvent no longer exists in the script");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
									goto IL_04B4;
								}
								array2 = endlessEventInfo.ParamList.Select((EndlessParameterInfo param) => param.DataType).ToArray<int>();
							}
							else
							{
								ComponentDefinition componentDefinition2;
								if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetComponentDefinition(type2, out componentDefinition2))
								{
									Debug.LogWarning("The entire component definition could not be found");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
									goto IL_04B4;
								}
								if (!MonoBehaviourSingleton<StageManager>.Instance.TryGetDataTypeSignature(componentDefinition2.AvailableReceivers, wireEntry.ReceiverMemberName, out array2))
								{
									Debug.LogWarning("The signature of the receiver couldn't be found");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
									goto IL_04B4;
								}
							}
							if (array2.Length == 0)
							{
								if (wireEntry.StaticParameters.Length != 0)
								{
									Debug.LogWarning("Receiver receives no parameters but we had leftover static parameters");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
									goto IL_04B4;
								}
								goto IL_04B4;
							}
							else if (wireEntry.StaticParameters.Length != 0)
							{
								int[] array3 = wireEntry.StaticParameters.Select((StoredParameter staticParam) => staticParam.DataType).ToArray<int>();
								if (!MonoBehaviourSingleton<StageManager>.Instance.SignaturesMatch(array2, array3))
								{
									Debug.LogWarning("Receiver did not match the static parameter data type signature");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
									goto IL_04B4;
								}
								goto IL_04B4;
							}
							else
							{
								int[] array;
								if (!MonoBehaviourSingleton<StageManager>.Instance.SignaturesMatch(array2, array))
								{
									Debug.LogWarning("Receiver did not match the emitter's parameter data type signature");
									flag = true;
									wireBundle.Wires.RemoveAt(j);
									goto IL_04B4;
								}
								goto IL_04B4;
							}
						}
						if (wireBundle.Wires.Count == 0)
						{
							flag = true;
							levelState.RemoveBundle(levelState.WireBundles[i]);
						}
					}
				}
			}
			if (flag)
			{
				Debug.Log(string.Format("Wire Sanitization was required for prop: {0}: {1}.", this.GetRuntimePropInfo(propEntry.AssetId).PropData.Name, propEntry.InstanceId));
			}
			return flag;
		}

		// Token: 0x06001EE5 RID: 7909 RVA: 0x00087B8B File Offset: 0x00085D8B
		public bool IsInjectedProp(string propDataAssetID)
		{
			return this.injectedPropIds.Contains(propDataAssetID);
		}

		// Token: 0x06001EE6 RID: 7910 RVA: 0x00087B9E File Offset: 0x00085D9E
		public List<SerializableGuid> GetBaseTypeList(string propDataBaseTypeId)
		{
			return this.GetBaseTypeList(new SerializableGuid[] { propDataBaseTypeId });
		}

		// Token: 0x06001EE7 RID: 7911 RVA: 0x00087BBC File Offset: 0x00085DBC
		public List<SerializableGuid> GetBaseTypeList(SerializableGuid[] propDataBaseTypeId)
		{
			return (from info in this.loadedPropMap.Values
				where propDataBaseTypeId.Contains(info.PropData.BaseTypeId)
				select info.PropData.AssetID).ToList<SerializableGuid>();
		}

		// Token: 0x040017F6 RID: 6134
		private readonly ReferenceFilter[] dynamicFilters = new ReferenceFilter[]
		{
			ReferenceFilter.Npc,
			ReferenceFilter.InventoryItem
		};

		// Token: 0x040017F7 RID: 6135
		private Dictionary<AssetReference, PropLibrary.RuntimePropInfo> loadedPropMap = new Dictionary<AssetReference, PropLibrary.RuntimePropInfo>();

		// Token: 0x040017F8 RID: 6136
		private List<SerializableGuid> injectedPropIds = new List<SerializableGuid>();

		// Token: 0x040017F9 RID: 6137
		private HashSet<AssetReference> inflightLoadRequests = new HashSet<AssetReference>();

		// Token: 0x040017FA RID: 6138
		private Dictionary<ReferenceFilter, List<PropLibrary.RuntimePropInfo>> _referenceFilterMap;

		// Token: 0x040017FB RID: 6139
		private readonly EndlessProp loadingObjectProp;

		// Token: 0x040017FC RID: 6140
		private readonly Transform prefabSpawnRoot;

		// Token: 0x040017FD RID: 6141
		private readonly EndlessProp basePropPrefab;

		// Token: 0x040017FE RID: 6142
		private readonly EndlessProp missingObjectPrefab;

		// Token: 0x020004EB RID: 1259
		public class RuntimePropInfo
		{
			// Token: 0x170005FE RID: 1534
			// (get) Token: 0x06001EE8 RID: 7912 RVA: 0x00087C1B File Offset: 0x00085E1B
			// (set) Token: 0x06001EE9 RID: 7913 RVA: 0x00087C23 File Offset: 0x00085E23
			public bool IsLoading { get; set; } = true;

			// Token: 0x170005FF RID: 1535
			// (get) Token: 0x06001EEA RID: 7914 RVA: 0x00087C2C File Offset: 0x00085E2C
			// (set) Token: 0x06001EEB RID: 7915 RVA: 0x00087C34 File Offset: 0x00085E34
			public bool IsMissingObject { get; set; }

			// Token: 0x06001EEC RID: 7916 RVA: 0x00087C40 File Offset: 0x00085E40
			public List<ComponentDefinition> GetAllDefinitions()
			{
				List<ComponentDefinition> list = new List<ComponentDefinition>();
				if (this.PropData.BaseTypeId != SerializableGuid.Empty)
				{
					list.Add(this.GetBaseTypeDefinition());
				}
				list.AddRange(this.GetComponentDefinitions());
				return list;
			}

			// Token: 0x06001EED RID: 7917 RVA: 0x00087C88 File Offset: 0x00085E88
			public ComponentDefinition GetBaseTypeDefinition()
			{
				BaseTypeDefinition baseTypeDefinition;
				if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(this.PropData.BaseTypeId, out baseTypeDefinition))
				{
					return baseTypeDefinition;
				}
				return null;
			}

			// Token: 0x06001EEE RID: 7918 RVA: 0x00087CBC File Offset: 0x00085EBC
			public List<ComponentDefinition> GetComponentDefinitions()
			{
				List<ComponentDefinition> list = new List<ComponentDefinition>();
				foreach (string text in this.PropData.ComponentIds)
				{
					ComponentDefinition componentDefinition;
					if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(text, out componentDefinition))
					{
						list.Add(componentDefinition);
					}
				}
				return list;
			}

			// Token: 0x040017FF RID: 6143
			public Prop PropData;

			// Token: 0x04001800 RID: 6144
			public Sprite Icon;

			// Token: 0x04001801 RID: 6145
			public EndlessProp EndlessProp;
		}
	}
}
