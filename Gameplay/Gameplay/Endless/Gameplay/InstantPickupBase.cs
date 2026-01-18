using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000322 RID: 802
	public abstract class InstantPickupBase : EndlessNetworkBehaviour, NetClock.ISimulateFrameEnvironmentSubscriber, IPersistantStateSubscriber, IStartSubscriber, IGameEndSubscriber
	{
		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x06001298 RID: 4760 RVA: 0x0005BC47 File Offset: 0x00059E47
		// (set) Token: 0x06001299 RID: 4761 RVA: 0x0005BC4F File Offset: 0x00059E4F
		public bool AllowPickupWhileDowned { get; set; }

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x0600129A RID: 4762 RVA: 0x0005BC58 File Offset: 0x00059E58
		// (set) Token: 0x0600129B RID: 4763 RVA: 0x0005BC60 File Offset: 0x00059E60
		public PickupFilter CurrentPickupFilter { get; set; }

		// Token: 0x0600129C RID: 4764 RVA: 0x0005BC69 File Offset: 0x00059E69
		private void OnEnable()
		{
			MonoBehaviourSingleton<RigidbodyManager>.Instance.AddListener(new UnityAction(this.HandleDisableRigidbodySimulation), new UnityAction(this.HandleEnableRigidbodySimulation));
		}

		// Token: 0x0600129D RID: 4765 RVA: 0x0005BC8D File Offset: 0x00059E8D
		private void OnDisable()
		{
			MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveListener(new UnityAction(this.HandleDisableRigidbodySimulation), new UnityAction(this.HandleEnableRigidbodySimulation));
		}

		// Token: 0x0600129E RID: 4766 RVA: 0x0005BCB4 File Offset: 0x00059EB4
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (!base.IsServer)
			{
				if (this.collected.Value)
				{
					this.SetActive(false);
				}
				NetworkVariable<bool> networkVariable = this.collected;
				networkVariable.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(this.HandleCollected));
			}
			else
			{
				this.netState.Value = new InstantPickupBase.NetState(InstantPickupBase.State.Ground, this.tossRigidbody.transform.parent.position);
			}
			NetworkVariable<InstantPickupBase.NetState> networkVariable2 = this.netState;
			networkVariable2.OnValueChanged = (NetworkVariable<InstantPickupBase.NetState>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<InstantPickupBase.NetState>.OnValueChangedDelegate(this.HandleNetStateChanged));
		}

		// Token: 0x0600129F RID: 4767
		protected abstract void SetActive(bool active);

		// Token: 0x060012A0 RID: 4768 RVA: 0x0005BD59 File Offset: 0x00059F59
		private void HandleCollected(bool previousValue, bool newValue)
		{
			this.SetActive(!newValue);
		}

		// Token: 0x060012A1 RID: 4769 RVA: 0x0005BD68 File Offset: 0x00059F68
		private bool CanBePickedUp(WorldCollidable worldCollidable)
		{
			if (!worldCollidable.WorldObject)
			{
				return false;
			}
			NpcEntity npcEntity = null;
			PlayerLuaComponent playerLuaComponent = null;
			bool flag = this.CurrentPickupFilter != PickupFilter.Players && worldCollidable.WorldObject.TryGetUserComponent<NpcEntity>(out npcEntity);
			bool flag2 = !flag && worldCollidable.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent);
			if (this.AllowPickupWhileDowned)
			{
				switch (this.CurrentPickupFilter)
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
				switch (this.CurrentPickupFilter)
				{
				case PickupFilter.Players:
					return flag2 && playerLuaComponent.References.HealthComponent.CurrentHealth > 0;
				case PickupFilter.Characters:
					return (flag2 && playerLuaComponent.References.HealthComponent.CurrentHealth > 0) || (flag && !npcEntity.IsDowned);
				case PickupFilter.Anything:
					return true;
				}
			}
			return false;
		}

		// Token: 0x060012A2 RID: 4770 RVA: 0x0005BE44 File Offset: 0x0005A044
		private void HandlePickup(WorldCollidable worldCollidable, bool isRollbackFrame)
		{
			if (base.IsServer && !this.collected.Value && this.CanBePickedUp(worldCollidable) && this.ExternalAttemptPickup(worldCollidable.WorldObject.Context))
			{
				this.collected.Value = true;
				this.ApplyPickupResult(worldCollidable.WorldObject);
				this.BroadcastPickupResult(worldCollidable.WorldObject.Context);
				this.ShowCollectedFX_ClientRPC();
				this.SetActive(false);
			}
		}

		// Token: 0x060012A3 RID: 4771 RVA: 0x00017586 File Offset: 0x00015786
		protected virtual bool ExternalAttemptPickup(Context context)
		{
			return true;
		}

		// Token: 0x060012A4 RID: 4772 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void ApplyPickupResult(WorldObject worldObject)
		{
		}

		// Token: 0x060012A5 RID: 4773 RVA: 0x0005BEB8 File Offset: 0x0005A0B8
		private void BroadcastPickupResult(Context context)
		{
			this.OnCollected.Invoke(context);
		}

		// Token: 0x060012A6 RID: 4774 RVA: 0x0005BEC8 File Offset: 0x0005A0C8
		[ClientRpc]
		private void ShowCollectedFX_ClientRPC()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3456888992U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 3456888992U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.ShowCollectedFX();
		}

		// Token: 0x060012A7 RID: 4775
		protected abstract void ShowCollectedFX();

		// Token: 0x060012A8 RID: 4776 RVA: 0x0005BFA1 File Offset: 0x0005A1A1
		public void Respawn()
		{
			if (this.collected.Value)
			{
				this.collected.Value = false;
				this.SetActive(true);
			}
		}

		// Token: 0x060012A9 RID: 4777 RVA: 0x0005BFC3 File Offset: 0x0005A1C3
		public void ForceCollect()
		{
			if (!this.collected.Value)
			{
				this.collected.Value = true;
				this.SetActive(false);
			}
		}

		// Token: 0x060012AA RID: 4778 RVA: 0x0005BFE5 File Offset: 0x0005A1E5
		public void TriggerTeleport(global::UnityEngine.Vector3 position, TeleportType teleportType)
		{
			this.netState.Value = new InstantPickupBase.NetState(teleportType, base.NetworkObject.transform.position, position, NetClock.CurrentFrame + RuntimeDatabase.GetTeleportInfo(teleportType).FramesToTeleport);
		}

		// Token: 0x060012AB RID: 4779 RVA: 0x0005C01C File Offset: 0x0005A21C
		public void SimulateFrameEnvironment(uint frame)
		{
			if (this.netState.Value.Teleporting && frame == this.netState.Value.TeleportFrame)
			{
				base.transform.position = this.netState.Value.TeleportPosition;
				if (base.IsServer)
				{
					this.netState.Value = new InstantPickupBase.NetState(InstantPickupBase.State.Ground, this.netState.Value.TeleportPosition);
				}
			}
		}

		// Token: 0x060012AC RID: 4780 RVA: 0x0005C098 File Offset: 0x0005A298
		private void FixedUpdate()
		{
			if (!base.IsSpawned || !this.netStateInitialized)
			{
				base.transform.localPosition = global::UnityEngine.Vector3.zero;
				return;
			}
			if (base.IsServer && this.netState.Value.State == InstantPickupBase.State.Tossed)
			{
				if (this.tossRigidbody.velocity.magnitude < 0.005f)
				{
					this.netState.Value = new InstantPickupBase.NetState(InstantPickupBase.State.Ground, base.transform.position);
					return;
				}
				if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && this.tossRigidbody.position.y < MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageResetHeight)
				{
					base.NetworkObject.Despawn(true);
				}
			}
		}

		// Token: 0x060012AD RID: 4781 RVA: 0x0005C15C File Offset: 0x0005A35C
		private void HandleNetStateChanged(InstantPickupBase.NetState oldState, InstantPickupBase.NetState newState)
		{
			this.netStateInitialized = true;
			if (newState.State == InstantPickupBase.State.Tossed)
			{
				this.tossRigidbody.position = newState.Position;
				this.tossRigidbody.isKinematic = false;
				this.tossRigidbody.rotation = Quaternion.identity;
				this.tossRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				this.tossRigidbody.velocity = global::UnityEngine.Vector3.Lerp(newState.TossDirection, global::UnityEngine.Vector3.up, 0.6f) * 6f / this.tossRigidbody.mass;
			}
			else
			{
				this.tossRigidbody.isKinematic = true;
				this.tossRigidbody.interpolation = RigidbodyInterpolation.None;
				this.tossRigidbody.position = newState.Position;
			}
			if (oldState.State != InstantPickupBase.State.Teleporting && newState.State == InstantPickupBase.State.Teleporting)
			{
				RuntimeDatabase.GetTeleportInfo(newState.TeleportType).TeleportStart(base.GetComponent<WorldObject>().EndlessVisuals, null, newState.Position);
				return;
			}
			if (oldState.State == InstantPickupBase.State.Teleporting && newState.State != InstantPickupBase.State.Teleporting)
			{
				RuntimeDatabase.GetTeleportInfo(oldState.TeleportType).TeleportEnd(base.GetComponent<WorldObject>().EndlessVisuals, null, newState.Position);
			}
		}

		// Token: 0x060012AE RID: 4782 RVA: 0x0005C280 File Offset: 0x0005A480
		private void HandleDisableRigidbodySimulation()
		{
			this.cachedRigidbodyVelocity = this.tossRigidbody.velocity;
			this.tossRigidbody.isKinematic = true;
		}

		// Token: 0x060012AF RID: 4783 RVA: 0x0005C29F File Offset: 0x0005A49F
		private void HandleEnableRigidbodySimulation()
		{
			if (this.netState.Value.State == InstantPickupBase.State.Tossed)
			{
				this.tossRigidbody.isKinematic = false;
				this.tossRigidbody.velocity = this.cachedRigidbodyVelocity;
				return;
			}
			this.tossRigidbody.isKinematic = true;
		}

		// Token: 0x060012B0 RID: 4784 RVA: 0x0005C2E0 File Offset: 0x0005A4E0
		public void InitializeNewItem(bool launch, global::UnityEngine.Vector3 position, float rotation)
		{
			base.NetworkObject.Spawn(false);
			if (launch)
			{
				this.netState.Value = new InstantPickupBase.NetState(InstantPickupBase.State.Tossed, position, Quaternion.Euler(0f, rotation, 0f) * global::UnityEngine.Vector3.forward);
				return;
			}
			this.netState.Value = new InstantPickupBase.NetState(InstantPickupBase.State.Ground, position);
		}

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x060012B1 RID: 4785 RVA: 0x0005C33B File Offset: 0x0005A53B
		// (set) Token: 0x060012B2 RID: 4786 RVA: 0x0005C343 File Offset: 0x0005A543
		public bool ShouldSaveAndLoad { get; set; } = true;

		// Token: 0x060012B3 RID: 4787 RVA: 0x0005C34C File Offset: 0x0005A54C
		public object GetSaveState()
		{
			return this.collected.Value;
		}

		// Token: 0x060012B4 RID: 4788 RVA: 0x0005C35E File Offset: 0x0005A55E
		public void LoadState(object loadedState)
		{
			if (base.IsServer && loadedState != null)
			{
				this.collected.Value = (bool)loadedState;
				this.SetActive(!this.collected.Value);
			}
		}

		// Token: 0x060012B5 RID: 4789 RVA: 0x0005C390 File Offset: 0x0005A590
		public void EndlessStart()
		{
			this.worldTrigger.OnTriggerEnter.AddListener(new UnityAction<WorldCollidable, bool>(this.HandlePickup));
		}

		// Token: 0x060012B6 RID: 4790 RVA: 0x0005C3AE File Offset: 0x0005A5AE
		public void EndlessGameEnd()
		{
			this.worldTrigger.OnTriggerEnter.RemoveListener(new UnityAction<WorldCollidable, bool>(this.HandlePickup));
			this.SetActive(true);
			if (base.IsServer)
			{
				this.collected.Value = false;
			}
		}

		// Token: 0x060012B8 RID: 4792 RVA: 0x0005C434 File Offset: 0x0005A634
		protected override void __initializeVariables()
		{
			bool flag = this.collected == null;
			if (flag)
			{
				throw new Exception("InstantPickupBase.collected cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.collected.Initialize(this);
			base.__nameNetworkVariable(this.collected, "collected");
			this.NetworkVariableFields.Add(this.collected);
			flag = this.netState == null;
			if (flag)
			{
				throw new Exception("InstantPickupBase.netState cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.netState.Initialize(this);
			base.__nameNetworkVariable(this.netState, "netState");
			this.NetworkVariableFields.Add(this.netState);
			base.__initializeVariables();
		}

		// Token: 0x060012B9 RID: 4793 RVA: 0x0005C4E4 File Offset: 0x0005A6E4
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3456888992U, new NetworkBehaviour.RpcReceiveHandler(InstantPickupBase.__rpc_handler_3456888992), "ShowCollectedFX_ClientRPC");
			base.__initializeRpcs();
		}

		// Token: 0x060012BA RID: 4794 RVA: 0x0005C50C File Offset: 0x0005A70C
		private static void __rpc_handler_3456888992(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InstantPickupBase)target).ShowCollectedFX_ClientRPC();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060012BB RID: 4795 RVA: 0x0005C55D File Offset: 0x0005A75D
		protected internal override string __getTypeName()
		{
			return "InstantPickupBase";
		}

		// Token: 0x04000FEC RID: 4076
		[SerializeField]
		protected WorldTrigger worldTrigger;

		// Token: 0x04000FED RID: 4077
		[SerializeField]
		private Rigidbody tossRigidbody;

		// Token: 0x04000FEE RID: 4078
		private NetworkVariable<bool> collected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000FEF RID: 4079
		private NetworkVariable<InstantPickupBase.NetState> netState = new NetworkVariable<InstantPickupBase.NetState>(default(InstantPickupBase.NetState), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000FF0 RID: 4080
		public EndlessEvent OnCollected = new EndlessEvent();

		// Token: 0x04000FF3 RID: 4083
		private global::UnityEngine.Vector3 cachedRigidbodyVelocity;

		// Token: 0x04000FF4 RID: 4084
		private bool netStateInitialized;

		// Token: 0x02000323 RID: 803
		private struct NetState : INetworkSerializable
		{
			// Token: 0x170003B6 RID: 950
			// (get) Token: 0x060012BC RID: 4796 RVA: 0x0005C564 File Offset: 0x0005A764
			public bool Teleporting
			{
				get
				{
					return this.State == InstantPickupBase.State.Teleporting;
				}
			}

			// Token: 0x060012BD RID: 4797 RVA: 0x0005C56F File Offset: 0x0005A76F
			public NetState(InstantPickupBase.State state, global::UnityEngine.Vector3 pos)
			{
				this.State = state;
				this.Position = pos;
				this.TossDirection = global::UnityEngine.Vector3.zero;
				this.TeleportType = TeleportType.Instant;
				this.TeleportPosition = default(global::UnityEngine.Vector3);
				this.TeleportFrame = 0U;
			}

			// Token: 0x060012BE RID: 4798 RVA: 0x0005C5A4 File Offset: 0x0005A7A4
			public NetState(InstantPickupBase.State state, global::UnityEngine.Vector3 pos, global::UnityEngine.Vector3 tossDirection)
			{
				this.State = state;
				this.Position = pos;
				this.TossDirection = tossDirection;
				this.TeleportType = TeleportType.Instant;
				this.TeleportPosition = default(global::UnityEngine.Vector3);
				this.TeleportFrame = 0U;
			}

			// Token: 0x060012BF RID: 4799 RVA: 0x0005C5D5 File Offset: 0x0005A7D5
			public NetState(TeleportType teleportType, global::UnityEngine.Vector3 currentPosition, global::UnityEngine.Vector3 teleportPosition, uint teleportFrame)
			{
				this.State = InstantPickupBase.State.Teleporting;
				this.Position = currentPosition;
				this.TossDirection = global::UnityEngine.Vector3.zero;
				this.TeleportType = teleportType;
				this.TeleportPosition = teleportPosition;
				this.TeleportFrame = teleportFrame;
			}

			// Token: 0x060012C0 RID: 4800 RVA: 0x0005C608 File Offset: 0x0005A808
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<InstantPickupBase.State>(ref this.State, default(FastBufferWriter.ForEnums));
				if (this.State == InstantPickupBase.State.Ground)
				{
					serializer.SerializeValue(ref this.Position);
					return;
				}
				if (this.State == InstantPickupBase.State.Tossed)
				{
					serializer.SerializeValue(ref this.Position);
					serializer.SerializeValue(ref this.TossDirection);
					return;
				}
				if (this.State == InstantPickupBase.State.Teleporting)
				{
					serializer.SerializeValue(ref this.Position);
					serializer.SerializeValue<TeleportType>(ref this.TeleportType, default(FastBufferWriter.ForEnums));
					serializer.SerializeValue(ref this.TeleportPosition);
					serializer.SerializeValue<uint>(ref this.TeleportFrame, default(FastBufferWriter.ForPrimitives));
				}
			}

			// Token: 0x04000FF6 RID: 4086
			public InstantPickupBase.State State;

			// Token: 0x04000FF7 RID: 4087
			public global::UnityEngine.Vector3 Position;

			// Token: 0x04000FF8 RID: 4088
			public global::UnityEngine.Vector3 TossDirection;

			// Token: 0x04000FF9 RID: 4089
			public TeleportType TeleportType;

			// Token: 0x04000FFA RID: 4090
			public global::UnityEngine.Vector3 TeleportPosition;

			// Token: 0x04000FFB RID: 4091
			public uint TeleportFrame;
		}

		// Token: 0x02000324 RID: 804
		private enum State
		{
			// Token: 0x04000FFD RID: 4093
			Ground,
			// Token: 0x04000FFE RID: 4094
			Tossed,
			// Token: 0x04000FFF RID: 4095
			Teleporting
		}
	}
}
