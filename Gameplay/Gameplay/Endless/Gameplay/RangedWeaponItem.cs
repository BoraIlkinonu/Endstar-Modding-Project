using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Audio;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002B7 RID: 695
	public class RangedWeaponItem : Item, ILateUpdateSubscriber, IScriptInjector
	{
		// Token: 0x17000305 RID: 773
		// (get) Token: 0x06000F90 RID: 3984 RVA: 0x0005072F File Offset: 0x0004E92F
		protected override Item.VisualsInfo GroundVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoGround;
			}
		}

		// Token: 0x17000306 RID: 774
		// (get) Token: 0x06000F91 RID: 3985 RVA: 0x00050737 File Offset: 0x0004E937
		protected override Item.VisualsInfo EquippedVisualsInfo
		{
			get
			{
				return this.tempVisualsInfoEqupped;
			}
		}

		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000F92 RID: 3986 RVA: 0x0005073F File Offset: 0x0004E93F
		public ProjectileShooter ProjectileShooter
		{
			get
			{
				return this.projectileShooter;
			}
		}

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06000F93 RID: 3987 RVA: 0x00050747 File Offset: 0x0004E947
		public CrosshairBase Crosshair
		{
			get
			{
				return this.crosshair;
			}
		}

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06000F94 RID: 3988 RVA: 0x00050750 File Offset: 0x0004E950
		// (set) Token: 0x06000F95 RID: 3989 RVA: 0x000507A2 File Offset: 0x0004E9A2
		public int AmmoCount
		{
			get
			{
				if (base.IsServer)
				{
					return this.ammoCount.Value;
				}
				if (this.lastShotTime < NetworkManager.Singleton.ServerTime.Time)
				{
					this.clientLocalAmmoCount = this.ammoCount.Value;
				}
				return this.clientLocalAmmoCount;
			}
			set
			{
				if (base.IsServer)
				{
					this.ammoCount.Value = value;
					this.ammoCount.SetDirty(true);
				}
			}
		}

		// Token: 0x1700030A RID: 778
		// (get) Token: 0x06000F96 RID: 3990 RVA: 0x000507C4 File Offset: 0x0004E9C4
		public bool HasAmmo
		{
			get
			{
				return this.AmmoCount != 0;
			}
		}

		// Token: 0x1700030B RID: 779
		// (get) Token: 0x06000F97 RID: 3991 RVA: 0x000507CF File Offset: 0x0004E9CF
		public bool IsFull
		{
			get
			{
				return this.AmmoCount == this.rangedDefinition.AmmoCount;
			}
		}

		// Token: 0x1700030C RID: 780
		// (get) Token: 0x06000F98 RID: 3992 RVA: 0x000507E4 File Offset: 0x0004E9E4
		// (set) Token: 0x06000F99 RID: 3993 RVA: 0x000507EC File Offset: 0x0004E9EC
		public uint ReloadFrame { get; protected set; }

		// Token: 0x1700030D RID: 781
		// (get) Token: 0x06000F9A RID: 3994 RVA: 0x000507F5 File Offset: 0x0004E9F5
		// (set) Token: 0x06000F9B RID: 3995 RVA: 0x000507FD File Offset: 0x0004E9FD
		public bool ReloadStarted { get; protected set; }

		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06000F9C RID: 3996 RVA: 0x00050806 File Offset: 0x0004EA06
		public bool ReloadRequested
		{
			get
			{
				return this.reloadRequested;
			}
		}

		// Token: 0x1700030F RID: 783
		// (get) Token: 0x06000F9D RID: 3997 RVA: 0x0005080E File Offset: 0x0004EA0E
		public float CurrentRecoilAccumulation
		{
			get
			{
				return this.currentFiringRecoil + this.currentMovePenaltyRecoil;
			}
		}

		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06000F9E RID: 3998 RVA: 0x0005081D File Offset: 0x0004EA1D
		// (set) Token: 0x06000F9F RID: 3999 RVA: 0x00050825 File Offset: 0x0004EA25
		public uint ServerFinishFrame { get; private set; }

		// Token: 0x06000FA0 RID: 4000 RVA: 0x00050830 File Offset: 0x0004EA30
		[ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Unreliable)]
		public void UpdateFinishFrame_ServerRpc(uint frame, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2905700668U, serverRpcParams, RpcDelivery.Unreliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, frame);
				base.__endSendServerRpc(ref fastBufferWriter, 2905700668U, serverRpcParams, RpcDelivery.Unreliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (base.Carrier && base.Carrier.OwnerClientId == serverRpcParams.Receive.SenderClientId && frame > this.ServerFinishFrame)
			{
				this.ServerFinishFrame = frame;
			}
		}

		// Token: 0x06000FA1 RID: 4001 RVA: 0x00050945 File Offset: 0x0004EB45
		public bool IsLocal()
		{
			return this.player != null && this.player == PlayerReferenceManager.LocalInstance;
		}

		// Token: 0x06000FA2 RID: 4002 RVA: 0x00050968 File Offset: 0x0004EB68
		void ILateUpdateSubscriber.EndlessLateUpdate()
		{
			if (base.IsServer && this.ReloadStarted && NetClock.CurrentFrame > this.ReloadFrame)
			{
				this.FinishReload();
			}
			this.recoilSettleDelay = Mathf.MoveTowards(this.recoilSettleDelay, 0f, Time.deltaTime);
			if (this.currentLinearSpread > 0f && this.recoilSettleDelay <= 0f)
			{
				this.currentLinearSpread = Mathf.MoveTowards(this.currentLinearSpread, 0f, this.rangedDefinition.RecoilSettleAmount * Time.deltaTime);
				float num = this.rangedDefinition.RecoilSettleCurve.Evaluate(1f - this.currentLinearSpread / this.lastNewSpread);
				this.currentFiringRecoil = Mathf.Lerp(this.lastNewSpread, 0f, num);
				if (Mathf.Approximately(this.currentLinearSpread, 0f))
				{
					this.currentLinearSpread = 0f;
					this.lastNewSpread = 0f;
					this.currentFiringRecoil = 0f;
				}
			}
			float num2 = 0f;
			if (this.player != null)
			{
				num2 = this.player.PlayerController.LastFrameMoveSpeedPercent;
				if (num2 > 0f)
				{
					this.lastFrameMoved = NetClock.CurrentFrame;
				}
			}
			bool flag = NetClock.CurrentFrame - this.lastFrameMoved < 5U;
			this.currentMovePenaltyRecoil = Mathf.MoveTowards(this.currentMovePenaltyRecoil, flag ? (this.rangedDefinition.MovementAimPenalty * num2) : 0f, this.movePenaltyInterpSpeed * Time.deltaTime);
		}

		// Token: 0x06000FA3 RID: 4003 RVA: 0x00050AE4 File Offset: 0x0004ECE4
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			this.Setup();
			NetworkVariable<int> networkVariable = this.ammoCount;
			networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(this.OnAmmoCountChanged));
		}

		// Token: 0x06000FA4 RID: 4004 RVA: 0x00050B19 File Offset: 0x0004ED19
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			this.ReloadStarted = false;
			this.ReloadFrame = 0U;
			this.ammoCount.OnValueChanged = null;
		}

		// Token: 0x06000FA5 RID: 4005 RVA: 0x00050B3B File Offset: 0x0004ED3B
		private void OnAmmoCountChanged(int previousCount, int newCount)
		{
			if ((this.ReloadStarted || this.reloadRequested) && newCount >= previousCount)
			{
				this.FinishReload();
			}
			if (this.IsLocal())
			{
				CrosshairUI.Instance.SetHasAmmo(newCount > 0);
			}
		}

		// Token: 0x06000FA6 RID: 4006 RVA: 0x00050B70 File Offset: 0x0004ED70
		public override void CopyToItem(Item item)
		{
			base.CopyToItem(item);
			RangedWeaponItem rangedWeaponItem = item as RangedWeaponItem;
			rangedWeaponItem.rangedDefinition = rangedWeaponItem.inventoryUsableDefinition as RangedAttackUsableDefinition;
			rangedWeaponItem.AmmoCount = this.AmmoCount;
			rangedWeaponItem.ReloadFrame = this.ReloadFrame;
			rangedWeaponItem.ReloadStarted = false;
			rangedWeaponItem.isSetup = this.isSetup;
		}

		// Token: 0x06000FA7 RID: 4007 RVA: 0x00050BC8 File Offset: 0x0004EDC8
		public void Setup()
		{
			if (!this.isSetup)
			{
				this.isSetup = true;
				this.reloadRequested = false;
				this.rangedDefinition = this.inventoryUsableDefinition as RangedAttackUsableDefinition;
				if (this.rangedDefinition != null)
				{
					this.Refill();
				}
				if (base.NetworkManager.IsClient)
				{
					this.clientShootingRingBuffer = new RingBuffer<ShootingState>(10);
				}
			}
		}

		// Token: 0x06000FA8 RID: 4008 RVA: 0x00050C2A File Offset: 0x0004EE2A
		public void Refill()
		{
			this.AmmoCount = this.rangedDefinition.AmmoCount;
		}

		// Token: 0x06000FA9 RID: 4009 RVA: 0x00050C40 File Offset: 0x0004EE40
		public void FireShot(int count = 1)
		{
			this.lastShotTime = NetworkManager.Singleton.LocalTime.Time;
			this.clientLocalAmmoCount--;
			this.currentFiringRecoil = Mathf.Min(this.currentFiringRecoil + this.rangedDefinition.RecoilAmount * this.rangedDefinition.WeaponStrength / this.rangedDefinition.WeaponAccuracy, this.rangedDefinition.MaxRecoil);
			this.currentLinearSpread = (this.lastNewSpread = this.currentFiringRecoil);
			this.recoilSettleDelay = this.rangedDefinition.RecoilSettleDelay;
			if (this.IsLocal())
			{
				float weaponStrength = this.rangedDefinition.WeaponStrength;
				CrosshairUI.Instance.ApplySpread(this.rangedDefinition.RecoilAmount, weaponStrength, this.rangedDefinition.MaxRecoil / this.rangedDefinition.RecoilAmount, this.rangedDefinition.RecoilSettleAmount / this.rangedDefinition.RecoilAmount, this.recoilSettleDelay);
				MonoBehaviourSingleton<CameraController>.Instance.OnShotFired(this.rangedDefinition.CameraClimbAmount, this.rangedDefinition.MaxCameraClimb, this.rangedDefinition.CameraClimbSettleAmount, this.recoilSettleDelay);
			}
			this.PlayFireAudio();
		}

		// Token: 0x06000FAA RID: 4010 RVA: 0x00050D74 File Offset: 0x0004EF74
		private void PlayFireAudio()
		{
			this.fireAudioGroup.SpawnAndPlayWithManagedPool(this, this.audioSourcePrefab, this.projectileShooter.FirePoint.position, default(Quaternion));
		}

		// Token: 0x06000FAB RID: 4011 RVA: 0x00050DAC File Offset: 0x0004EFAC
		public void StartReload()
		{
			if (base.IsClient)
			{
				if (!this.reloadRequested && (this.IsLocal() || base.IsServer) && (this.rangedDefinition.AllowReloadWhenFullAmmo || this.AmmoCount < this.rangedDefinition.AmmoCount))
				{
					this.reloadRequested = true;
					this.StartReload_ServerRpc(NetClock.CurrentFrame);
					CrosshairUI.Instance.StartReload();
					if (this.player != null)
					{
						this.player.ApperanceController.AppearanceAnimator.TriggerAnimation("Reload");
						return;
					}
				}
			}
			else
			{
				uint frameFromTime = NetClock.GetFrameFromTime(NetClock.LocalNetworkTime + (double)this.rangedDefinition.ReloadTime);
				this.StartReload_ClientRpc(frameFromTime);
			}
		}

		// Token: 0x06000FAC RID: 4012 RVA: 0x00050E68 File Offset: 0x0004F068
		[ServerRpc(RequireOwnership = false)]
		private void StartReload_ServerRpc(uint startFrame)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3025399645U, serverRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, startFrame);
				base.__endSendServerRpc(ref fastBufferWriter, 3025399645U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!this.ReloadStarted)
			{
				this.serverShootingState.waitingForShot = false;
				uint num = NetClock.CurrentFrame - startFrame;
				uint num2 = NetClock.GetFrameFromTime(NetClock.LocalNetworkTime + (double)this.rangedDefinition.ReloadTime) - num;
				this.ReloadFrame = num2;
				this.ReloadStarted = true;
				this.StartReload_ClientRpc(num2);
			}
		}

		// Token: 0x06000FAD RID: 4013 RVA: 0x00050F94 File Offset: 0x0004F194
		[ClientRpc]
		private void StartReload_ClientRpc(uint finishFrame)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2724129946U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, finishFrame);
				base.__endSendClientRpc(ref fastBufferWriter, 2724129946U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.IsServer)
			{
				this.ReloadFrame = finishFrame;
				this.ReloadStarted = true;
			}
			if (!this.reloadRequested)
			{
				if (this.player != null)
				{
					this.player.ApperanceController.AppearanceAnimator.TriggerAnimation("Reload");
				}
				if (this.IsLocal())
				{
					CrosshairUI.Instance.StartReload();
				}
			}
			this.reloadRequested = false;
		}

		// Token: 0x06000FAE RID: 4014 RVA: 0x000510D3 File Offset: 0x0004F2D3
		private void FinishReload()
		{
			this.reloadRequested = false;
			this.ReloadStarted = false;
			this.Refill();
			if (this.IsLocal())
			{
				CrosshairUI.Instance.SetHasAmmo(this.HasAmmo);
				CrosshairUI.Instance.FinishReload();
			}
		}

		// Token: 0x06000FAF RID: 4015 RVA: 0x0005110B File Offset: 0x0004F30B
		public void ServerCancelReload()
		{
			this.ReloadFrame = 0U;
			this.ReloadStarted = false;
			this.CancelReload_ClientRpc();
		}

		// Token: 0x06000FB0 RID: 4016 RVA: 0x00051124 File Offset: 0x0004F324
		[ClientRpc]
		public void CancelReload_ClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1199079781U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 1199079781U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.reloadRequested = false;
			this.ReloadFrame = 0U;
			this.ReloadStarted = false;
			if (this.IsLocal())
			{
				CrosshairUI.Instance.FinishReload();
			}
			if (this.player != null)
			{
				this.player.ApperanceController.AppearanceAnimator.TriggerAnimation("CancelReload");
			}
		}

		// Token: 0x06000FB1 RID: 4017 RVA: 0x00051246 File Offset: 0x0004F446
		public void RegisterAmmoCountHandler(NetworkVariable<int>.OnValueChangedDelegate handler)
		{
			NetworkVariable<int> networkVariable = this.ammoCount;
			networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, handler);
		}

		// Token: 0x06000FB2 RID: 4018 RVA: 0x00051264 File Offset: 0x0004F464
		public void RemoveAmmoCountHandler(NetworkVariable<int>.OnValueChangedDelegate handler)
		{
			NetworkVariable<int> networkVariable = this.ammoCount;
			networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(networkVariable.OnValueChanged, handler);
		}

		// Token: 0x06000FB3 RID: 4019 RVA: 0x00051284 File Offset: 0x0004F484
		protected override void HandleOnEquipped(PlayerReferenceManager player)
		{
			this.player = player;
			if (player.IsOwner)
			{
				CrosshairUI.Instance.CreateCrosshair(this.crosshair, new CrosshairSettings
				{
					maxSpread = this.rangedDefinition.MaxRecoil / this.rangedDefinition.RecoilAmount,
					resetSpeed = this.rangedDefinition.RecoilSettleAmount / this.rangedDefinition.RecoilAmount,
					weaponStrength = this.rangedDefinition.WeaponStrength,
					weaponAccuracy = this.rangedDefinition.WeaponAccuracy,
					movementPenalty = this.rangedDefinition.MovementAimPenalty / this.rangedDefinition.RecoilAmount,
					recoilSettleCurve = this.rangedDefinition.RecoilSettleCurve
				}, false);
				CrosshairUI.Instance.SetHasAmmo(this.HasAmmo);
			}
			this.movePenaltyInterpSpeed = this.rangedDefinition.MovementAimPenalty / this.rangedDefinition.RecoilAmount * 4f;
			if (this.magEjectEvent != null)
			{
				player.ApperanceController.AppearanceAnimator.RegisterAnimationEventCallback(this.magEjectEvent, new Action<AnimationEventReference>(this.OnAnimationEvent));
			}
			if (this.magAttachEvent != null)
			{
				player.ApperanceController.AppearanceAnimator.RegisterAnimationEventCallback(this.magAttachEvent, new Action<AnimationEventReference>(this.OnAnimationEvent));
			}
		}

		// Token: 0x06000FB4 RID: 4020 RVA: 0x000513E0 File Offset: 0x0004F5E0
		protected override void HandleOnUnequipped(PlayerReferenceManager player)
		{
			if (this.player == player)
			{
				this.player = null;
			}
			if (player.IsOwner)
			{
				CrosshairUI.Instance.DestroyCrosshair();
			}
			if (this.magEjectEvent != null)
			{
				player.ApperanceController.AppearanceAnimator.RemoveAnimationEventCallback(this.magEjectEvent, new Action<AnimationEventReference>(this.OnAnimationEvent));
			}
			if (this.magAttachEvent != null)
			{
				player.ApperanceController.AppearanceAnimator.RemoveAnimationEventCallback(this.magAttachEvent, new Action<AnimationEventReference>(this.OnAnimationEvent));
			}
			if (this.ReloadStarted)
			{
				this.CancelReload_ClientRpc();
			}
		}

		// Token: 0x06000FB5 RID: 4021 RVA: 0x00051484 File Offset: 0x0004F684
		private void OnAnimationEvent(AnimationEventReference reference)
		{
			if (reference == this.magEjectEvent)
			{
				if (this.projectileShooter.Magazine != null)
				{
					this.projectileShooter.Magazine.gameObject.SetActive(false);
					this.projectileShooter.SpawnMagazine();
					return;
				}
			}
			else if (reference == this.magAttachEvent && this.projectileShooter.Magazine != null)
			{
				this.projectileShooter.Magazine.gameObject.SetActive(true);
			}
		}

		// Token: 0x06000FB6 RID: 4022 RVA: 0x0005150B File Offset: 0x0004F70B
		public void ShotAnimTriggered()
		{
			if (this.player != null)
			{
				this.player.ApperanceController.AppearanceAnimator.TriggerAnimation("Attack");
			}
		}

		// Token: 0x06000FB7 RID: 4023 RVA: 0x00051535 File Offset: 0x0004F735
		public ShootingState GetShootingState(uint frame)
		{
			if (base.NetworkManager.IsServer)
			{
				return this.serverShootingState;
			}
			return this.clientShootingRingBuffer.GetValue(frame);
		}

		// Token: 0x06000FB8 RID: 4024 RVA: 0x00051557 File Offset: 0x0004F757
		public void ShootingStateUpdated(uint netFrame, ref ShootingState state)
		{
			if (base.NetworkManager.IsServer)
			{
				this.serverShootingState = state;
				return;
			}
			this.clientShootingRingBuffer.UpdateValue(ref state);
		}

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06000FB9 RID: 4025 RVA: 0x0005157F File Offset: 0x0004F77F
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new RangedWeapon(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06000FBA RID: 4026 RVA: 0x0005159B File Offset: 0x0004F79B
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(RangedWeapon);
			}
		}

		// Token: 0x06000FBB RID: 4027 RVA: 0x0004EE71 File Offset: 0x0004D071
		void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06000FBD RID: 4029 RVA: 0x000515C0 File Offset: 0x0004F7C0
		protected override void __initializeVariables()
		{
			bool flag = this.ammoCount == null;
			if (flag)
			{
				throw new Exception("RangedWeaponItem.ammoCount cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.ammoCount.Initialize(this);
			base.__nameNetworkVariable(this.ammoCount, "ammoCount");
			this.NetworkVariableFields.Add(this.ammoCount);
			base.__initializeVariables();
		}

		// Token: 0x06000FBE RID: 4030 RVA: 0x00051624 File Offset: 0x0004F824
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2905700668U, new NetworkBehaviour.RpcReceiveHandler(RangedWeaponItem.__rpc_handler_2905700668), "UpdateFinishFrame_ServerRpc");
			base.__registerRpc(3025399645U, new NetworkBehaviour.RpcReceiveHandler(RangedWeaponItem.__rpc_handler_3025399645), "StartReload_ServerRpc");
			base.__registerRpc(2724129946U, new NetworkBehaviour.RpcReceiveHandler(RangedWeaponItem.__rpc_handler_2724129946), "StartReload_ClientRpc");
			base.__registerRpc(1199079781U, new NetworkBehaviour.RpcReceiveHandler(RangedWeaponItem.__rpc_handler_1199079781), "CancelReload_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000FBF RID: 4031 RVA: 0x000516AC File Offset: 0x0004F8AC
		private static void __rpc_handler_2905700668(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((RangedWeaponItem)target).UpdateFinishFrame_ServerRpc(num, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FC0 RID: 4032 RVA: 0x0005171C File Offset: 0x0004F91C
		private static void __rpc_handler_3025399645(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((RangedWeaponItem)target).StartReload_ServerRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FC1 RID: 4033 RVA: 0x00051780 File Offset: 0x0004F980
		private static void __rpc_handler_2724129946(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((RangedWeaponItem)target).StartReload_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FC2 RID: 4034 RVA: 0x000517E4 File Offset: 0x0004F9E4
		private static void __rpc_handler_1199079781(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((RangedWeaponItem)target).CancelReload_ClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000FC3 RID: 4035 RVA: 0x00051835 File Offset: 0x0004FA35
		protected internal override string __getTypeName()
		{
			return "RangedWeaponItem";
		}

		// Token: 0x04000D99 RID: 3481
		[SerializeField]
		protected ProjectileShooter projectileShooter;

		// Token: 0x04000D9A RID: 3482
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoGround;

		// Token: 0x04000D9B RID: 3483
		[SerializeField]
		private Item.VisualsInfo tempVisualsInfoEqupped;

		// Token: 0x04000D9C RID: 3484
		[SerializeField]
		private AnimationEventReference magEjectEvent;

		// Token: 0x04000D9D RID: 3485
		[SerializeField]
		private AnimationEventReference magAttachEvent;

		// Token: 0x04000D9E RID: 3486
		[SerializeField]
		private CrosshairBase crosshair;

		// Token: 0x04000D9F RID: 3487
		[Header("Audio")]
		[SerializeField]
		private PoolableAudioSource audioSourcePrefab;

		// Token: 0x04000DA0 RID: 3488
		[SerializeField]
		private AudioGroup fireAudioGroup;

		// Token: 0x04000DA1 RID: 3489
		private double lastShotTime;

		// Token: 0x04000DA2 RID: 3490
		private int clientLocalAmmoCount;

		// Token: 0x04000DA5 RID: 3493
		private bool isSetup;

		// Token: 0x04000DA6 RID: 3494
		private NetworkVariable<int> ammoCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

		// Token: 0x04000DA7 RID: 3495
		[NonSerialized]
		private RangedAttackUsableDefinition rangedDefinition;

		// Token: 0x04000DA8 RID: 3496
		[NonSerialized]
		private PlayerReferenceManager player;

		// Token: 0x04000DA9 RID: 3497
		[NonSerialized]
		private float currentLinearSpread;

		// Token: 0x04000DAA RID: 3498
		[NonSerialized]
		private float lastNewSpread;

		// Token: 0x04000DAB RID: 3499
		[NonSerialized]
		private float currentFiringRecoil;

		// Token: 0x04000DAC RID: 3500
		[NonSerialized]
		private float currentMovePenaltyRecoil;

		// Token: 0x04000DAD RID: 3501
		[NonSerialized]
		private float movePenaltyInterpSpeed;

		// Token: 0x04000DAE RID: 3502
		[NonSerialized]
		private uint lastFrameMoved;

		// Token: 0x04000DAF RID: 3503
		[NonSerialized]
		private bool reloadRequested;

		// Token: 0x04000DB0 RID: 3504
		[NonSerialized]
		private float recoilSettleDelay;

		// Token: 0x04000DB1 RID: 3505
		[NonSerialized]
		private RingBuffer<ShootingState> clientShootingRingBuffer;

		// Token: 0x04000DB2 RID: 3506
		[NonSerialized]
		public ShootingState serverShootingState;

		// Token: 0x04000DB3 RID: 3507
		[SerializeField]
		[HideInInspector]
		public Transform offhandPlacement;

		// Token: 0x04000DB5 RID: 3509
		private RangedWeapon luaInterface;
	}
}
