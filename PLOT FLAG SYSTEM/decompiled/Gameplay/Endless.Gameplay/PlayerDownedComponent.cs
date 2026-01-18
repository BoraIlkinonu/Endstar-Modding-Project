using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class PlayerDownedComponent : NetworkBehaviour
{
	private enum DownedAnim : byte
	{
		Alive,
		Down,
		Reviving
	}

	[Serializable]
	public struct ToggleState : INetworkSerializable
	{
		public bool Toggle;

		public uint Frame;

		public bool GetToggled(uint frame)
		{
			if (frame >= Frame)
			{
				return Toggle;
			}
			return !Toggle;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref Toggle, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref Frame, default(FastBufferWriter.ForPrimitives));
		}
	}

	private const uint DOWNED_FRAME_DELAY = 8u;

	[SerializeField]
	private GameplayPlayerReferenceManager playerReferences;

	private NetworkVariable<ToggleState> downedState = new NetworkVariable<ToggleState>();

	private NetworkVariable<ToggleState> revivingState = new NetworkVariable<ToggleState>();

	private NetworkVariable<DownedAnim> anim = new NetworkVariable<DownedAnim>(DownedAnim.Alive);

	public UnityEvent OnDowned = new UnityEvent();

	public override void OnNetworkSpawn()
	{
		if (base.IsServer)
		{
			downedState.Value = new ToggleState
			{
				Toggle = false,
				Frame = 0u
			};
			revivingState.Value = new ToggleState
			{
				Toggle = false,
				Frame = 0u
			};
			playerReferences.HealthComponent.OnHealthChanged.AddListener(HandleHealthChanged);
		}
		NetworkVariable<DownedAnim> networkVariable = anim;
		networkVariable.OnValueChanged = (NetworkVariable<DownedAnim>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<DownedAnim>.OnValueChangedDelegate(HandleAnimationStateChanged));
	}

	private void HandleAnimationStateChanged(DownedAnim oldState, DownedAnim newState)
	{
		playerReferences.ApperanceController.AppearanceAnimator.Animator.SetBool("DBNO", newState != DownedAnim.Alive);
		playerReferences.ApperanceController.AppearanceAnimator.Animator.SetBool("Reviving", newState == DownedAnim.Reviving);
	}

	private void HandleHealthChanged(int previousValue, int newValue)
	{
		if (newValue <= 0 && previousValue > 0)
		{
			if (base.IsServer)
			{
				if (NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(playerReferences.NetworkObject.OwnerClientId, out var userId))
				{
					MonoBehaviourSingleton<StatTracker>.Instance.TrackPlayerDown(userId);
				}
				else
				{
					Debug.LogError("No user id for player!");
				}
			}
			SetDowned(b: true);
		}
		else if (previousValue <= 0)
		{
			Revive();
		}
	}

	public void HandleReviveInteractionCompleted(InteractorBase interactor)
	{
		PlayerReferenceManager component = interactor.GetComponent<PlayerReferenceManager>();
		if (component.HealthComponent.CurrentHealth > 1)
		{
			int num = component.HealthComponent.CurrentHealth / 2;
			if (num > playerReferences.HealthComponent.MaxHealth)
			{
				num = playerReferences.HealthComponent.MaxHealth;
			}
			playerReferences.HittableComponent.ModifyHealth(new HealthModificationArgs(num, component.WorldObject.Context, DamageType.Normal, HealthChangeType.Revive));
			component.HittableComponent.ModifyHealth(new HealthModificationArgs(-num, component.WorldObject.Context, DamageType.Normal, HealthChangeType.Revive));
			if (NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(playerReferences.NetworkObject.OwnerClientId, out var userId))
			{
				MonoBehaviourSingleton<StatTracker>.Instance.TrackRevive(userId);
			}
			else
			{
				Debug.LogError("No user id for player!");
			}
		}
		else
		{
			playerReferences.HittableComponent.ModifyHealth(new HealthModificationArgs(1, component.WorldObject.Context, DamageType.Normal, HealthChangeType.Revive));
		}
		SetReviving(b: false);
		if (base.IsServer)
		{
			Revived_ClientRpc();
		}
	}

	[ClientRpc]
	private void Revived_ClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(3250423172u, clientRpcParams, RpcDelivery.Reliable);
				__endSendClientRpc(ref bufferWriter, 3250423172u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				playerReferences.ApperanceController.AppearanceAnimator.TriggerAnimation("Revived");
			}
		}
	}

	private void Revive()
	{
		SetDowned(b: false);
	}

	private void UpdateAnimData()
	{
		if (base.IsServer)
		{
			anim.Value = (downedState.Value.Toggle ? ((!revivingState.Value.Toggle) ? DownedAnim.Down : DownedAnim.Reviving) : DownedAnim.Alive);
		}
	}

	public void SetDowned(bool b)
	{
		if (base.IsServer && b != downedState.Value.Toggle)
		{
			ToggleState value = downedState.Value;
			value.Toggle = b;
			value.Frame = NetClock.CurrentFrame + 8;
			downedState.Value = value;
			UpdateAnimData();
			OnDowned.Invoke();
		}
	}

	public void SetReviving(bool b)
	{
		if (base.IsServer && b != revivingState.Value.Toggle)
		{
			ToggleState value = revivingState.Value;
			value.Toggle = b;
			value.Frame = NetClock.CurrentFrame + 8;
			revivingState.Value = value;
			UpdateAnimData();
		}
	}

	public bool GetDowned(uint frame)
	{
		return downedState.Value.GetToggled(frame);
	}

	public bool GetReviving(uint frame)
	{
		return revivingState.Value.GetToggled(frame);
	}

	protected override void __initializeVariables()
	{
		if (downedState == null)
		{
			throw new Exception("PlayerDownedComponent.downedState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		downedState.Initialize(this);
		__nameNetworkVariable(downedState, "downedState");
		NetworkVariableFields.Add(downedState);
		if (revivingState == null)
		{
			throw new Exception("PlayerDownedComponent.revivingState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		revivingState.Initialize(this);
		__nameNetworkVariable(revivingState, "revivingState");
		NetworkVariableFields.Add(revivingState);
		if (anim == null)
		{
			throw new Exception("PlayerDownedComponent.anim cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		anim.Initialize(this);
		__nameNetworkVariable(anim, "anim");
		NetworkVariableFields.Add(anim);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3250423172u, __rpc_handler_3250423172, "Revived_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3250423172(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerDownedComponent)target).Revived_ClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "PlayerDownedComponent";
	}
}
