using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002F6 RID: 758
	public class BasicLevelGate : EndlessNetworkBehaviour, IStartSubscriber, IGameEndSubscriber, IBaseType, IComponentBase, IScriptInjector, ISpawnPoint
	{
		// Token: 0x06001110 RID: 4368 RVA: 0x00055B0C File Offset: 0x00053D0C
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsServer)
			{
				this.UpdatePlayerCount(NetworkManager.Singleton.ConnectedClientsIds.Count);
			}
			if (base.IsClient)
			{
				NetworkVariable<float> networkVariable = this.levelTransitionStartTime;
				networkVariable.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.HandleTransitionTimeUpdated));
			}
		}

		// Token: 0x06001111 RID: 4369 RVA: 0x00055B6B File Offset: 0x00053D6B
		private void UpdatePlayerCount(int count)
		{
			this.playerCount = count;
			this.UpdatePlayerCount();
		}

		// Token: 0x06001112 RID: 4370 RVA: 0x00055B7A File Offset: 0x00053D7A
		private void HandlePlayerLeft(ulong obj)
		{
			this.UpdatePlayerCount(NetworkManager.Singleton.ConnectedClientsIds.Count - 1);
		}

		// Token: 0x06001113 RID: 4371 RVA: 0x00055B93 File Offset: 0x00053D93
		private void HandlePlayerJoined(ulong obj)
		{
			this.UpdatePlayerCount(NetworkManager.Singleton.ConnectedClientsIds.Count);
		}

		// Token: 0x06001114 RID: 4372 RVA: 0x00055BAA File Offset: 0x00053DAA
		private void HandleTransitionTimeUpdated(float previousValue, float newValue)
		{
			this.references.TimerFillImage.enabled = newValue > 0f;
		}

		// Token: 0x06001115 RID: 4373 RVA: 0x00055BC4 File Offset: 0x00053DC4
		private float CalculateTimerDuration()
		{
			float num = (float)this.playersReady.Count / (float)this.playerCount;
			float num2 = Mathf.InverseLerp(0.5f, 1f, num);
			return Mathf.Lerp(this.fullTransitionTime, this.minTransitionTime, num2);
		}

		// Token: 0x06001116 RID: 4374 RVA: 0x00055C0C File Offset: 0x00053E0C
		private void ModifyLiveTimer()
		{
			float num = (float)NetworkManager.Singleton.ServerTime.Time;
			float num2 = this.CalculateTimerDuration();
			float num3 = (this.levelTransitionStartTime.Value + this.levelTransitionDuration.Value - num) / this.levelTransitionDuration.Value;
			float num4 = num2 * num3;
			float num5 = num + num4;
			this.levelTransitionStartTime.Value = num5 - num2;
			this.levelTransitionDuration.Value = num2;
		}

		// Token: 0x06001117 RID: 4375 RVA: 0x00055C80 File Offset: 0x00053E80
		public void StartCountdown(Context context)
		{
			this.triggeringContext = context;
			this.isCountingDown = true;
			this.levelTransitionDuration.Value = this.CalculateTimerDuration();
			this.levelTransitionStartTime.Value = (float)NetworkManager.Singleton.ServerTime.Time;
		}

		// Token: 0x06001118 RID: 4376 RVA: 0x00055CCC File Offset: 0x00053ECC
		public void PlayerReady(Context playerContext)
		{
			if (this.isTransitioning)
			{
				return;
			}
			PlayerLuaComponent playerLuaComponent;
			if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
			{
				ulong ownerClientId = playerLuaComponent.References.OwnerClientId;
				if (!this.playersReady.Contains(ownerClientId))
				{
					this.playersReady.Add(ownerClientId);
					this.playersReadyCount = this.playersReady.Count;
					this.UpdatePlayerCount();
					if (this.isCountingDown)
					{
						this.ModifyLiveTimer();
					}
				}
			}
		}

		// Token: 0x06001119 RID: 4377 RVA: 0x00055D3C File Offset: 0x00053F3C
		public void PlayerUnready(Context playerContext)
		{
			if (this.isTransitioning)
			{
				return;
			}
			PlayerLuaComponent playerLuaComponent;
			if (playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out playerLuaComponent))
			{
				ulong ownerClientId = playerLuaComponent.References.OwnerClientId;
				if (this.playersReady.Contains(ownerClientId))
				{
					this.playersReady.Remove(ownerClientId);
					this.playersReadyCount = this.playersReady.Count;
					this.UpdatePlayerCount();
					if ((float)this.playersReady.Count / (float)this.playerCount < 0.5f)
					{
						if (this.isCountingDown)
						{
							this.isCountingDown = false;
							object[] array;
							this.scriptComponent.TryExecuteFunction("CountdownCancelled", out array, new object[] { playerContext });
							this.levelTransitionStartTime.Value = 0f;
							return;
						}
					}
					else if (this.isCountingDown)
					{
						this.ModifyLiveTimer();
					}
				}
			}
		}

		// Token: 0x0600111A RID: 4378 RVA: 0x00055E0C File Offset: 0x0005400C
		public void UpdatePlayerCount()
		{
			if (this.scriptComponent != null && this.scriptComponent.IsScriptReady)
			{
				object[] array;
				this.scriptComponent.TryExecuteFunction("PlayerCountUpdated", out array, new object[] { this.playersReadyCount, this.playerCount });
			}
		}

		// Token: 0x0600111B RID: 4379 RVA: 0x00055E69 File Offset: 0x00054069
		protected virtual void StartTransition()
		{
			this.isTransitioning = true;
			this.PlayTransitionParticleEffect_ClientRpc();
			this.HandleTransition();
		}

		// Token: 0x0600111C RID: 4380 RVA: 0x00055E7E File Offset: 0x0005407E
		private void CountdownFinished()
		{
			this.StartTransition();
		}

		// Token: 0x0600111D RID: 4381 RVA: 0x00055E88 File Offset: 0x00054088
		protected virtual void HandleTransition()
		{
			if (this.triggeringContext != null && this.triggeringContext.WorldObject != null)
			{
				this.OnTransitionStarted.Invoke(this.triggeringContext);
			}
			else
			{
				this.OnTransitionStarted.Invoke(this.Context);
			}
			base.StartCoroutine(this.BaseTransitionDelay());
		}

		// Token: 0x0600111E RID: 4382 RVA: 0x00055EE4 File Offset: 0x000540E4
		[ClientRpc]
		private void PlayTransitionParticleEffect_ClientRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2516210707U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 2516210707U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.references.PartialReadyParticles && this.references.PartialReadyParticles.RuntimeParticleSystem)
			{
				this.references.PartialReadyParticles.RuntimeParticleSystem.Stop();
			}
			if (this.references.ReadyParticles && this.references.ReadyParticles.RuntimeParticleSystem)
			{
				this.references.ReadyParticles.RuntimeParticleSystem.Stop();
			}
			if (this.references.StartingParticles && this.references.StartingParticles.RuntimeParticleSystem)
			{
				this.references.StartingParticles.RuntimeParticleSystem.Play();
			}
		}

		// Token: 0x0600111F RID: 4383 RVA: 0x00056071 File Offset: 0x00054271
		private IEnumerator BaseTransitionDelay()
		{
			yield return new WaitForSeconds(this.finalTransitionDelay);
			this.levelDestination.ChangeLevel(this.Context);
			yield break;
		}

		// Token: 0x06001120 RID: 4384 RVA: 0x00056080 File Offset: 0x00054280
		private void Update()
		{
			if (this.references.TimerFillImage.enabled)
			{
				float num = this.levelTransitionStartTime.Value + this.levelTransitionDuration.Value - (float)base.NetworkManager.ServerTime.Time;
				this.references.TimerFillImage.fillAmount = 1f - Mathf.InverseLerp(0f, this.levelTransitionDuration.Value, num);
			}
			if (base.IsServer && this.levelTransitionStartTime.Value > 0f && !this.isTransitioning && NetworkManager.Singleton.ServerTime.Time >= (double)(this.levelTransitionStartTime.Value + this.levelTransitionDuration.Value))
			{
				this.CountdownFinished();
			}
		}

		// Token: 0x06001121 RID: 4385 RVA: 0x0005614C File Offset: 0x0005434C
		public void ToggleReadyParticles(bool ready)
		{
			this.ToggleReadyParticles_ClientRpc(ready);
		}

		// Token: 0x06001122 RID: 4386 RVA: 0x00056158 File Offset: 0x00054358
		[ClientRpc]
		private void ToggleReadyParticles_ClientRpc(bool ready)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1715564670U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<bool>(in ready, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 1715564670U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (ready)
			{
				if (this.references.PartialReadyParticles && this.references.PartialReadyParticles.RuntimeParticleSystem)
				{
					this.references.PartialReadyParticles.RuntimeParticleSystem.Stop();
				}
				if (this.references.ReadyParticles && this.references.ReadyParticles.RuntimeParticleSystem)
				{
					this.references.ReadyParticles.RuntimeParticleSystem.Play();
					return;
				}
			}
			else
			{
				if (this.references.PartialReadyParticles && this.references.PartialReadyParticles.RuntimeParticleSystem)
				{
					this.references.PartialReadyParticles.RuntimeParticleSystem.Play();
				}
				if (this.references.ReadyParticles && this.references.ReadyParticles.RuntimeParticleSystem)
				{
					this.references.ReadyParticles.RuntimeParticleSystem.Stop();
				}
			}
		}

		// Token: 0x06001123 RID: 4387 RVA: 0x0005634B File Offset: 0x0005454B
		public bool IsValidDestination()
		{
			return this.levelDestination.TargetLevelId != SerializableGuid.Empty;
		}

		// Token: 0x06001124 RID: 4388 RVA: 0x00056364 File Offset: 0x00054564
		public void EndlessStart()
		{
			if (base.IsServer)
			{
				this.UpdatePlayerCount(NetworkManager.Singleton.ConnectedClientsIds.Count);
				base.NetworkManager.OnClientConnectedCallback += this.HandlePlayerJoined;
				base.NetworkManager.OnClientDisconnectCallback += this.HandlePlayerLeft;
			}
		}

		// Token: 0x06001125 RID: 4389 RVA: 0x000563BC File Offset: 0x000545BC
		public void EndlessGameEnd()
		{
			if (base.IsServer)
			{
				base.NetworkManager.OnClientConnectedCallback += this.HandlePlayerJoined;
				base.NetworkManager.OnClientDisconnectCallback += this.HandlePlayerLeft;
			}
		}

		// Token: 0x1700035F RID: 863
		// (get) Token: 0x06001126 RID: 4390 RVA: 0x000563F4 File Offset: 0x000545F4
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(BasicLevelGateReferences);
			}
		}

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x06001127 RID: 4391 RVA: 0x00056400 File Offset: 0x00054600
		// (set) Token: 0x06001128 RID: 4392 RVA: 0x00056408 File Offset: 0x00054608
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x06001129 RID: 4393 RVA: 0x00056414 File Offset: 0x00054614
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

		// Token: 0x0600112A RID: 4394 RVA: 0x0005643F File Offset: 0x0005463F
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x00056448 File Offset: 0x00054648
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (BasicLevelGateReferences)referenceBase;
		}

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x0600112C RID: 4396 RVA: 0x00056458 File Offset: 0x00054658
		public object LuaObject
		{
			get
			{
				BasicLevelGate basicLevelGate;
				if ((basicLevelGate = this.luaInterface) == null)
				{
					basicLevelGate = (this.luaInterface = new BasicLevelGate(this));
				}
				return basicLevelGate;
			}
		}

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x0600112D RID: 4397 RVA: 0x0005647E File Offset: 0x0005467E
		public Type LuaObjectType
		{
			get
			{
				return typeof(BasicLevelGate);
			}
		}

		// Token: 0x0600112E RID: 4398 RVA: 0x0005648A File Offset: 0x0005468A
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x0600112F RID: 4399 RVA: 0x00056493 File Offset: 0x00054693
		public Transform GetSpawnPosition(int index)
		{
			if (this.references.SpawnPoints.Length == 0)
			{
				return base.transform;
			}
			return this.references.SpawnPoints[index % this.references.SpawnPoints.Length];
		}

		// Token: 0x06001130 RID: 4400 RVA: 0x000564C8 File Offset: 0x000546C8
		public void ConfigurePlayer(GameplayPlayerReferenceManager playerReferenceManager)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("HandleNewPlayerSpawned", out array, new object[] { playerReferenceManager.PlayerContext });
		}

		// Token: 0x06001131 RID: 4401 RVA: 0x000564F8 File Offset: 0x000546F8
		public void HandlePlayerEnteredLevel(GameplayPlayerReferenceManager playerReferenceManager)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("HandlePlayerLevelChange", out array, new object[] { playerReferenceManager.PlayerContext });
		}

		// Token: 0x06001133 RID: 4403 RVA: 0x00056598 File Offset: 0x00054798
		protected override void __initializeVariables()
		{
			bool flag = this.levelTransitionStartTime == null;
			if (flag)
			{
				throw new Exception("BasicLevelGate.levelTransitionStartTime cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.levelTransitionStartTime.Initialize(this);
			base.__nameNetworkVariable(this.levelTransitionStartTime, "levelTransitionStartTime");
			this.NetworkVariableFields.Add(this.levelTransitionStartTime);
			flag = this.levelTransitionDuration == null;
			if (flag)
			{
				throw new Exception("BasicLevelGate.levelTransitionDuration cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.levelTransitionDuration.Initialize(this);
			base.__nameNetworkVariable(this.levelTransitionDuration, "levelTransitionDuration");
			this.NetworkVariableFields.Add(this.levelTransitionDuration);
			base.__initializeVariables();
		}

		// Token: 0x06001134 RID: 4404 RVA: 0x00056648 File Offset: 0x00054848
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2516210707U, new NetworkBehaviour.RpcReceiveHandler(BasicLevelGate.__rpc_handler_2516210707), "PlayTransitionParticleEffect_ClientRpc");
			base.__registerRpc(1715564670U, new NetworkBehaviour.RpcReceiveHandler(BasicLevelGate.__rpc_handler_1715564670), "ToggleReadyParticles_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06001135 RID: 4405 RVA: 0x00056698 File Offset: 0x00054898
		private static void __rpc_handler_2516210707(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((BasicLevelGate)target).PlayTransitionParticleEffect_ClientRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x000566EC File Offset: 0x000548EC
		private static void __rpc_handler_1715564670(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((BasicLevelGate)target).ToggleReadyParticles_ClientRpc(flag);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06001137 RID: 4407 RVA: 0x0005675C File Offset: 0x0005495C
		protected internal override string __getTypeName()
		{
			return "BasicLevelGate";
		}

		// Token: 0x04000EC7 RID: 3783
		[SerializeField]
		private float fullTransitionTime = 15f;

		// Token: 0x04000EC8 RID: 3784
		[SerializeField]
		private float minTransitionTime = 3f;

		// Token: 0x04000EC9 RID: 3785
		[SerializeField]
		private float finalTransitionDelay = 0.5f;

		// Token: 0x04000ECA RID: 3786
		[SerializeField]
		private LevelDestination levelDestination;

		// Token: 0x04000ECB RID: 3787
		public EndlessEvent OnTransitionStarted = new EndlessEvent();

		// Token: 0x04000ECC RID: 3788
		private List<ulong> playersReady = new List<ulong>();

		// Token: 0x04000ECD RID: 3789
		private int playersReadyCount;

		// Token: 0x04000ECE RID: 3790
		private int playerCount;

		// Token: 0x04000ECF RID: 3791
		private NetworkVariable<float> levelTransitionStartTime = new NetworkVariable<float>(-1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000ED0 RID: 3792
		private NetworkVariable<float> levelTransitionDuration = new NetworkVariable<float>(-1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000ED1 RID: 3793
		private bool isCountingDown;

		// Token: 0x04000ED2 RID: 3794
		private bool isTransitioning;

		// Token: 0x04000ED3 RID: 3795
		private Context triggeringContext;

		// Token: 0x04000ED4 RID: 3796
		private Context context;

		// Token: 0x04000ED5 RID: 3797
		[SerializeField]
		private BasicLevelGateReferences references;

		// Token: 0x04000ED7 RID: 3799
		private BasicLevelGate luaInterface;

		// Token: 0x04000ED8 RID: 3800
		private EndlessScriptComponent scriptComponent;
	}
}
