using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;

namespace Endless.Gameplay;

public class UnifiedStateUpdater : EndlessBehaviourSingleton<UnifiedStateUpdater>, IGameEndSubscriber, IAwakeSubscriber, NetClock.ISimulateFrameActorsSubscriber
{
	public delegate void ProcessAiState(ref NpcState currentState);

	public delegate void ConsumeAiState(ref NpcState state);

	private static readonly ProfilerMarker npcCheckWorldTriggers = new ProfilerMarker("Npc Check World Triggers");

	private static readonly ProfilerMarker npcOnProcessTransitions = new ProfilerMarker("Npc Process Transitions");

	private static readonly ProfilerMarker npcCleanupTriggers = new ProfilerMarker("Npc Cleanup Triggers");

	private static readonly ProfilerMarker npcWholeNetFrameMarker = new ProfilerMarker("Npc Whole Net Frame");

	private static readonly ProfilerMarker npcUpdatePerceptionMarker = new ProfilerMarker("Npc Update Perception");

	private static readonly ProfilerMarker npcUpdateAwarenessMarker = new ProfilerMarker("Npc Update Awareness");

	private static readonly ProfilerMarker npcUpdateTargetsMarker = new ProfilerMarker("Npc Update Targets");

	private static readonly ProfilerMarker npcUpdateCombatMarker = new ProfilerMarker("Npc Update Combat");

	private static readonly ProfilerMarker npcUpdatePaths = new ProfilerMarker("Npc Update Paths");

	private static readonly ProfilerMarker npcTickMarker = new ProfilerMarker("Npc Tick");

	private static readonly ProfilerMarker npcProcessRequests = new ProfilerMarker("Npc Process Requests");

	private static readonly ProfilerMarker npcUpdateStateMarker = new ProfilerMarker("Npc Update State");

	private static readonly ProfilerMarker npcWriteStateMarker = new ProfilerMarker("Npc Write State");

	private static readonly ProfilerMarker npcSendStateMarker = new ProfilerMarker("Npc Send State");

	private static readonly ProfilerMarker npcReadStateMarker = new ProfilerMarker("Npc Read State");

	private static readonly List<IAwarenessComponent> awarenessComponents = new List<IAwarenessComponent>();

	private static readonly List<IHostilityComponent> hostilityComponents = new List<IHostilityComponent>();

	public static event Action OnCleanupTriggers;

	public static event Action OnUpdatePaths;

	public static event Action OnUpdateTargets;

	public static event Action<uint> OnUpdateCombat;

	public static event Action<uint> OnCheckWorldTriggers;

	public static event Action OnTickAi;

	public static event Action OnProcessRequests;

	public static event Action OnProcessTransitions;

	public static event Action<uint> OnUpdateState;

	public static event Action<uint> OnWriteState;

	public static event Action<uint> OnSendState;

	public static event Action<uint> OnReadState;

	public static event Action<double> OnInterpolateState;

	public static event Action OnCheckGrounding;

	public void EndlessAwake()
	{
		NetClock.Register(this);
	}

	public void EndlessGameEnd()
	{
		NetClock.Unregister(this);
		awarenessComponents.Clear();
		hostilityComponents.Clear();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		NetClock.Unregister(this);
	}

	public void LateUpdate()
	{
		double obj = (NetworkManager.Singleton.IsHost ? NetClock.ServerAppearanceTime : NetClock.ClientInterpolatedAppearanceTime);
		UnifiedStateUpdater.OnInterpolateState?.Invoke(obj);
	}

	public static void RegisterUpdateComponent(IUpdateComponent updateComponent)
	{
		if (!(updateComponent is IAwarenessComponent item))
		{
			if (updateComponent is IHostilityComponent item2)
			{
				hostilityComponents.Add(item2);
			}
		}
		else
		{
			awarenessComponents.Add(item);
		}
	}

	public static void UnregisterUpdateComponent(IUpdateComponent updateComponent)
	{
		if (!(updateComponent is IAwarenessComponent item))
		{
			if (updateComponent is IHostilityComponent item2)
			{
				hostilityComponents.Remove(item2);
			}
		}
		else
		{
			awarenessComponents.Remove(item);
		}
	}

	public void SimulateFrameActors(uint frame)
	{
		if (base.IsServer)
		{
			UnifiedStateUpdater.OnCheckWorldTriggers?.Invoke(frame);
			UnifiedStateUpdater.OnProcessTransitions?.Invoke();
			UnifiedStateUpdater.OnCleanupTriggers?.Invoke();
			foreach (IHostilityComponent hostilityComponent in hostilityComponents)
			{
				try
				{
					hostilityComponent?.UpdateHostility();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			foreach (IAwarenessComponent awarenessComponent in awarenessComponents)
			{
				try
				{
					awarenessComponent?.UpdateAwareness();
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
			}
			UnifiedStateUpdater.OnUpdateTargets?.Invoke();
			UnifiedStateUpdater.OnUpdateCombat?.Invoke(frame);
			UnifiedStateUpdater.OnTickAi?.Invoke();
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				UnifiedStateUpdater.OnProcessRequests?.Invoke();
				UnifiedStateUpdater.OnUpdateState?.Invoke(frame);
				UnifiedStateUpdater.OnWriteState?.Invoke(frame);
				UnifiedStateUpdater.OnSendState?.Invoke(frame);
				UnifiedStateUpdater.OnReadState?.Invoke(frame);
				UnifiedStateUpdater.OnCheckGrounding?.Invoke();
			}
		}
		else
		{
			if (NetClock.CurrentFrame != frame)
			{
				return;
			}
			foreach (IHostilityComponent hostilityComponent2 in hostilityComponents)
			{
				try
				{
					hostilityComponent2?.UpdateHostility();
				}
				catch (Exception exception3)
				{
					Debug.LogException(exception3);
				}
			}
			foreach (IAwarenessComponent awarenessComponent2 in awarenessComponents)
			{
				try
				{
					awarenessComponent2?.UpdateAwareness();
				}
				catch (Exception exception4)
				{
					Debug.LogException(exception4);
				}
			}
			UnifiedStateUpdater.OnProcessRequests?.Invoke();
			UnifiedStateUpdater.OnUpdateState?.Invoke(frame);
			UnifiedStateUpdater.OnReadState?.Invoke(frame);
			UnifiedStateUpdater.OnCheckGrounding?.Invoke();
		}
	}
}
