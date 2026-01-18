using System;
using System.Collections;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x0200034E RID: 846
	public class HealthComponent : EndlessNetworkBehaviour, IComponentBase, IPersistantStateSubscriber, IScriptInjector
	{
		// Token: 0x17000450 RID: 1104
		// (get) Token: 0x060014E8 RID: 5352 RVA: 0x00064643 File Offset: 0x00062843
		public int MaxHealth
		{
			get
			{
				return this.maxHealth.Value;
			}
		}

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x060014E9 RID: 5353 RVA: 0x00064650 File Offset: 0x00062850
		// (set) Token: 0x060014EA RID: 5354 RVA: 0x0006465D File Offset: 0x0006285D
		public int CurrentHealth
		{
			get
			{
				return this.currentHealth.Value;
			}
			private set
			{
				this.currentHealth.Value = Mathf.Clamp(value, 0, this.MaxHealth);
			}
		}

		// Token: 0x060014EB RID: 5355 RVA: 0x00064678 File Offset: 0x00062878
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsServer)
			{
				this.maxHealth.Value = this.startingMaxHealth;
				this.currentHealth.Value = this.startingMaxHealth;
			}
			NetworkVariable<int> networkVariable = this.currentHealth;
			networkVariable.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(this.HandleHealthChanged));
			NetworkVariable<int> networkVariable2 = this.maxHealth;
			networkVariable2.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(this.HandleMaxHealthChanged));
		}

		// Token: 0x060014EC RID: 5356 RVA: 0x00064703 File Offset: 0x00062903
		public void SetHealthZeroedBehaviour(HealthZeroedBehavior targetHealthZeroedBehavior)
		{
			this.healthZeroedBehavior = targetHealthZeroedBehavior;
		}

		// Token: 0x060014ED RID: 5357 RVA: 0x0006470C File Offset: 0x0006290C
		public HealthChangeResult ModifyHealth(HealthModificationArgs args)
		{
			if (this.currentHealth.Value > 0 && args.Delta < 0 && args.HealthChangeType != HealthChangeType.HealthChanged)
			{
				this.DamageReaction_Local(args.Source, args.Delta);
			}
			if (!base.IsServer)
			{
				return HealthChangeResult.NoChange;
			}
			int num = this.CurrentHealth;
			this.CurrentHealth += args.Delta;
			if (num != this.CurrentHealth)
			{
				Health health = this.luaInterface;
				if (health != null)
				{
					health.OnHealthChanged.InvokeEvent(new object[]
					{
						this.WorldObject.Context,
						this.CurrentHealth
					});
				}
			}
			int delta = args.Delta;
			if (delta <= 0)
			{
				if (delta >= 0)
				{
					return HealthChangeResult.NoChange;
				}
				if (num <= 0)
				{
					return HealthChangeResult.NoChange;
				}
				if (this.CurrentHealth <= 0)
				{
					this.OnHealthZeroed_Internal.Invoke();
					Health health2 = this.luaInterface;
					if (health2 != null)
					{
						health2.OnDefeated.InvokeEvent(new object[] { args.Source });
					}
					this.OnDefeated.Invoke(args.Source);
					return HealthChangeResult.HealthZeroed;
				}
				return HealthChangeResult.DamagedHealth;
			}
			else
			{
				if (num <= 0)
				{
					return HealthChangeResult.Revived;
				}
				if (num == this.CurrentHealth)
				{
					return HealthChangeResult.NoChange;
				}
				return HealthChangeResult.Healed;
			}
		}

		// Token: 0x060014EE RID: 5358 RVA: 0x0006482C File Offset: 0x00062A2C
		private void DamageReaction_Local(Context damageSource, int damageDelta)
		{
			this.client_latest_damage_flinch = NetClock.CurrentSimulationFrame;
			UnityEvent<HealthComponent.HealthLostData> onHealthLost = this.OnHealthLost;
			HealthComponent.HealthLostData healthLostData = default(HealthComponent.HealthLostData);
			healthLostData.frame = NetClock.CurrentFrame;
			NetworkObject networkObject;
			if (damageSource == null)
			{
				networkObject = null;
			}
			else
			{
				WorldObject worldObject = damageSource.WorldObject;
				networkObject = ((worldObject != null) ? worldObject.NetworkObject : null);
			}
			healthLostData.damageSource = networkObject;
			healthLostData.damageDelta = damageDelta;
			healthLostData.networked = false;
			onHealthLost.Invoke(healthLostData);
			if (!base.IsServer)
			{
				return;
			}
			global::UnityEngine.Object @object;
			if (damageSource == null)
			{
				@object = null;
			}
			else
			{
				WorldObject worldObject2 = damageSource.WorldObject;
				@object = ((worldObject2 != null) ? worldObject2.NetworkObject : null);
			}
			if (@object)
			{
				uint currentFrame = NetClock.CurrentFrame;
				NetworkObject networkObject2;
				if (damageSource == null)
				{
					networkObject2 = null;
				}
				else
				{
					WorldObject worldObject3 = damageSource.WorldObject;
					networkObject2 = ((worldObject3 != null) ? worldObject3.NetworkObject : null);
				}
				this.DamageReactionClientRPC(currentFrame, networkObject2, damageDelta);
				return;
			}
			this.DamageReactionClientRPC(NetClock.CurrentFrame, damageDelta);
		}

		// Token: 0x060014EF RID: 5359 RVA: 0x000648F4 File Offset: 0x00062AF4
		[ClientRpc]
		private void DamageReactionClientRPC(uint frame, NetworkObjectReference damageSourceNetObj, int damageDelta)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(703732775U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, frame);
				fastBufferWriter.WriteValueSafe<NetworkObjectReference>(in damageSourceNetObj, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, damageDelta);
				base.__endSendClientRpc(ref fastBufferWriter, 703732775U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.IsServer)
			{
				NetworkObject networkObject;
				if ((damageSourceNetObj.TryGet(out networkObject, null) && networkObject.IsOwner) || frame <= this.client_latest_damage_flinch)
				{
					return;
				}
				this.InvokeOnHealthLost(frame, damageSourceNetObj, damageDelta);
			}
		}

		// Token: 0x060014F0 RID: 5360 RVA: 0x00064A30 File Offset: 0x00062C30
		private void InvokeOnHealthLost(uint frame, NetworkObject damageSourceNetObj, int damageDelta)
		{
			this.OnHealthLost.Invoke(new HealthComponent.HealthLostData
			{
				frame = frame,
				damageSource = damageSourceNetObj,
				damageDelta = damageDelta,
				networked = true
			});
		}

		// Token: 0x060014F1 RID: 5361 RVA: 0x00064A74 File Offset: 0x00062C74
		[ClientRpc]
		private void DamageReactionClientRPC(uint frame, int damageDelta)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(4169615286U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, frame);
				BytePacker.WriteValueBitPacked(fastBufferWriter, damageDelta);
				base.__endSendClientRpc(ref fastBufferWriter, 4169615286U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.IsServer)
			{
				this.InvokeOnHealthLost(frame, null, damageDelta);
			}
		}

		// Token: 0x060014F2 RID: 5362 RVA: 0x00064B72 File Offset: 0x00062D72
		public void SetHealth(int newValue)
		{
			this.CurrentHealth = newValue;
		}

		// Token: 0x060014F3 RID: 5363 RVA: 0x00064B7C File Offset: 0x00062D7C
		public void SetMaxHealth(Context context, int newValue)
		{
			if (this.WorldObject.BaseType is PlayerLuaComponent)
			{
				newValue = Mathf.Clamp(newValue, 1, 20);
			}
			else
			{
				newValue = Mathf.Max(newValue, 1);
			}
			int num = newValue - this.maxHealth.Value;
			this.maxHealth.Value = newValue;
			if (num < 0)
			{
				if (this.MaxHealth < this.CurrentHealth)
				{
					HealthModificationArgs healthModificationArgs = new HealthModificationArgs(-(this.CurrentHealth - this.MaxHealth), context, DamageType.Normal, HealthChangeType.HealthChanged);
					this.ModifyHealth(healthModificationArgs);
					return;
				}
			}
			else
			{
				HealthModificationArgs healthModificationArgs2 = new HealthModificationArgs(num, context, DamageType.Normal, HealthChangeType.HealthChanged);
				this.ModifyHealth(healthModificationArgs2);
			}
		}

		// Token: 0x060014F4 RID: 5364 RVA: 0x00064C11 File Offset: 0x00062E11
		public void ModifyMaxHealth(int delta, Context context)
		{
			this.SetMaxHealth(context, this.MaxHealth - delta);
		}

		// Token: 0x060014F5 RID: 5365 RVA: 0x00064C24 File Offset: 0x00062E24
		private void HandleHealthChanged(int previousValue, int newValue)
		{
			if (base.IsServer && newValue == 0 && previousValue > 0)
			{
				HealthZeroedBehavior healthZeroedBehavior = this.healthZeroedBehavior;
				if (healthZeroedBehavior != HealthZeroedBehavior.Destroy)
				{
					if (healthZeroedBehavior == HealthZeroedBehavior.Custom)
					{
						if (this.scriptComponent)
						{
							object[] array;
							this.scriptComponent.TryExecuteFunction("OnHealthZeroed", out array, Array.Empty<object>());
						}
					}
				}
				else
				{
					global::UnityEngine.Object.Destroy(this.WorldObject.gameObject);
				}
			}
			if (this.scriptComponent)
			{
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnHealthChanged", out array, new object[] { previousValue, newValue });
			}
			if (newValue > previousValue)
			{
				this.OnHealthGained.Invoke(newValue - previousValue);
			}
			this.OnHealthChanged.Invoke(previousValue, newValue);
		}

		// Token: 0x060014F6 RID: 5366 RVA: 0x00064CDE File Offset: 0x00062EDE
		private void HandleMaxHealthChanged(int previousValue, int newValue)
		{
			this.OnMaxHealthChanged.Invoke(previousValue, newValue);
		}

		// Token: 0x060014F7 RID: 5367 RVA: 0x00064CED File Offset: 0x00062EED
		private void Destroy(float delay)
		{
			base.StartCoroutine(this.DestroyRoutine(delay));
		}

		// Token: 0x060014F8 RID: 5368 RVA: 0x00064CFD File Offset: 0x00062EFD
		private IEnumerator DestroyRoutine(float delay)
		{
			yield return new WaitForSeconds(delay);
			global::UnityEngine.Object.Destroy(base.gameObject);
			yield break;
		}

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x060014F9 RID: 5369 RVA: 0x00064D13 File Offset: 0x00062F13
		// (set) Token: 0x060014FA RID: 5370 RVA: 0x00064D1B File Offset: 0x00062F1B
		public WorldObject WorldObject { get; private set; }

		// Token: 0x060014FB RID: 5371 RVA: 0x00064D24 File Offset: 0x00062F24
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x060014FC RID: 5372 RVA: 0x00064D2D File Offset: 0x00062F2D
		// (set) Token: 0x060014FD RID: 5373 RVA: 0x00064D35 File Offset: 0x00062F35
		public bool ShouldSaveAndLoad { get; set; } = true;

		// Token: 0x060014FE RID: 5374 RVA: 0x00064D3E File Offset: 0x00062F3E
		public object GetSaveState()
		{
			return new ValueTuple<int, int>(this.MaxHealth, this.CurrentHealth);
		}

		// Token: 0x060014FF RID: 5375 RVA: 0x00064D58 File Offset: 0x00062F58
		public void LoadState(object loadedState)
		{
			if (base.IsServer && loadedState != null)
			{
				ValueTuple<int, int> valueTuple = (ValueTuple<int, int>)loadedState;
				this.CurrentHealth = valueTuple.Item2;
				this.maxHealth.Value = valueTuple.Item1;
			}
		}

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x06001500 RID: 5376 RVA: 0x00064D94 File Offset: 0x00062F94
		public object LuaObject
		{
			get
			{
				Health health;
				if ((health = this.luaInterface) == null)
				{
					health = (this.luaInterface = new Health(this));
				}
				return health;
			}
		}

		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x06001501 RID: 5377 RVA: 0x00064DBA File Offset: 0x00062FBA
		public Type LuaObjectType
		{
			get
			{
				return typeof(Health);
			}
		}

		// Token: 0x06001502 RID: 5378 RVA: 0x00064DC6 File Offset: 0x00062FC6
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001504 RID: 5380 RVA: 0x00064E54 File Offset: 0x00063054
		protected override void __initializeVariables()
		{
			bool flag = this.currentHealth == null;
			if (flag)
			{
				throw new Exception("HealthComponent.currentHealth cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.currentHealth.Initialize(this);
			base.__nameNetworkVariable(this.currentHealth, "currentHealth");
			this.NetworkVariableFields.Add(this.currentHealth);
			flag = this.maxHealth == null;
			if (flag)
			{
				throw new Exception("HealthComponent.maxHealth cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.maxHealth.Initialize(this);
			base.__nameNetworkVariable(this.maxHealth, "maxHealth");
			this.NetworkVariableFields.Add(this.maxHealth);
			base.__initializeVariables();
		}

		// Token: 0x06001505 RID: 5381 RVA: 0x00064F04 File Offset: 0x00063104
		protected override void __initializeRpcs()
		{
			base.__registerRpc(703732775U, new NetworkBehaviour.RpcReceiveHandler(HealthComponent.__rpc_handler_703732775), "DamageReactionClientRPC");
			base.__registerRpc(4169615286U, new NetworkBehaviour.RpcReceiveHandler(HealthComponent.__rpc_handler_4169615286), "DamageReactionClientRPC");
			base.__initializeRpcs();
		}

		// Token: 0x06001506 RID: 5382 RVA: 0x00064F54 File Offset: 0x00063154
		private static void __rpc_handler_703732775(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			NetworkObjectReference networkObjectReference;
			reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((HealthComponent)target).DamageReactionClientRPC(num, networkObjectReference, num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001507 RID: 5383 RVA: 0x00064FE8 File Offset: 0x000631E8
		private static void __rpc_handler_4169615286(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((HealthComponent)target).DamageReactionClientRPC(num, num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001508 RID: 5384 RVA: 0x0006505B File Offset: 0x0006325B
		protected internal override string __getTypeName()
		{
			return "HealthComponent";
		}

		// Token: 0x04001144 RID: 4420
		[SerializeField]
		private int startingMaxHealth = 10;

		// Token: 0x04001145 RID: 4421
		[SerializeField]
		internal HealthZeroedBehavior healthZeroedBehavior;

		// Token: 0x04001146 RID: 4422
		internal readonly NetworkVariable<int> currentHealth = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001147 RID: 4423
		internal readonly NetworkVariable<int> maxHealth = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001148 RID: 4424
		private EndlessScriptComponent scriptComponent;

		// Token: 0x04001149 RID: 4425
		public EndlessEvent OnDefeated = new EndlessEvent();

		// Token: 0x0400114A RID: 4426
		public UnityEvent OnHealthZeroed_Internal = new UnityEvent();

		// Token: 0x0400114B RID: 4427
		public UnityEvent<int, int> OnHealthChanged = new UnityEvent<int, int>();

		// Token: 0x0400114C RID: 4428
		public UnityEvent<int, int> OnMaxHealthChanged = new UnityEvent<int, int>();

		// Token: 0x0400114D RID: 4429
		public UnityEvent<HealthComponent.HealthLostData> OnHealthLost = new UnityEvent<HealthComponent.HealthLostData>();

		// Token: 0x0400114E RID: 4430
		public UnityEvent<int> OnHealthGained = new UnityEvent<int>();

		// Token: 0x0400114F RID: 4431
		private uint client_latest_damage_flinch;

		// Token: 0x04001152 RID: 4434
		private Health luaInterface;

		// Token: 0x0200034F RID: 847
		public struct HealthLostData
		{
			// Token: 0x04001153 RID: 4435
			public uint frame;

			// Token: 0x04001154 RID: 4436
			public NetworkObject damageSource;

			// Token: 0x04001155 RID: 4437
			public int damageDelta;

			// Token: 0x04001156 RID: 4438
			public bool networked;
		}
	}
}
