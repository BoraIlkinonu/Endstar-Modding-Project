using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing.Level;
using Endless.ParticleSystems.Assets;
using Endless.ParticleSystems.Components;
using Endless.Props;
using Endless.Props.Assets;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.UnityExtensions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004A1 RID: 1185
	public class EndlessProp : EndlessBehaviour, IScriptAwakeSubscriber
	{
		// Token: 0x170005A7 RID: 1447
		// (get) Token: 0x06001D2E RID: 7470 RVA: 0x0007F3DA File Offset: 0x0007D5DA
		public EndlessScriptComponent ScriptComponent
		{
			get
			{
				return this.scriptComponent;
			}
		}

		// Token: 0x170005A8 RID: 1448
		// (get) Token: 0x06001D2F RID: 7471 RVA: 0x0007F3E2 File Offset: 0x0007D5E2
		// (set) Token: 0x06001D30 RID: 7472 RVA: 0x0007F3EA File Offset: 0x0007D5EA
		public ReferenceFilter ReferenceFilter { get; private set; }

		// Token: 0x170005A9 RID: 1449
		// (get) Token: 0x06001D31 RID: 7473 RVA: 0x0007F3F3 File Offset: 0x0007D5F3
		// (set) Token: 0x06001D32 RID: 7474 RVA: 0x0007F3FB File Offset: 0x0007D5FB
		public NavType NavValue { get; private set; }

		// Token: 0x170005AA RID: 1450
		// (get) Token: 0x06001D33 RID: 7475 RVA: 0x0007F404 File Offset: 0x0007D604
		// (set) Token: 0x06001D34 RID: 7476 RVA: 0x0007F40C File Offset: 0x0007D60C
		public Prop Prop { get; private set; }

		// Token: 0x170005AB RID: 1451
		// (get) Token: 0x06001D35 RID: 7477 RVA: 0x0007F415 File Offset: 0x0007D615
		public bool IsNetworked
		{
			get
			{
				return this.isNetworked;
			}
		}

		// Token: 0x170005AC RID: 1452
		// (get) Token: 0x06001D36 RID: 7478 RVA: 0x0007F41D File Offset: 0x0007D61D
		public IScriptInjector[] ScriptInjectors
		{
			get
			{
				if (this.scriptInjectors == null)
				{
					this.scriptInjectors = base.GetComponentsInChildren<IScriptInjector>();
				}
				return this.scriptInjectors;
			}
		}

		// Token: 0x170005AD RID: 1453
		// (get) Token: 0x06001D37 RID: 7479 RVA: 0x0007F439 File Offset: 0x0007D639
		public WorldObject WorldObject
		{
			get
			{
				return this.worldObject;
			}
		}

		// Token: 0x170005AE RID: 1454
		// (get) Token: 0x06001D38 RID: 7480 RVA: 0x0007F444 File Offset: 0x0007D644
		public Dictionary<SerializableGuid, Transform> TransformMap
		{
			get
			{
				if (this.transformMap == null)
				{
					this.transformMap = new Dictionary<SerializableGuid, Transform>();
					foreach (TransformIdentifier transformIdentifier in base.GetComponentsInChildren<TransformIdentifier>())
					{
						if (!this.transformMap.TryAdd(transformIdentifier.UniqueId, transformIdentifier.transform))
						{
							throw new DuplicateNameException("The prop " + this.WorldObject.gameObject.name + " has a duplicated Transform ID. This object needs to be fixed via the SDK.");
						}
					}
				}
				return this.transformMap;
			}
		}

		// Token: 0x06001D39 RID: 7481 RVA: 0x0007F4C4 File Offset: 0x0007D6C4
		public async Task BuildPrefab(Prop prop, GameObject testPrefab = null, Script testScript = null, CancellationToken cancelToken = default(CancellationToken))
		{
			this.Prop = prop;
			IBaseType baseType = null;
			NetworkObject networkObject = null;
			GameObject prefabInstance = null;
			Component baseTypeComponent = null;
			List<IComponentBase> components = new List<IComponentBase>();
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(prop.BaseTypeId, out baseTypeDefinition))
			{
				this.isNetworked = this.isNetworked || baseTypeDefinition.IsNetworked;
				baseType = this.SetupBaseType(base.transform, baseTypeDefinition);
				baseTypeComponent = baseType as Component;
			}
			else
			{
				Debug.LogException(new Exception("Base type " + prop.BaseTypeId + " not found!"));
			}
			if (testPrefab == null)
			{
				if (prop.PrefabAsset != null && !string.IsNullOrEmpty(prop.PrefabAsset.AssetID))
				{
					AssetCacheResult<EndlessPrefabAsset> assetCacheResult = await EndlessAssetCache.GetAssetAsync<EndlessPrefabAsset>(prop.PrefabAsset.AssetID, prop.PrefabAsset.AssetVersion);
					if (cancelToken.IsCancellationRequested)
					{
						return;
					}
					if (assetCacheResult.HasErrors)
					{
						throw assetCacheResult.GetErrorMessage();
					}
					EndlessPrefabAsset endlessPrefabAsset = assetCacheResult.Asset;
					if (endlessPrefabAsset.BundleFileAsset != null)
					{
						AssetBundle loadedBundle = await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAssetBundleAsync(this, endlessPrefabAsset.BundleFileAsset.AssetFileInstanceId);
						if (cancelToken.IsCancellationRequested)
						{
							return;
						}
						GameObject gameObject = await loadedBundle.LoadAssetAsyncAwaitable(endlessPrefabAsset.PrefabFileName);
						HashSet<Shader> hashSet = EndlessProp.UpdateMaterials(loadedBundle);
						if (cancelToken.IsCancellationRequested)
						{
							return;
						}
						MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, endlessPrefabAsset.BundleFileAsset.AssetFileInstanceId);
						prefabInstance = global::UnityEngine.Object.Instantiate<GameObject>(gameObject, baseTypeComponent.transform);
						foreach (Shader shader in hashSet)
						{
							Resources.UnloadAsset(shader);
						}
						loadedBundle = null;
					}
					endlessPrefabAsset = null;
				}
			}
			else
			{
				prefabInstance = global::UnityEngine.Object.Instantiate<GameObject>(testPrefab, baseTypeComponent.transform);
			}
			if (prefabInstance)
			{
				await this.PopulateParticleSystems(prop, prefabInstance);
				if (cancelToken.IsCancellationRequested)
				{
					return;
				}
			}
			Component component = null;
			if (baseType.ComponentReferenceType != null)
			{
				component = prefabInstance.GetComponent(baseType.ComponentReferenceType);
			}
			baseType.ComponentInitialize(component as ReferenceBase, this);
			foreach (string text in prop.ComponentIds)
			{
				SerializableGuid serializableGuid = text;
				ComponentDefinition componentDefinition;
				if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(serializableGuid, out componentDefinition))
				{
					this.isNetworked = this.isNetworked || componentDefinition.IsNetworked;
					components.Add(this.SetupComponent(prefabInstance, baseTypeComponent.transform, componentDefinition, prop));
				}
				else
				{
					Debug.LogException(new Exception(string.Format("Component type {0} not found!", serializableGuid)));
				}
			}
			if (this.isNetworked)
			{
				networkObject = base.gameObject.AddComponent<NetworkObject>();
				base.gameObject.AddComponent<EndlessNetworkObject>();
				networkObject.AutoObjectParentSync = false;
				uint num = BitConverter.ToUInt32(prop.AssetID.Guid.ToByteArray());
				typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(networkObject, num);
			}
			if (testScript == null)
			{
				if (prop.HasScript)
				{
					AssetCacheResult<Script> assetCacheResult2 = await EndlessAssetCache.GetAssetAsync<Script>(prop.ScriptAsset.AssetID, prop.ScriptAsset.AssetVersion);
					if (cancelToken.IsCancellationRequested)
					{
						return;
					}
					if (assetCacheResult2.HasErrors)
					{
						throw assetCacheResult2.GetErrorMessage();
					}
					Script asset = assetCacheResult2.Asset;
					this.scriptComponent.Setup(asset, prop, baseTypeComponent);
				}
			}
			else
			{
				this.scriptComponent.Setup(testScript, prop, baseTypeComponent);
			}
			this.worldObject.Initialize(baseTypeComponent, components, networkObject);
			baseType.PrefabInitialize(this.worldObject);
			foreach (IComponentBase componentBase in components)
			{
				componentBase.PrefabInitialize(this.worldObject);
			}
			if (prefabInstance != null)
			{
				this.endlessVisuals.FindAndManageChildRenderers(prefabInstance);
			}
			if (this.isNetworked)
			{
				NetworkManager.Singleton.AddNetworkPrefab(base.gameObject);
				if (!base.IsServer)
				{
					NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.ClientBuiltNetworkProp_ServerRpc((ulong)networkObject.PrefabIdHash, MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying, default(ServerRpcParams));
				}
			}
		}

		// Token: 0x06001D3A RID: 7482 RVA: 0x0007F528 File Offset: 0x0007D728
		public async Task PopulateParticleSystems(Prop prop, GameObject particleObject)
		{
			SwappableParticleSystem[] componentsInChildren = particleObject.GetComponentsInChildren<SwappableParticleSystem>();
			List<SwappableParticleSystem> particleSystemsToPopulate = new List<SwappableParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].IsEmbedded())
				{
					componentsInChildren[i].InitializeWithEmbedded();
				}
				else if (!(componentsInChildren[i].ReferencedAsset.AssetID == SerializableGuid.Empty))
				{
					particleSystemsToPopulate.Add(componentsInChildren[i]);
				}
			}
			if (particleSystemsToPopulate.Count > 0)
			{
				for (int iterator = 0; iterator < particleSystemsToPopulate.Count; iterator++)
				{
					AssetReference particleAssetFromReferenceList = this.GetParticleAssetFromReferenceList(particleSystemsToPopulate[iterator].ReferencedAsset.AssetID, prop.VisualAssets);
					if (particleAssetFromReferenceList == null)
					{
						Debug.LogException(new Exception(string.Concat(new string[]
						{
							"Unable to find Asset Id: ",
							particleSystemsToPopulate[iterator].ReferencedAsset.AssetID,
							" in reference list for prop: ",
							prop.AssetID,
							", Version: ",
							prop.AssetVersion
						})));
					}
					else
					{
						AssetCacheResult<ParticleSystemAsset> assetCacheResult = await EndlessAssetCache.GetAssetAsync<ParticleSystemAsset>(particleAssetFromReferenceList.AssetID, particleAssetFromReferenceList.AssetVersion);
						if (assetCacheResult.HasErrors)
						{
							Debug.LogException(assetCacheResult.GetErrorMessage());
						}
						else
						{
							try
							{
								ParticleSystemAsset particleSystemAsset = assetCacheResult.Asset;
								GameObject gameObject = await (await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAssetBundleAsync(this, particleSystemAsset.BundleFileAsset.AssetFileInstanceId)).LoadAssetAsyncAwaitable(particleSystemAsset.PrefabFileName);
								if (particleSystemsToPopulate[iterator].AutoSpawn)
								{
									Transform transform = particleSystemsToPopulate[iterator].transform;
									GameObject gameObject2 = global::UnityEngine.Object.Instantiate<GameObject>(gameObject, transform.position, transform.rotation, transform);
									particleSystemsToPopulate[iterator].InitializeWithValue(gameObject2.GetComponent<ParticleSystem>());
								}
								else
								{
									particleSystemsToPopulate[iterator].InitializeWithValue(gameObject.GetComponent<ParticleSystem>());
								}
								MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, particleSystemAsset.BundleFileAsset.AssetFileInstanceId);
								particleSystemAsset = null;
							}
							catch (Exception ex)
							{
								Debug.LogException(ex);
							}
						}
					}
				}
			}
		}

		// Token: 0x06001D3B RID: 7483 RVA: 0x0007F57C File Offset: 0x0007D77C
		private AssetReference GetParticleAssetFromReferenceList(string assetId, List<AssetReference> referenceList)
		{
			return referenceList.FirstOrDefault((AssetReference entry) => entry.AssetID == assetId);
		}

		// Token: 0x06001D3C RID: 7484 RVA: 0x0007F5A8 File Offset: 0x0007D7A8
		private static HashSet<Shader> UpdateMaterials(AssetBundle loadedBundle)
		{
			HashSet<Shader> hashSet = new HashSet<Shader>();
			List<Shader> list = new List<Shader>();
			Material[] array = loadedBundle.LoadAllAssets<Material>();
			string text = string.Empty;
			foreach (Material material in array)
			{
				text = ((material.shader.name == "Shader Graphs/Endless_Shader") ? "Shader Graphs/Endless_Shader_No_Fade" : material.shader.name);
				Shader shader = material.shader;
				EndlessVisuals.SwapShader(material, Shader.Find(text), true);
				if (!list.Contains(shader))
				{
					hashSet.Add(shader);
				}
			}
			return hashSet;
		}

		// Token: 0x06001D3D RID: 7485 RVA: 0x0007F640 File Offset: 0x0007D840
		private IBaseType SetupBaseType(Transform parentTransform, ComponentDefinition componentDefinition)
		{
			IBaseType component = global::UnityEngine.Object.Instantiate<GameObject>(componentDefinition.Prefab, parentTransform).GetComponent<IBaseType>();
			this.ReferenceFilter |= component.Filter;
			this.NavValue = (NavType)Mathf.Max((int)this.NavValue, (int)component.NavValue);
			return component;
		}

		// Token: 0x06001D3E RID: 7486 RVA: 0x0007F68C File Offset: 0x0007D88C
		private IComponentBase SetupComponent(GameObject prefabInstance, Transform parentTransform, ComponentDefinition componentDefinition, Prop prop)
		{
			IComponentBase component = global::UnityEngine.Object.Instantiate<GameObject>(componentDefinition.Prefab, parentTransform).GetComponent<IComponentBase>();
			this.ReferenceFilter |= component.Filter;
			this.NavValue = (NavType)Mathf.Max((int)this.NavValue, (int)component.NavValue);
			Component component2 = null;
			if (component.ComponentReferenceType != null)
			{
				component2 = prefabInstance.GetComponent(component.ComponentReferenceType);
			}
			component.ComponentInitialize(component2 as ReferenceBase, this);
			return component;
		}

		// Token: 0x06001D3F RID: 7487 RVA: 0x0007F700 File Offset: 0x0007D900
		public void EndlessScriptAwake()
		{
			if (base.IsServer)
			{
				this.scriptComponent.Initialize();
				foreach (IScriptInjector scriptInjector in this.ScriptInjectors)
				{
					if (scriptInjector.LuaObject != null)
					{
						this.scriptComponent.RegisterObject(scriptInjector.LuaObjectName, scriptInjector.LuaObject);
					}
					scriptInjector.ScriptInitialize(this.scriptComponent);
				}
				this.scriptComponent.RunScript();
			}
		}

		// Token: 0x06001D40 RID: 7488 RVA: 0x0007F76F File Offset: 0x0007D96F
		public void Cleanup()
		{
			if (this.isNetworked)
			{
				NetworkManager.Singleton.RemoveNetworkPrefab(base.gameObject);
			}
		}

		// Token: 0x06001D41 RID: 7489 RVA: 0x0007F789 File Offset: 0x0007D989
		public void HandleInspectionStateChanged(bool isInspected)
		{
			this.OnInspectionStateChanged.Invoke(isInspected);
		}

		// Token: 0x06001D42 RID: 7490 RVA: 0x0007F798 File Offset: 0x0007D998
		public void CalculateReferenceFilter(Prop prop)
		{
			ReferenceFilter referenceFilter = ReferenceFilter.None;
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(prop.BaseTypeId, out baseTypeDefinition))
			{
				referenceFilter |= baseTypeDefinition.Filter;
			}
			foreach (string text in prop.ComponentIds)
			{
				SerializableGuid serializableGuid = text;
				ComponentDefinition componentDefinition;
				if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(serializableGuid, out componentDefinition))
				{
					referenceFilter |= componentDefinition.Filter;
				}
			}
			this.ReferenceFilter = referenceFilter;
		}

		// Token: 0x040016D9 RID: 5849
		[SerializeField]
		private EndlessScriptComponent scriptComponent;

		// Token: 0x040016DA RID: 5850
		[SerializeField]
		private WorldObject worldObject;

		// Token: 0x040016DB RID: 5851
		[SerializeField]
		private EndlessVisuals endlessVisuals;

		// Token: 0x040016DC RID: 5852
		public UnityEvent<bool> OnInspectionStateChanged = new UnityEvent<bool>();

		// Token: 0x040016DD RID: 5853
		private bool isNetworked;

		// Token: 0x040016E1 RID: 5857
		private IScriptInjector[] scriptInjectors;

		// Token: 0x040016E2 RID: 5858
		private Dictionary<SerializableGuid, Transform> transformMap;
	}
}
