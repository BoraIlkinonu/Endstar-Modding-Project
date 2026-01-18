using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class BasicLevelGate : EndlessNetworkBehaviour, IStartSubscriber, IGameEndSubscriber, IBaseType, IComponentBase, IScriptInjector, ISpawnPoint
{
	[SerializeField]
	private float fullTransitionTime = 15f;

	[SerializeField]
	private float minTransitionTime = 3f;

	[SerializeField]
	private float finalTransitionDelay = 0.5f;

	[SerializeField]
	private LevelDestination levelDestination;

	public EndlessEvent OnTransitionStarted = new EndlessEvent();

	private List<ulong> playersReady = new List<ulong>();

	private int playersReadyCount;

	private int playerCount;

	private NetworkVariable<float> levelTransitionStartTime = new NetworkVariable<float>(-1f);

	private NetworkVariable<float> levelTransitionDuration = new NetworkVariable<float>(-1f);

	private bool isCountingDown;

	private bool isTransitioning;

	private Context triggeringContext;

	private Context context;

	[SerializeField]
	private BasicLevelGateReferences references;

	private Endless.Gameplay.LuaInterfaces.BasicLevelGate luaInterface;

	private EndlessScriptComponent scriptComponent;

	public Type ComponentReferenceType => typeof(BasicLevelGateReferences);

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	public Context Context => context ?? (context = new Context(WorldObject));

	public object LuaObject => luaInterface ?? (luaInterface = new Endless.Gameplay.LuaInterfaces.BasicLevelGate(this));

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.BasicLevelGate);

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (base.IsServer)
		{
			UpdatePlayerCount(NetworkManager.Singleton.ConnectedClientsIds.Count);
		}
		if (base.IsClient)
		{
			NetworkVariable<float> networkVariable = levelTransitionStartTime;
			networkVariable.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(HandleTransitionTimeUpdated));
		}
	}

	private void UpdatePlayerCount(int count)
	{
		playerCount = count;
		UpdatePlayerCount();
	}

	private void HandlePlayerLeft(ulong obj)
	{
		UpdatePlayerCount(NetworkManager.Singleton.ConnectedClientsIds.Count - 1);
	}

	private void HandlePlayerJoined(ulong obj)
	{
		UpdatePlayerCount(NetworkManager.Singleton.ConnectedClientsIds.Count);
	}

	private void HandleTransitionTimeUpdated(float previousValue, float newValue)
	{
		references.TimerFillImage.enabled = newValue > 0f;
	}

	private float CalculateTimerDuration()
	{
		float value = (float)playersReady.Count / (float)playerCount;
		float t = Mathf.InverseLerp(0.5f, 1f, value);
		return Mathf.Lerp(fullTransitionTime, minTransitionTime, t);
	}

	private void ModifyLiveTimer()
	{
		float num = (float)NetworkManager.Singleton.ServerTime.Time;
		float num2 = CalculateTimerDuration();
		float num3 = (levelTransitionStartTime.Value + levelTransitionDuration.Value - num) / levelTransitionDuration.Value;
		float num4 = num2 * num3;
		float num5 = num + num4;
		levelTransitionStartTime.Value = num5 - num2;
		levelTransitionDuration.Value = num2;
	}

	public void StartCountdown(Context context)
	{
		triggeringContext = context;
		isCountingDown = true;
		levelTransitionDuration.Value = CalculateTimerDuration();
		levelTransitionStartTime.Value = (float)NetworkManager.Singleton.ServerTime.Time;
	}

	public void PlayerReady(Context playerContext)
	{
		if (isTransitioning || !playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component))
		{
			return;
		}
		ulong ownerClientId = component.References.OwnerClientId;
		if (!playersReady.Contains(ownerClientId))
		{
			playersReady.Add(ownerClientId);
			playersReadyCount = playersReady.Count;
			UpdatePlayerCount();
			if (isCountingDown)
			{
				ModifyLiveTimer();
			}
		}
	}

	public void PlayerUnready(Context playerContext)
	{
		if (isTransitioning || !playerContext.WorldObject.TryGetUserComponent<PlayerLuaComponent>(out var component))
		{
			return;
		}
		ulong ownerClientId = component.References.OwnerClientId;
		if (!playersReady.Contains(ownerClientId))
		{
			return;
		}
		playersReady.Remove(ownerClientId);
		playersReadyCount = playersReady.Count;
		UpdatePlayerCount();
		if ((float)playersReady.Count / (float)playerCount < 0.5f)
		{
			if (isCountingDown)
			{
				isCountingDown = false;
				scriptComponent.TryExecuteFunction("CountdownCancelled", out var _, playerContext);
				levelTransitionStartTime.Value = 0f;
			}
		}
		else if (isCountingDown)
		{
			ModifyLiveTimer();
		}
	}

	public void UpdatePlayerCount()
	{
		if (scriptComponent != null && scriptComponent.IsScriptReady)
		{
			scriptComponent.TryExecuteFunction("PlayerCountUpdated", out var _, playersReadyCount, playerCount);
		}
	}

	protected virtual void StartTransition()
	{
		isTransitioning = true;
		PlayTransitionParticleEffect_ClientRpc();
		HandleTransition();
	}

	private void CountdownFinished()
	{
		StartTransition();
	}

	protected virtual void HandleTransition()
	{
		if (triggeringContext != null && triggeringContext.WorldObject != null)
		{
			OnTransitionStarted.Invoke(triggeringContext);
		}
		else
		{
			OnTransitionStarted.Invoke(Context);
		}
		StartCoroutine(BaseTransitionDelay());
	}

	[ClientRpc]
	private void PlayTransitionParticleEffect_ClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2516210707u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 2516210707u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if ((bool)references.PartialReadyParticles && (bool)references.PartialReadyParticles.RuntimeParticleSystem)
			{
				references.PartialReadyParticles.RuntimeParticleSystem.Stop();
			}
			if ((bool)references.ReadyParticles && (bool)references.ReadyParticles.RuntimeParticleSystem)
			{
				references.ReadyParticles.RuntimeParticleSystem.Stop();
			}
			if ((bool)references.StartingParticles && (bool)references.StartingParticles.RuntimeParticleSystem)
			{
				references.StartingParticles.RuntimeParticleSystem.Play();
			}
		}
	}

	private IEnumerator BaseTransitionDelay()
	{
		yield return new WaitForSeconds(finalTransitionDelay);
		levelDestination.ChangeLevel(Context);
	}

	private void Update()
	{
		if (references.TimerFillImage.enabled)
		{
			float value = levelTransitionStartTime.Value + levelTransitionDuration.Value - (float)base.NetworkManager.ServerTime.Time;
			references.TimerFillImage.fillAmount = 1f - Mathf.InverseLerp(0f, levelTransitionDuration.Value, value);
		}
		if (base.IsServer && levelTransitionStartTime.Value > 0f && !isTransitioning && NetworkManager.Singleton.ServerTime.Time >= (double)(levelTransitionStartTime.Value + levelTransitionDuration.Value))
		{
			CountdownFinished();
		}
	}

	public void ToggleReadyParticles(bool ready)
	{
		ToggleReadyParticles_ClientRpc(ready);
	}

	[ClientRpc]
	private void ToggleReadyParticles_ClientRpc(bool ready)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1715564670u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in ready, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 1715564670u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (ready)
		{
			if ((bool)references.PartialReadyParticles && (bool)references.PartialReadyParticles.RuntimeParticleSystem)
			{
				references.PartialReadyParticles.RuntimeParticleSystem.Stop();
			}
			if ((bool)references.ReadyParticles && (bool)references.ReadyParticles.RuntimeParticleSystem)
			{
				references.ReadyParticles.RuntimeParticleSystem.Play();
			}
		}
		else
		{
			if ((bool)references.PartialReadyParticles && (bool)references.PartialReadyParticles.RuntimeParticleSystem)
			{
				references.PartialReadyParticles.RuntimeParticleSystem.Play();
			}
			if ((bool)references.ReadyParticles && (bool)references.ReadyParticles.RuntimeParticleSystem)
			{
				references.ReadyParticles.RuntimeParticleSystem.Stop();
			}
		}
	}

	public bool IsValidDestination()
	{
		return levelDestination.TargetLevelId != SerializableGuid.Empty;
	}

	public void EndlessStart()
	{
		if (base.IsServer)
		{
			UpdatePlayerCount(NetworkManager.Singleton.ConnectedClientsIds.Count);
			base.NetworkManager.OnClientConnectedCallback += HandlePlayerJoined;
			base.NetworkManager.OnClientDisconnectCallback += HandlePlayerLeft;
		}
	}

	public void EndlessGameEnd()
	{
		if (base.IsServer)
		{
			base.NetworkManager.OnClientConnectedCallback += HandlePlayerJoined;
			base.NetworkManager.OnClientDisconnectCallback += HandlePlayerLeft;
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		references = (BasicLevelGateReferences)referenceBase;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	public Transform GetSpawnPosition(int index)
	{
		if (references.SpawnPoints.Length == 0)
		{
			return base.transform;
		}
		return references.SpawnPoints[index % references.SpawnPoints.Length];
	}

	public void ConfigurePlayer(GameplayPlayerReferenceManager playerReferenceManager)
	{
		scriptComponent.TryExecuteFunction("HandleNewPlayerSpawned", out var _, playerReferenceManager.PlayerContext);
	}

	public void HandlePlayerEnteredLevel(GameplayPlayerReferenceManager playerReferenceManager)
	{
		scriptComponent.TryExecuteFunction("HandlePlayerLevelChange", out var _, playerReferenceManager.PlayerContext);
	}

	protected override void __initializeVariables()
	{
		if (levelTransitionStartTime == null)
		{
			throw new Exception("BasicLevelGate.levelTransitionStartTime cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		levelTransitionStartTime.Initialize(this);
		__nameNetworkVariable(levelTransitionStartTime, "levelTransitionStartTime");
		NetworkVariableFields.Add(levelTransitionStartTime);
		if (levelTransitionDuration == null)
		{
			throw new Exception("BasicLevelGate.levelTransitionDuration cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		levelTransitionDuration.Initialize(this);
		__nameNetworkVariable(levelTransitionDuration, "levelTransitionDuration");
		NetworkVariableFields.Add(levelTransitionDuration);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2516210707u, __rpc_handler_2516210707, "PlayTransitionParticleEffect_ClientRpc");
		__registerRpc(1715564670u, __rpc_handler_1715564670, "ToggleReadyParticles_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2516210707(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((BasicLevelGate)target).PlayTransitionParticleEffect_ClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1715564670(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((BasicLevelGate)target).ToggleReadyParticles_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "BasicLevelGate";
	}
}
