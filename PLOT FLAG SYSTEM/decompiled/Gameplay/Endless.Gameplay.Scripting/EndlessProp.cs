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

namespace Endless.Gameplay.Scripting;

public class EndlessProp : EndlessBehaviour, IScriptAwakeSubscriber
{
	[SerializeField]
	private EndlessScriptComponent scriptComponent;

	[SerializeField]
	private WorldObject worldObject;

	[SerializeField]
	private EndlessVisuals endlessVisuals;

	public UnityEvent<bool> OnInspectionStateChanged = new UnityEvent<bool>();

	private bool isNetworked;

	private IScriptInjector[] scriptInjectors;

	private Dictionary<SerializableGuid, Transform> transformMap;

	public EndlessScriptComponent ScriptComponent => scriptComponent;

	[field: SerializeField]
	public ReferenceFilter ReferenceFilter { get; private set; }

	[field: SerializeField]
	public NavType NavValue { get; private set; }

	[field: SerializeField]
	[field: HideInInspector]
	public Prop Prop { get; private set; }

	public bool IsNetworked => isNetworked;

	public IScriptInjector[] ScriptInjectors
	{
		get
		{
			if (scriptInjectors == null)
			{
				scriptInjectors = GetComponentsInChildren<IScriptInjector>();
			}
			return scriptInjectors;
		}
	}

	public WorldObject WorldObject => worldObject;

	public Dictionary<SerializableGuid, Transform> TransformMap
	{
		get
		{
			if (transformMap == null)
			{
				transformMap = new Dictionary<SerializableGuid, Transform>();
				TransformIdentifier[] componentsInChildren = GetComponentsInChildren<TransformIdentifier>();
				foreach (TransformIdentifier transformIdentifier in componentsInChildren)
				{
					if (!transformMap.TryAdd(transformIdentifier.UniqueId, transformIdentifier.transform))
					{
						throw new DuplicateNameException("The prop " + WorldObject.gameObject.name + " has a duplicated Transform ID. This object needs to be fixed via the SDK.");
					}
				}
			}
			return transformMap;
		}
	}

	public async Task BuildPrefab(Prop prop, GameObject testPrefab = null, Script testScript = null, CancellationToken cancelToken = default(CancellationToken))
	{
		Prop = prop;
		IBaseType baseType = null;
		NetworkObject networkObject = null;
		GameObject prefabInstance = null;
		Component baseTypeComponent = null;
		List<IComponentBase> components = new List<IComponentBase>();
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(prop.BaseTypeId, out var componentDefinition))
		{
			isNetworked = isNetworked || componentDefinition.IsNetworked;
			baseType = SetupBaseType(base.transform, componentDefinition);
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
					GameObject original = await loadedBundle.LoadAssetAsyncAwaitable<GameObject>(endlessPrefabAsset.PrefabFileName);
					HashSet<Shader> hashSet = UpdateMaterials(loadedBundle);
					if (cancelToken.IsCancellationRequested)
					{
						return;
					}
					MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, endlessPrefabAsset.BundleFileAsset.AssetFileInstanceId);
					prefabInstance = UnityEngine.Object.Instantiate(original, baseTypeComponent.transform);
					foreach (Shader item in hashSet)
					{
						Resources.UnloadAsset(item);
					}
				}
			}
		}
		else
		{
			prefabInstance = UnityEngine.Object.Instantiate(testPrefab, baseTypeComponent.transform);
		}
		if ((bool)prefabInstance)
		{
			await PopulateParticleSystems(prop, prefabInstance);
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
		foreach (string componentId in prop.ComponentIds)
		{
			SerializableGuid serializableGuid = componentId;
			if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(serializableGuid, out var componentDefinition2))
			{
				isNetworked = isNetworked || componentDefinition2.IsNetworked;
				components.Add(SetupComponent(prefabInstance, baseTypeComponent.transform, componentDefinition2, prop));
			}
			else
			{
				Debug.LogException(new Exception($"Component type {serializableGuid} not found!"));
			}
		}
		if (isNetworked)
		{
			networkObject = base.gameObject.AddComponent<NetworkObject>();
			base.gameObject.AddComponent<EndlessNetworkObject>();
			networkObject.AutoObjectParentSync = false;
			uint num = BitConverter.ToUInt32(((SerializableGuid)prop.AssetID).Guid.ToByteArray());
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
				scriptComponent.Setup(asset, prop, baseTypeComponent);
			}
		}
		else
		{
			scriptComponent.Setup(testScript, prop, baseTypeComponent);
		}
		worldObject.Initialize(baseTypeComponent, components, networkObject);
		baseType.PrefabInitialize(worldObject);
		foreach (IComponentBase item2 in components)
		{
			item2.PrefabInitialize(worldObject);
		}
		if (prefabInstance != null)
		{
			endlessVisuals.FindAndManageChildRenderers(prefabInstance);
		}
		if (isNetworked)
		{
			NetworkManager.Singleton.AddNetworkPrefab(base.gameObject);
			if (!base.IsServer)
			{
				NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.ClientBuiltNetworkProp_ServerRpc(networkObject.PrefabIdHash, MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying);
			}
		}
	}

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
			else if (!((SerializableGuid)componentsInChildren[i].ReferencedAsset.AssetID == SerializableGuid.Empty))
			{
				particleSystemsToPopulate.Add(componentsInChildren[i]);
			}
		}
		if (particleSystemsToPopulate.Count <= 0)
		{
			return;
		}
		for (int iterator = 0; iterator < particleSystemsToPopulate.Count; iterator++)
		{
			AssetReference particleAssetFromReferenceList = GetParticleAssetFromReferenceList(particleSystemsToPopulate[iterator].ReferencedAsset.AssetID, prop.VisualAssets);
			if ((object)particleAssetFromReferenceList == null)
			{
				Debug.LogException(new Exception("Unable to find Asset Id: " + particleSystemsToPopulate[iterator].ReferencedAsset.AssetID + " in reference list for prop: " + prop.AssetID + ", Version: " + prop.AssetVersion));
				continue;
			}
			AssetCacheResult<ParticleSystemAsset> assetCacheResult = await EndlessAssetCache.GetAssetAsync<ParticleSystemAsset>(particleAssetFromReferenceList.AssetID, particleAssetFromReferenceList.AssetVersion);
			if (assetCacheResult.HasErrors)
			{
				Debug.LogException(assetCacheResult.GetErrorMessage());
				continue;
			}
			try
			{
				ParticleSystemAsset particleSystemAsset = assetCacheResult.Asset;
				GameObject gameObject = await (await MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAssetBundleAsync(this, particleSystemAsset.BundleFileAsset.AssetFileInstanceId)).LoadAssetAsyncAwaitable<GameObject>(particleSystemAsset.PrefabFileName);
				if (particleSystemsToPopulate[iterator].AutoSpawn)
				{
					Transform transform = particleSystemsToPopulate[iterator].transform;
					GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, transform.position, transform.rotation, transform);
					particleSystemsToPopulate[iterator].InitializeWithValue(gameObject2.GetComponent<ParticleSystem>());
				}
				else
				{
					particleSystemsToPopulate[iterator].InitializeWithValue(gameObject.GetComponent<ParticleSystem>());
				}
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, particleSystemAsset.BundleFileAsset.AssetFileInstanceId);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private AssetReference GetParticleAssetFromReferenceList(string assetId, List<AssetReference> referenceList)
	{
		return referenceList.FirstOrDefault((AssetReference entry) => entry.AssetID == assetId);
	}

	private static HashSet<Shader> UpdateMaterials(AssetBundle loadedBundle)
	{
		HashSet<Shader> hashSet = new HashSet<Shader>();
		List<Shader> list = new List<Shader>();
		Material[] array = loadedBundle.LoadAllAssets<Material>();
		string empty = string.Empty;
		Material[] array2 = array;
		foreach (Material material in array2)
		{
			empty = ((material.shader.name == "Shader Graphs/Endless_Shader") ? "Shader Graphs/Endless_Shader_No_Fade" : material.shader.name);
			Shader shader = material.shader;
			EndlessVisuals.SwapShader(material, Shader.Find(empty));
			if (!list.Contains(shader))
			{
				hashSet.Add(shader);
			}
		}
		return hashSet;
	}

	private IBaseType SetupBaseType(Transform parentTransform, ComponentDefinition componentDefinition)
	{
		IBaseType component = UnityEngine.Object.Instantiate(componentDefinition.Prefab, parentTransform).GetComponent<IBaseType>();
		ReferenceFilter |= component.Filter;
		NavValue = (NavType)Mathf.Max((int)NavValue, (int)component.NavValue);
		return component;
	}

	private IComponentBase SetupComponent(GameObject prefabInstance, Transform parentTransform, ComponentDefinition componentDefinition, Prop prop)
	{
		IComponentBase component = UnityEngine.Object.Instantiate(componentDefinition.Prefab, parentTransform).GetComponent<IComponentBase>();
		ReferenceFilter |= component.Filter;
		NavValue = (NavType)Mathf.Max((int)NavValue, (int)component.NavValue);
		Component component2 = null;
		if (component.ComponentReferenceType != null)
		{
			component2 = prefabInstance.GetComponent(component.ComponentReferenceType);
		}
		component.ComponentInitialize(component2 as ReferenceBase, this);
		return component;
	}

	public void EndlessScriptAwake()
	{
		if (!base.IsServer)
		{
			return;
		}
		scriptComponent.Initialize();
		IScriptInjector[] array = ScriptInjectors;
		foreach (IScriptInjector scriptInjector in array)
		{
			if (scriptInjector.LuaObject != null)
			{
				scriptComponent.RegisterObject(scriptInjector.LuaObjectName, scriptInjector.LuaObject);
			}
			scriptInjector.ScriptInitialize(scriptComponent);
		}
		scriptComponent.RunScript();
	}

	public void Cleanup()
	{
		if (isNetworked)
		{
			NetworkManager.Singleton.RemoveNetworkPrefab(base.gameObject);
		}
	}

	public void HandleInspectionStateChanged(bool isInspected)
	{
		OnInspectionStateChanged.Invoke(isInspected);
	}

	public void CalculateReferenceFilter(Prop prop)
	{
		ReferenceFilter referenceFilter = ReferenceFilter.None;
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(prop.BaseTypeId, out var componentDefinition))
		{
			referenceFilter |= componentDefinition.Filter;
		}
		foreach (string componentId2 in prop.ComponentIds)
		{
			SerializableGuid componentId = componentId2;
			if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentId, out var componentDefinition2))
			{
				referenceFilter |= componentDefinition2.Filter;
			}
		}
		ReferenceFilter = referenceFilter;
	}
}
