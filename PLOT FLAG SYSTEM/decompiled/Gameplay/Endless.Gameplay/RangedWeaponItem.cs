using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Audio;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class RangedWeaponItem : Item, ILateUpdateSubscriber, IScriptInjector
{
	[SerializeField]
	protected ProjectileShooter projectileShooter;

	[SerializeField]
	private VisualsInfo tempVisualsInfoGround;

	[SerializeField]
	private VisualsInfo tempVisualsInfoEqupped;

	[SerializeField]
	private AnimationEventReference magEjectEvent;

	[SerializeField]
	private AnimationEventReference magAttachEvent;

	[SerializeField]
	private CrosshairBase crosshair;

	[Header("Audio")]
	[SerializeField]
	private PoolableAudioSource audioSourcePrefab;

	[SerializeField]
	private AudioGroup fireAudioGroup;

	private double lastShotTime;

	private int clientLocalAmmoCount;

	private bool isSetup;

	private NetworkVariable<int> ammoCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	[NonSerialized]
	private RangedAttackUsableDefinition rangedDefinition;

	[NonSerialized]
	private PlayerReferenceManager player;

	[NonSerialized]
	private float currentLinearSpread;

	[NonSerialized]
	private float lastNewSpread;

	[NonSerialized]
	private float currentFiringRecoil;

	[NonSerialized]
	private float currentMovePenaltyRecoil;

	[NonSerialized]
	private float movePenaltyInterpSpeed;

	[NonSerialized]
	private uint lastFrameMoved;

	[NonSerialized]
	private bool reloadRequested;

	[NonSerialized]
	private float recoilSettleDelay;

	[NonSerialized]
	private RingBuffer<ShootingState> clientShootingRingBuffer;

	[NonSerialized]
	public ShootingState serverShootingState;

	[SerializeField]
	[HideInInspector]
	public Transform offhandPlacement;

	private RangedWeapon luaInterface;

	protected override VisualsInfo GroundVisualsInfo => tempVisualsInfoGround;

	protected override VisualsInfo EquippedVisualsInfo => tempVisualsInfoEqupped;

	public ProjectileShooter ProjectileShooter => projectileShooter;

	public CrosshairBase Crosshair => crosshair;

	public int AmmoCount
	{
		get
		{
			if (base.IsServer)
			{
				return ammoCount.Value;
			}
			if (lastShotTime < NetworkManager.Singleton.ServerTime.Time)
			{
				clientLocalAmmoCount = ammoCount.Value;
			}
			return clientLocalAmmoCount;
		}
		set
		{
			if (base.IsServer)
			{
				ammoCount.Value = value;
				ammoCount.SetDirty(isDirty: true);
			}
		}
	}

	public bool HasAmmo => AmmoCount != 0;

	public bool IsFull => AmmoCount == rangedDefinition.AmmoCount;

	public uint ReloadFrame { get; protected set; }

	public bool ReloadStarted { get; protected set; }

	public bool ReloadRequested => reloadRequested;

	public float CurrentRecoilAccumulation => currentFiringRecoil + currentMovePenaltyRecoil;

	public uint ServerFinishFrame { get; private set; }

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new RangedWeapon(this);
			}
			return luaInterface;
		}
	}

	Type IScriptInjector.LuaObjectType => typeof(RangedWeapon);

	[ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Unreliable)]
	public void UpdateFinishFrame_ServerRpc(uint frame, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(2905700668u, serverRpcParams, RpcDelivery.Unreliable);
			BytePacker.WriteValueBitPacked(bufferWriter, frame);
			__endSendServerRpc(ref bufferWriter, 2905700668u, serverRpcParams, RpcDelivery.Unreliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((bool)base.Carrier && base.Carrier.OwnerClientId == serverRpcParams.Receive.SenderClientId && frame > ServerFinishFrame)
			{
				ServerFinishFrame = frame;
			}
		}
	}

	public bool IsLocal()
	{
		if (player != null)
		{
			return player == PlayerReferenceManager.LocalInstance;
		}
		return false;
	}

	void ILateUpdateSubscriber.EndlessLateUpdate()
	{
		if (base.IsServer && ReloadStarted && NetClock.CurrentFrame > ReloadFrame)
		{
			FinishReload();
		}
		recoilSettleDelay = Mathf.MoveTowards(recoilSettleDelay, 0f, Time.deltaTime);
		if (currentLinearSpread > 0f && recoilSettleDelay <= 0f)
		{
			currentLinearSpread = Mathf.MoveTowards(currentLinearSpread, 0f, rangedDefinition.RecoilSettleAmount * Time.deltaTime);
			float t = rangedDefinition.RecoilSettleCurve.Evaluate(1f - currentLinearSpread / lastNewSpread);
			currentFiringRecoil = Mathf.Lerp(lastNewSpread, 0f, t);
			if (Mathf.Approximately(currentLinearSpread, 0f))
			{
				currentLinearSpread = 0f;
				lastNewSpread = 0f;
				currentFiringRecoil = 0f;
			}
		}
		float num = 0f;
		if (player != null)
		{
			num = player.PlayerController.LastFrameMoveSpeedPercent;
			if (num > 0f)
			{
				lastFrameMoved = NetClock.CurrentFrame;
			}
		}
		bool flag = NetClock.CurrentFrame - lastFrameMoved < 5;
		currentMovePenaltyRecoil = Mathf.MoveTowards(currentMovePenaltyRecoil, flag ? (rangedDefinition.MovementAimPenalty * num) : 0f, movePenaltyInterpSpeed * Time.deltaTime);
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		Setup();
		NetworkVariable<int> networkVariable = ammoCount;
		networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnAmmoCountChanged));
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		ReloadStarted = false;
		ReloadFrame = 0u;
		ammoCount.OnValueChanged = null;
	}

	private void OnAmmoCountChanged(int previousCount, int newCount)
	{
		if ((ReloadStarted || reloadRequested) && newCount >= previousCount)
		{
			FinishReload();
		}
		if (IsLocal())
		{
			CrosshairUI.Instance.SetHasAmmo(newCount > 0);
		}
	}

	public override void CopyToItem(Item item)
	{
		base.CopyToItem(item);
		RangedWeaponItem obj = item as RangedWeaponItem;
		obj.rangedDefinition = obj.inventoryUsableDefinition as RangedAttackUsableDefinition;
		obj.AmmoCount = AmmoCount;
		obj.ReloadFrame = ReloadFrame;
		obj.ReloadStarted = false;
		obj.isSetup = isSetup;
	}

	public void Setup()
	{
		if (!isSetup)
		{
			isSetup = true;
			reloadRequested = false;
			rangedDefinition = inventoryUsableDefinition as RangedAttackUsableDefinition;
			if (rangedDefinition != null)
			{
				Refill();
			}
			if (base.NetworkManager.IsClient)
			{
				clientShootingRingBuffer = new RingBuffer<ShootingState>(10);
			}
		}
	}

	public void Refill()
	{
		AmmoCount = rangedDefinition.AmmoCount;
	}

	public void FireShot(int count = 1)
	{
		lastShotTime = NetworkManager.Singleton.LocalTime.Time;
		clientLocalAmmoCount--;
		currentFiringRecoil = Mathf.Min(currentFiringRecoil + rangedDefinition.RecoilAmount * rangedDefinition.WeaponStrength / rangedDefinition.WeaponAccuracy, rangedDefinition.MaxRecoil);
		currentLinearSpread = (lastNewSpread = currentFiringRecoil);
		recoilSettleDelay = rangedDefinition.RecoilSettleDelay;
		if (IsLocal())
		{
			float weaponStrength = rangedDefinition.WeaponStrength;
			CrosshairUI.Instance.ApplySpread(rangedDefinition.RecoilAmount, weaponStrength, rangedDefinition.MaxRecoil / rangedDefinition.RecoilAmount, rangedDefinition.RecoilSettleAmount / rangedDefinition.RecoilAmount, recoilSettleDelay);
			MonoBehaviourSingleton<CameraController>.Instance.OnShotFired(rangedDefinition.CameraClimbAmount, rangedDefinition.MaxCameraClimb, rangedDefinition.CameraClimbSettleAmount, recoilSettleDelay);
		}
		PlayFireAudio();
	}

	private void PlayFireAudio()
	{
		fireAudioGroup.SpawnAndPlayWithManagedPool(this, audioSourcePrefab, projectileShooter.FirePoint.position);
	}

	public void StartReload()
	{
		if (base.IsClient)
		{
			if (!reloadRequested && (IsLocal() || base.IsServer) && (rangedDefinition.AllowReloadWhenFullAmmo || AmmoCount < rangedDefinition.AmmoCount))
			{
				reloadRequested = true;
				StartReload_ServerRpc(NetClock.CurrentFrame);
				CrosshairUI.Instance.StartReload();
				if (player != null)
				{
					player.ApperanceController.AppearanceAnimator.TriggerAnimation("Reload");
				}
			}
		}
		else
		{
			uint frameFromTime = NetClock.GetFrameFromTime(NetClock.LocalNetworkTime + (double)rangedDefinition.ReloadTime);
			StartReload_ClientRpc(frameFromTime);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void StartReload_ServerRpc(uint startFrame)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(3025399645u, serverRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, startFrame);
			__endSendServerRpc(ref bufferWriter, 3025399645u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!ReloadStarted)
			{
				serverShootingState.waitingForShot = false;
				uint num = NetClock.CurrentFrame - startFrame;
				uint finishFrame = (ReloadFrame = NetClock.GetFrameFromTime(NetClock.LocalNetworkTime + (double)rangedDefinition.ReloadTime) - num);
				ReloadStarted = true;
				StartReload_ClientRpc(finishFrame);
			}
		}
	}

	[ClientRpc]
	private void StartReload_ClientRpc(uint finishFrame)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2724129946u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, finishFrame);
			__endSendClientRpc(ref bufferWriter, 2724129946u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (!base.IsServer)
		{
			ReloadFrame = finishFrame;
			ReloadStarted = true;
		}
		if (!reloadRequested)
		{
			if (player != null)
			{
				player.ApperanceController.AppearanceAnimator.TriggerAnimation("Reload");
			}
			if (IsLocal())
			{
				CrosshairUI.Instance.StartReload();
			}
		}
		reloadRequested = false;
	}

	private void FinishReload()
	{
		reloadRequested = false;
		ReloadStarted = false;
		Refill();
		if (IsLocal())
		{
			CrosshairUI.Instance.SetHasAmmo(HasAmmo);
			CrosshairUI.Instance.FinishReload();
		}
	}

	public void ServerCancelReload()
	{
		ReloadFrame = 0u;
		ReloadStarted = false;
		CancelReload_ClientRpc();
	}

	[ClientRpc]
	public void CancelReload_ClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1199079781u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 1199079781u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			reloadRequested = false;
			ReloadFrame = 0u;
			ReloadStarted = false;
			if (IsLocal())
			{
				CrosshairUI.Instance.FinishReload();
			}
			if (player != null)
			{
				player.ApperanceController.AppearanceAnimator.TriggerAnimation("CancelReload");
			}
		}
	}

	public void RegisterAmmoCountHandler(NetworkVariable<int>.OnValueChangedDelegate handler)
	{
		NetworkVariable<int> networkVariable = ammoCount;
		networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, handler);
	}

	public void RemoveAmmoCountHandler(NetworkVariable<int>.OnValueChangedDelegate handler)
	{
		NetworkVariable<int> networkVariable = ammoCount;
		networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(networkVariable.OnValueChanged, handler);
	}

	protected override void HandleOnEquipped(PlayerReferenceManager player)
	{
		this.player = player;
		if (player.IsOwner)
		{
			CrosshairUI.Instance.CreateCrosshair(crosshair, new CrosshairSettings
			{
				maxSpread = rangedDefinition.MaxRecoil / rangedDefinition.RecoilAmount,
				resetSpeed = rangedDefinition.RecoilSettleAmount / rangedDefinition.RecoilAmount,
				weaponStrength = rangedDefinition.WeaponStrength,
				weaponAccuracy = rangedDefinition.WeaponAccuracy,
				movementPenalty = rangedDefinition.MovementAimPenalty / rangedDefinition.RecoilAmount,
				recoilSettleCurve = rangedDefinition.RecoilSettleCurve
			});
			CrosshairUI.Instance.SetHasAmmo(HasAmmo);
		}
		movePenaltyInterpSpeed = rangedDefinition.MovementAimPenalty / rangedDefinition.RecoilAmount * 4f;
		if (magEjectEvent != null)
		{
			player.ApperanceController.AppearanceAnimator.RegisterAnimationEventCallback(magEjectEvent, OnAnimationEvent);
		}
		if (magAttachEvent != null)
		{
			player.ApperanceController.AppearanceAnimator.RegisterAnimationEventCallback(magAttachEvent, OnAnimationEvent);
		}
	}

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
		if (magEjectEvent != null)
		{
			player.ApperanceController.AppearanceAnimator.RemoveAnimationEventCallback(magEjectEvent, OnAnimationEvent);
		}
		if (magAttachEvent != null)
		{
			player.ApperanceController.AppearanceAnimator.RemoveAnimationEventCallback(magAttachEvent, OnAnimationEvent);
		}
		if (ReloadStarted)
		{
			CancelReload_ClientRpc();
		}
	}

	private void OnAnimationEvent(AnimationEventReference reference)
	{
		if (reference == magEjectEvent)
		{
			if (projectileShooter.Magazine != null)
			{
				projectileShooter.Magazine.gameObject.SetActive(value: false);
				projectileShooter.SpawnMagazine();
			}
		}
		else if (reference == magAttachEvent && projectileShooter.Magazine != null)
		{
			projectileShooter.Magazine.gameObject.SetActive(value: true);
		}
	}

	public void ShotAnimTriggered()
	{
		if (player != null)
		{
			player.ApperanceController.AppearanceAnimator.TriggerAnimation("Attack");
		}
	}

	public ShootingState GetShootingState(uint frame)
	{
		if (base.NetworkManager.IsServer)
		{
			return serverShootingState;
		}
		return clientShootingRingBuffer.GetValue(frame);
	}

	public void ShootingStateUpdated(uint netFrame, ref ShootingState state)
	{
		if (base.NetworkManager.IsServer)
		{
			serverShootingState = state;
		}
		else
		{
			clientShootingRingBuffer.UpdateValue(ref state);
		}
	}

	void IScriptInjector.ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		if (ammoCount == null)
		{
			throw new Exception("RangedWeaponItem.ammoCount cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		ammoCount.Initialize(this);
		__nameNetworkVariable(ammoCount, "ammoCount");
		NetworkVariableFields.Add(ammoCount);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2905700668u, __rpc_handler_2905700668, "UpdateFinishFrame_ServerRpc");
		__registerRpc(3025399645u, __rpc_handler_3025399645, "StartReload_ServerRpc");
		__registerRpc(2724129946u, __rpc_handler_2724129946, "StartReload_ClientRpc");
		__registerRpc(1199079781u, __rpc_handler_1199079781, "CancelReload_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2905700668(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((RangedWeaponItem)target).UpdateFinishFrame_ServerRpc(value, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3025399645(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((RangedWeaponItem)target).StartReload_ServerRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2724129946(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((RangedWeaponItem)target).StartReload_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1199079781(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((RangedWeaponItem)target).CancelReload_ClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "RangedWeaponItem";
	}
}
