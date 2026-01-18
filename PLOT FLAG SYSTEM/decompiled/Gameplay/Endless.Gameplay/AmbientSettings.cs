using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class AmbientSettings : EndlessNetworkBehaviour, IRegisteredSubscriber, IBaseType, IComponentBase, IAwakeSubscriber, IPropPlacedSubscriber
{
	public enum Skybox
	{
		Sunrise,
		EarlyMidMorning,
		HighNoon,
		Evening,
		SpookyNight,
		Sunrise2,
		ClearNight,
		DustStormDay,
		Darkness,
		Overcast,
		Plague,
		EndstarAcretion,
		SnowyDay,
		SnowyFog,
		Volcanic,
		Heavenly,
		Rainy,
		Haze,
		Aurora,
		Storm,
		LightningStorm,
		ClearMorning
	}

	[Serializable]
	public class AmbientPair
	{
		[HideInInspector]
		public UnityEvent<Skybox> OnDeactivated = new UnityEvent<Skybox>();

		public Skybox Skybox;

		public SimpleAmbientEntry AmbientEntry;

		public void Initialize()
		{
			AmbientEntry.OnDeactivated.AddListener(HandleDeactivated);
		}

		private void HandleDeactivated(AmbientEntry _)
		{
			OnDeactivated.Invoke(Skybox);
		}
	}

	[SerializeField]
	private List<AmbientPair> ambientPairs = new List<AmbientPair>();

	private NetworkVariable<Skybox> activeSkybox = new NetworkVariable<Skybox>((Skybox)(-1));

	private SerializableGuid instanceId = SerializableGuid.Empty;

	private Skybox skybox;

	private Dictionary<Skybox, SimpleAmbientEntry> ambientEntryMap;

	private Context context;

	private SerializableGuid InstanceId
	{
		get
		{
			if (instanceId == SerializableGuid.Empty)
			{
				instanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject);
			}
			return instanceId;
		}
	}

	[EndlessNonSerialized]
	public bool IsDefault
	{
		get
		{
			if (MonoBehaviourSingleton<StageManager>.Instance != null && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage != null)
			{
				return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.IsDefaultEnvironment(InstanceId);
			}
			return false;
		}
		set
		{
			if (value)
			{
				if (activeSkybox.Value == (Skybox)(-1))
				{
					MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SetDefaultEnvironment(InstanceId);
					CurrentSkybox = CurrentSkybox;
				}
			}
			else if (IsDefault)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ClearDefaultEnvironment();
				MonoBehaviourSingleton<AmbientManager>.Instance.SetAmbientEntry(null);
			}
		}
	}

	public Skybox CurrentSkybox
	{
		get
		{
			return skybox;
		}
		set
		{
			skybox = value;
			if (activeSkybox.Value == (Skybox)(-1) && (IsDefault || MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying))
			{
				MonoBehaviourSingleton<AmbientManager>.Instance.SetAmbientEntry(AmbientEntryMap[skybox]);
			}
		}
	}

	public Dictionary<Skybox, SimpleAmbientEntry> AmbientEntryMap
	{
		get
		{
			if (ambientEntryMap == null)
			{
				ambientEntryMap = new Dictionary<Skybox, SimpleAmbientEntry>();
				foreach (AmbientPair ambientPair in ambientPairs)
				{
					ambientEntryMap.Add(ambientPair.Skybox, ambientPair.AmbientEntry);
				}
			}
			return ambientEntryMap;
		}
	}

	public Context Context => context ?? (context = new Context(WorldObject));

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public NavType NavValue => NavType.Intangible;

	private void Awake()
	{
		foreach (AmbientPair ambientPair in ambientPairs)
		{
			ambientPair.Initialize();
			ambientPair.OnDeactivated.AddListener(HandleAmbientEntryDeactivated);
		}
	}

	private void HandleAmbientEntryDeactivated(Skybox deactivatedValue)
	{
		if (base.IsServer && activeSkybox.Value == deactivatedValue)
		{
			activeSkybox.Value = (Skybox)(-1);
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsClient)
		{
			if (activeSkybox.Value != (Skybox)(-1))
			{
				ApplySkybox(activeSkybox.Value);
			}
			NetworkVariable<Skybox> networkVariable = activeSkybox;
			networkVariable.OnValueChanged = (NetworkVariable<Skybox>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<Skybox>.OnValueChangedDelegate(HandleActiveSkyboxChanged));
		}
	}

	private void HandleActiveSkyboxChanged(Skybox previousValue, Skybox newValue)
	{
		if (newValue != (Skybox)(-1))
		{
			ApplySkybox(newValue);
		}
	}

	public void ChangeSkybox(Context context, Skybox newSkybox)
	{
		if (base.IsServer)
		{
			activeSkybox.Value = newSkybox;
		}
	}

	public void ApplySkybox(Skybox newSkybox)
	{
		MonoBehaviourSingleton<AmbientManager>.Instance.SetAmbientEntry(AmbientEntryMap[newSkybox]);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance && IsDefault)
		{
			if ((bool)MonoBehaviourSingleton<AmbientManager>.Instance)
			{
				MonoBehaviourSingleton<AmbientManager>.Instance.SetAmbientEntry(null);
			}
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.ClearDefaultEnvironment();
		}
	}

	public void EndlessRegistered()
	{
		if (base.IsClient && !MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && IsDefault && activeSkybox.Value == (Skybox)(-1))
		{
			ApplySkybox(skybox);
		}
	}

	[ClientRpc]
	private void SetDefaultEnvironment_ClientRpc(SerializableGuid instanceId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3934989725u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in instanceId, default(FastBufferWriter.ForNetworkSerializable));
			__endSendClientRpc(ref bufferWriter, 3934989725u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SetDefaultEnvironment(instanceId);
			if (activeSkybox.Value == (Skybox)(-1))
			{
				ApplySkybox(CurrentSkybox);
			}
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void EndlessAwake()
	{
		if (base.IsServer && IsDefault)
		{
			activeSkybox.Value = CurrentSkybox;
		}
	}

	public void PropPlaced(SerializableGuid instanceId, bool isCopy)
	{
		if (!isCopy && !MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.HasDefaultEnvironmentSet)
		{
			SetDefaultEnvironment_ClientRpc(instanceId);
			MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.NetworkSetDefaultEnvironment(instanceId);
			if (!base.IsClient)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.SetDefaultEnvironment(instanceId);
			}
		}
	}

	protected override void __initializeVariables()
	{
		if (activeSkybox == null)
		{
			throw new Exception("AmbientSettings.activeSkybox cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		activeSkybox.Initialize(this);
		__nameNetworkVariable(activeSkybox, "activeSkybox");
		NetworkVariableFields.Add(activeSkybox);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3934989725u, __rpc_handler_3934989725, "SetDefaultEnvironment_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3934989725(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SerializableGuid value, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((AmbientSettings)target).SetDefaultEnvironment_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "AmbientSettings";
	}
}
