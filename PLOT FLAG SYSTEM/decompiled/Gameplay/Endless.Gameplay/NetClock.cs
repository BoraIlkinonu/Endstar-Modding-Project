using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;

namespace Endless.Gameplay;

public class NetClock : NetworkBehaviourSingleton<NetClock>
{
	public interface ISimulateFrameEarlySubscriber
	{
		void SimulateFrameEarly(uint frame);
	}

	public interface ISimulateFrameEnvironmentSubscriber
	{
		void SimulateFrameEnvironment(uint frame);
	}

	public interface ISimulateFrameActorsSubscriber
	{
		void SimulateFrameActors(uint frame);
	}

	public interface ISimulateFrameLateSubscriber
	{
		void SimulateFrameLate(uint frame);
	}

	public interface IPreFixedUpdateSubscriber
	{
		void PreFixedUpdate(uint frame);
	}

	public interface IRollbackSubscriber
	{
		void Rollback(uint frame);
	}

	public interface IPostFixedUpdateSubscriber
	{
		void PostFixedUpdate(uint frame);
	}

	public interface ILoadingFrameSubscriber
	{
		void LoadingFrame(uint frame);
	}

	private static List<ISimulateFrameEarlySubscriber> simulateFrameEarlySubscribers = new List<ISimulateFrameEarlySubscriber>();

	private static List<ISimulateFrameEnvironmentSubscriber> simulateFrameEnvironmentSubscribers = new List<ISimulateFrameEnvironmentSubscriber>();

	private static List<ISimulateFrameActorsSubscriber> simulateFrameActorsSubscribers = new List<ISimulateFrameActorsSubscriber>();

	private static List<ISimulateFrameLateSubscriber> simulateFrameLateSubscribers = new List<ISimulateFrameLateSubscriber>();

	private static List<IPreFixedUpdateSubscriber> preFixedUpdateSubscribers = new List<IPreFixedUpdateSubscriber>();

	private static List<IRollbackSubscriber> rollbackSubscribers = new List<IRollbackSubscriber>();

	private static List<IPostFixedUpdateSubscriber> postFixedUpdateSubscribers = new List<IPostFixedUpdateSubscriber>();

	private static List<ILoadingFrameSubscriber> loadingFrameSubscribers = new List<ILoadingFrameSubscriber>();

	private const float NET_FRAME_FIXED_DELTA_TIME = 0.05f;

	private const int NETWORKED_PHYSICS_SUB_FRAMES = 3;

	private const uint ADDITIONAL_ROLLBACK_FRAMES = 1u;

	private const uint ADDITIONAL_EXTRAPOLATION_FRAMES = 3u;

	private const uint MAX_SIMULATION_FRAMES = 12u;

	private ProfilerMarker NetClock_RigidbodyBeforeSimulationMarker = new ProfilerMarker("NetClock_RigidbodyBeforeSimulationMarker");

	private ProfilerMarker NetClock_PreFixedUpdateMarker = new ProfilerMarker("NetClock_PreFixedUpdateMarker");

	private ProfilerMarker NetClock_SimulateFrameMarker = new ProfilerMarker("NetClock_SimulateFrameMarker");

	private ProfilerMarker NetClock_PostFixedUpdateMarker = new ProfilerMarker("NetClock_PostFixedUpdateMarker");

	private ProfilerMarker NetClock_RigidbodyManagerAfterSimulationMarker = new ProfilerMarker("NetClock_RigidbodyManagerAfterSimulationMarker");

	private ProfilerMarker NetClock_SimulateFrameEarlyMarker = new ProfilerMarker("NetClock_SimulateFrameEarlyMarker");

	private ProfilerMarker NetClock_SimulaterameEnvironmentMarker = new ProfilerMarker("NetClock_SimulaterameEnvironmentMarker");

	private ProfilerMarker NetClock_SimulateFrameActorsMarker = new ProfilerMarker("NetClock_SimulateFrameActorsMarker");

	private ProfilerMarker NetClock_SimulateFrameLateMarker = new ProfilerMarker("NetClock_SimulateFrameLateMarker");

	private ProfilerMarker NetClock_Physics = new ProfilerMarker("NetClock_Physics");

	public NetworkVariable<uint> GameplayReadyFrame = new NetworkVariable<uint>(uint.MaxValue);

	public NetworkVariable<bool> GameplayStreamActive = new NetworkVariable<bool>(value: false);

	private bool ready;

	private uint currentFrame;

	private uint currentRollbackFrame;

	private uint currentSimulationFrame;

	private float calculatedRTT;

	private float currentNetworkRTT;

	private float networkRTTSmoothingVelo;

	private float networkRTTSmoothingTime = 3f;

	private bool isPaused;

	public static uint CurrentFrame
	{
		get
		{
			return NetworkBehaviourSingleton<NetClock>.Instance.currentFrame;
		}
		private set
		{
			NetworkBehaviourSingleton<NetClock>.Instance.currentFrame = value;
		}
	}

	public static uint CurrentSimulationFrame => NetworkBehaviourSingleton<NetClock>.Instance.currentSimulationFrame;

	public static double LocalNetworkTime => NetworkManager.Singleton.LocalTime.Time;

	public static float RoundTripTime
	{
		get
		{
			if (!NetworkBehaviourSingleton<NetClock>.Instance)
			{
				return 0f;
			}
			return NetworkBehaviourSingleton<NetClock>.Instance.calculatedRTT;
		}
	}

	public static double ClientInterpolatedAppearanceTime => NetworkManager.Singleton.ServerTime.Time - (double)FixedDeltaTime * 1.1;

	public static double ServerAppearanceTime => ClientInterpolatedAppearanceTime;

	public static double ClientExtrapolatedAppearanceTime => ClientExtrapolatedTime - (double)FixedDeltaTime * 1.1;

	public static double ClientExtrapolatedTime => NetworkManager.Singleton.LocalTime.Time + (double)(FixedDeltaTime * 3f);

	public static float FixedDeltaTime => 0.05f;

	public static uint CurrentRollbackFrame
	{
		get
		{
			if (!NetworkManager.Singleton.IsServer)
			{
				return NetworkBehaviourSingleton<NetClock>.Instance.currentRollbackFrame;
			}
			return CurrentFrame;
		}
	}

	private void Start()
	{
		Physics.simulationMode = SimulationMode.Script;
	}

	public static void Register(Component component)
	{
		if (component is ISimulateFrameEarlySubscriber item)
		{
			simulateFrameEarlySubscribers.Add(item);
		}
		if (component is ISimulateFrameEnvironmentSubscriber item2)
		{
			simulateFrameEnvironmentSubscribers.Add(item2);
		}
		if (component is ISimulateFrameActorsSubscriber item3)
		{
			simulateFrameActorsSubscribers.Add(item3);
		}
		if (component is ISimulateFrameLateSubscriber item4)
		{
			simulateFrameLateSubscribers.Add(item4);
		}
		if (component is IPreFixedUpdateSubscriber item5)
		{
			preFixedUpdateSubscribers.Add(item5);
		}
		if (component is IRollbackSubscriber item6)
		{
			rollbackSubscribers.Add(item6);
		}
		if (component is IPostFixedUpdateSubscriber item7)
		{
			postFixedUpdateSubscribers.Add(item7);
		}
		if (component is ILoadingFrameSubscriber item8)
		{
			loadingFrameSubscribers.Add(item8);
		}
	}

	public static void Unregister(Component component)
	{
		if (component is ISimulateFrameEarlySubscriber item)
		{
			simulateFrameEarlySubscribers.Remove(item);
		}
		if (component is ISimulateFrameEnvironmentSubscriber item2)
		{
			simulateFrameEnvironmentSubscribers.Remove(item2);
		}
		if (component is ISimulateFrameActorsSubscriber item3)
		{
			simulateFrameActorsSubscribers.Remove(item3);
		}
		if (component is ISimulateFrameLateSubscriber item4)
		{
			simulateFrameLateSubscribers.Remove(item4);
		}
		if (component is IPreFixedUpdateSubscriber item5)
		{
			preFixedUpdateSubscribers.Remove(item5);
		}
		if (component is IRollbackSubscriber item6)
		{
			rollbackSubscribers.Remove(item6);
		}
		if (component is IPostFixedUpdateSubscriber item7)
		{
			postFixedUpdateSubscribers.Remove(item7);
		}
		if (component is ILoadingFrameSubscriber item8)
		{
			loadingFrameSubscribers.Remove(item8);
		}
	}

	private void FixedUpdate()
	{
		Physics.Simulate(Time.fixedDeltaTime);
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (NetworkBehaviourSingleton<NetClock>.Instance.IsServer)
		{
			CurrentFrame = (uint)NetworkBehaviourSingleton<NetClock>.Instance.NetworkManager.NetworkTickSystem.ServerTime.Tick;
		}
		else
		{
			CurrentFrame = (uint)(NetworkBehaviourSingleton<NetClock>.Instance.NetworkManager.NetworkTickSystem.LocalTime.Tick + 3);
		}
		base.NetworkManager.NetworkTickSystem.Tick += NetworkFixedUpdate;
		StartCoroutine(CalculateRTTLoop());
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		base.NetworkManager.NetworkTickSystem.Tick -= NetworkFixedUpdate;
	}

	private IEnumerator CalculateRTTLoop()
	{
		while (base.IsSpawned)
		{
			ArbitraryReliableMessage_ServerRPC();
			currentNetworkRTT = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(0uL);
			yield return new WaitForSecondsRealtime(0.1f);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void ArbitraryReliableMessage_ServerRPC()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams = default(ServerRpcParams);
				FastBufferWriter bufferWriter = __beginSendServerRpc(3151196681u, serverRpcParams, RpcDelivery.Reliable);
				__endSendServerRpc(ref bufferWriter, 3151196681u, serverRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
			}
		}
	}

	public static double GetFrameTime(uint frame)
	{
		return NetworkManager.Singleton.NetworkTickSystem.LocalTime.FixedTime + (double)(FixedDeltaTime * (float)(frame - NetworkManager.Singleton.NetworkTickSystem.LocalTime.Tick));
	}

	public static uint GetFrameFromTime(double networkTime)
	{
		return (uint)(networkTime / (double)FixedDeltaTime);
	}

	private void Update()
	{
		if (!base.IsServer)
		{
			calculatedRTT = Mathf.SmoothDamp(calculatedRTT, currentNetworkRTT, ref networkRTTSmoothingVelo, networkRTTSmoothingTime);
		}
	}

	private void NetworkFixedUpdate()
	{
		if (isPaused)
		{
			return;
		}
		MonoBehaviourSingleton<RigidbodyManager>.Instance.BeforeSimulation();
		List<IPreFixedUpdateSubscriber> list = new List<IPreFixedUpdateSubscriber>(preFixedUpdateSubscribers);
		List<IPostFixedUpdateSubscriber> list2 = new List<IPostFixedUpdateSubscriber>(postFixedUpdateSubscribers);
		if (base.IsServer)
		{
			uint tick = (uint)base.NetworkManager.NetworkTickSystem.ServerTime.Tick;
			while (CurrentFrame <= tick)
			{
				currentFrame++;
				foreach (IPreFixedUpdateSubscriber item in list)
				{
					try
					{
						item?.PreFixedUpdate(currentFrame);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
				if (CurrentFrame < GameplayReadyFrame.Value)
				{
					foreach (ILoadingFrameSubscriber item2 in new List<ILoadingFrameSubscriber>(loadingFrameSubscribers))
					{
						try
						{
							item2?.LoadingFrame(currentFrame);
						}
						catch (Exception exception2)
						{
							Debug.LogException(exception2);
						}
					}
				}
				else
				{
					SimulateFrame(CurrentFrame);
				}
				foreach (IPostFixedUpdateSubscriber item3 in list2)
				{
					try
					{
						item3?.PostFixedUpdate(currentFrame);
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
					}
				}
			}
		}
		else
		{
			uint tick2 = (uint)base.NetworkManager.NetworkTickSystem.ServerTime.Tick;
			int tick3 = base.NetworkManager.NetworkTickSystem.LocalTime.Tick;
			uint num = tick2 - 1;
			uint num2 = (uint)(tick3 + 3);
			currentRollbackFrame = num;
			bool num3 = CurrentFrame >= num2;
			bool flag = num2 - num > 12;
			if (num3 || flag)
			{
				MonoBehaviourSingleton<NetworkStatusIndicator>.Instance.ServerTimeDesync();
				return;
			}
			CurrentFrame = num2;
			foreach (IPreFixedUpdateSubscriber item4 in list)
			{
				try
				{
					item4?.PreFixedUpdate(currentFrame);
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4);
				}
			}
			foreach (IRollbackSubscriber item5 in new List<IRollbackSubscriber>(rollbackSubscribers))
			{
				try
				{
					item5?.Rollback(num);
				}
				catch (Exception exception5)
				{
					Debug.LogException(exception5);
				}
			}
			for (uint num4 = num + 1; num4 <= CurrentFrame; num4++)
			{
				if (num4 < GameplayReadyFrame.Value)
				{
					foreach (ILoadingFrameSubscriber item6 in new List<ILoadingFrameSubscriber>(loadingFrameSubscribers))
					{
						try
						{
							item6?.LoadingFrame(CurrentFrame);
						}
						catch (Exception exception6)
						{
							Debug.LogException(exception6);
						}
					}
				}
				else
				{
					SimulateFrame(num4);
				}
			}
			foreach (IPostFixedUpdateSubscriber item7 in list2)
			{
				try
				{
					item7?.PostFixedUpdate(currentFrame);
				}
				catch (Exception exception7)
				{
					Debug.LogException(exception7);
				}
			}
		}
		MonoBehaviourSingleton<RigidbodyManager>.Instance.AfterSimulation();
	}

	private void SimulateFrame(uint frame)
	{
		currentSimulationFrame = frame;
		foreach (ISimulateFrameEarlySubscriber item in new List<ISimulateFrameEarlySubscriber>(simulateFrameEarlySubscribers))
		{
			try
			{
				item?.SimulateFrameEarly(frame);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		for (int i = 0; i < 3; i++)
		{
			Physics.Simulate(FixedDeltaTime / 3f);
		}
		foreach (ISimulateFrameEnvironmentSubscriber item2 in new List<ISimulateFrameEnvironmentSubscriber>(simulateFrameEnvironmentSubscribers))
		{
			try
			{
				item2?.SimulateFrameEnvironment(frame);
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
		}
		foreach (ISimulateFrameActorsSubscriber item3 in new List<ISimulateFrameActorsSubscriber>(simulateFrameActorsSubscribers))
		{
			try
			{
				item3?.SimulateFrameActors(frame);
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
		}
		foreach (ISimulateFrameLateSubscriber item4 in new List<ISimulateFrameLateSubscriber>(simulateFrameLateSubscribers))
		{
			try
			{
				item4?.SimulateFrameLate(frame);
			}
			catch (Exception exception4)
			{
				Debug.LogException(exception4);
			}
		}
	}

	public void Suspend()
	{
		isPaused = true;
	}

	public void Unsuspend()
	{
		isPaused = false;
	}

	protected override void __initializeVariables()
	{
		if (GameplayReadyFrame == null)
		{
			throw new Exception("NetClock.GameplayReadyFrame cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		GameplayReadyFrame.Initialize(this);
		__nameNetworkVariable(GameplayReadyFrame, "GameplayReadyFrame");
		NetworkVariableFields.Add(GameplayReadyFrame);
		if (GameplayStreamActive == null)
		{
			throw new Exception("NetClock.GameplayStreamActive cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		GameplayStreamActive.Initialize(this);
		__nameNetworkVariable(GameplayStreamActive, "GameplayStreamActive");
		NetworkVariableFields.Add(GameplayStreamActive);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3151196681u, __rpc_handler_3151196681, "ArbitraryReliableMessage_ServerRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3151196681(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NetClock)target).ArbitraryReliableMessage_ServerRPC();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "NetClock";
	}
}
