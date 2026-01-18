using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Netcode;
using Unity.Profiling;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000103 RID: 259
	public class NetClock : NetworkBehaviourSingleton<NetClock>
	{
		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060005C7 RID: 1479 RVA: 0x0001CB5F File Offset: 0x0001AD5F
		// (set) Token: 0x060005C8 RID: 1480 RVA: 0x0001CB6B File Offset: 0x0001AD6B
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

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060005C9 RID: 1481 RVA: 0x0001CB78 File Offset: 0x0001AD78
		public static uint CurrentSimulationFrame
		{
			get
			{
				return NetworkBehaviourSingleton<NetClock>.Instance.currentSimulationFrame;
			}
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060005CA RID: 1482 RVA: 0x0001CB84 File Offset: 0x0001AD84
		public static double LocalNetworkTime
		{
			get
			{
				return NetworkManager.Singleton.LocalTime.Time;
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060005CB RID: 1483 RVA: 0x0001CBA3 File Offset: 0x0001ADA3
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

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060005CC RID: 1484 RVA: 0x0001CBC4 File Offset: 0x0001ADC4
		public static double ClientInterpolatedAppearanceTime
		{
			get
			{
				return NetworkManager.Singleton.ServerTime.Time - (double)NetClock.FixedDeltaTime * 1.1;
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060005CD RID: 1485 RVA: 0x0001CBF4 File Offset: 0x0001ADF4
		public static double ServerAppearanceTime
		{
			get
			{
				return NetClock.ClientInterpolatedAppearanceTime;
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x060005CE RID: 1486 RVA: 0x0001CBFB File Offset: 0x0001ADFB
		public static double ClientExtrapolatedAppearanceTime
		{
			get
			{
				return NetClock.ClientExtrapolatedTime - (double)NetClock.FixedDeltaTime * 1.1;
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060005CF RID: 1487 RVA: 0x0001CC14 File Offset: 0x0001AE14
		public static double ClientExtrapolatedTime
		{
			get
			{
				return NetworkManager.Singleton.LocalTime.Time + (double)(NetClock.FixedDeltaTime * 3f);
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060005D0 RID: 1488 RVA: 0x0001CC40 File Offset: 0x0001AE40
		public static float FixedDeltaTime
		{
			get
			{
				return 0.05f;
			}
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060005D1 RID: 1489 RVA: 0x0001CC47 File Offset: 0x0001AE47
		public static uint CurrentRollbackFrame
		{
			get
			{
				if (!NetworkManager.Singleton.IsServer)
				{
					return NetworkBehaviourSingleton<NetClock>.Instance.currentRollbackFrame;
				}
				return NetClock.CurrentFrame;
			}
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x0001CC65 File Offset: 0x0001AE65
		private void Start()
		{
			Physics.simulationMode = SimulationMode.Script;
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x0001CC70 File Offset: 0x0001AE70
		public static void Register(Component component)
		{
			NetClock.ISimulateFrameEarlySubscriber simulateFrameEarlySubscriber = component as NetClock.ISimulateFrameEarlySubscriber;
			if (simulateFrameEarlySubscriber != null)
			{
				NetClock.simulateFrameEarlySubscribers.Add(simulateFrameEarlySubscriber);
			}
			NetClock.ISimulateFrameEnvironmentSubscriber simulateFrameEnvironmentSubscriber = component as NetClock.ISimulateFrameEnvironmentSubscriber;
			if (simulateFrameEnvironmentSubscriber != null)
			{
				NetClock.simulateFrameEnvironmentSubscribers.Add(simulateFrameEnvironmentSubscriber);
			}
			NetClock.ISimulateFrameActorsSubscriber simulateFrameActorsSubscriber = component as NetClock.ISimulateFrameActorsSubscriber;
			if (simulateFrameActorsSubscriber != null)
			{
				NetClock.simulateFrameActorsSubscribers.Add(simulateFrameActorsSubscriber);
			}
			NetClock.ISimulateFrameLateSubscriber simulateFrameLateSubscriber = component as NetClock.ISimulateFrameLateSubscriber;
			if (simulateFrameLateSubscriber != null)
			{
				NetClock.simulateFrameLateSubscribers.Add(simulateFrameLateSubscriber);
			}
			NetClock.IPreFixedUpdateSubscriber preFixedUpdateSubscriber = component as NetClock.IPreFixedUpdateSubscriber;
			if (preFixedUpdateSubscriber != null)
			{
				NetClock.preFixedUpdateSubscribers.Add(preFixedUpdateSubscriber);
			}
			NetClock.IRollbackSubscriber rollbackSubscriber = component as NetClock.IRollbackSubscriber;
			if (rollbackSubscriber != null)
			{
				NetClock.rollbackSubscribers.Add(rollbackSubscriber);
			}
			NetClock.IPostFixedUpdateSubscriber postFixedUpdateSubscriber = component as NetClock.IPostFixedUpdateSubscriber;
			if (postFixedUpdateSubscriber != null)
			{
				NetClock.postFixedUpdateSubscribers.Add(postFixedUpdateSubscriber);
			}
			NetClock.ILoadingFrameSubscriber loadingFrameSubscriber = component as NetClock.ILoadingFrameSubscriber;
			if (loadingFrameSubscriber != null)
			{
				NetClock.loadingFrameSubscribers.Add(loadingFrameSubscriber);
			}
		}

		// Token: 0x060005D4 RID: 1492 RVA: 0x0001CD34 File Offset: 0x0001AF34
		public static void Unregister(Component component)
		{
			NetClock.ISimulateFrameEarlySubscriber simulateFrameEarlySubscriber = component as NetClock.ISimulateFrameEarlySubscriber;
			if (simulateFrameEarlySubscriber != null)
			{
				NetClock.simulateFrameEarlySubscribers.Remove(simulateFrameEarlySubscriber);
			}
			NetClock.ISimulateFrameEnvironmentSubscriber simulateFrameEnvironmentSubscriber = component as NetClock.ISimulateFrameEnvironmentSubscriber;
			if (simulateFrameEnvironmentSubscriber != null)
			{
				NetClock.simulateFrameEnvironmentSubscribers.Remove(simulateFrameEnvironmentSubscriber);
			}
			NetClock.ISimulateFrameActorsSubscriber simulateFrameActorsSubscriber = component as NetClock.ISimulateFrameActorsSubscriber;
			if (simulateFrameActorsSubscriber != null)
			{
				NetClock.simulateFrameActorsSubscribers.Remove(simulateFrameActorsSubscriber);
			}
			NetClock.ISimulateFrameLateSubscriber simulateFrameLateSubscriber = component as NetClock.ISimulateFrameLateSubscriber;
			if (simulateFrameLateSubscriber != null)
			{
				NetClock.simulateFrameLateSubscribers.Remove(simulateFrameLateSubscriber);
			}
			NetClock.IPreFixedUpdateSubscriber preFixedUpdateSubscriber = component as NetClock.IPreFixedUpdateSubscriber;
			if (preFixedUpdateSubscriber != null)
			{
				NetClock.preFixedUpdateSubscribers.Remove(preFixedUpdateSubscriber);
			}
			NetClock.IRollbackSubscriber rollbackSubscriber = component as NetClock.IRollbackSubscriber;
			if (rollbackSubscriber != null)
			{
				NetClock.rollbackSubscribers.Remove(rollbackSubscriber);
			}
			NetClock.IPostFixedUpdateSubscriber postFixedUpdateSubscriber = component as NetClock.IPostFixedUpdateSubscriber;
			if (postFixedUpdateSubscriber != null)
			{
				NetClock.postFixedUpdateSubscribers.Remove(postFixedUpdateSubscriber);
			}
			NetClock.ILoadingFrameSubscriber loadingFrameSubscriber = component as NetClock.ILoadingFrameSubscriber;
			if (loadingFrameSubscriber != null)
			{
				NetClock.loadingFrameSubscribers.Remove(loadingFrameSubscriber);
			}
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x0001CDFD File Offset: 0x0001AFFD
		private void FixedUpdate()
		{
			Physics.Simulate(Time.fixedDeltaTime);
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x0001CE0C File Offset: 0x0001B00C
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (NetworkBehaviourSingleton<NetClock>.Instance.IsServer)
			{
				NetClock.CurrentFrame = (uint)NetworkBehaviourSingleton<NetClock>.Instance.NetworkManager.NetworkTickSystem.ServerTime.Tick;
			}
			else
			{
				NetClock.CurrentFrame = (uint)(NetworkBehaviourSingleton<NetClock>.Instance.NetworkManager.NetworkTickSystem.LocalTime.Tick + 3);
			}
			base.NetworkManager.NetworkTickSystem.Tick += this.NetworkFixedUpdate;
			base.StartCoroutine(this.CalculateRTTLoop());
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x0001CE9A File Offset: 0x0001B09A
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			base.NetworkManager.NetworkTickSystem.Tick -= this.NetworkFixedUpdate;
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x0001CEBE File Offset: 0x0001B0BE
		private IEnumerator CalculateRTTLoop()
		{
			while (base.IsSpawned)
			{
				this.ArbitraryReliableMessage_ServerRPC();
				this.currentNetworkRTT = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(0UL);
				yield return new WaitForSecondsRealtime(0.1f);
			}
			yield break;
		}

		// Token: 0x060005D9 RID: 1497 RVA: 0x0001CED0 File Offset: 0x0001B0D0
		[ServerRpc(RequireOwnership = false)]
		private void ArbitraryReliableMessage_ServerRPC()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3151196681U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 3151196681U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x0001CFA4 File Offset: 0x0001B1A4
		public static double GetFrameTime(uint frame)
		{
			return NetworkManager.Singleton.NetworkTickSystem.LocalTime.FixedTime + (double)(NetClock.FixedDeltaTime * (float)((ulong)frame - (ulong)((long)NetworkManager.Singleton.NetworkTickSystem.LocalTime.Tick)));
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x0001CFEC File Offset: 0x0001B1EC
		public static uint GetFrameFromTime(double networkTime)
		{
			return (uint)(networkTime / (double)NetClock.FixedDeltaTime);
		}

		// Token: 0x060005DC RID: 1500 RVA: 0x0001CFF7 File Offset: 0x0001B1F7
		private void Update()
		{
			if (!base.IsServer)
			{
				this.calculatedRTT = Mathf.SmoothDamp(this.calculatedRTT, this.currentNetworkRTT, ref this.networkRTTSmoothingVelo, this.networkRTTSmoothingTime);
			}
		}

		// Token: 0x060005DD RID: 1501 RVA: 0x0001D024 File Offset: 0x0001B224
		private void NetworkFixedUpdate()
		{
			if (this.isPaused)
			{
				return;
			}
			MonoBehaviourSingleton<RigidbodyManager>.Instance.BeforeSimulation();
			List<NetClock.IPreFixedUpdateSubscriber> list = new List<NetClock.IPreFixedUpdateSubscriber>(NetClock.preFixedUpdateSubscribers);
			List<NetClock.IPostFixedUpdateSubscriber> list2 = new List<NetClock.IPostFixedUpdateSubscriber>(NetClock.postFixedUpdateSubscribers);
			if (base.IsServer)
			{
				uint tick = (uint)base.NetworkManager.NetworkTickSystem.ServerTime.Tick;
				while (NetClock.CurrentFrame <= tick)
				{
					this.currentFrame += 1U;
					foreach (NetClock.IPreFixedUpdateSubscriber preFixedUpdateSubscriber in list)
					{
						try
						{
							if (preFixedUpdateSubscriber != null)
							{
								preFixedUpdateSubscriber.PreFixedUpdate(this.currentFrame);
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}
					if (NetClock.CurrentFrame < this.GameplayReadyFrame.Value)
					{
						using (List<NetClock.ILoadingFrameSubscriber>.Enumerator enumerator2 = new List<NetClock.ILoadingFrameSubscriber>(NetClock.loadingFrameSubscribers).GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								NetClock.ILoadingFrameSubscriber loadingFrameSubscriber = enumerator2.Current;
								try
								{
									if (loadingFrameSubscriber != null)
									{
										loadingFrameSubscriber.LoadingFrame(this.currentFrame);
									}
								}
								catch (Exception ex2)
								{
									Debug.LogException(ex2);
								}
							}
							goto IL_0112;
						}
						goto IL_0107;
					}
					goto IL_0107;
					IL_0112:
					foreach (NetClock.IPostFixedUpdateSubscriber postFixedUpdateSubscriber in list2)
					{
						try
						{
							if (postFixedUpdateSubscriber != null)
							{
								postFixedUpdateSubscriber.PostFixedUpdate(this.currentFrame);
							}
						}
						catch (Exception ex3)
						{
							Debug.LogException(ex3);
						}
					}
					continue;
					IL_0107:
					this.SimulateFrame(NetClock.CurrentFrame);
					goto IL_0112;
				}
			}
			else
			{
				uint tick2 = (uint)base.NetworkManager.NetworkTickSystem.ServerTime.Tick;
				uint tick3 = (uint)base.NetworkManager.NetworkTickSystem.LocalTime.Tick;
				uint num = tick2 - 1U;
				uint num2 = tick3 + 3U;
				this.currentRollbackFrame = num;
				bool flag = NetClock.CurrentFrame >= num2;
				bool flag2 = num2 - num > 12U;
				if (flag || flag2)
				{
					MonoBehaviourSingleton<NetworkStatusIndicator>.Instance.ServerTimeDesync();
					return;
				}
				NetClock.CurrentFrame = num2;
				foreach (NetClock.IPreFixedUpdateSubscriber preFixedUpdateSubscriber2 in list)
				{
					try
					{
						if (preFixedUpdateSubscriber2 != null)
						{
							preFixedUpdateSubscriber2.PreFixedUpdate(this.currentFrame);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
					}
				}
				foreach (NetClock.IRollbackSubscriber rollbackSubscriber in new List<NetClock.IRollbackSubscriber>(NetClock.rollbackSubscribers))
				{
					try
					{
						if (rollbackSubscriber != null)
						{
							rollbackSubscriber.Rollback(num);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
					}
				}
				uint num3 = num + 1U;
				while (num3 <= NetClock.CurrentFrame)
				{
					if (num3 < this.GameplayReadyFrame.Value)
					{
						using (List<NetClock.ILoadingFrameSubscriber>.Enumerator enumerator2 = new List<NetClock.ILoadingFrameSubscriber>(NetClock.loadingFrameSubscribers).GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								NetClock.ILoadingFrameSubscriber loadingFrameSubscriber2 = enumerator2.Current;
								try
								{
									if (loadingFrameSubscriber2 != null)
									{
										loadingFrameSubscriber2.LoadingFrame(NetClock.CurrentFrame);
									}
								}
								catch (Exception ex6)
								{
									Debug.LogException(ex6);
								}
							}
							goto IL_02D8;
						}
						goto IL_02D0;
					}
					goto IL_02D0;
					IL_02D8:
					num3 += 1U;
					continue;
					IL_02D0:
					this.SimulateFrame(num3);
					goto IL_02D8;
				}
				foreach (NetClock.IPostFixedUpdateSubscriber postFixedUpdateSubscriber2 in list2)
				{
					try
					{
						if (postFixedUpdateSubscriber2 != null)
						{
							postFixedUpdateSubscriber2.PostFixedUpdate(this.currentFrame);
						}
					}
					catch (Exception ex7)
					{
						Debug.LogException(ex7);
					}
				}
			}
			MonoBehaviourSingleton<RigidbodyManager>.Instance.AfterSimulation();
		}

		// Token: 0x060005DE RID: 1502 RVA: 0x0001D414 File Offset: 0x0001B614
		private void SimulateFrame(uint frame)
		{
			this.currentSimulationFrame = frame;
			foreach (NetClock.ISimulateFrameEarlySubscriber simulateFrameEarlySubscriber in new List<NetClock.ISimulateFrameEarlySubscriber>(NetClock.simulateFrameEarlySubscribers))
			{
				try
				{
					if (simulateFrameEarlySubscriber != null)
					{
						simulateFrameEarlySubscriber.SimulateFrameEarly(frame);
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			for (int i = 0; i < 3; i++)
			{
				Physics.Simulate(NetClock.FixedDeltaTime / 3f);
			}
			foreach (NetClock.ISimulateFrameEnvironmentSubscriber simulateFrameEnvironmentSubscriber in new List<NetClock.ISimulateFrameEnvironmentSubscriber>(NetClock.simulateFrameEnvironmentSubscribers))
			{
				try
				{
					if (simulateFrameEnvironmentSubscriber != null)
					{
						simulateFrameEnvironmentSubscriber.SimulateFrameEnvironment(frame);
					}
				}
				catch (Exception ex2)
				{
					Debug.LogException(ex2);
				}
			}
			foreach (NetClock.ISimulateFrameActorsSubscriber simulateFrameActorsSubscriber in new List<NetClock.ISimulateFrameActorsSubscriber>(NetClock.simulateFrameActorsSubscribers))
			{
				try
				{
					if (simulateFrameActorsSubscriber != null)
					{
						simulateFrameActorsSubscriber.SimulateFrameActors(frame);
					}
				}
				catch (Exception ex3)
				{
					Debug.LogException(ex3);
				}
			}
			foreach (NetClock.ISimulateFrameLateSubscriber simulateFrameLateSubscriber in new List<NetClock.ISimulateFrameLateSubscriber>(NetClock.simulateFrameLateSubscribers))
			{
				try
				{
					if (simulateFrameLateSubscriber != null)
					{
						simulateFrameLateSubscriber.SimulateFrameLate(frame);
					}
				}
				catch (Exception ex4)
				{
					Debug.LogException(ex4);
				}
			}
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x0001D5CC File Offset: 0x0001B7CC
		public void Suspend()
		{
			this.isPaused = true;
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x0001D5D5 File Offset: 0x0001B7D5
		public void Unsuspend()
		{
			this.isPaused = false;
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x0001D71C File Offset: 0x0001B91C
		protected override void __initializeVariables()
		{
			bool flag = this.GameplayReadyFrame == null;
			if (flag)
			{
				throw new Exception("NetClock.GameplayReadyFrame cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.GameplayReadyFrame.Initialize(this);
			base.__nameNetworkVariable(this.GameplayReadyFrame, "GameplayReadyFrame");
			this.NetworkVariableFields.Add(this.GameplayReadyFrame);
			flag = this.GameplayStreamActive == null;
			if (flag)
			{
				throw new Exception("NetClock.GameplayStreamActive cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.GameplayStreamActive.Initialize(this);
			base.__nameNetworkVariable(this.GameplayStreamActive, "GameplayStreamActive");
			this.NetworkVariableFields.Add(this.GameplayStreamActive);
			base.__initializeVariables();
		}

		// Token: 0x060005E4 RID: 1508 RVA: 0x0001D7CC File Offset: 0x0001B9CC
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3151196681U, new NetworkBehaviour.RpcReceiveHandler(NetClock.__rpc_handler_3151196681), "ArbitraryReliableMessage_ServerRPC");
			base.__initializeRpcs();
		}

		// Token: 0x060005E5 RID: 1509 RVA: 0x0001D7F4 File Offset: 0x0001B9F4
		private static void __rpc_handler_3151196681(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NetClock)target).ArbitraryReliableMessage_ServerRPC();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060005E6 RID: 1510 RVA: 0x0001D845 File Offset: 0x0001BA45
		protected internal override string __getTypeName()
		{
			return "NetClock";
		}

		// Token: 0x04000452 RID: 1106
		private static List<NetClock.ISimulateFrameEarlySubscriber> simulateFrameEarlySubscribers = new List<NetClock.ISimulateFrameEarlySubscriber>();

		// Token: 0x04000453 RID: 1107
		private static List<NetClock.ISimulateFrameEnvironmentSubscriber> simulateFrameEnvironmentSubscribers = new List<NetClock.ISimulateFrameEnvironmentSubscriber>();

		// Token: 0x04000454 RID: 1108
		private static List<NetClock.ISimulateFrameActorsSubscriber> simulateFrameActorsSubscribers = new List<NetClock.ISimulateFrameActorsSubscriber>();

		// Token: 0x04000455 RID: 1109
		private static List<NetClock.ISimulateFrameLateSubscriber> simulateFrameLateSubscribers = new List<NetClock.ISimulateFrameLateSubscriber>();

		// Token: 0x04000456 RID: 1110
		private static List<NetClock.IPreFixedUpdateSubscriber> preFixedUpdateSubscribers = new List<NetClock.IPreFixedUpdateSubscriber>();

		// Token: 0x04000457 RID: 1111
		private static List<NetClock.IRollbackSubscriber> rollbackSubscribers = new List<NetClock.IRollbackSubscriber>();

		// Token: 0x04000458 RID: 1112
		private static List<NetClock.IPostFixedUpdateSubscriber> postFixedUpdateSubscribers = new List<NetClock.IPostFixedUpdateSubscriber>();

		// Token: 0x04000459 RID: 1113
		private static List<NetClock.ILoadingFrameSubscriber> loadingFrameSubscribers = new List<NetClock.ILoadingFrameSubscriber>();

		// Token: 0x0400045A RID: 1114
		private const float NET_FRAME_FIXED_DELTA_TIME = 0.05f;

		// Token: 0x0400045B RID: 1115
		private const int NETWORKED_PHYSICS_SUB_FRAMES = 3;

		// Token: 0x0400045C RID: 1116
		private const uint ADDITIONAL_ROLLBACK_FRAMES = 1U;

		// Token: 0x0400045D RID: 1117
		private const uint ADDITIONAL_EXTRAPOLATION_FRAMES = 3U;

		// Token: 0x0400045E RID: 1118
		private const uint MAX_SIMULATION_FRAMES = 12U;

		// Token: 0x0400045F RID: 1119
		private ProfilerMarker NetClock_RigidbodyBeforeSimulationMarker = new ProfilerMarker("NetClock_RigidbodyBeforeSimulationMarker");

		// Token: 0x04000460 RID: 1120
		private ProfilerMarker NetClock_PreFixedUpdateMarker = new ProfilerMarker("NetClock_PreFixedUpdateMarker");

		// Token: 0x04000461 RID: 1121
		private ProfilerMarker NetClock_SimulateFrameMarker = new ProfilerMarker("NetClock_SimulateFrameMarker");

		// Token: 0x04000462 RID: 1122
		private ProfilerMarker NetClock_PostFixedUpdateMarker = new ProfilerMarker("NetClock_PostFixedUpdateMarker");

		// Token: 0x04000463 RID: 1123
		private ProfilerMarker NetClock_RigidbodyManagerAfterSimulationMarker = new ProfilerMarker("NetClock_RigidbodyManagerAfterSimulationMarker");

		// Token: 0x04000464 RID: 1124
		private ProfilerMarker NetClock_SimulateFrameEarlyMarker = new ProfilerMarker("NetClock_SimulateFrameEarlyMarker");

		// Token: 0x04000465 RID: 1125
		private ProfilerMarker NetClock_SimulaterameEnvironmentMarker = new ProfilerMarker("NetClock_SimulaterameEnvironmentMarker");

		// Token: 0x04000466 RID: 1126
		private ProfilerMarker NetClock_SimulateFrameActorsMarker = new ProfilerMarker("NetClock_SimulateFrameActorsMarker");

		// Token: 0x04000467 RID: 1127
		private ProfilerMarker NetClock_SimulateFrameLateMarker = new ProfilerMarker("NetClock_SimulateFrameLateMarker");

		// Token: 0x04000468 RID: 1128
		private ProfilerMarker NetClock_Physics = new ProfilerMarker("NetClock_Physics");

		// Token: 0x04000469 RID: 1129
		public NetworkVariable<uint> GameplayReadyFrame = new NetworkVariable<uint>(uint.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x0400046A RID: 1130
		public NetworkVariable<bool> GameplayStreamActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x0400046B RID: 1131
		private bool ready;

		// Token: 0x0400046C RID: 1132
		private uint currentFrame;

		// Token: 0x0400046D RID: 1133
		private uint currentRollbackFrame;

		// Token: 0x0400046E RID: 1134
		private uint currentSimulationFrame;

		// Token: 0x0400046F RID: 1135
		private float calculatedRTT;

		// Token: 0x04000470 RID: 1136
		private float currentNetworkRTT;

		// Token: 0x04000471 RID: 1137
		private float networkRTTSmoothingVelo;

		// Token: 0x04000472 RID: 1138
		private float networkRTTSmoothingTime = 3f;

		// Token: 0x04000473 RID: 1139
		private bool isPaused;

		// Token: 0x02000104 RID: 260
		public interface ISimulateFrameEarlySubscriber
		{
			// Token: 0x060005E7 RID: 1511
			void SimulateFrameEarly(uint frame);
		}

		// Token: 0x02000105 RID: 261
		public interface ISimulateFrameEnvironmentSubscriber
		{
			// Token: 0x060005E8 RID: 1512
			void SimulateFrameEnvironment(uint frame);
		}

		// Token: 0x02000106 RID: 262
		public interface ISimulateFrameActorsSubscriber
		{
			// Token: 0x060005E9 RID: 1513
			void SimulateFrameActors(uint frame);
		}

		// Token: 0x02000107 RID: 263
		public interface ISimulateFrameLateSubscriber
		{
			// Token: 0x060005EA RID: 1514
			void SimulateFrameLate(uint frame);
		}

		// Token: 0x02000108 RID: 264
		public interface IPreFixedUpdateSubscriber
		{
			// Token: 0x060005EB RID: 1515
			void PreFixedUpdate(uint frame);
		}

		// Token: 0x02000109 RID: 265
		public interface IRollbackSubscriber
		{
			// Token: 0x060005EC RID: 1516
			void Rollback(uint frame);
		}

		// Token: 0x0200010A RID: 266
		public interface IPostFixedUpdateSubscriber
		{
			// Token: 0x060005ED RID: 1517
			void PostFixedUpdate(uint frame);
		}

		// Token: 0x0200010B RID: 267
		public interface ILoadingFrameSubscriber
		{
			// Token: 0x060005EE RID: 1518
			void LoadingFrame(uint frame);
		}
	}
}
