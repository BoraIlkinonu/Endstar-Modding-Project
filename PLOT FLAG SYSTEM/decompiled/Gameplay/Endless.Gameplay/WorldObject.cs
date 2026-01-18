using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class WorldObject : EndlessBehaviour, IAwakeSubscriber
{
	[SerializeField]
	private List<MonoBehaviour> components = new List<MonoBehaviour>();

	[SerializeField]
	[HideInInspector]
	private Component baseTypeComponent;

	private Dictionary<Type, object> componentMap;

	private IBaseType baseType;

	private SerializableGuid instanceId = SerializableGuid.Empty;

	private bool hasAwoken;

	[field: SerializeField]
	public EndlessProp EndlessProp { get; private set; }

	[field: SerializeField]
	public NetworkObject NetworkObject { get; private set; }

	[field: SerializeField]
	public EndlessVisuals EndlessVisuals { get; private set; }

	private Dictionary<Type, object> ComponentMap
	{
		get
		{
			if (componentMap == null)
			{
				componentMap = new Dictionary<Type, object>();
				foreach (MonoBehaviour component in components)
				{
					if (!componentMap.TryAdd(component.GetType(), component))
					{
						Debug.LogException(new InvalidOperationException($"Attempted to track {component.GetType()}, but it was already tracked"), base.gameObject);
					}
				}
			}
			return componentMap;
		}
	}

	public Context Context => BaseType.Context;

	public IBaseType BaseType => baseType ?? (baseType = baseTypeComponent as IBaseType);

	public Component BaseTypeComponent => baseTypeComponent;

	public SerializableGuid InstanceId
	{
		get
		{
			if (instanceId.IsEmpty)
			{
				instanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.gameObject);
			}
			return instanceId;
		}
	}

	public bool TryGetNetworkObjectId(out uint networkObjectId)
	{
		networkObjectId = (uint)(NetworkObject ? NetworkObject.NetworkObjectId : 0u);
		return NetworkObject;
	}

	public void Initialize(Component baseTypeComponent, IEnumerable<IComponentBase> componentBases, NetworkObject netObject = null)
	{
		if (componentBases != null)
		{
			components.AddRange(componentBases.Cast<MonoBehaviour>());
		}
		this.baseTypeComponent = baseTypeComponent;
		components.Add((MonoBehaviour)baseTypeComponent);
		NetworkObject = netObject;
	}

	public bool TryGetUserComponent(Type type, out object component)
	{
		component = null;
		if (!ComponentMap.TryGetValue(type, out var value))
		{
			return false;
		}
		component = value;
		return true;
	}

	public bool TryGetUserComponent<T>(out T component) where T : IComponentBase
	{
		component = default(T);
		if (!ComponentMap.TryGetValue(typeof(T), out var value))
		{
			return false;
		}
		component = (T)value;
		return true;
	}

	public T GetUserComponent<T>() where T : IComponentBase
	{
		return (T)ComponentMap[typeof(T)];
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!ExitManager.IsQuitting && hasAwoken && (bool)NetworkObject)
		{
			MonoBehaviourSingleton<NetworkedWorldObjectMap>.Instance.ObjectMap.Remove((uint)NetworkObject.NetworkObjectId);
		}
	}

	public void EndlessAwake()
	{
		if (!hasAwoken)
		{
			hasAwoken = true;
			if ((bool)NetworkObject)
			{
				MonoBehaviourSingleton<NetworkedWorldObjectMap>.Instance.ObjectMap.TryAdd((uint)NetworkObject.NetworkObjectId, this);
			}
		}
	}
}
