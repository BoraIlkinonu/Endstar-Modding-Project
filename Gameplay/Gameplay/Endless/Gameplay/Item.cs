using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing;
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
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020002AB RID: 683
	public abstract class Item : EndlessNetworkBehaviour, IEquatable<Item>, NetClock.ISimulateFrameEnvironmentSubscriber, IPersistantStateSubscriber, IAwakeSubscriber, IStartSubscriber, IGameEndSubscriber, IBaseType, IComponentBase
	{
		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x06000F13 RID: 3859 RVA: 0x0004F027 File Offset: 0x0004D227
		public bool CanBePickedUp
		{
			get
			{
				return this.ItemState == Item.State.Ground;
			}
		}

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06000F14 RID: 3860 RVA: 0x0004F032 File Offset: 0x0004D232
		public VisualEquipmentSlot EquipmentVisualSlot
		{
			get
			{
				return this.equipmentSlot;
			}
		}

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06000F15 RID: 3861 RVA: 0x0004F03A File Offset: 0x0004D23A
		public Item.InventorySlotType InventorySlot
		{
			get
			{
				return this.inventorySlot;
			}
		}

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06000F16 RID: 3862 RVA: 0x0004F042 File Offset: 0x0004D242
		public InventoryUsableDefinition InventoryUsableDefinition
		{
			get
			{
				return this.inventoryUsableDefinition;
			}
		}

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06000F17 RID: 3863 RVA: 0x0004F04A File Offset: 0x0004D24A
		// (set) Token: 0x06000F18 RID: 3864 RVA: 0x0004F052 File Offset: 0x0004D252
		public bool IsLevelItem { get; private set; }

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06000F19 RID: 3865 RVA: 0x0004F05B File Offset: 0x0004D25B
		public SerializableGuid AssetID
		{
			get
			{
				return this.assetID;
			}
		}

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06000F1A RID: 3866 RVA: 0x0001965C File Offset: 0x0001785C
		public virtual bool IsStackable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06000F1B RID: 3867 RVA: 0x00017586 File Offset: 0x00015786
		public virtual int StackCount
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06000F1C RID: 3868 RVA: 0x0004F063 File Offset: 0x0004D263
		public Item.State ItemState
		{
			get
			{
				return this.netState.Value.State;
			}
		}

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06000F1D RID: 3869 RVA: 0x0004F075 File Offset: 0x0004D275
		public bool IsPickupable
		{
			get
			{
				return this.netState.Value.State == Item.State.Ground || this.netState.Value.State == Item.State.Tossed;
			}
		}

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06000F1E RID: 3870 RVA: 0x0004F0A0 File Offset: 0x0004D2A0
		public PlayerReferenceManager Carrier
		{
			get
			{
				return this.netState.Value.Carrier;
			}
		}

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06000F1F RID: 3871
		protected abstract Item.VisualsInfo GroundVisualsInfo { get; }

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06000F20 RID: 3872
		protected abstract Item.VisualsInfo EquippedVisualsInfo { get; }

		// Token: 0x06000F21 RID: 3873 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleComponentInitialzed(ReferenceBase referenceBase, Prop prop)
		{
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandlePrefabInitialized(WorldObject worldObject)
		{
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleVisualReferenceInitialized(ComponentReferences references)
		{
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x00002D9F File Offset: 0x00000F9F
		protected virtual object SaveData()
		{
			return null;
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void LoadData(object data)
		{
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x0004F0C0 File Offset: 0x0004D2C0
		private void OnEnable()
		{
			MonoBehaviourSingleton<RigidbodyManager>.Instance.AddListener(new UnityAction(this.HandleDisableRigidbodySimulation), new UnityAction(this.HandleEnableRigidbodySimulation));
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x0004F0E4 File Offset: 0x0004D2E4
		private void OnDisable()
		{
			MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveListener(new UnityAction(this.HandleDisableRigidbodySimulation), new UnityAction(this.HandleEnableRigidbodySimulation));
		}

		// Token: 0x06000F28 RID: 3880 RVA: 0x0004F108 File Offset: 0x0004D308
		public void Drop(PlayerReferenceManager player)
		{
			if (base.IsServer && player == this.netState.Value.Carrier)
			{
				global::UnityEngine.Vector3 forward = player.ApperanceController.VisualsTransform.forward;
				global::UnityEngine.Vector3 vector = base.transform.position + global::UnityEngine.Vector3.up;
				this.netState.Value = new Item.NetState(Item.State.Tossed, vector, forward);
			}
		}

		// Token: 0x06000F29 RID: 3881 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public virtual void CopyToItem(Item item)
		{
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x0004F174 File Offset: 0x0004D374
		public void TriggerTeleport(global::UnityEngine.Vector3 position, TeleportType teleportType)
		{
			Item item = this;
			if (this.IsLevelItem && this.levelItemsPickedUpCopy != null)
			{
				item = this.levelItemsPickedUpCopy;
			}
			if (item == null)
			{
				return;
			}
			if (item.netState.Value.Carrier != null)
			{
				item.netState.Value.Carrier.Inventory.EnsureItemIsDropped(item);
			}
			item.netState.Value = new Item.NetState(teleportType, base.NetworkObject.transform.position, position, NetClock.CurrentFrame + RuntimeDatabase.GetTeleportInfo(teleportType).FramesToTeleport);
		}

		// Token: 0x06000F2B RID: 3883 RVA: 0x0004F218 File Offset: 0x0004D418
		public void SimulateFrameEnvironment(uint frame)
		{
			if (this.netState.Value.Teleporting && frame == this.netState.Value.TeleportFrame)
			{
				base.transform.position = this.netState.Value.TeleportPosition;
				if (base.IsServer)
				{
					this.netState.Value = new Item.NetState(Item.State.Ground, this.netState.Value.TeleportPosition);
				}
			}
		}

		// Token: 0x06000F2C RID: 3884 RVA: 0x0004F294 File Offset: 0x0004D494
		public Item Pickup(PlayerReferenceManager player)
		{
			if (!base.IsServer)
			{
				return null;
			}
			if (!this.IsLevelItem)
			{
				this.netState.Value = new Item.NetState(Item.State.PickedUp, player.NetworkObject);
				return this;
			}
			this.netState.Value = new Item.NetState(Item.State.Destroyed, null);
			PropLibrary.RuntimePropInfo runtimePropInfo;
			MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(this.assetID, out runtimePropInfo);
			MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject));
			Item componentInChildren = global::UnityEngine.Object.Instantiate<GameObject>(runtimePropInfo.EndlessProp.gameObject).GetComponentInChildren<Item>();
			componentInChildren.scriptComponent = componentInChildren.GetComponentInParent<EndlessScriptComponent>();
			componentInChildren.scriptComponent.ShouldSaveAndLoad = false;
			componentInChildren.NetworkObject.Spawn(false);
			this.CopyToItem(componentInChildren);
			componentInChildren.netState.Value = new Item.NetState(Item.State.PickedUp, player.NetworkObject);
			return componentInChildren;
		}

		// Token: 0x06000F2D RID: 3885 RVA: 0x0004F37C File Offset: 0x0004D57C
		public void InitializeNewItem(bool launch, global::UnityEngine.Vector3 position, float rotation)
		{
			this.scriptComponent = base.GetComponentInParent<EndlessScriptComponent>();
			this.scriptComponent.ShouldSaveAndLoad = false;
			base.NetworkObject.Spawn(false);
			if (launch)
			{
				this.netState.Value = new Item.NetState(Item.State.Tossed, position, Quaternion.Euler(0f, rotation, 0f) * global::UnityEngine.Vector3.forward);
				return;
			}
			this.netState.Value = new Item.NetState(Item.State.Ground, position);
		}

		// Token: 0x06000F2E RID: 3886 RVA: 0x0004F3EF File Offset: 0x0004D5EF
		public void LevelItemPickupFinished(PlayerReferenceManager player, Item itemCopy)
		{
			this.levelItemsPickedUpCopy = itemCopy;
			this.OnPickup.Invoke(player.WorldObject.Context);
			global::UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}

		// Token: 0x06000F2F RID: 3887 RVA: 0x0004F424 File Offset: 0x0004D624
		public Item SpawnFromReference(PlayerReferenceManager player)
		{
			PropLibrary.RuntimePropInfo runtimePropInfo;
			MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(this.assetID, out runtimePropInfo);
			MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject));
			Item componentInChildren = global::UnityEngine.Object.Instantiate<GameObject>(runtimePropInfo.EndlessProp.gameObject).GetComponentInChildren<Item>();
			componentInChildren.scriptComponent = componentInChildren.GetComponentInParent<EndlessScriptComponent>();
			componentInChildren.scriptComponent.ShouldSaveAndLoad = false;
			componentInChildren.NetworkObject.Spawn(false);
			componentInChildren.netState.Value = new Item.NetState(Item.State.PickedUp, player.NetworkObject);
			return componentInChildren;
		}

		// Token: 0x06000F30 RID: 3888 RVA: 0x0004F4C2 File Offset: 0x0004D6C2
		public void Equip(PlayerReferenceManager player)
		{
			if (base.IsServer)
			{
				this.netState.Value = new Item.NetState(Item.State.Equipped, player.NetworkObject);
			}
			if (!this.shown)
			{
				this.HandleOnEquipped(player);
				this.shown = true;
			}
		}

		// Token: 0x06000F31 RID: 3889 RVA: 0x0004F4FC File Offset: 0x0004D6FC
		public void UnEquip(PlayerReferenceManager player)
		{
			if (base.IsServer && this.netState.Value.State == Item.State.Equipped)
			{
				this.netState.Value = new Item.NetState(Item.State.PickedUp, player.NetworkObject);
			}
			if (this.shown)
			{
				this.HandleOnUnequipped(player);
				this.shown = false;
			}
		}

		// Token: 0x06000F32 RID: 3890 RVA: 0x0004F554 File Offset: 0x0004D754
		public void ToggleLocalVisibility(PlayerReferenceManager playerReferences, bool visible, bool useEquipmentAnimation)
		{
			if (this.netState.Value.State != Item.State.Equipped)
			{
				return;
			}
			this.runtimeEquippedVisuals.SetActive(visible);
			if (this.equippedItemParamName != string.Empty)
			{
				if (useEquipmentAnimation)
				{
					playerReferences.ApperanceController.AppearanceAnimator.Animator.SetInteger(this.equippedItemParamName, this.equippedItemID);
					return;
				}
				playerReferences.ApperanceController.AppearanceAnimator.Animator.SetInteger(this.equippedItemParamName, 0);
			}
		}

		// Token: 0x06000F33 RID: 3891 RVA: 0x0004F5D4 File Offset: 0x0004D7D4
		public void Destroy()
		{
			if (base.IsServer)
			{
				this.netState.Value = new Item.NetState(Item.State.Destroyed, null);
			}
		}

		// Token: 0x06000F34 RID: 3892 RVA: 0x0004F5F0 File Offset: 0x0004D7F0
		public void ResetAppearance()
		{
			this.HandleNetStateChanged(new Item.NetState(Item.State.Destroyed, null), this.netState.Value);
		}

		// Token: 0x06000F35 RID: 3893 RVA: 0x0004F60C File Offset: 0x0004D80C
		private void HandleNetStateChanged(Item.NetState oldState, Item.NetState newState)
		{
			if (!this.networkSetup)
			{
				return;
			}
			GameObject gameObject = this.runtimeGroundVisuals;
			Item.State state = newState.State;
			gameObject.SetActive(state == Item.State.Ground || state == Item.State.Tossed || state == Item.State.Teleporting);
			GameObject gameObject2 = this.interactable.gameObject;
			state = newState.State;
			gameObject2.SetActive(state == Item.State.Ground || state == Item.State.Tossed);
			this.runtimeEquippedVisuals.SetActive(newState.State == Item.State.Equipped);
			this.runtimeGroundedVisualsParentGameObject.SetActive(newState.State != Item.State.Destroyed);
			PlayerReferenceManager carrier = newState.Carrier;
			if (carrier)
			{
				Transform transform = base.NetworkObject.transform;
				transform.SetParent(carrier.PlayerEquipmentManager.GetBoneForEquipment(this.EquipmentVisualSlot));
				transform.localPosition = global::UnityEngine.Vector3.zero;
				transform.localRotation = Quaternion.identity;
				base.transform.localRotation = Quaternion.identity;
				this.tossItemRigidbody.isKinematic = true;
				this.tossItemRigidbody.interpolation = RigidbodyInterpolation.None;
				this.tossItemRigidbody.transform.localPosition = global::UnityEngine.Vector3.zero;
			}
			else
			{
				Transform transform2 = base.NetworkObject.transform;
				transform2.SetParent(null);
				transform2.localRotation = Quaternion.identity;
				if (newState.State == Item.State.Tossed)
				{
					this.tossItemRigidbody.position = newState.Position;
					this.tossItemRigidbody.isKinematic = false;
					this.tossItemRigidbody.rotation = Quaternion.identity;
					this.tossItemRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
					this.tossItemRigidbody.velocity = global::UnityEngine.Vector3.Lerp(newState.TossDirection, global::UnityEngine.Vector3.up, 0.6f) * (6f + global::UnityEngine.Random.Range(-0.25f, 0.25f)) / this.tossItemRigidbody.mass;
				}
				else
				{
					if (newState.State != Item.State.Destroyed)
					{
						transform2.position = newState.Position;
					}
					this.tossItemRigidbody.isKinematic = true;
					this.tossItemRigidbody.interpolation = RigidbodyInterpolation.None;
					this.tossItemRigidbody.transform.localPosition = global::UnityEngine.Vector3.zero;
					this.tossItemRigidbody.transform.localRotation = Quaternion.identity;
				}
			}
			if (newState.State == Item.State.Equipped && oldState.State != Item.State.Equipped)
			{
				carrier.ApperanceController.OnEquipmentInUseChanged.AddListener(new UnityAction<SerializableGuid, bool>(this.HandleInUseChangedCheck));
				carrier.ApperanceController.OnEquipmentAvailableChanged.AddListener(new UnityAction<SerializableGuid, bool>(this.HandleAvailableChangedCheck));
				carrier.ApperanceController.OnEquipmentCooldownChanged.AddListener(new UnityAction<SerializableGuid, float, float>(this.HandleCooldownChangedCheck));
				carrier.ApperanceController.OnEquipmentResourceChanged.AddListener(new UnityAction<SerializableGuid, float>(this.HandleResourceChangedCheck));
				carrier.ApperanceController.OnEquipmentUseStateChanged.AddListener(new UnityAction<SerializableGuid, UsableDefinition.UseState>(this.HandleEquipmentUseStateChangedCheck));
				carrier.ApperanceController.AppearanceAnimator.OnEquippedItemChanged(this, true);
			}
			else if (oldState.State == Item.State.Equipped && newState.State != Item.State.Equipped)
			{
				PlayerReferenceManager carrier2 = oldState.Carrier;
				if (this.equippedItemParamName != string.Empty)
				{
					carrier2.ApperanceController.AppearanceAnimator.Animator.SetInteger(this.equippedItemParamName, 0);
				}
				carrier2.ApperanceController.OnEquipmentInUseChanged.RemoveListener(new UnityAction<SerializableGuid, bool>(this.HandleInUseChangedCheck));
				carrier2.ApperanceController.OnEquipmentAvailableChanged.RemoveListener(new UnityAction<SerializableGuid, bool>(this.HandleAvailableChangedCheck));
				carrier2.ApperanceController.OnEquipmentCooldownChanged.RemoveListener(new UnityAction<SerializableGuid, float, float>(this.HandleCooldownChangedCheck));
				carrier2.ApperanceController.OnEquipmentResourceChanged.RemoveListener(new UnityAction<SerializableGuid, float>(this.HandleResourceChangedCheck));
				carrier2.ApperanceController.OnEquipmentUseStateChanged.RemoveListener(new UnityAction<SerializableGuid, UsableDefinition.UseState>(this.HandleEquipmentUseStateChangedCheck));
				carrier2.ApperanceController.AppearanceAnimator.OnEquippedItemChanged(this, false);
				if (this.shown)
				{
					this.HandleOnUnequipped(carrier2);
					this.shown = false;
				}
			}
			if (oldState.State != Item.State.Teleporting && newState.State == Item.State.Teleporting)
			{
				RuntimeDatabase.GetTeleportInfo(newState.TeleportType).TeleportStart(this.WorldObject.EndlessVisuals, null, newState.Position);
				return;
			}
			if (oldState.State == Item.State.Teleporting && newState.State != Item.State.Teleporting)
			{
				RuntimeDatabase.GetTeleportInfo(oldState.TeleportType).TeleportEnd(this.WorldObject.EndlessVisuals, null, newState.Position);
			}
		}

		// Token: 0x06000F36 RID: 3894 RVA: 0x0004FA4F File Offset: 0x0004DC4F
		private void HandleInUseChangedCheck(SerializableGuid guid, bool inUse)
		{
			if (guid.Equals(this.inventoryUsableDefinition.Guid))
			{
				this.HandleInUseChanged(inUse);
			}
		}

		// Token: 0x06000F37 RID: 3895 RVA: 0x0004FA6C File Offset: 0x0004DC6C
		private void HandleAvailableChangedCheck(SerializableGuid guid, bool available)
		{
			if (guid.Equals(this.inventoryUsableDefinition.Guid))
			{
				this.HandleAvailableChanged(available);
			}
		}

		// Token: 0x06000F38 RID: 3896 RVA: 0x0004FA89 File Offset: 0x0004DC89
		private void HandleCooldownChangedCheck(SerializableGuid guid, float currentSeconds, float totalSeconds)
		{
			if (guid.Equals(this.inventoryUsableDefinition.Guid))
			{
				this.HandleCooldownChanged(currentSeconds, totalSeconds);
			}
		}

		// Token: 0x06000F39 RID: 3897 RVA: 0x0004FAA7 File Offset: 0x0004DCA7
		private void HandleResourceChangedCheck(SerializableGuid guid, float percent)
		{
			if (guid.Equals(this.inventoryUsableDefinition.Guid))
			{
				this.HandleResourceChanged(percent);
			}
		}

		// Token: 0x06000F3A RID: 3898 RVA: 0x0004FAC4 File Offset: 0x0004DCC4
		private void HandleEquipmentUseStateChangedCheck(SerializableGuid guid, UsableDefinition.UseState eus)
		{
			if (guid.Equals(this.inventoryUsableDefinition.Guid))
			{
				this.HandleEquipmentUseStateChanged(eus);
			}
		}

		// Token: 0x06000F3B RID: 3899 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleInUseChanged(bool inUse)
		{
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleAvailableChanged(bool available)
		{
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleCooldownChanged(float currentSeconds, float totalSeconds)
		{
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleResourceChanged(float percent)
		{
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleEquipmentUseStateChanged(UsableDefinition.UseState eus)
		{
		}

		// Token: 0x06000F40 RID: 3904 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleOnEquipped(PlayerReferenceManager player)
		{
		}

		// Token: 0x06000F41 RID: 3905 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleOnUnequipped(PlayerReferenceManager player)
		{
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x0004FAE1 File Offset: 0x0004DCE1
		private void HandleDisableRigidbodySimulation()
		{
			this.cachedRigidbodyVelocity = this.tossItemRigidbody.velocity;
			this.tossItemRigidbody.isKinematic = true;
		}

		// Token: 0x06000F43 RID: 3907 RVA: 0x0004FB00 File Offset: 0x0004DD00
		private void HandleEnableRigidbodySimulation()
		{
			if (this.netState.Value.State == Item.State.Tossed)
			{
				this.tossItemRigidbody.isKinematic = false;
				this.tossItemRigidbody.velocity = this.cachedRigidbodyVelocity;
				return;
			}
			this.tossItemRigidbody.isKinematic = true;
			this.tossItemRigidbody.transform.localPosition = global::UnityEngine.Vector3.zero;
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x0004FB60 File Offset: 0x0004DD60
		private void FixedUpdate()
		{
			if (base.IsServer && this.ItemState == Item.State.Tossed)
			{
				if (this.tossItemRigidbody.velocity.magnitude < 0.005f)
				{
					this.netState.Value = new Item.NetState(Item.State.Ground, this.tossItemRigidbody.position);
					return;
				}
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && this.tossItemRigidbody.position.y < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageResetHeight)
				{
					this.netState.Value = new Item.NetState(Item.State.Destroyed, null);
				}
			}
		}

		// Token: 0x06000F45 RID: 3909 RVA: 0x0004FBFC File Offset: 0x0004DDFC
		public override void OnNetworkSpawn()
		{
			NetworkVariable<Item.NetState> networkVariable = this.netState;
			networkVariable.OnValueChanged = (NetworkVariable<Item.NetState>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<Item.NetState>.OnValueChangedDelegate(this.HandleNetStateChanged));
			base.StartCoroutine("WaitForCarrierLoad");
			if (base.IsServer)
			{
				this.networkSetup = true;
				this.netState.Value = new Item.NetState(this.ItemState, base.transform.parent.position);
			}
			base.OnNetworkSpawn();
		}

		// Token: 0x06000F46 RID: 3910 RVA: 0x0004FC77 File Offset: 0x0004DE77
		private IEnumerator WaitForCarrierLoad()
		{
			for (;;)
			{
				yield return null;
				Item.State state = this.netState.Value.State;
				if (state != Item.State.PickedUp && state != Item.State.Equipped)
				{
					goto IL_00DE;
				}
				PlayerReferenceManager carrier = this.netState.Value.Carrier;
				if (carrier && carrier.ApperanceController && carrier.ApperanceController.AppearanceAnimator)
				{
					break;
				}
				yield return null;
			}
			yield return null;
			this.networkSetup = true;
			this.HandleNetStateChanged(default(Item.NetState), this.netState.Value);
			goto IL_0135;
			IL_00DE:
			yield return null;
			this.networkSetup = true;
			this.HandleNetStateChanged(default(Item.NetState), this.netState.Value);
			IL_0135:
			yield break;
		}

		// Token: 0x06000F47 RID: 3911 RVA: 0x0004FC86 File Offset: 0x0004DE86
		public bool Equals(Item other)
		{
			return this == other;
		}

		// Token: 0x170002F1 RID: 753
		// (get) Token: 0x06000F48 RID: 3912 RVA: 0x0004FC8F File Offset: 0x0004DE8F
		bool IPersistantStateSubscriber.ShouldSaveAndLoad
		{
			get
			{
				return this.IsLevelItem;
			}
		}

		// Token: 0x06000F49 RID: 3913 RVA: 0x0004FC97 File Offset: 0x0004DE97
		object IPersistantStateSubscriber.GetSaveState()
		{
			return new ValueTuple<Item.State, global::UnityEngine.Vector3, int, object>(this.ItemState, this.netState.Value.Position, this.StackCount, this.SaveData());
		}

		// Token: 0x06000F4A RID: 3914 RVA: 0x0004FCC8 File Offset: 0x0004DEC8
		void IPersistantStateSubscriber.LoadState(object loadedState)
		{
			if (loadedState != null)
			{
				ValueTuple<Item.State, global::UnityEngine.Vector3, int, object> valueTuple = (ValueTuple<Item.State, global::UnityEngine.Vector3, int, object>)loadedState;
				this.LoadedCount(valueTuple.Item3);
				if (base.IsServer)
				{
					this.netState.Value = new Item.NetState(valueTuple.Item1, valueTuple.Item2);
				}
				this.LoadData(valueTuple.Item4);
			}
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void LoadedCount(int count)
		{
		}

		// Token: 0x06000F4C RID: 3916 RVA: 0x0004FD1B File Offset: 0x0004DF1B
		void IAwakeSubscriber.EndlessAwake()
		{
			this.isPlaying = true;
			if (base.IsServer)
			{
				this.IsLevelItem = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(base.transform.parent.gameObject) != SerializableGuid.Empty;
			}
		}

		// Token: 0x06000F4D RID: 3917 RVA: 0x0001CAAE File Offset: 0x0001ACAE
		void IStartSubscriber.EndlessStart()
		{
			NetClock.Register(this);
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x0004FD5C File Offset: 0x0004DF5C
		void IGameEndSubscriber.EndlessGameEnd()
		{
			NetClock.Unregister(this);
			this.isPlaying = false;
			if (base.NetworkManager.IsServer && !this.IsLevelItem)
			{
				Item.State state = this.netState.Value.State;
				if (state != Item.State.PickedUp && state != Item.State.Equipped)
				{
					base.NetworkObject.Despawn(true);
					global::UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06000F4F RID: 3919 RVA: 0x0004FDBC File Offset: 0x0004DFBC
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06000F50 RID: 3920 RVA: 0x0004FDE7 File Offset: 0x0004DFE7
		// (set) Token: 0x06000F51 RID: 3921 RVA: 0x0004FDEF File Offset: 0x0004DFEF
		public WorldObject WorldObject { get; private set; }

		// Token: 0x170002F4 RID: 756
		// (get) Token: 0x06000F52 RID: 3922 RVA: 0x00002D9F File Offset: 0x00000F9F
		public virtual Type ComponentReferenceType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06000F53 RID: 3923 RVA: 0x0001BD04 File Offset: 0x00019F04
		public NavType NavValue
		{
			get
			{
				return NavType.Intangible;
			}
		}

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06000F54 RID: 3924 RVA: 0x0004FDF8 File Offset: 0x0004DFF8
		public virtual ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.NonStatic | ReferenceFilter.InventoryItem;
			}
		}

		// Token: 0x06000F55 RID: 3925 RVA: 0x0004FDFC File Offset: 0x0004DFFC
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.assetID = endlessProp.Prop.AssetID;
			this.runtimeGroundVisuals = global::UnityEngine.Object.Instantiate<GameObject>(this.GroundVisualsInfo.GameObject, this.runtimeGroundedVisualsParentGameObject.transform.position + this.GroundVisualsInfo.Position, this.runtimeGroundedVisualsParentGameObject.transform.rotation * Quaternion.Euler(this.GroundVisualsInfo.Angles), this.runtimeGroundedVisualsParentGameObject.transform);
			this.runtimeEquippedVisuals = global::UnityEngine.Object.Instantiate<GameObject>(this.EquippedVisualsInfo.GameObject, base.transform.position + this.EquippedVisualsInfo.Position, base.transform.rotation * Quaternion.Euler(this.EquippedVisualsInfo.Angles), base.transform);
			base.transform.localPosition = global::UnityEngine.Vector3.zero;
			base.transform.rotation = Quaternion.identity;
			SwappableParticleSystem[] componentsInChildren = this.runtimeEquippedVisuals.GetComponentsInChildren<SwappableParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].InitializeWithEmbedded();
			}
			this.HandleVisualReferenceInitialized(this.runtimeEquippedVisuals.GetComponent<ComponentReferences>());
			this.HandleComponentInitialzed(referenceBase, endlessProp.Prop);
		}

		// Token: 0x06000F56 RID: 3926 RVA: 0x0004FF3C File Offset: 0x0004E13C
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
			List<Renderer> list = this.runtimeGroundVisuals.GetComponentsInChildren<Renderer>().ToList<Renderer>();
			list.AddRange(this.runtimeEquippedVisuals.GetComponentsInChildren<Renderer>());
			this.WorldObject.EndlessVisuals.ManageRenderers(list);
			this.runtimeEquippedVisuals.SetActive(false);
			this.HandlePrefabInitialized(worldObject);
		}

		// Token: 0x06000F57 RID: 3927 RVA: 0x0004FF98 File Offset: 0x0004E198
		public static void NetworkWrite<T>(Item item, BufferSerializer<T> serializer) where T : IReaderWriter
		{
			if (item == null)
			{
				NetworkObjectReference networkObjectReference = default(NetworkObjectReference);
				serializer.SerializeValue<NetworkObjectReference>(ref networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
				return;
			}
			NetworkObjectReference networkObjectReference2 = new NetworkObjectReference(item.NetworkObject);
			serializer.SerializeValue<NetworkObjectReference>(ref networkObjectReference2, default(FastBufferWriter.ForNetworkSerializable));
		}

		// Token: 0x06000F58 RID: 3928 RVA: 0x0004FFE8 File Offset: 0x0004E1E8
		public static Item NetworkRead<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			NetworkObjectReference networkObjectReference = default(NetworkObjectReference);
			serializer.SerializeValue<NetworkObjectReference>(ref networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			NetworkObject networkObject;
			if (networkObjectReference.TryGet(out networkObject, null))
			{
				return networkObject.GetComponentInChildren<Item>();
			}
			return null;
		}

		// Token: 0x06000F5A RID: 3930 RVA: 0x0005006C File Offset: 0x0004E26C
		protected override void __initializeVariables()
		{
			bool flag = this.netState == null;
			if (flag)
			{
				throw new Exception("Item.netState cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.netState.Initialize(this);
			base.__nameNetworkVariable(this.netState, "netState");
			this.NetworkVariableFields.Add(this.netState);
			base.__initializeVariables();
		}

		// Token: 0x06000F5B RID: 3931 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000F5C RID: 3932 RVA: 0x000500CF File Offset: 0x0004E2CF
		protected internal override string __getTypeName()
		{
			return "Item";
		}

		// Token: 0x04000D5F RID: 3423
		public const float TOSS_FORCE = 6f;

		// Token: 0x04000D60 RID: 3424
		public const float VELOCITY_SLEEP_THRESHOLD = 0.005f;

		// Token: 0x04000D61 RID: 3425
		[SerializeField]
		[HideInInspector]
		private GameObject runtimeGroundVisuals;

		// Token: 0x04000D62 RID: 3426
		[SerializeField]
		[HideInInspector]
		private GameObject runtimeEquippedVisuals;

		// Token: 0x04000D63 RID: 3427
		[SerializeField]
		private VisualEquipmentSlot equipmentSlot;

		// Token: 0x04000D64 RID: 3428
		[SerializeField]
		private InteractableBase interactable;

		// Token: 0x04000D65 RID: 3429
		[SerializeField]
		private Item.InventorySlotType inventorySlot;

		// Token: 0x04000D66 RID: 3430
		[SerializeField]
		protected InventoryUsableDefinition inventoryUsableDefinition;

		// Token: 0x04000D67 RID: 3431
		[SerializeField]
		private string equippedItemParamName = string.Empty;

		// Token: 0x04000D68 RID: 3432
		[SerializeField]
		private int equippedItemID = 1;

		// Token: 0x04000D69 RID: 3433
		[SerializeField]
		private GameObject runtimeGroundedVisualsParentGameObject;

		// Token: 0x04000D6A RID: 3434
		[SerializeField]
		private Rigidbody tossItemRigidbody;

		// Token: 0x04000D6B RID: 3435
		private NetworkVariable<Item.NetState> netState = new NetworkVariable<Item.NetState>(default(Item.NetState), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000D6C RID: 3436
		private global::UnityEngine.Vector3 cachedRigidbodyVelocity;

		// Token: 0x04000D6D RID: 3437
		private bool networkSetup;

		// Token: 0x04000D6E RID: 3438
		protected EndlessScriptComponent scriptComponent;

		// Token: 0x04000D6F RID: 3439
		public EndlessEvent OnPickup = new EndlessEvent();

		// Token: 0x04000D71 RID: 3441
		private Item levelItemsPickedUpCopy;

		// Token: 0x04000D72 RID: 3442
		private bool shown;

		// Token: 0x04000D73 RID: 3443
		private bool isPlaying;

		// Token: 0x04000D74 RID: 3444
		[SerializeField]
		[HideInInspector]
		private SerializableGuid assetID;

		// Token: 0x04000D75 RID: 3445
		private Context context;

		// Token: 0x020002AC RID: 684
		private struct NetState : INetworkSerializable
		{
			// Token: 0x170002F7 RID: 759
			// (get) Token: 0x06000F5D RID: 3933 RVA: 0x000500D8 File Offset: 0x0004E2D8
			public PlayerReferenceManager Carrier
			{
				get
				{
					NetworkObject networkObject;
					if (!this.CarrierObjectReference.TryGet(out networkObject, null))
					{
						return null;
					}
					return networkObject.GetComponent<PlayerReferenceManager>();
				}
			}

			// Token: 0x170002F8 RID: 760
			// (get) Token: 0x06000F5E RID: 3934 RVA: 0x000500FD File Offset: 0x0004E2FD
			public bool Teleporting
			{
				get
				{
					return this.State == Item.State.Teleporting;
				}
			}

			// Token: 0x06000F5F RID: 3935 RVA: 0x00050108 File Offset: 0x0004E308
			public NetState(Item.State state, global::UnityEngine.Vector3 pos)
			{
				this.State = state;
				this.CarrierObjectReference = default(NetworkObjectReference);
				this.Position = pos;
				this.TossDirection = global::UnityEngine.Vector3.zero;
				this.TeleportType = TeleportType.Instant;
				this.TeleportPosition = default(global::UnityEngine.Vector3);
				this.TeleportFrame = 0U;
			}

			// Token: 0x06000F60 RID: 3936 RVA: 0x00050154 File Offset: 0x0004E354
			public NetState(Item.State state, global::UnityEngine.Vector3 pos, global::UnityEngine.Vector3 tossDirection)
			{
				this.State = state;
				this.CarrierObjectReference = default(NetworkObjectReference);
				this.Position = pos;
				this.TossDirection = tossDirection;
				this.TeleportType = TeleportType.Instant;
				this.TeleportPosition = default(global::UnityEngine.Vector3);
				this.TeleportFrame = 0U;
			}

			// Token: 0x06000F61 RID: 3937 RVA: 0x00050194 File Offset: 0x0004E394
			public NetState(Item.State state, NetworkObject carrier = null)
			{
				this.State = state;
				this.CarrierObjectReference = (carrier ? new NetworkObjectReference(carrier) : default(NetworkObjectReference));
				this.Position = global::UnityEngine.Vector3.zero;
				this.TossDirection = global::UnityEngine.Vector3.zero;
				this.TeleportType = TeleportType.Instant;
				this.TeleportPosition = default(global::UnityEngine.Vector3);
				this.TeleportFrame = 0U;
			}

			// Token: 0x06000F62 RID: 3938 RVA: 0x000501F7 File Offset: 0x0004E3F7
			public NetState(TeleportType teleportType, global::UnityEngine.Vector3 currentPosition, global::UnityEngine.Vector3 teleportPosition, uint teleportFrame)
			{
				this.State = Item.State.Teleporting;
				this.Position = currentPosition;
				this.CarrierObjectReference = default(NetworkObjectReference);
				this.TossDirection = global::UnityEngine.Vector3.zero;
				this.TeleportType = teleportType;
				this.TeleportPosition = teleportPosition;
				this.TeleportFrame = teleportFrame;
			}

			// Token: 0x06000F63 RID: 3939 RVA: 0x00050234 File Offset: 0x0004E434
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<Item.State>(ref this.State, default(FastBufferWriter.ForEnums));
				serializer.SerializeValue<NetworkObjectReference>(ref this.CarrierObjectReference, default(FastBufferWriter.ForNetworkSerializable));
				if (this.State == Item.State.Ground)
				{
					serializer.SerializeValue(ref this.Position);
					return;
				}
				if (this.State == Item.State.Tossed)
				{
					serializer.SerializeValue(ref this.Position);
					serializer.SerializeValue(ref this.TossDirection);
					return;
				}
				if (this.State == Item.State.Teleporting)
				{
					serializer.SerializeValue(ref this.Position);
					serializer.SerializeValue<TeleportType>(ref this.TeleportType, default(FastBufferWriter.ForEnums));
					serializer.SerializeValue(ref this.TeleportPosition);
					serializer.SerializeValue<uint>(ref this.TeleportFrame, default(FastBufferWriter.ForPrimitives));
				}
			}

			// Token: 0x06000F64 RID: 3940 RVA: 0x000502F8 File Offset: 0x0004E4F8
			public override string ToString()
			{
				NetworkObject networkObject;
				return string.Format("[ NetState: {0}, Player: {1} | Pos: {2} | Dir: {3} ] ", new object[]
				{
					this.State,
					this.CarrierObjectReference.TryGet(out networkObject, null),
					this.Position,
					this.TossDirection
				});
			}

			// Token: 0x04000D77 RID: 3447
			public Item.State State;

			// Token: 0x04000D78 RID: 3448
			public NetworkObjectReference CarrierObjectReference;

			// Token: 0x04000D79 RID: 3449
			public global::UnityEngine.Vector3 Position;

			// Token: 0x04000D7A RID: 3450
			public global::UnityEngine.Vector3 TossDirection;

			// Token: 0x04000D7B RID: 3451
			public TeleportType TeleportType;

			// Token: 0x04000D7C RID: 3452
			public global::UnityEngine.Vector3 TeleportPosition;

			// Token: 0x04000D7D RID: 3453
			public uint TeleportFrame;

			// Token: 0x020002AD RID: 685
			public struct ThrowInfo
			{
				// Token: 0x04000D7E RID: 3454
				public global::UnityEngine.Vector3 startPosition;

				// Token: 0x04000D7F RID: 3455
				public global::UnityEngine.Vector3 eulerAngles;

				// Token: 0x04000D80 RID: 3456
				public global::UnityEngine.Vector3 groundPosition;
			}
		}

		// Token: 0x020002AE RID: 686
		[Serializable]
		protected struct VisualsInfo
		{
			// Token: 0x04000D81 RID: 3457
			public GameObject GameObject;

			// Token: 0x04000D82 RID: 3458
			public global::UnityEngine.Vector3 Position;

			// Token: 0x04000D83 RID: 3459
			public global::UnityEngine.Vector3 Angles;
		}

		// Token: 0x020002AF RID: 687
		public enum InventorySlotType
		{
			// Token: 0x04000D85 RID: 3461
			Major,
			// Token: 0x04000D86 RID: 3462
			Minor
		}

		// Token: 0x020002B0 RID: 688
		public enum State
		{
			// Token: 0x04000D88 RID: 3464
			Ground,
			// Token: 0x04000D89 RID: 3465
			PickedUp,
			// Token: 0x04000D8A RID: 3466
			Equipped,
			// Token: 0x04000D8B RID: 3467
			Tossed,
			// Token: 0x04000D8C RID: 3468
			Teleporting,
			// Token: 0x04000D8D RID: 3469
			Destroyed
		}
	}
}
