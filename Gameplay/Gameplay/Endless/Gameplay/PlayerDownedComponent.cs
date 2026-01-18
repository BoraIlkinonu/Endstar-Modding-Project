using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000270 RID: 624
	public class PlayerDownedComponent : NetworkBehaviour
	{
		// Token: 0x06000D22 RID: 3362 RVA: 0x00047AE4 File Offset: 0x00045CE4
		public override void OnNetworkSpawn()
		{
			if (base.IsServer)
			{
				this.downedState.Value = new PlayerDownedComponent.ToggleState
				{
					Toggle = false,
					Frame = 0U
				};
				this.revivingState.Value = new PlayerDownedComponent.ToggleState
				{
					Toggle = false,
					Frame = 0U
				};
				this.playerReferences.HealthComponent.OnHealthChanged.AddListener(new UnityAction<int, int>(this.HandleHealthChanged));
			}
			NetworkVariable<PlayerDownedComponent.DownedAnim> networkVariable = this.anim;
			networkVariable.OnValueChanged = (NetworkVariable<PlayerDownedComponent.DownedAnim>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<PlayerDownedComponent.DownedAnim>.OnValueChangedDelegate(this.HandleAnimationStateChanged));
		}

		// Token: 0x06000D23 RID: 3363 RVA: 0x00047B8C File Offset: 0x00045D8C
		private void HandleAnimationStateChanged(PlayerDownedComponent.DownedAnim oldState, PlayerDownedComponent.DownedAnim newState)
		{
			this.playerReferences.ApperanceController.AppearanceAnimator.Animator.SetBool("DBNO", newState > PlayerDownedComponent.DownedAnim.Alive);
			this.playerReferences.ApperanceController.AppearanceAnimator.Animator.SetBool("Reviving", newState == PlayerDownedComponent.DownedAnim.Reviving);
		}

		// Token: 0x06000D24 RID: 3364 RVA: 0x00047BE0 File Offset: 0x00045DE0
		private void HandleHealthChanged(int previousValue, int newValue)
		{
			if (newValue <= 0 && previousValue > 0)
			{
				if (base.IsServer)
				{
					int num;
					if (NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(this.playerReferences.NetworkObject.OwnerClientId, out num))
					{
						MonoBehaviourSingleton<StatTracker>.Instance.TrackPlayerDown(num);
					}
					else
					{
						Debug.LogError("No user id for player!");
					}
				}
				this.SetDowned(true);
				return;
			}
			if (previousValue <= 0)
			{
				this.Revive();
			}
		}

		// Token: 0x06000D25 RID: 3365 RVA: 0x00047C44 File Offset: 0x00045E44
		public void HandleReviveInteractionCompleted(InteractorBase interactor)
		{
			PlayerReferenceManager component = interactor.GetComponent<PlayerReferenceManager>();
			if (component.HealthComponent.CurrentHealth > 1)
			{
				int num = component.HealthComponent.CurrentHealth / 2;
				if (num > this.playerReferences.HealthComponent.MaxHealth)
				{
					num = this.playerReferences.HealthComponent.MaxHealth;
				}
				this.playerReferences.HittableComponent.ModifyHealth(new HealthModificationArgs(num, component.WorldObject.Context, DamageType.Normal, HealthChangeType.Revive));
				component.HittableComponent.ModifyHealth(new HealthModificationArgs(-num, component.WorldObject.Context, DamageType.Normal, HealthChangeType.Revive));
				int num2;
				if (NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(this.playerReferences.NetworkObject.OwnerClientId, out num2))
				{
					MonoBehaviourSingleton<StatTracker>.Instance.TrackRevive(num2);
				}
				else
				{
					Debug.LogError("No user id for player!");
				}
			}
			else
			{
				this.playerReferences.HittableComponent.ModifyHealth(new HealthModificationArgs(1, component.WorldObject.Context, DamageType.Normal, HealthChangeType.Revive));
			}
			this.SetReviving(false);
			if (base.IsServer)
			{
				this.Revived_ClientRpc();
			}
		}

		// Token: 0x06000D26 RID: 3366 RVA: 0x00047D50 File Offset: 0x00045F50
		[ClientRpc]
		private void Revived_ClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3250423172U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 3250423172U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.playerReferences.ApperanceController.AppearanceAnimator.TriggerAnimation("Revived");
		}

		// Token: 0x06000D27 RID: 3367 RVA: 0x00047E3D File Offset: 0x0004603D
		private void Revive()
		{
			this.SetDowned(false);
		}

		// Token: 0x06000D28 RID: 3368 RVA: 0x00047E48 File Offset: 0x00046048
		private void UpdateAnimData()
		{
			if (!base.IsServer)
			{
				return;
			}
			this.anim.Value = ((!this.downedState.Value.Toggle) ? PlayerDownedComponent.DownedAnim.Alive : (this.revivingState.Value.Toggle ? PlayerDownedComponent.DownedAnim.Reviving : PlayerDownedComponent.DownedAnim.Down));
		}

		// Token: 0x06000D29 RID: 3369 RVA: 0x00047E94 File Offset: 0x00046094
		public void SetDowned(bool b)
		{
			if (!base.IsServer || b == this.downedState.Value.Toggle)
			{
				return;
			}
			PlayerDownedComponent.ToggleState value = this.downedState.Value;
			value.Toggle = b;
			value.Frame = NetClock.CurrentFrame + 8U;
			this.downedState.Value = value;
			this.UpdateAnimData();
			this.OnDowned.Invoke();
		}

		// Token: 0x06000D2A RID: 3370 RVA: 0x00047EFC File Offset: 0x000460FC
		public void SetReviving(bool b)
		{
			if (!base.IsServer || b == this.revivingState.Value.Toggle)
			{
				return;
			}
			PlayerDownedComponent.ToggleState value = this.revivingState.Value;
			value.Toggle = b;
			value.Frame = NetClock.CurrentFrame + 8U;
			this.revivingState.Value = value;
			this.UpdateAnimData();
		}

		// Token: 0x06000D2B RID: 3371 RVA: 0x00047F5C File Offset: 0x0004615C
		public bool GetDowned(uint frame)
		{
			return this.downedState.Value.GetToggled(frame);
		}

		// Token: 0x06000D2C RID: 3372 RVA: 0x00047F80 File Offset: 0x00046180
		public bool GetReviving(uint frame)
		{
			return this.revivingState.Value.GetToggled(frame);
		}

		// Token: 0x06000D2E RID: 3374 RVA: 0x00047FFC File Offset: 0x000461FC
		protected override void __initializeVariables()
		{
			bool flag = this.downedState == null;
			if (flag)
			{
				throw new Exception("PlayerDownedComponent.downedState cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.downedState.Initialize(this);
			base.__nameNetworkVariable(this.downedState, "downedState");
			this.NetworkVariableFields.Add(this.downedState);
			flag = this.revivingState == null;
			if (flag)
			{
				throw new Exception("PlayerDownedComponent.revivingState cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.revivingState.Initialize(this);
			base.__nameNetworkVariable(this.revivingState, "revivingState");
			this.NetworkVariableFields.Add(this.revivingState);
			flag = this.anim == null;
			if (flag)
			{
				throw new Exception("PlayerDownedComponent.anim cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.anim.Initialize(this);
			base.__nameNetworkVariable(this.anim, "anim");
			this.NetworkVariableFields.Add(this.anim);
			base.__initializeVariables();
		}

		// Token: 0x06000D2F RID: 3375 RVA: 0x000480F9 File Offset: 0x000462F9
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3250423172U, new NetworkBehaviour.RpcReceiveHandler(PlayerDownedComponent.__rpc_handler_3250423172), "Revived_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000D30 RID: 3376 RVA: 0x00048120 File Offset: 0x00046320
		private static void __rpc_handler_3250423172(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((PlayerDownedComponent)target).Revived_ClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000D31 RID: 3377 RVA: 0x00048171 File Offset: 0x00046371
		protected internal override string __getTypeName()
		{
			return "PlayerDownedComponent";
		}

		// Token: 0x04000C27 RID: 3111
		private const uint DOWNED_FRAME_DELAY = 8U;

		// Token: 0x04000C28 RID: 3112
		[SerializeField]
		private GameplayPlayerReferenceManager playerReferences;

		// Token: 0x04000C29 RID: 3113
		private NetworkVariable<PlayerDownedComponent.ToggleState> downedState = new NetworkVariable<PlayerDownedComponent.ToggleState>(default(PlayerDownedComponent.ToggleState), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000C2A RID: 3114
		private NetworkVariable<PlayerDownedComponent.ToggleState> revivingState = new NetworkVariable<PlayerDownedComponent.ToggleState>(default(PlayerDownedComponent.ToggleState), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000C2B RID: 3115
		private NetworkVariable<PlayerDownedComponent.DownedAnim> anim = new NetworkVariable<PlayerDownedComponent.DownedAnim>(PlayerDownedComponent.DownedAnim.Alive, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000C2C RID: 3116
		public UnityEvent OnDowned = new UnityEvent();

		// Token: 0x02000271 RID: 625
		private enum DownedAnim : byte
		{
			// Token: 0x04000C2E RID: 3118
			Alive,
			// Token: 0x04000C2F RID: 3119
			Down,
			// Token: 0x04000C30 RID: 3120
			Reviving
		}

		// Token: 0x02000272 RID: 626
		[Serializable]
		public struct ToggleState : INetworkSerializable
		{
			// Token: 0x06000D32 RID: 3378 RVA: 0x00048178 File Offset: 0x00046378
			public bool GetToggled(uint frame)
			{
				if (frame >= this.Frame)
				{
					return this.Toggle;
				}
				return !this.Toggle;
			}

			// Token: 0x06000D33 RID: 3379 RVA: 0x00048194 File Offset: 0x00046394
			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue<bool>(ref this.Toggle, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<uint>(ref this.Frame, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x04000C31 RID: 3121
			public bool Toggle;

			// Token: 0x04000C32 RID: 3122
			public uint Frame;
		}
	}
}
