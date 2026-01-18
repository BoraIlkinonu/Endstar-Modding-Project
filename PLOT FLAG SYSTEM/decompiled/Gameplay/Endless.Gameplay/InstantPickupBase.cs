using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class InstantPickupBase : EndlessNetworkBehaviour, NetClock.ISimulateFrameEnvironmentSubscriber, IPersistantStateSubscriber, IStartSubscriber, IGameEndSubscriber
{
	private struct NetState : INetworkSerializable
	{
		public State State;

		public UnityEngine.Vector3 Position;

		public UnityEngine.Vector3 TossDirection;

		public TeleportType TeleportType;

		public UnityEngine.Vector3 TeleportPosition;

		public uint TeleportFrame;

		public bool Teleporting => State == State.Teleporting;

		public NetState(State state, UnityEngine.Vector3 pos)
		{
			State = state;
			Position = pos;
			TossDirection = UnityEngine.Vector3.zero;
			TeleportType = TeleportType.Instant;
			TeleportPosition = default(UnityEngine.Vector3);
			TeleportFrame = 0u;
		}

		public NetState(State state, UnityEngine.Vector3 pos, UnityEngine.Vector3 tossDirection)
		{
			State = state;
			Position = pos;
			TossDirection = tossDirection;
			TeleportType = TeleportType.Instant;
			TeleportPosition = default(UnityEngine.Vector3);
			TeleportFrame = 0u;
		}

		public NetState(TeleportType teleportType, UnityEngine.Vector3 currentPosition, UnityEngine.Vector3 teleportPosition, uint teleportFrame)
		{
			State = State.Teleporting;
			Position = currentPosition;
			TossDirection = UnityEngine.Vector3.zero;
			TeleportType = teleportType;
			TeleportPosition = teleportPosition;
			TeleportFrame = teleportFrame;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref State, default(FastBufferWriter.ForEnums));
			if (State == State.Ground)
			{
				serializer.SerializeValue(ref Position);
			}
			else if (State == State.Tossed)
			{
				serializer.SerializeValue(ref Position);
				serializer.SerializeValue(ref TossDirection);
			}
			else if (State == State.Teleporting)
			{
				serializer.SerializeValue(ref Position);
				serializer.SerializeValue(ref TeleportType, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue(ref TeleportPosition);
				serializer.SerializeValue(ref TeleportFrame, default(FastBufferWriter.ForPrimitives));
			}
		}
	}

	private enum State
	{
		Ground,
		Tossed,
		Teleporting
	}

	[SerializeField]
	protected WorldTrigger worldTrigger;

	[SerializeField]
	private Rigidbody tossRigidbody;

	private NetworkVariable<bool> collected = new NetworkVariable<bool>(value: false);

	private NetworkVariable<NetState> netState = new NetworkVariable<NetState>();

	public EndlessEvent OnCollected = new EndlessEvent();

	private UnityEngine.Vector3 cachedRigidbodyVelocity;

	private bool netStateInitialized;

	public bool AllowPickupWhileDowned { get; set; }

	public PickupFilter CurrentPickupFilter { get; set; }

	public bool ShouldSaveAndLoad { get; set; } = true;

	private void OnEnable()
	{
		MonoBehaviourSingleton<RigidbodyManager>.Instance.AddListener(HandleDisableRigidbodySimulation, HandleEnableRigidbodySimulation);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveListener(HandleDisableRigidbodySimulation, HandleEnableRigidbodySimulation);
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (!base.IsServer)
		{
			if (collected.Value)
			{
				SetActive(active: false);
			}
			NetworkVariable<bool> networkVariable = collected;
			networkVariable.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(HandleCollected));
		}
		else
		{
			netState.Value = new NetState(State.Ground, tossRigidbody.transform.parent.position);
		}
		NetworkVariable<NetState> networkVariable2 = netState;
		networkVariable2.OnValueChanged = (NetworkVariable<NetState>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<NetState>.OnValueChangedDelegate(HandleNetStateChanged));
	}

	protected abstract void SetActive(bool active);

	private void HandleCollected(bool previousValue, bool newValue)
	{
		SetActive(!newValue);
	}

	private bool CanBePickedUp(WorldCollidable worldCollidable)
	{
		if (!worldCollidable.WorldObject)
		{
			return false;
		}
		NpcEntity component = null;
		PlayerLuaComponent component2 = null;
		bool flag = CurrentPickupFilter != PickupFilter.Players && worldCollidable.WorldObject.TryGetUserComponent<NpcEntity>(out component);
		bool flag2 = !flag && worldCollidable.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out component2);
		if (AllowPickupWhileDowned)
		{
			switch (CurrentPickupFilter)
			{
			case PickupFilter.Players:
				return flag2;
			case PickupFilter.Characters:
				return flag2 || flag;
			case PickupFilter.Anything:
				return true;
			}
		}
		else
		{
			switch (CurrentPickupFilter)
			{
			case PickupFilter.Players:
				if (flag2)
				{
					return component2.References.HealthComponent.CurrentHealth > 0;
				}
				return false;
			case PickupFilter.Characters:
				if (!flag2 || component2.References.HealthComponent.CurrentHealth <= 0)
				{
					if (flag)
					{
						return !component.IsDowned;
					}
					return false;
				}
				return true;
			case PickupFilter.Anything:
				return true;
			}
		}
		return false;
	}

	private void HandlePickup(WorldCollidable worldCollidable, bool isRollbackFrame)
	{
		if (base.IsServer && !collected.Value && CanBePickedUp(worldCollidable) && ExternalAttemptPickup(worldCollidable.WorldObject.Context))
		{
			collected.Value = true;
			ApplyPickupResult(worldCollidable.WorldObject);
			BroadcastPickupResult(worldCollidable.WorldObject.Context);
			ShowCollectedFX_ClientRPC();
			SetActive(active: false);
		}
	}

	protected virtual bool ExternalAttemptPickup(Context context)
	{
		return true;
	}

	protected virtual void ApplyPickupResult(WorldObject worldObject)
	{
	}

	private void BroadcastPickupResult(Context context)
	{
		OnCollected.Invoke(context);
	}

	[ClientRpc]
	private void ShowCollectedFX_ClientRPC()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(3456888992u, clientRpcParams, RpcDelivery.Reliable);
				__endSendClientRpc(ref bufferWriter, 3456888992u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				ShowCollectedFX();
			}
		}
	}

	protected abstract void ShowCollectedFX();

	public void Respawn()
	{
		if (collected.Value)
		{
			collected.Value = false;
			SetActive(active: true);
		}
	}

	public void ForceCollect()
	{
		if (!collected.Value)
		{
			collected.Value = true;
			SetActive(active: false);
		}
	}

	public void TriggerTeleport(UnityEngine.Vector3 position, TeleportType teleportType)
	{
		netState.Value = new NetState(teleportType, base.NetworkObject.transform.position, position, NetClock.CurrentFrame + RuntimeDatabase.GetTeleportInfo(teleportType).FramesToTeleport);
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		if (netState.Value.Teleporting && frame == netState.Value.TeleportFrame)
		{
			base.transform.position = netState.Value.TeleportPosition;
			if (base.IsServer)
			{
				netState.Value = new NetState(State.Ground, netState.Value.TeleportPosition);
			}
		}
	}

	private void FixedUpdate()
	{
		if (!base.IsSpawned || !netStateInitialized)
		{
			base.transform.localPosition = UnityEngine.Vector3.zero;
		}
		else if (base.IsServer && netState.Value.State == State.Tossed)
		{
			if (tossRigidbody.velocity.magnitude < 0.005f)
			{
				netState.Value = new NetState(State.Ground, base.transform.position);
			}
			else if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && tossRigidbody.position.y < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageResetHeight)
			{
				base.NetworkObject.Despawn();
			}
		}
	}

	private void HandleNetStateChanged(NetState oldState, NetState newState)
	{
		netStateInitialized = true;
		if (newState.State == State.Tossed)
		{
			tossRigidbody.position = newState.Position;
			tossRigidbody.isKinematic = false;
			tossRigidbody.rotation = Quaternion.identity;
			tossRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			tossRigidbody.velocity = UnityEngine.Vector3.Lerp(newState.TossDirection, UnityEngine.Vector3.up, 0.6f) * 6f / tossRigidbody.mass;
		}
		else
		{
			tossRigidbody.isKinematic = true;
			tossRigidbody.interpolation = RigidbodyInterpolation.None;
			tossRigidbody.position = newState.Position;
		}
		if (oldState.State != State.Teleporting && newState.State == State.Teleporting)
		{
			RuntimeDatabase.GetTeleportInfo(newState.TeleportType).TeleportStart(GetComponent<WorldObject>().EndlessVisuals, null, newState.Position);
		}
		else if (oldState.State == State.Teleporting && newState.State != State.Teleporting)
		{
			RuntimeDatabase.GetTeleportInfo(oldState.TeleportType).TeleportEnd(GetComponent<WorldObject>().EndlessVisuals, null, newState.Position);
		}
	}

	private void HandleDisableRigidbodySimulation()
	{
		cachedRigidbodyVelocity = tossRigidbody.velocity;
		tossRigidbody.isKinematic = true;
	}

	private void HandleEnableRigidbodySimulation()
	{
		if (netState.Value.State == State.Tossed)
		{
			tossRigidbody.isKinematic = false;
			tossRigidbody.velocity = cachedRigidbodyVelocity;
		}
		else
		{
			tossRigidbody.isKinematic = true;
		}
	}

	public void InitializeNewItem(bool launch, UnityEngine.Vector3 position, float rotation)
	{
		base.NetworkObject.Spawn();
		if (launch)
		{
			netState.Value = new NetState(State.Tossed, position, Quaternion.Euler(0f, rotation, 0f) * UnityEngine.Vector3.forward);
		}
		else
		{
			netState.Value = new NetState(State.Ground, position);
		}
	}

	public object GetSaveState()
	{
		return collected.Value;
	}

	public void LoadState(object loadedState)
	{
		if (base.IsServer && loadedState != null)
		{
			collected.Value = (bool)loadedState;
			SetActive(!collected.Value);
		}
	}

	public void EndlessStart()
	{
		worldTrigger.OnTriggerEnter.AddListener(HandlePickup);
	}

	public void EndlessGameEnd()
	{
		worldTrigger.OnTriggerEnter.RemoveListener(HandlePickup);
		SetActive(active: true);
		if (base.IsServer)
		{
			collected.Value = false;
		}
	}

	protected override void __initializeVariables()
	{
		if (collected == null)
		{
			throw new Exception("InstantPickupBase.collected cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		collected.Initialize(this);
		__nameNetworkVariable(collected, "collected");
		NetworkVariableFields.Add(collected);
		if (netState == null)
		{
			throw new Exception("InstantPickupBase.netState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		netState.Initialize(this);
		__nameNetworkVariable(netState, "netState");
		NetworkVariableFields.Add(netState);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3456888992u, __rpc_handler_3456888992, "ShowCollectedFX_ClientRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3456888992(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((InstantPickupBase)target).ShowCollectedFX_ClientRPC();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "InstantPickupBase";
	}
}
