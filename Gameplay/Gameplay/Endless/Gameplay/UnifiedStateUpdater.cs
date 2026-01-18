using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200024B RID: 587
	public class UnifiedStateUpdater : EndlessBehaviourSingleton<UnifiedStateUpdater>, IGameEndSubscriber, IAwakeSubscriber, NetClock.ISimulateFrameActorsSubscriber
	{
		// Token: 0x14000016 RID: 22
		// (add) Token: 0x06000C21 RID: 3105 RVA: 0x00042168 File Offset: 0x00040368
		// (remove) Token: 0x06000C22 RID: 3106 RVA: 0x0004219C File Offset: 0x0004039C
		public static event Action OnCleanupTriggers;

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x06000C23 RID: 3107 RVA: 0x000421D0 File Offset: 0x000403D0
		// (remove) Token: 0x06000C24 RID: 3108 RVA: 0x00042204 File Offset: 0x00040404
		public static event Action OnUpdatePaths;

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x06000C25 RID: 3109 RVA: 0x00042238 File Offset: 0x00040438
		// (remove) Token: 0x06000C26 RID: 3110 RVA: 0x0004226C File Offset: 0x0004046C
		public static event Action OnUpdateTargets;

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x06000C27 RID: 3111 RVA: 0x000422A0 File Offset: 0x000404A0
		// (remove) Token: 0x06000C28 RID: 3112 RVA: 0x000422D4 File Offset: 0x000404D4
		public static event Action<uint> OnUpdateCombat;

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x06000C29 RID: 3113 RVA: 0x00042308 File Offset: 0x00040508
		// (remove) Token: 0x06000C2A RID: 3114 RVA: 0x0004233C File Offset: 0x0004053C
		public static event Action<uint> OnCheckWorldTriggers;

		// Token: 0x1400001B RID: 27
		// (add) Token: 0x06000C2B RID: 3115 RVA: 0x00042370 File Offset: 0x00040570
		// (remove) Token: 0x06000C2C RID: 3116 RVA: 0x000423A4 File Offset: 0x000405A4
		public static event Action OnTickAi;

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x06000C2D RID: 3117 RVA: 0x000423D8 File Offset: 0x000405D8
		// (remove) Token: 0x06000C2E RID: 3118 RVA: 0x0004240C File Offset: 0x0004060C
		public static event Action OnProcessRequests;

		// Token: 0x1400001D RID: 29
		// (add) Token: 0x06000C2F RID: 3119 RVA: 0x00042440 File Offset: 0x00040640
		// (remove) Token: 0x06000C30 RID: 3120 RVA: 0x00042474 File Offset: 0x00040674
		public static event Action OnProcessTransitions;

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x06000C31 RID: 3121 RVA: 0x000424A8 File Offset: 0x000406A8
		// (remove) Token: 0x06000C32 RID: 3122 RVA: 0x000424DC File Offset: 0x000406DC
		public static event Action<uint> OnUpdateState;

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x06000C33 RID: 3123 RVA: 0x00042510 File Offset: 0x00040710
		// (remove) Token: 0x06000C34 RID: 3124 RVA: 0x00042544 File Offset: 0x00040744
		public static event Action<uint> OnWriteState;

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x06000C35 RID: 3125 RVA: 0x00042578 File Offset: 0x00040778
		// (remove) Token: 0x06000C36 RID: 3126 RVA: 0x000425AC File Offset: 0x000407AC
		public static event Action<uint> OnSendState;

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x06000C37 RID: 3127 RVA: 0x000425E0 File Offset: 0x000407E0
		// (remove) Token: 0x06000C38 RID: 3128 RVA: 0x00042614 File Offset: 0x00040814
		public static event Action<uint> OnReadState;

		// Token: 0x14000022 RID: 34
		// (add) Token: 0x06000C39 RID: 3129 RVA: 0x00042648 File Offset: 0x00040848
		// (remove) Token: 0x06000C3A RID: 3130 RVA: 0x0004267C File Offset: 0x0004087C
		public static event Action<double> OnInterpolateState;

		// Token: 0x14000023 RID: 35
		// (add) Token: 0x06000C3B RID: 3131 RVA: 0x000426B0 File Offset: 0x000408B0
		// (remove) Token: 0x06000C3C RID: 3132 RVA: 0x000426E4 File Offset: 0x000408E4
		public static event Action OnCheckGrounding;

		// Token: 0x06000C3D RID: 3133 RVA: 0x0001CAAE File Offset: 0x0001ACAE
		public void EndlessAwake()
		{
			NetClock.Register(this);
		}

		// Token: 0x06000C3E RID: 3134 RVA: 0x00042717 File Offset: 0x00040917
		public void EndlessGameEnd()
		{
			NetClock.Unregister(this);
			UnifiedStateUpdater.awarenessComponents.Clear();
			UnifiedStateUpdater.hostilityComponents.Clear();
		}

		// Token: 0x06000C3F RID: 3135 RVA: 0x00042733 File Offset: 0x00040933
		protected override void OnDestroy()
		{
			base.OnDestroy();
			NetClock.Unregister(this);
		}

		// Token: 0x06000C40 RID: 3136 RVA: 0x00042744 File Offset: 0x00040944
		public void LateUpdate()
		{
			double num = (NetworkManager.Singleton.IsHost ? NetClock.ServerAppearanceTime : NetClock.ClientInterpolatedAppearanceTime);
			Action<double> onInterpolateState = UnifiedStateUpdater.OnInterpolateState;
			if (onInterpolateState == null)
			{
				return;
			}
			onInterpolateState(num);
		}

		// Token: 0x06000C41 RID: 3137 RVA: 0x0004277C File Offset: 0x0004097C
		public static void RegisterUpdateComponent(IUpdateComponent updateComponent)
		{
			IAwarenessComponent awarenessComponent = updateComponent as IAwarenessComponent;
			if (awarenessComponent != null)
			{
				UnifiedStateUpdater.awarenessComponents.Add(awarenessComponent);
				return;
			}
			IHostilityComponent hostilityComponent = updateComponent as IHostilityComponent;
			if (hostilityComponent == null)
			{
				return;
			}
			UnifiedStateUpdater.hostilityComponents.Add(hostilityComponent);
		}

		// Token: 0x06000C42 RID: 3138 RVA: 0x000427B8 File Offset: 0x000409B8
		public static void UnregisterUpdateComponent(IUpdateComponent updateComponent)
		{
			IAwarenessComponent awarenessComponent = updateComponent as IAwarenessComponent;
			if (awarenessComponent != null)
			{
				UnifiedStateUpdater.awarenessComponents.Remove(awarenessComponent);
				return;
			}
			IHostilityComponent hostilityComponent = updateComponent as IHostilityComponent;
			if (hostilityComponent == null)
			{
				return;
			}
			UnifiedStateUpdater.hostilityComponents.Remove(hostilityComponent);
		}

		// Token: 0x06000C43 RID: 3139 RVA: 0x000427F4 File Offset: 0x000409F4
		public void SimulateFrameActors(uint frame)
		{
			if (!base.IsServer)
			{
				if (NetClock.CurrentFrame == frame)
				{
					foreach (IHostilityComponent hostilityComponent in UnifiedStateUpdater.hostilityComponents)
					{
						try
						{
							if (hostilityComponent != null)
							{
								hostilityComponent.UpdateHostility();
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}
					foreach (IAwarenessComponent awarenessComponent in UnifiedStateUpdater.awarenessComponents)
					{
						try
						{
							if (awarenessComponent != null)
							{
								awarenessComponent.UpdateAwareness();
							}
						}
						catch (Exception ex2)
						{
							Debug.LogException(ex2);
						}
					}
					Action onProcessRequests = UnifiedStateUpdater.OnProcessRequests;
					if (onProcessRequests != null)
					{
						onProcessRequests();
					}
					Action<uint> onUpdateState = UnifiedStateUpdater.OnUpdateState;
					if (onUpdateState != null)
					{
						onUpdateState(frame);
					}
					Action<uint> onReadState = UnifiedStateUpdater.OnReadState;
					if (onReadState != null)
					{
						onReadState(frame);
					}
					Action onCheckGrounding = UnifiedStateUpdater.OnCheckGrounding;
					if (onCheckGrounding == null)
					{
						return;
					}
					onCheckGrounding();
				}
				return;
			}
			Action<uint> onCheckWorldTriggers = UnifiedStateUpdater.OnCheckWorldTriggers;
			if (onCheckWorldTriggers != null)
			{
				onCheckWorldTriggers(frame);
			}
			Action onProcessTransitions = UnifiedStateUpdater.OnProcessTransitions;
			if (onProcessTransitions != null)
			{
				onProcessTransitions();
			}
			Action onCleanupTriggers = UnifiedStateUpdater.OnCleanupTriggers;
			if (onCleanupTriggers != null)
			{
				onCleanupTriggers();
			}
			foreach (IHostilityComponent hostilityComponent2 in UnifiedStateUpdater.hostilityComponents)
			{
				try
				{
					if (hostilityComponent2 != null)
					{
						hostilityComponent2.UpdateHostility();
					}
				}
				catch (Exception ex3)
				{
					Debug.LogException(ex3);
				}
			}
			foreach (IAwarenessComponent awarenessComponent2 in UnifiedStateUpdater.awarenessComponents)
			{
				try
				{
					if (awarenessComponent2 != null)
					{
						awarenessComponent2.UpdateAwareness();
					}
				}
				catch (Exception ex4)
				{
					Debug.LogException(ex4);
				}
			}
			Action onUpdateTargets = UnifiedStateUpdater.OnUpdateTargets;
			if (onUpdateTargets != null)
			{
				onUpdateTargets();
			}
			Action<uint> onUpdateCombat = UnifiedStateUpdater.OnUpdateCombat;
			if (onUpdateCombat != null)
			{
				onUpdateCombat(frame);
			}
			Action onTickAi = UnifiedStateUpdater.OnTickAi;
			if (onTickAi != null)
			{
				onTickAi();
			}
			if (!MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				return;
			}
			Action onProcessRequests2 = UnifiedStateUpdater.OnProcessRequests;
			if (onProcessRequests2 != null)
			{
				onProcessRequests2();
			}
			Action<uint> onUpdateState2 = UnifiedStateUpdater.OnUpdateState;
			if (onUpdateState2 != null)
			{
				onUpdateState2(frame);
			}
			Action<uint> onWriteState = UnifiedStateUpdater.OnWriteState;
			if (onWriteState != null)
			{
				onWriteState(frame);
			}
			Action<uint> onSendState = UnifiedStateUpdater.OnSendState;
			if (onSendState != null)
			{
				onSendState(frame);
			}
			Action<uint> onReadState2 = UnifiedStateUpdater.OnReadState;
			if (onReadState2 != null)
			{
				onReadState2(frame);
			}
			Action onCheckGrounding2 = UnifiedStateUpdater.OnCheckGrounding;
			if (onCheckGrounding2 == null)
			{
				return;
			}
			onCheckGrounding2();
		}

		// Token: 0x04000B30 RID: 2864
		private static readonly ProfilerMarker npcCheckWorldTriggers = new ProfilerMarker("Npc Check World Triggers");

		// Token: 0x04000B31 RID: 2865
		private static readonly ProfilerMarker npcOnProcessTransitions = new ProfilerMarker("Npc Process Transitions");

		// Token: 0x04000B32 RID: 2866
		private static readonly ProfilerMarker npcCleanupTriggers = new ProfilerMarker("Npc Cleanup Triggers");

		// Token: 0x04000B33 RID: 2867
		private static readonly ProfilerMarker npcWholeNetFrameMarker = new ProfilerMarker("Npc Whole Net Frame");

		// Token: 0x04000B34 RID: 2868
		private static readonly ProfilerMarker npcUpdatePerceptionMarker = new ProfilerMarker("Npc Update Perception");

		// Token: 0x04000B35 RID: 2869
		private static readonly ProfilerMarker npcUpdateAwarenessMarker = new ProfilerMarker("Npc Update Awareness");

		// Token: 0x04000B36 RID: 2870
		private static readonly ProfilerMarker npcUpdateTargetsMarker = new ProfilerMarker("Npc Update Targets");

		// Token: 0x04000B37 RID: 2871
		private static readonly ProfilerMarker npcUpdateCombatMarker = new ProfilerMarker("Npc Update Combat");

		// Token: 0x04000B38 RID: 2872
		private static readonly ProfilerMarker npcUpdatePaths = new ProfilerMarker("Npc Update Paths");

		// Token: 0x04000B39 RID: 2873
		private static readonly ProfilerMarker npcTickMarker = new ProfilerMarker("Npc Tick");

		// Token: 0x04000B3A RID: 2874
		private static readonly ProfilerMarker npcProcessRequests = new ProfilerMarker("Npc Process Requests");

		// Token: 0x04000B3B RID: 2875
		private static readonly ProfilerMarker npcUpdateStateMarker = new ProfilerMarker("Npc Update State");

		// Token: 0x04000B3C RID: 2876
		private static readonly ProfilerMarker npcWriteStateMarker = new ProfilerMarker("Npc Write State");

		// Token: 0x04000B3D RID: 2877
		private static readonly ProfilerMarker npcSendStateMarker = new ProfilerMarker("Npc Send State");

		// Token: 0x04000B3E RID: 2878
		private static readonly ProfilerMarker npcReadStateMarker = new ProfilerMarker("Npc Read State");

		// Token: 0x04000B4D RID: 2893
		private static readonly List<IAwarenessComponent> awarenessComponents = new List<IAwarenessComponent>();

		// Token: 0x04000B4E RID: 2894
		private static readonly List<IHostilityComponent> hostilityComponents = new List<IHostilityComponent>();

		// Token: 0x0200024C RID: 588
		// (Invoke) Token: 0x06000C47 RID: 3143
		public delegate void ProcessAiState(ref NpcState currentState);

		// Token: 0x0200024D RID: 589
		// (Invoke) Token: 0x06000C4B RID: 3147
		public delegate void ConsumeAiState(ref NpcState state);
	}
}
