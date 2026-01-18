using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class Door : EndlessNetworkBehaviour, IPersistantStateSubscriber, IBaseType, IComponentBase, IScriptInjector
{
	protected struct DoorNetworkData : INetworkSerializable
	{
		public DoorState currentState;

		public bool doorDirection;

		public uint triggerFrame;

		public void NetworkSerialize<DoorNetworkData>(BufferSerializer<DoorNetworkData> serializer) where DoorNetworkData : IReaderWriter
		{
			serializer.SerializeValue(ref currentState, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref doorDirection, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref triggerFrame, default(FastBufferWriter.ForPrimitives));
		}
	}

	protected enum DoorState : byte
	{
		Closed,
		Opening,
		Open,
		Closing
	}

	public enum NpcDoorInteraction
	{
		NotOpenable,
		Openable,
		OpenAndCloseBehind
	}

	[SerializeField]
	private NpcDoorInteraction npcDoorInteraction = NpcDoorInteraction.Openable;

	[SerializeField]
	[HideInInspector]
	private Lockable lockable;

	public readonly EndlessEvent OnDoorOpened = new EndlessEvent();

	public readonly EndlessEvent OnDoorClosed = new EndlessEvent();

	private readonly NetworkVariable<DoorNetworkData> doorData = new NetworkVariable<DoorNetworkData>();

	private bool switchQueued;

	private Coroutine currentAnimationCoroutine;

	private bool lastOpenDirection;

	private Context lastInstigator;

	private Context context;

	[SerializeField]
	private DoorReferences references;

	private Endless.Gameplay.LuaInterfaces.Door luaInterface;

	private EndlessScriptComponent scriptComponent;

	public bool IsLocked
	{
		get
		{
			if (lockable != null)
			{
				return lockable.IsLocked;
			}
			return false;
		}
	}

	public bool IsOpenOrOpening
	{
		get
		{
			DoorState currentState = doorData.Value.currentState;
			return currentState == DoorState.Open || currentState == DoorState.Opening;
		}
	}

	public NpcDoorInteraction CurrentNpcDoorInteraction => npcDoorInteraction;

	public bool ShouldSaveAndLoad => true;

	public Type ComponentReferenceType => typeof(DoorReferences);

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public NavType NavValue => NavType.Dynamic;

	public object LuaObject => luaInterface ?? (luaInterface = new Endless.Gameplay.LuaInterfaces.Door(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.Door);

	internal void OpenFromUser(Context instigator)
	{
		Open(instigator, !GetIsUserForward(instigator));
	}

	private bool GetIsUserForward(Context instigator)
	{
		return base.transform.InverseTransformPoint(instigator.WorldObject.transform.position).z > 0f;
	}

	public void Open(Context instigator, bool forwardDirection)
	{
		if (!references.OpenBothDirections)
		{
			forwardDirection = true;
		}
		if (base.IsServer && !IsLocked)
		{
			lastInstigator = instigator;
			DoorNetworkData value = doorData.Value;
			if (value.currentState == DoorState.Closed)
			{
				value.doorDirection = forwardDirection;
				value.currentState = DoorState.Opening;
				value.triggerFrame = NetClock.CurrentFrame + references.DoorOpenDelayFrames;
			}
			else if (value.currentState == DoorState.Closing)
			{
				value.doorDirection = forwardDirection;
				switchQueued = true;
			}
			else if (value.currentState == DoorState.Opening)
			{
				switchQueued = false;
			}
			doorData.Value = value;
		}
	}

	public void Close(Context instigator)
	{
		if (base.IsServer)
		{
			lastInstigator = instigator;
			DoorNetworkData value = doorData.Value;
			if (value.currentState == DoorState.Open)
			{
				value.currentState = DoorState.Closing;
				value.triggerFrame = NetClock.CurrentFrame + references.DoorOpenDelayFrames;
			}
			else if (value.currentState == DoorState.Opening)
			{
				switchQueued = true;
			}
			else if (value.currentState == DoorState.Closing)
			{
				switchQueued = false;
			}
			doorData.Value = value;
		}
	}

	public void ToggleOpen(Context instigator, bool forwardDirection)
	{
		switch (doorData.Value.currentState)
		{
		case DoorState.Closed:
			Open(instigator, forwardDirection);
			break;
		case DoorState.Opening:
			switchQueued = true;
			break;
		case DoorState.Open:
			Close(instigator);
			break;
		case DoorState.Closing:
			switchQueued = true;
			lastOpenDirection = forwardDirection;
			break;
		}
	}

	public void ToggleOpenFromUser(Context instigator)
	{
		switch (doorData.Value.currentState)
		{
		case DoorState.Closed:
			OpenFromUser(instigator);
			break;
		case DoorState.Opening:
			switchQueued = true;
			break;
		case DoorState.Open:
			Close(instigator);
			break;
		case DoorState.Closing:
			switchQueued = true;
			lastOpenDirection = !GetIsUserForward(instigator);
			break;
		}
	}

	protected virtual void HandleDoorDataUpdated(DoorNetworkData previousValue, DoorNetworkData newValue)
	{
		if (previousValue.currentState == newValue.currentState)
		{
			return;
		}
		if (doorData.Value.currentState == DoorState.Opening)
		{
			if (currentAnimationCoroutine != null)
			{
				StopCoroutine(currentAnimationCoroutine);
			}
			currentAnimationCoroutine = StartCoroutine(OpenAnimation(doorData.Value.doorDirection));
		}
		else if (doorData.Value.currentState == DoorState.Closing)
		{
			if (currentAnimationCoroutine != null)
			{
				StopCoroutine(currentAnimationCoroutine);
			}
			currentAnimationCoroutine = StartCoroutine(CloseAnimation());
		}
	}

	private IEnumerator OpenAnimation(bool forward)
	{
		lastOpenDirection = forward;
		double frameTime = NetClock.GetFrameTime(doorData.Value.triggerFrame);
		double num = ((!base.IsServer) ? NetClock.ClientExtrapolatedAppearanceTime : NetClock.LocalNetworkTime);
		float elapsedTime;
		for (elapsedTime = (float)(num - frameTime); elapsedTime < 0f; elapsedTime += Time.deltaTime)
		{
			yield return null;
		}
		if ((bool)references.DoorAnimator)
		{
			references.DoorAnimator.SetTrigger(references.UnlockAnimName);
			yield return new WaitForSeconds(references.UnlockAnimTime);
		}
		Quaternion startRotation = Quaternion.identity;
		Quaternion endRotationPrimary = Quaternion.identity * Quaternion.Euler(forward ? references.RotationDegrees : (-references.RotationDegrees));
		Quaternion endRotationSecondary = Quaternion.identity * Quaternion.Euler(forward ? (-references.RotationDegrees) : references.RotationDegrees);
		for (; elapsedTime < references.DoorOpenTime; elapsedTime += Time.deltaTime)
		{
			float t = elapsedTime / references.DoorOpenTime;
			references.PrimaryDoor.localRotation = Quaternion.Slerp(startRotation, endRotationPrimary, t);
			if ((bool)references.SecondaryDoor)
			{
				references.SecondaryDoor.localRotation = Quaternion.Slerp(startRotation, endRotationSecondary, t);
			}
			yield return null;
		}
		references.PrimaryDoor.localRotation = endRotationPrimary;
		if ((bool)references.SecondaryDoor)
		{
			references.SecondaryDoor.localRotation = endRotationSecondary;
		}
		if (base.IsServer)
		{
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= HandlePathfindingUpdatedAfterOpen;
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated += HandlePathfindingUpdatedAfterOpen;
			MonoBehaviourSingleton<NavGraph>.Instance.PropStateChanged(WorldObject, isBlocking: false);
		}
	}

	private void HandlePathfindingUpdatedAfterOpen(HashSet<SerializableGuid> updatedProps)
	{
		if (updatedProps.Contains(WorldObject.InstanceId))
		{
			OnDoorOpened.Invoke(lastInstigator);
			scriptComponent.TryExecuteFunction("OnDoorOpened", out var _, lastInstigator);
			DoorNetworkData value = doorData.Value;
			value.currentState = DoorState.Open;
			doorData.Value = value;
			if (switchQueued)
			{
				switchQueued = false;
				Close(lastInstigator);
			}
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= HandlePathfindingUpdatedAfterOpen;
		}
	}

	private IEnumerator CloseAnimation()
	{
		double frameTime = NetClock.GetFrameTime(doorData.Value.triggerFrame);
		double num = ((!base.IsServer) ? NetClock.ClientExtrapolatedAppearanceTime : NetClock.LocalNetworkTime);
		float elapsedTime;
		for (elapsedTime = (float)(num - frameTime); elapsedTime < 0f; elapsedTime += Time.deltaTime)
		{
			yield return null;
		}
		Quaternion startRotationPrimary = Quaternion.Euler(lastOpenDirection ? references.RotationDegrees : (-references.RotationDegrees));
		Quaternion startRotationSecondary = Quaternion.Euler(lastOpenDirection ? (-references.RotationDegrees) : references.RotationDegrees);
		Quaternion endRotation = Quaternion.identity;
		for (; elapsedTime < references.DoorOpenTime; elapsedTime += Time.deltaTime)
		{
			float t = elapsedTime / references.DoorOpenTime;
			references.PrimaryDoor.localRotation = Quaternion.Slerp(startRotationPrimary, endRotation, t);
			if ((bool)references.SecondaryDoor)
			{
				references.SecondaryDoor.localRotation = Quaternion.Slerp(startRotationSecondary, endRotation, t);
			}
			yield return null;
		}
		references.PrimaryDoor.localRotation = endRotation;
		if ((bool)references.SecondaryDoor)
		{
			references.SecondaryDoor.localRotation = endRotation;
		}
		if ((bool)references.DoorAnimator)
		{
			references.DoorAnimator.SetTrigger(references.LockAnimName);
			yield return new WaitForSeconds(references.LockAnimTime);
		}
		if (base.IsServer)
		{
			MonoBehaviourSingleton<NavGraph>.Instance.PropStateChanged(WorldObject, isBlocking: true);
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= HandlePathfindingUpdatedAfterClose;
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated += HandlePathfindingUpdatedAfterClose;
		}
	}

	private void HandlePathfindingUpdatedAfterClose(HashSet<SerializableGuid> obj)
	{
		if (obj.Contains(WorldObject.InstanceId))
		{
			OnDoorClosed.Invoke(lastInstigator);
			scriptComponent.TryExecuteFunction("OnDoorClosed", out var _, lastInstigator);
			DoorNetworkData value = doorData.Value;
			value.currentState = DoorState.Closed;
			doorData.Value = value;
			if (switchQueued)
			{
				switchQueued = false;
				Open(lastInstigator, lastOpenDirection);
			}
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= HandlePathfindingUpdatedAfterClose;
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		NetworkVariable<DoorNetworkData> networkVariable = doorData;
		networkVariable.OnValueChanged = (NetworkVariable<DoorNetworkData>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<DoorNetworkData>.OnValueChangedDelegate(HandleDoorDataUpdated));
		DoorState currentState = doorData.Value.currentState;
		if (currentState == DoorState.Open || currentState == DoorState.Opening)
		{
			bool doorDirection = doorData.Value.doorDirection;
			ForceDoorPositionOpen(doorDirection);
			HandleInitializedOpen();
		}
	}

	private void ForceDoorPositionOpen(bool direction)
	{
		Quaternion localRotation = Quaternion.identity * Quaternion.Euler(direction ? references.RotationDegrees : (-references.RotationDegrees));
		Quaternion localRotation2 = Quaternion.identity * Quaternion.Euler(direction ? (-references.RotationDegrees) : references.RotationDegrees);
		references.PrimaryDoor.localRotation = localRotation;
		if ((bool)references.SecondaryDoor)
		{
			references.SecondaryDoor.localRotation = localRotation2;
		}
	}

	private void ForceDoorPositionClosed()
	{
		references.PrimaryDoor.localRotation = Quaternion.identity;
		if ((bool)references.SecondaryDoor)
		{
			references.SecondaryDoor.localRotation = Quaternion.identity;
		}
	}

	protected virtual void HandleInitializedOpen()
	{
		if ((bool)references.DoorAnimator)
		{
			references.DoorAnimator.SetTrigger(references.UnlockAnimName);
		}
	}

	public List<int3> CaptureClosedCells()
	{
		ForceDoorPositionClosed();
		return CaptureDoorCells();
	}

	public List<int3> OpenForwardAndReturnCells()
	{
		ForceDoorPositionOpen(direction: true);
		return CaptureDoorCells();
	}

	private List<int3> CaptureDoorCells()
	{
		List<int3> list = new List<int3>();
		int layer = LayerMask.NameToLayer("Default");
		IEnumerable<Collider> enumerable = from currentCollider in references.PrimaryDoor.GetComponentsInChildren<Collider>()
			where currentCollider.gameObject.layer == layer
			select currentCollider;
		if (references.SecondaryDoor != null)
		{
			enumerable = enumerable.Concat(from currentCollider in references.SecondaryDoor.GetComponentsInChildren<Collider>()
				where currentCollider.gameObject.layer == layer
				select currentCollider);
		}
		Bounds bounds = default(Bounds);
		foreach (Collider item in enumerable)
		{
			if (bounds.size == UnityEngine.Vector3.zero)
			{
				bounds = item.bounds;
			}
			else
			{
				bounds.Encapsulate(item.bounds);
			}
		}
		Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(bounds.min);
		Vector3Int vector3Int2 = Stage.WorldSpacePointToGridCoordinate(bounds.max);
		UnityEngine.Vector3 halfExtents = UnityEngine.Vector3.one / 2f;
		Collider[] array = new Collider[100];
		for (int num = vector3Int.x; num <= vector3Int2.x; num++)
		{
			for (int num2 = vector3Int.y; num2 <= vector3Int2.y; num2++)
			{
				for (int num3 = vector3Int.z; num3 <= vector3Int2.z; num3++)
				{
					Vector3Int vector3Int3 = new Vector3Int(num, num2, num3);
					int mask = LayerMask.GetMask("Default");
					int num4 = Physics.OverlapBoxNonAlloc(vector3Int3, halfExtents, array, Quaternion.identity, mask);
					bool flag = false;
					for (int num5 = 0; num5 < num4; num5++)
					{
						if (enumerable.Contains(array[num5]))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						list.Add(new int3(num, num2, num3));
					}
				}
			}
		}
		return list;
	}

	public List<int3> OpenBackwardAndReturnCells()
	{
		if (!references.OpenBothDirections)
		{
			return new List<int3>();
		}
		ForceDoorPositionOpen(direction: false);
		return CaptureDoorCells();
	}

	public void Close()
	{
		ForceDoorPositionClosed();
	}

	public object GetSaveState()
	{
		DoorState currentState = doorData.Value.currentState;
		return (currentState == DoorState.Open || currentState == DoorState.Opening, doorData.Value.doorDirection);
	}

	public void LoadState(object loadedState)
	{
		if (base.IsServer && loadedState != null)
		{
			(bool, bool) tuple = ((bool, bool))loadedState;
			if (tuple.Item1)
			{
				LoadOpen(tuple.Item2);
			}
		}
	}

	private void LoadOpen(bool direction)
	{
		Quaternion localRotation = Quaternion.identity * Quaternion.Euler(direction ? references.RotationDegrees : (-references.RotationDegrees));
		Quaternion localRotation2 = Quaternion.identity * Quaternion.Euler(direction ? (-references.RotationDegrees) : references.RotationDegrees);
		references.PrimaryDoor.localRotation = localRotation;
		if ((bool)references.SecondaryDoor)
		{
			references.SecondaryDoor.localRotation = localRotation2;
		}
		if (base.IsServer)
		{
			DoorNetworkData value = doorData.Value;
			value.doorDirection = direction;
			value.currentState = DoorState.Open;
			doorData.Value = value;
			LoadOpen_ClientRpc(direction);
		}
	}

	[ClientRpc]
	private void LoadOpen_ClientRpc(bool direction)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2445460209u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in direction, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 2445460209u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				LoadOpen(direction);
			}
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
		if (worldObject.TryGetUserComponent(typeof(Lockable), out var component))
		{
			lockable = (Lockable)component;
		}
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (DoorReferences)referenceBase;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		if (doorData == null)
		{
			throw new Exception("Door.doorData cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		doorData.Initialize(this);
		__nameNetworkVariable(doorData, "doorData");
		NetworkVariableFields.Add(doorData);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2445460209u, __rpc_handler_2445460209, "LoadOpen_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2445460209(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Door)target).LoadOpen_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "Door";
	}
}
