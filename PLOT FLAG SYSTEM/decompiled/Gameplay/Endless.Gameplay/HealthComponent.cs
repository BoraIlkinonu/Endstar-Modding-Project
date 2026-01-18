using System;
using System.Collections;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class HealthComponent : EndlessNetworkBehaviour, IComponentBase, IPersistantStateSubscriber, IScriptInjector
{
	public struct HealthLostData
	{
		public uint frame;

		public NetworkObject damageSource;

		public int damageDelta;

		public bool networked;
	}

	[SerializeField]
	private int startingMaxHealth = 10;

	[SerializeField]
	internal HealthZeroedBehavior healthZeroedBehavior;

	internal readonly NetworkVariable<int> currentHealth = new NetworkVariable<int>(10);

	internal readonly NetworkVariable<int> maxHealth = new NetworkVariable<int>(10);

	private EndlessScriptComponent scriptComponent;

	public EndlessEvent OnDefeated = new EndlessEvent();

	public UnityEvent OnHealthZeroed_Internal = new UnityEvent();

	public UnityEvent<int, int> OnHealthChanged = new UnityEvent<int, int>();

	public UnityEvent<int, int> OnMaxHealthChanged = new UnityEvent<int, int>();

	public UnityEvent<HealthLostData> OnHealthLost = new UnityEvent<HealthLostData>();

	public UnityEvent<int> OnHealthGained = new UnityEvent<int>();

	private uint client_latest_damage_flinch;

	private Health luaInterface;

	public int MaxHealth => maxHealth.Value;

	public int CurrentHealth
	{
		get
		{
			return currentHealth.Value;
		}
		private set
		{
			currentHealth.Value = Mathf.Clamp(value, 0, MaxHealth);
		}
	}

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public bool ShouldSaveAndLoad { get; set; } = true;

	public object LuaObject => luaInterface ?? (luaInterface = new Health(this));

	public Type LuaObjectType => typeof(Health);

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsServer)
		{
			maxHealth.Value = startingMaxHealth;
			currentHealth.Value = startingMaxHealth;
		}
		NetworkVariable<int> networkVariable = currentHealth;
		networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(HandleHealthChanged));
		NetworkVariable<int> networkVariable2 = maxHealth;
		networkVariable2.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(HandleMaxHealthChanged));
	}

	public void SetHealthZeroedBehaviour(HealthZeroedBehavior targetHealthZeroedBehavior)
	{
		healthZeroedBehavior = targetHealthZeroedBehavior;
	}

	public HealthChangeResult ModifyHealth(HealthModificationArgs args)
	{
		if (currentHealth.Value > 0 && args.Delta < 0 && args.HealthChangeType != HealthChangeType.HealthChanged)
		{
			DamageReaction_Local(args.Source, args.Delta);
		}
		if (!base.IsServer)
		{
			return HealthChangeResult.NoChange;
		}
		int num = CurrentHealth;
		CurrentHealth += args.Delta;
		if (num != CurrentHealth)
		{
			luaInterface?.OnHealthChanged.InvokeEvent(WorldObject.Context, CurrentHealth);
		}
		int delta = args.Delta;
		if (delta <= 0)
		{
			if (delta < 0)
			{
				if (num <= 0)
				{
					return HealthChangeResult.NoChange;
				}
				if (CurrentHealth <= 0)
				{
					OnHealthZeroed_Internal.Invoke();
					luaInterface?.OnDefeated.InvokeEvent(args.Source);
					OnDefeated.Invoke(args.Source);
					return HealthChangeResult.HealthZeroed;
				}
				return HealthChangeResult.DamagedHealth;
			}
			return HealthChangeResult.NoChange;
		}
		if (num <= 0)
		{
			return HealthChangeResult.Revived;
		}
		if (num == CurrentHealth)
		{
			return HealthChangeResult.NoChange;
		}
		return HealthChangeResult.Healed;
	}

	private void DamageReaction_Local(Context damageSource, int damageDelta)
	{
		client_latest_damage_flinch = NetClock.CurrentSimulationFrame;
		OnHealthLost.Invoke(new HealthLostData
		{
			frame = NetClock.CurrentFrame,
			damageSource = damageSource?.WorldObject?.NetworkObject,
			damageDelta = damageDelta,
			networked = false
		});
		if (base.IsServer)
		{
			if ((bool)damageSource?.WorldObject?.NetworkObject)
			{
				DamageReactionClientRPC(NetClock.CurrentFrame, damageSource?.WorldObject?.NetworkObject, damageDelta);
			}
			else
			{
				DamageReactionClientRPC(NetClock.CurrentFrame, damageDelta);
			}
		}
	}

	[ClientRpc]
	private void DamageReactionClientRPC(uint frame, NetworkObjectReference damageSourceNetObj, int damageDelta)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(703732775u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, frame);
			bufferWriter.WriteValueSafe(in damageSourceNetObj, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, damageDelta);
			__endSendClientRpc(ref bufferWriter, 703732775u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer && (!damageSourceNetObj.TryGet(out var networkObject) || !networkObject.IsOwner) && frame > client_latest_damage_flinch)
			{
				InvokeOnHealthLost(frame, damageSourceNetObj, damageDelta);
			}
		}
	}

	private void InvokeOnHealthLost(uint frame, NetworkObject damageSourceNetObj, int damageDelta)
	{
		OnHealthLost.Invoke(new HealthLostData
		{
			frame = frame,
			damageSource = damageSourceNetObj,
			damageDelta = damageDelta,
			networked = true
		});
	}

	[ClientRpc]
	private void DamageReactionClientRPC(uint frame, int damageDelta)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(4169615286u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, frame);
			BytePacker.WriteValueBitPacked(bufferWriter, damageDelta);
			__endSendClientRpc(ref bufferWriter, 4169615286u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				InvokeOnHealthLost(frame, null, damageDelta);
			}
		}
	}

	public void SetHealth(int newValue)
	{
		CurrentHealth = newValue;
	}

	public void SetMaxHealth(Context context, int newValue)
	{
		newValue = ((!(WorldObject.BaseType is PlayerLuaComponent)) ? Mathf.Max(newValue, 1) : Mathf.Clamp(newValue, 1, 20));
		int num = newValue - maxHealth.Value;
		maxHealth.Value = newValue;
		if (num < 0)
		{
			if (MaxHealth < CurrentHealth)
			{
				HealthModificationArgs args = new HealthModificationArgs(-(CurrentHealth - MaxHealth), context, DamageType.Normal, HealthChangeType.HealthChanged);
				ModifyHealth(args);
			}
		}
		else
		{
			HealthModificationArgs args2 = new HealthModificationArgs(num, context, DamageType.Normal, HealthChangeType.HealthChanged);
			ModifyHealth(args2);
		}
	}

	public void ModifyMaxHealth(int delta, Context context)
	{
		SetMaxHealth(context, MaxHealth - delta);
	}

	private void HandleHealthChanged(int previousValue, int newValue)
	{
		object[] returnValues;
		if (base.IsServer && newValue == 0 && previousValue > 0)
		{
			switch (healthZeroedBehavior)
			{
			case HealthZeroedBehavior.Destroy:
				UnityEngine.Object.Destroy(WorldObject.gameObject);
				break;
			case HealthZeroedBehavior.Custom:
				if ((bool)scriptComponent)
				{
					scriptComponent.TryExecuteFunction("OnHealthZeroed", out returnValues);
				}
				break;
			}
		}
		if ((bool)scriptComponent)
		{
			scriptComponent.TryExecuteFunction("OnHealthChanged", out returnValues, previousValue, newValue);
		}
		if (newValue > previousValue)
		{
			OnHealthGained.Invoke(newValue - previousValue);
		}
		OnHealthChanged.Invoke(previousValue, newValue);
	}

	private void HandleMaxHealthChanged(int previousValue, int newValue)
	{
		OnMaxHealthChanged.Invoke(previousValue, newValue);
	}

	private void Destroy(float delay)
	{
		StartCoroutine(DestroyRoutine(delay));
	}

	private IEnumerator DestroyRoutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public object GetSaveState()
	{
		return (MaxHealth, CurrentHealth);
	}

	public void LoadState(object loadedState)
	{
		if (base.IsServer && loadedState != null)
		{
			(int, int) tuple = ((int, int))loadedState;
			CurrentHealth = tuple.Item2;
			maxHealth.Value = tuple.Item1;
		}
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		if (currentHealth == null)
		{
			throw new Exception("HealthComponent.currentHealth cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		currentHealth.Initialize(this);
		__nameNetworkVariable(currentHealth, "currentHealth");
		NetworkVariableFields.Add(currentHealth);
		if (maxHealth == null)
		{
			throw new Exception("HealthComponent.maxHealth cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		maxHealth.Initialize(this);
		__nameNetworkVariable(maxHealth, "maxHealth");
		NetworkVariableFields.Add(maxHealth);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(703732775u, __rpc_handler_703732775, "DamageReactionClientRPC");
		__registerRpc(4169615286u, __rpc_handler_4169615286, "DamageReactionClientRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_703732775(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			reader.ReadValueSafe(out NetworkObjectReference value2, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out int value3);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((HealthComponent)target).DamageReactionClientRPC(value, value2, value3);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4169615286(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((HealthComponent)target).DamageReactionClientRPC(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "HealthComponent";
	}
}
