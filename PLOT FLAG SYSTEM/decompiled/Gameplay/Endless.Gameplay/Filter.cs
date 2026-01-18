using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Endless.Gameplay;

public class Filter : EndlessNetworkBehaviour, IGameEndSubscriber, IBaseType, IComponentBase, IAwakeSubscriber
{
	public enum FilterType
	{
		None,
		FilmNoir,
		Sepia,
		Blurred,
		Neon,
		Warm,
		Curse,
		Meadow,
		Curved
	}

	[Serializable]
	public class FilterTypePair
	{
		public Volume Volume;

		public FilterType FilterType;
	}

	[Tooltip("This is expected to contain an entry for each filter type except none!")]
	[SerializeField]
	private List<FilterTypePair> filterTypes = new List<FilterTypePair>();

	[SerializeField]
	private float transitionTime = 2f;

	[SerializeField]
	private bool visibleInCreator = true;

	[FormerlySerializedAs("currentFilter")]
	[SerializeField]
	private FilterType serverFilter;

	private Dictionary<FilterType, Volume> filterMap = new Dictionary<FilterType, Volume>();

	private Dictionary<FilterType, Coroutine> liveTransitions = new Dictionary<FilterType, Coroutine>();

	private FilterType localFilter;

	private FilterType appliedFilter;

	private bool serverChanged;

	private Context context;

	public FilterType CurrentFilterType
	{
		get
		{
			return serverFilter;
		}
		set
		{
			if (serverChanged || serverFilter == value)
			{
				return;
			}
			serverFilter = value;
			if (visibleInCreator && base.IsClient)
			{
				if (appliedFilter != FilterType.None)
				{
					StartTransition(appliedFilter, 1f, 0f);
				}
				appliedFilter = value;
				if (appliedFilter != FilterType.None)
				{
					StartTransition(appliedFilter, 0f, 1f);
				}
			}
		}
	}

	public bool VisibleInCreator
	{
		get
		{
			return visibleInCreator;
		}
		set
		{
			if (serverChanged || visibleInCreator == value)
			{
				return;
			}
			visibleInCreator = value;
			if (visibleInCreator)
			{
				if (serverFilter != FilterType.None)
				{
					appliedFilter = serverFilter;
					StartTransition(serverFilter, 0f, 1f);
				}
			}
			else if (appliedFilter != FilterType.None)
			{
				StartTransition(appliedFilter, 1f, 0f);
				appliedFilter = FilterType.None;
			}
		}
	}

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public NavType NavValue => NavType.Intangible;

	public void SetFilterType(Context context, FilterType value)
	{
		if (base.IsClient && appliedFilter != FilterType.None)
		{
			StartTransition(appliedFilter, 1f, 0f);
		}
		if (base.IsServer)
		{
			serverFilter = value;
			serverChanged = true;
			SetServerFilter_ClientRpc(value);
		}
		if (base.IsClient)
		{
			appliedFilter = value;
			if (appliedFilter != FilterType.None)
			{
				StartTransition(appliedFilter, 0f, 1f);
			}
		}
	}

	[ClientRpc]
	private void SetServerFilter_ClientRpc(FilterType value)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2009833482u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForEnums));
			__endSendClientRpc(ref bufferWriter, 2009833482u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				serverFilter = value;
				SetFilterType(Context, value);
			}
		}
	}

	public void SetFilterTypeForPlayer(Context context, FilterType value)
	{
		if (context.IsPlayer())
		{
			PlayerLuaComponent userComponent = context.WorldObject.GetUserComponent<PlayerLuaComponent>();
			ClientRpcParams clientParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[1] { userComponent.WorldObject.NetworkObject.OwnerClientId }
				}
			};
			SetLocalFilterType_ClientRpc(value, clientParams);
		}
	}

	[ClientRpc]
	public void SetLocalFilterType_ClientRpc(FilterType value, ClientRpcParams clientParams)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(4004194627u, clientParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForEnums));
			__endSendClientRpc(ref bufferWriter, 4004194627u, clientParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		localFilter = value;
		if (serverFilter == FilterType.None)
		{
			if (appliedFilter != FilterType.None)
			{
				StartTransition(appliedFilter, 1f, 0f);
			}
			appliedFilter = value;
			if (appliedFilter != FilterType.None)
			{
				StartTransition(appliedFilter, 0f, 1f);
			}
		}
	}

	private void Awake()
	{
		foreach (FilterTypePair filterType in filterTypes)
		{
			filterMap.Add(filterType.FilterType, filterType.Volume);
		}
	}

	public void EndlessGameEnd()
	{
		StopAllCoroutines();
		serverChanged = false;
		localFilter = FilterType.None;
		serverFilter = FilterType.None;
		if (appliedFilter != FilterType.None)
		{
			filterMap[appliedFilter].weight = 0f;
			appliedFilter = FilterType.None;
		}
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		base.OnSynchronize(ref serializer);
		serializer.SerializeValue(ref serverFilter, default(FastBufferWriter.ForEnums));
		serializer.SerializeValue(ref serverChanged, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsReader && serverFilter != FilterType.None)
		{
			appliedFilter = serverFilter;
			filterMap[serverFilter].weight = 1f;
		}
	}

	public void SetTransitionTime(Context context, float newTransitionTime)
	{
		transitionTime = newTransitionTime;
	}

	private void StartTransition(FilterType filterType, float start, float end)
	{
		if (liveTransitions.ContainsKey(filterType))
		{
			StopCoroutine(liveTransitions[filterType]);
			liveTransitions[filterType] = StartCoroutine(LerpVolume(filterType, filterMap[filterType], start, end));
		}
		else
		{
			liveTransitions.Add(filterType, StartCoroutine(LerpVolume(filterType, filterMap[filterType], start, end)));
		}
	}

	private IEnumerator LerpVolume(FilterType filterType, Volume volume, float start, float end)
	{
		yield return null;
		float num = Mathf.InverseLerp(start, end, volume.weight);
		for (float elapsedTime = num * transitionTime; elapsedTime < transitionTime; elapsedTime += Time.deltaTime)
		{
			volume.weight = Mathf.Lerp(start, end, Mathf.Clamp01(elapsedTime / transitionTime));
			yield return null;
		}
		volume.weight = end;
		liveTransitions.Remove(filterType);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void EndlessAwake()
	{
		if (serverFilter != FilterType.None)
		{
			appliedFilter = serverFilter;
			filterMap[appliedFilter].weight = 1f;
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2009833482u, __rpc_handler_2009833482, "SetServerFilter_ClientRpc");
		__registerRpc(4004194627u, __rpc_handler_4004194627, "SetLocalFilterType_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2009833482(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out FilterType value, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Filter)target).SetServerFilter_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4004194627(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out FilterType value, default(FastBufferWriter.ForEnums));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Filter)target).SetLocalFilterType_ClientRpc(value, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "Filter";
	}
}
