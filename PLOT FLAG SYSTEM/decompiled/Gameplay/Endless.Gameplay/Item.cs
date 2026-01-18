using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.PlayerInventory;
using Endless.Gameplay.Scripting;
using Endless.ParticleSystems.Components;
using Endless.Props.Assets;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class Item : EndlessNetworkBehaviour, IEquatable<Item>, NetClock.ISimulateFrameEnvironmentSubscriber, IPersistantStateSubscriber, IAwakeSubscriber, IStartSubscriber, IGameEndSubscriber, IBaseType, IComponentBase
{
	private struct NetState : INetworkSerializable
	{
		public struct ThrowInfo
		{
			public UnityEngine.Vector3 startPosition;

			public UnityEngine.Vector3 eulerAngles;

			public UnityEngine.Vector3 groundPosition;
		}

		public State State;

		public NetworkObjectReference CarrierObjectReference;

		public UnityEngine.Vector3 Position;

		public UnityEngine.Vector3 TossDirection;

		public TeleportType TeleportType;

		public UnityEngine.Vector3 TeleportPosition;

		public uint TeleportFrame;

		public PlayerReferenceManager Carrier
		{
			get
			{
				if (!CarrierObjectReference.TryGet(out var networkObject))
				{
					return null;
				}
				return networkObject.GetComponent<PlayerReferenceManager>();
			}
		}

		public bool Teleporting => State == State.Teleporting;

		public NetState(State state, UnityEngine.Vector3 pos)
		{
			State = state;
			CarrierObjectReference = default(NetworkObjectReference);
			Position = pos;
			TossDirection = UnityEngine.Vector3.zero;
			TeleportType = TeleportType.Instant;
			TeleportPosition = default(UnityEngine.Vector3);
			TeleportFrame = 0u;
		}

		public NetState(State state, UnityEngine.Vector3 pos, UnityEngine.Vector3 tossDirection)
		{
			State = state;
			CarrierObjectReference = default(NetworkObjectReference);
			Position = pos;
			TossDirection = tossDirection;
			TeleportType = TeleportType.Instant;
			TeleportPosition = default(UnityEngine.Vector3);
			TeleportFrame = 0u;
		}

		public NetState(State state, NetworkObject carrier = null)
		{
			State = state;
			CarrierObjectReference = (carrier ? new NetworkObjectReference(carrier) : default(NetworkObjectReference));
			Position = UnityEngine.Vector3.zero;
			TossDirection = UnityEngine.Vector3.zero;
			TeleportType = TeleportType.Instant;
			TeleportPosition = default(UnityEngine.Vector3);
			TeleportFrame = 0u;
		}

		public NetState(TeleportType teleportType, UnityEngine.Vector3 currentPosition, UnityEngine.Vector3 teleportPosition, uint teleportFrame)
		{
			State = State.Teleporting;
			Position = currentPosition;
			CarrierObjectReference = default(NetworkObjectReference);
			TossDirection = UnityEngine.Vector3.zero;
			TeleportType = teleportType;
			TeleportPosition = teleportPosition;
			TeleportFrame = teleportFrame;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref State, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref CarrierObjectReference, default(FastBufferWriter.ForNetworkSerializable));
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

		public override string ToString()
		{
			NetworkObject networkObject;
			return $"[ NetState: {State}, Player: {CarrierObjectReference.TryGet(out networkObject)} | Pos: {Position} | Dir: {TossDirection} ] ";
		}
	}

	[Serializable]
	protected struct VisualsInfo
	{
		public GameObject GameObject;

		public UnityEngine.Vector3 Position;

		public UnityEngine.Vector3 Angles;
	}

	public enum InventorySlotType
	{
		Major,
		Minor
	}

	public enum State
	{
		Ground,
		PickedUp,
		Equipped,
		Tossed,
		Teleporting,
		Destroyed
	}

	public const float TOSS_FORCE = 6f;

	public const float VELOCITY_SLEEP_THRESHOLD = 0.005f;

	[SerializeField]
	[HideInInspector]
	private GameObject runtimeGroundVisuals;

	[SerializeField]
	[HideInInspector]
	private GameObject runtimeEquippedVisuals;

	[SerializeField]
	private VisualEquipmentSlot equipmentSlot;

	[SerializeField]
	private InteractableBase interactable;

	[SerializeField]
	private InventorySlotType inventorySlot;

	[SerializeField]
	protected InventoryUsableDefinition inventoryUsableDefinition;

	[SerializeField]
	private string equippedItemParamName = string.Empty;

	[SerializeField]
	private int equippedItemID = 1;

	[SerializeField]
	private GameObject runtimeGroundedVisualsParentGameObject;

	[SerializeField]
	private Rigidbody tossItemRigidbody;

	private NetworkVariable<NetState> netState = new NetworkVariable<NetState>();

	private UnityEngine.Vector3 cachedRigidbodyVelocity;

	private bool networkSetup;

	protected EndlessScriptComponent scriptComponent;

	public EndlessEvent OnPickup = new EndlessEvent();

	private Item levelItemsPickedUpCopy;

	private bool shown;

	private bool isPlaying;

	[SerializeField]
	[HideInInspector]
	private SerializableGuid assetID;

	private Context context;

	public bool CanBePickedUp => ItemState == State.Ground;

	public VisualEquipmentSlot EquipmentVisualSlot => equipmentSlot;

	public InventorySlotType InventorySlot => inventorySlot;

	public InventoryUsableDefinition InventoryUsableDefinition => inventoryUsableDefinition;

	public bool IsLevelItem { get; private set; }

	public SerializableGuid AssetID => assetID;

	public virtual bool IsStackable => false;

	public virtual int StackCount => 1;

	public State ItemState => netState.Value.State;

	public bool IsPickupable
	{
		get
		{
			if (netState.Value.State != State.Ground)
			{
				return netState.Value.State == State.Tossed;
			}
			return true;
		}
	}

	public PlayerReferenceManager Carrier => netState.Value.Carrier;

	protected abstract VisualsInfo GroundVisualsInfo { get; }

	protected abstract VisualsInfo EquippedVisualsInfo { get; }

	bool IPersistantStateSubscriber.ShouldSaveAndLoad => IsLevelItem;

	public Context Context => context ?? (context = new Context(WorldObject));

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public virtual Type ComponentReferenceType => null;

	public NavType NavValue => NavType.Intangible;

	public virtual ReferenceFilter Filter => ReferenceFilter.NonStatic | ReferenceFilter.InventoryItem;

	protected virtual void HandleComponentInitialzed(ReferenceBase referenceBase, Prop prop)
	{
	}

	protected virtual void HandlePrefabInitialized(WorldObject worldObject)
	{
	}

	protected virtual void HandleVisualReferenceInitialized(ComponentReferences references)
	{
	}

	protected virtual object SaveData()
	{
		return null;
	}

	protected virtual void LoadData(object data)
	{
	}

	private void OnEnable()
	{
		MonoBehaviourSingleton<RigidbodyManager>.Instance.AddListener(HandleDisableRigidbodySimulation, HandleEnableRigidbodySimulation);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveListener(HandleDisableRigidbodySimulation, HandleEnableRigidbodySimulation);
	}

	public void Drop(PlayerReferenceManager player)
	{
		if (base.IsServer && player == netState.Value.Carrier)
		{
			UnityEngine.Vector3 forward = player.ApperanceController.VisualsTransform.forward;
			UnityEngine.Vector3 pos = base.transform.position + UnityEngine.Vector3.up;
			netState.Value = new NetState(State.Tossed, pos, forward);
		}
	}

	public virtual void CopyToItem(Item item)
	{
	}

	public void TriggerTeleport(UnityEngine.Vector3 position, TeleportType teleportType)
	{
		Item item = this;
		if (IsLevelItem && levelItemsPickedUpCopy != null)
		{
			item = levelItemsPickedUpCopy;
		}
		if (!(item == null))
		{
			if (item.netState.Value.Carrier != null)
			{
				item.netState.Value.Carrier.Inventory.EnsureItemIsDropped(item);
			}
			item.netState.Value = new NetState(teleportType, base.NetworkObject.transform.position, position, NetClock.CurrentFrame + RuntimeDatabase.GetTeleportInfo(teleportType).FramesToTeleport);
		}
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

	public Item Pickup(PlayerReferenceManager player)
	{
		if (base.IsServer)
		{
			if (!IsLevelItem)
			{
				netState.Value = new NetState(State.PickedUp, player.NetworkObject);
				return this;
			}
			netState.Value = new NetState(State.Destroyed);
			MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetID, out var metadata);
			MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject));
			Item componentInChildren = UnityEngine.Object.Instantiate(metadata.EndlessProp.gameObject).GetComponentInChildren<Item>();
			componentInChildren.scriptComponent = componentInChildren.GetComponentInParent<EndlessScriptComponent>();
			componentInChildren.scriptComponent.ShouldSaveAndLoad = false;
			componentInChildren.NetworkObject.Spawn();
			CopyToItem(componentInChildren);
			componentInChildren.netState.Value = new NetState(State.PickedUp, player.NetworkObject);
			return componentInChildren;
		}
		return null;
	}

	public void InitializeNewItem(bool launch, UnityEngine.Vector3 position, float rotation)
	{
		scriptComponent = GetComponentInParent<EndlessScriptComponent>();
		scriptComponent.ShouldSaveAndLoad = false;
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

	public void LevelItemPickupFinished(PlayerReferenceManager player, Item itemCopy)
	{
		levelItemsPickedUpCopy = itemCopy;
		OnPickup.Invoke(player.WorldObject.Context);
		UnityEngine.Object.Destroy(base.transform.parent.gameObject);
	}

	public Item SpawnFromReference(PlayerReferenceManager player)
	{
		MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetID, out var metadata);
		MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject));
		Item componentInChildren = UnityEngine.Object.Instantiate(metadata.EndlessProp.gameObject).GetComponentInChildren<Item>();
		componentInChildren.scriptComponent = componentInChildren.GetComponentInParent<EndlessScriptComponent>();
		componentInChildren.scriptComponent.ShouldSaveAndLoad = false;
		componentInChildren.NetworkObject.Spawn();
		componentInChildren.netState.Value = new NetState(State.PickedUp, player.NetworkObject);
		return componentInChildren;
	}

	public void Equip(PlayerReferenceManager player)
	{
		if (base.IsServer)
		{
			netState.Value = new NetState(State.Equipped, player.NetworkObject);
		}
		if (!shown)
		{
			HandleOnEquipped(player);
			shown = true;
		}
	}

	public void UnEquip(PlayerReferenceManager player)
	{
		if (base.IsServer && netState.Value.State == State.Equipped)
		{
			netState.Value = new NetState(State.PickedUp, player.NetworkObject);
		}
		if (shown)
		{
			HandleOnUnequipped(player);
			shown = false;
		}
	}

	public void ToggleLocalVisibility(PlayerReferenceManager playerReferences, bool visible, bool useEquipmentAnimation)
	{
		if (netState.Value.State != State.Equipped)
		{
			return;
		}
		runtimeEquippedVisuals.SetActive(visible);
		if (equippedItemParamName != string.Empty)
		{
			if (useEquipmentAnimation)
			{
				playerReferences.ApperanceController.AppearanceAnimator.Animator.SetInteger(equippedItemParamName, equippedItemID);
			}
			else
			{
				playerReferences.ApperanceController.AppearanceAnimator.Animator.SetInteger(equippedItemParamName, 0);
			}
		}
	}

	public void Destroy()
	{
		if (base.IsServer)
		{
			netState.Value = new NetState(State.Destroyed);
		}
	}

	public void ResetAppearance()
	{
		HandleNetStateChanged(new NetState(State.Destroyed), netState.Value);
	}

	private void HandleNetStateChanged(NetState oldState, NetState newState)
	{
		if (!networkSetup)
		{
			return;
		}
		GameObject obj = runtimeGroundVisuals;
		State state = newState.State;
		obj.SetActive(state == State.Ground || state == State.Tossed || state == State.Teleporting);
		GameObject obj2 = interactable.gameObject;
		state = newState.State;
		obj2.SetActive(state == State.Ground || state == State.Tossed);
		runtimeEquippedVisuals.SetActive(newState.State == State.Equipped);
		runtimeGroundedVisualsParentGameObject.SetActive(newState.State != State.Destroyed);
		PlayerReferenceManager carrier = newState.Carrier;
		if ((bool)carrier)
		{
			Transform obj3 = base.NetworkObject.transform;
			obj3.SetParent(carrier.PlayerEquipmentManager.GetBoneForEquipment(EquipmentVisualSlot));
			obj3.localPosition = UnityEngine.Vector3.zero;
			obj3.localRotation = Quaternion.identity;
			base.transform.localRotation = Quaternion.identity;
			tossItemRigidbody.isKinematic = true;
			tossItemRigidbody.interpolation = RigidbodyInterpolation.None;
			tossItemRigidbody.transform.localPosition = UnityEngine.Vector3.zero;
		}
		else
		{
			Transform transform = base.NetworkObject.transform;
			transform.SetParent(null);
			transform.localRotation = Quaternion.identity;
			if (newState.State == State.Tossed)
			{
				tossItemRigidbody.position = newState.Position;
				tossItemRigidbody.isKinematic = false;
				tossItemRigidbody.rotation = Quaternion.identity;
				tossItemRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				tossItemRigidbody.velocity = UnityEngine.Vector3.Lerp(newState.TossDirection, UnityEngine.Vector3.up, 0.6f) * (6f + UnityEngine.Random.Range(-0.25f, 0.25f)) / tossItemRigidbody.mass;
			}
			else
			{
				if (newState.State != State.Destroyed)
				{
					transform.position = newState.Position;
				}
				tossItemRigidbody.isKinematic = true;
				tossItemRigidbody.interpolation = RigidbodyInterpolation.None;
				tossItemRigidbody.transform.localPosition = UnityEngine.Vector3.zero;
				tossItemRigidbody.transform.localRotation = Quaternion.identity;
			}
		}
		if (newState.State == State.Equipped && oldState.State != State.Equipped)
		{
			carrier.ApperanceController.OnEquipmentInUseChanged.AddListener(HandleInUseChangedCheck);
			carrier.ApperanceController.OnEquipmentAvailableChanged.AddListener(HandleAvailableChangedCheck);
			carrier.ApperanceController.OnEquipmentCooldownChanged.AddListener(HandleCooldownChangedCheck);
			carrier.ApperanceController.OnEquipmentResourceChanged.AddListener(HandleResourceChangedCheck);
			carrier.ApperanceController.OnEquipmentUseStateChanged.AddListener(HandleEquipmentUseStateChangedCheck);
			carrier.ApperanceController.AppearanceAnimator.OnEquippedItemChanged(this, equipped: true);
		}
		else if (oldState.State == State.Equipped && newState.State != State.Equipped)
		{
			PlayerReferenceManager carrier2 = oldState.Carrier;
			if (equippedItemParamName != string.Empty)
			{
				carrier2.ApperanceController.AppearanceAnimator.Animator.SetInteger(equippedItemParamName, 0);
			}
			carrier2.ApperanceController.OnEquipmentInUseChanged.RemoveListener(HandleInUseChangedCheck);
			carrier2.ApperanceController.OnEquipmentAvailableChanged.RemoveListener(HandleAvailableChangedCheck);
			carrier2.ApperanceController.OnEquipmentCooldownChanged.RemoveListener(HandleCooldownChangedCheck);
			carrier2.ApperanceController.OnEquipmentResourceChanged.RemoveListener(HandleResourceChangedCheck);
			carrier2.ApperanceController.OnEquipmentUseStateChanged.RemoveListener(HandleEquipmentUseStateChangedCheck);
			carrier2.ApperanceController.AppearanceAnimator.OnEquippedItemChanged(this, equipped: false);
			if (shown)
			{
				HandleOnUnequipped(carrier2);
				shown = false;
			}
		}
		if (oldState.State != State.Teleporting && newState.State == State.Teleporting)
		{
			RuntimeDatabase.GetTeleportInfo(newState.TeleportType).TeleportStart(WorldObject.EndlessVisuals, null, newState.Position);
		}
		else if (oldState.State == State.Teleporting && newState.State != State.Teleporting)
		{
			RuntimeDatabase.GetTeleportInfo(oldState.TeleportType).TeleportEnd(WorldObject.EndlessVisuals, null, newState.Position);
		}
	}

	private void HandleInUseChangedCheck(SerializableGuid guid, bool inUse)
	{
		if (guid.Equals(inventoryUsableDefinition.Guid))
		{
			HandleInUseChanged(inUse);
		}
	}

	private void HandleAvailableChangedCheck(SerializableGuid guid, bool available)
	{
		if (guid.Equals(inventoryUsableDefinition.Guid))
		{
			HandleAvailableChanged(available);
		}
	}

	private void HandleCooldownChangedCheck(SerializableGuid guid, float currentSeconds, float totalSeconds)
	{
		if (guid.Equals(inventoryUsableDefinition.Guid))
		{
			HandleCooldownChanged(currentSeconds, totalSeconds);
		}
	}

	private void HandleResourceChangedCheck(SerializableGuid guid, float percent)
	{
		if (guid.Equals(inventoryUsableDefinition.Guid))
		{
			HandleResourceChanged(percent);
		}
	}

	private void HandleEquipmentUseStateChangedCheck(SerializableGuid guid, UsableDefinition.UseState eus)
	{
		if (guid.Equals(inventoryUsableDefinition.Guid))
		{
			HandleEquipmentUseStateChanged(eus);
		}
	}

	protected virtual void HandleInUseChanged(bool inUse)
	{
	}

	protected virtual void HandleAvailableChanged(bool available)
	{
	}

	protected virtual void HandleCooldownChanged(float currentSeconds, float totalSeconds)
	{
	}

	protected virtual void HandleResourceChanged(float percent)
	{
	}

	protected virtual void HandleEquipmentUseStateChanged(UsableDefinition.UseState eus)
	{
	}

	protected virtual void HandleOnEquipped(PlayerReferenceManager player)
	{
	}

	protected virtual void HandleOnUnequipped(PlayerReferenceManager player)
	{
	}

	private void HandleDisableRigidbodySimulation()
	{
		cachedRigidbodyVelocity = tossItemRigidbody.velocity;
		tossItemRigidbody.isKinematic = true;
	}

	private void HandleEnableRigidbodySimulation()
	{
		if (netState.Value.State == State.Tossed)
		{
			tossItemRigidbody.isKinematic = false;
			tossItemRigidbody.velocity = cachedRigidbodyVelocity;
		}
		else
		{
			tossItemRigidbody.isKinematic = true;
			tossItemRigidbody.transform.localPosition = UnityEngine.Vector3.zero;
		}
	}

	private void FixedUpdate()
	{
		if (base.IsServer && ItemState == State.Tossed)
		{
			if (tossItemRigidbody.velocity.magnitude < 0.005f)
			{
				netState.Value = new NetState(State.Ground, tossItemRigidbody.position);
			}
			else if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && tossItemRigidbody.position.y < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageResetHeight)
			{
				netState.Value = new NetState(State.Destroyed);
			}
		}
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<NetState> networkVariable = netState;
		networkVariable.OnValueChanged = (NetworkVariable<NetState>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<NetState>.OnValueChangedDelegate(HandleNetStateChanged));
		StartCoroutine("WaitForCarrierLoad");
		if (base.IsServer)
		{
			networkSetup = true;
			netState.Value = new NetState(ItemState, base.transform.parent.position);
		}
		base.OnNetworkSpawn();
	}

	private IEnumerator WaitForCarrierLoad()
	{
		while (true)
		{
			yield return null;
			State state = netState.Value.State;
			if (state != State.PickedUp && state != State.Equipped)
			{
				break;
			}
			PlayerReferenceManager carrier = netState.Value.Carrier;
			if ((bool)carrier && (bool)carrier.ApperanceController && (bool)carrier.ApperanceController.AppearanceAnimator)
			{
				yield return null;
				networkSetup = true;
				HandleNetStateChanged(default(NetState), netState.Value);
				yield break;
			}
			yield return null;
		}
		yield return null;
		networkSetup = true;
		HandleNetStateChanged(default(NetState), netState.Value);
	}

	public bool Equals(Item other)
	{
		return this == other;
	}

	object IPersistantStateSubscriber.GetSaveState()
	{
		return (ItemState, netState.Value.Position, StackCount, SaveData());
	}

	void IPersistantStateSubscriber.LoadState(object loadedState)
	{
		if (loadedState != null)
		{
			(State, UnityEngine.Vector3, int, object) tuple = ((State, UnityEngine.Vector3, int, object))loadedState;
			LoadedCount(tuple.Item3);
			if (base.IsServer)
			{
				netState.Value = new NetState(tuple.Item1, tuple.Item2);
			}
			LoadData(tuple.Item4);
		}
	}

	protected virtual void LoadedCount(int count)
	{
	}

	void IAwakeSubscriber.EndlessAwake()
	{
		isPlaying = true;
		if (base.IsServer)
		{
			IsLevelItem = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject) != SerializableGuid.Empty;
		}
	}

	void IStartSubscriber.EndlessStart()
	{
		NetClock.Register(this);
	}

	void IGameEndSubscriber.EndlessGameEnd()
	{
		NetClock.Unregister(this);
		isPlaying = false;
		if (base.NetworkManager.IsServer && !IsLevelItem)
		{
			State state = netState.Value.State;
			if (state != State.PickedUp && state != State.Equipped)
			{
				base.NetworkObject.Despawn();
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		assetID = endlessProp.Prop.AssetID;
		runtimeGroundVisuals = UnityEngine.Object.Instantiate(GroundVisualsInfo.GameObject, runtimeGroundedVisualsParentGameObject.transform.position + GroundVisualsInfo.Position, runtimeGroundedVisualsParentGameObject.transform.rotation * Quaternion.Euler(GroundVisualsInfo.Angles), runtimeGroundedVisualsParentGameObject.transform);
		runtimeEquippedVisuals = UnityEngine.Object.Instantiate(EquippedVisualsInfo.GameObject, base.transform.position + EquippedVisualsInfo.Position, base.transform.rotation * Quaternion.Euler(EquippedVisualsInfo.Angles), base.transform);
		base.transform.localPosition = UnityEngine.Vector3.zero;
		base.transform.rotation = Quaternion.identity;
		SwappableParticleSystem[] componentsInChildren = runtimeEquippedVisuals.GetComponentsInChildren<SwappableParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].InitializeWithEmbedded();
		}
		HandleVisualReferenceInitialized(runtimeEquippedVisuals.GetComponent<ComponentReferences>());
		HandleComponentInitialzed(referenceBase, endlessProp.Prop);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
		List<Renderer> list = runtimeGroundVisuals.GetComponentsInChildren<Renderer>().ToList();
		list.AddRange(runtimeEquippedVisuals.GetComponentsInChildren<Renderer>());
		WorldObject.EndlessVisuals.ManageRenderers(list);
		runtimeEquippedVisuals.SetActive(value: false);
		HandlePrefabInitialized(worldObject);
	}

	public static void NetworkWrite<T>(Item item, BufferSerializer<T> serializer) where T : IReaderWriter
	{
		if (item == null)
		{
			NetworkObjectReference value = default(NetworkObjectReference);
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForNetworkSerializable));
		}
		else
		{
			NetworkObjectReference value2 = new NetworkObjectReference(item.NetworkObject);
			serializer.SerializeValue(ref value2, default(FastBufferWriter.ForNetworkSerializable));
		}
	}

	public static Item NetworkRead<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		NetworkObjectReference value = default(NetworkObjectReference);
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForNetworkSerializable));
		if (value.TryGet(out var networkObject))
		{
			return networkObject.GetComponentInChildren<Item>();
		}
		return null;
	}

	protected override void __initializeVariables()
	{
		if (netState == null)
		{
			throw new Exception("Item.netState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		netState.Initialize(this);
		__nameNetworkVariable(netState, "netState");
		NetworkVariableFields.Add(netState);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "Item";
	}
}
