using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class CutsceneManager : EndlessNetworkBehaviourSingleton<CutsceneManager>, IGameEndSubscriber
{
	public struct InProgressState : INetworkSerializable
	{
		public bool LateJoin;

		public double StartTime;

		public float ShotDuration;

		public CutsceneCamera.TargetInfo FollowInfo;

		public CutsceneCamera.TargetInfo LookAtInfo;

		public CutsceneCamera.MoveToInfo MoveToInfo;

		public InProgressState(bool lateJoin, float shotDuration, CutsceneCamera.TargetInfo follow, CutsceneCamera.TargetInfo lookAt, CutsceneCamera.MoveToInfo moveToInfo, double startTime = 0.0)
		{
			LateJoin = lateJoin;
			StartTime = startTime;
			ShotDuration = shotDuration;
			FollowInfo = follow;
			LookAtInfo = lookAt;
			MoveToInfo = moveToInfo;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref LateJoin, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref ShotDuration, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref FollowInfo, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue(ref LookAtInfo, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue(ref MoveToInfo, default(FastBufferWriter.ForNetworkSerializable));
			if (LateJoin)
			{
				serializer.SerializeValue(ref StartTime, default(FastBufferWriter.ForPrimitives));
			}
		}
	}

	private class GroupCutsceneState
	{
		public List<ulong> IncludedPlayers = new List<ulong>();

		public InputSettings InputSettings;

		public bool Invulnerable;

		private CutsceneCamera.TransitionInfo mostRecentTransition;

		private double ShotStartTime;

		private bool pendingInternalEnd;

		private CutsceneCamera.TargetInfo followInfo;

		private CutsceneCamera.TargetInfo lookAtInfo;

		private Coroutine transitionTimerCoroutine;

		public Scope CutsceneScope { get; private set; }

		public CutsceneCamera CurrentCutsceneCamera { get; private set; }

		public Context ManagingContext { get; private set; }

		public GroupCutsceneState(CutsceneCamera cutsceneCamera, Scope scope, Context managingContext, List<ulong> startingPlayers)
		{
			CutsceneScope = scope;
			InputSettings = cutsceneCamera.InputAllowed;
			Invulnerable = cutsceneCamera.InvulnerablePlayer;
			ManagingContext = managingContext;
			ShotStartTime = NetworkManager.Singleton.ServerTime.Time;
			foreach (ulong startingPlayer in startingPlayers)
			{
				uint num = (uint)startingPlayer;
				SetupPlayer(num);
			}
			TransitionTo(cutsceneCamera);
		}

		public void AddPlayer(ulong playerId)
		{
			SetupPlayer(playerId);
			ClientRpcParams rpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[1] { playerId }
				}
			};
			NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterCutscene_ClientRpc(CurrentCutsceneCamera.NetworkObject, mostRecentTransition, InputSettings, Invulnerable, new InProgressState(lateJoin: true, CurrentCutsceneCamera.BaseShotDuration, followInfo, lookAtInfo, CurrentCutsceneCamera.MoveToInfo_Server, ShotStartTime), rpcParams);
		}

		private void SetupPlayer(ulong playerId)
		{
			IncludedPlayers.Add(playerId);
			MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(playerId).HittableComponent.IsDamageable = !Invulnerable;
		}

		public void TransitionTo(CutsceneCamera cutsceneCamera)
		{
			ResetDamageable();
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				if ((bool)CurrentCutsceneCamera && !pendingInternalEnd)
				{
					pendingInternalEnd = true;
					CurrentCutsceneCamera.OnShotInterrupted.Invoke(ManagingContext);
				}
				followInfo = cutsceneCamera.CurrentFollowInfo_Server;
				lookAtInfo = cutsceneCamera.CurrentLookAtInfo_Server;
				mostRecentTransition = cutsceneCamera.TransitionIn_Server;
				InputSettings = cutsceneCamera.InputAllowed;
				Invulnerable = cutsceneCamera.InvulnerablePlayer;
				CurrentCutsceneCamera = cutsceneCamera;
				ShotStartTime = NetworkManager.Singleton.ServerTime.Time;
				pendingInternalEnd = false;
				if (transitionTimerCoroutine != null)
				{
					NetworkBehaviourSingleton<CutsceneManager>.Instance.StopCoroutine(transitionTimerCoroutine);
					transitionTimerCoroutine = null;
				}
				transitionTimerCoroutine = NetworkBehaviourSingleton<CutsceneManager>.Instance.StartCoroutine(TransitionTimerCoroutine(mostRecentTransition, cutsceneCamera.TotalShotDuration));
				ClientRpcParams rpcParams = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = IncludedPlayers
					}
				};
				NetworkBehaviourSingleton<CutsceneManager>.Instance.EnterCutscene_ClientRpc(cutsceneCamera.NetworkObject, mostRecentTransition, InputSettings, Invulnerable, new InProgressState(lateJoin: false, CurrentCutsceneCamera.BaseShotDuration, followInfo, lookAtInfo, cutsceneCamera.MoveToInfo_Server), rpcParams);
			}
		}

		public void ResetDamageable()
		{
			foreach (ulong includedPlayer in IncludedPlayers)
			{
				MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(includedPlayer).HittableComponent.IsDamageable = true;
			}
		}

		public void End()
		{
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				if ((bool)CurrentCutsceneCamera && !pendingInternalEnd)
				{
					pendingInternalEnd = true;
					CurrentCutsceneCamera.OnShotInterrupted.Invoke(ManagingContext);
				}
				ResetDamageable();
				ClientRpcParams rpcParams = new ClientRpcParams
				{
					Send = new ClientRpcSendParams
					{
						TargetClientIds = IncludedPlayers
					}
				};
				NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitCutscene_ClientRpc(CurrentCutsceneCamera.TransitionOut_Server, rpcParams);
			}
		}

		private IEnumerator TransitionTimerCoroutine(CutsceneCamera.TransitionInfo transitionInfo, float shotDuration)
		{
			if (transitionInfo.Type == CameraTransition.Fade)
			{
				float halfDur = transitionInfo.Duration / 2f;
				yield return new WaitForSeconds(halfDur);
				CurrentCutsceneCamera.OnShotStarted.Invoke(ManagingContext);
				yield return new WaitForSeconds(halfDur);
			}
			else if (transitionInfo.Type == CameraTransition.Cut)
			{
				CurrentCutsceneCamera.OnShotStarted.Invoke(ManagingContext);
			}
			else
			{
				yield return new WaitForSeconds(transitionInfo.Duration);
				CurrentCutsceneCamera.OnShotStarted.Invoke(ManagingContext);
			}
			if (shotDuration >= 0f)
			{
				yield return new WaitForSeconds(shotDuration);
				if (!pendingInternalEnd)
				{
					pendingInternalEnd = true;
					CurrentCutsceneCamera.OnShotFinished.Invoke(ManagingContext);
				}
				yield return null;
				switch (CutsceneScope)
				{
				case Scope.Global:
					NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitGlobalCutscene(CurrentCutsceneCamera);
					break;
				case Scope.Local:
					NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitLocalCutscene(ManagingContext);
					break;
				default:
					if (IncludedPlayers.Count > 0)
					{
						NetworkBehaviourSingleton<CutsceneManager>.Instance.ExitPrivateCutscene(IncludedPlayers[0], CurrentCutsceneCamera);
					}
					break;
				}
			}
			transitionTimerCoroutine = null;
		}
	}

	private Dictionary<Context, GroupCutsceneState> activeLocalCutscenes = new Dictionary<Context, GroupCutsceneState>();

	private Dictionary<ulong, GroupCutsceneState> playerCutsceneStates = new Dictionary<ulong, GroupCutsceneState>();

	private GroupCutsceneState activeGlobalCutscene;

	private Coroutine activeExitCutsceneCoroutine;

	public InputSettings CurrentCutsceneInputSettings { get; private set; }

	private void OnEnable()
	{
		MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(HandlePlayerJoined);
		MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.AddListener(HandlePlayerLeft);
	}

	private void OnDisable()
	{
		MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(HandlePlayerJoined);
		MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.RemoveListener(HandlePlayerLeft);
	}

	private void HandlePlayerJoined(ulong playedId, PlayerReferenceManager references)
	{
		if (activeGlobalCutscene != null)
		{
			activeGlobalCutscene.AddPlayer(playedId);
		}
	}

	private void HandlePlayerLeft(ulong playedId, PlayerReferenceManager references)
	{
		if (activeGlobalCutscene != null)
		{
			activeGlobalCutscene.IncludedPlayers.Remove(playedId);
		}
		else
		{
			if (!playerCutsceneStates.TryGetValue(playedId, out var state))
			{
				return;
			}
			if (state.CutsceneScope == Scope.Private)
			{
				playerCutsceneStates.Remove(playedId);
				return;
			}
			if (state.IncludedPlayers.Count > 1)
			{
				state.IncludedPlayers.Remove(playedId);
				playerCutsceneStates.Remove(playedId);
				return;
			}
			Context key = activeLocalCutscenes.FirstOrDefault((KeyValuePair<Context, GroupCutsceneState> x) => x.Value == state).Key;
			if (key != null)
			{
				activeLocalCutscenes.Remove(key);
			}
			playerCutsceneStates.Remove(playedId);
		}
	}

	public void EnterPrivateCutscene(Context playerContext, CutsceneCamera cutsceneCamera)
	{
		if (activeGlobalCutscene != null)
		{
			return;
		}
		ulong ownerClientId = playerContext.WorldObject.NetworkObject.OwnerClientId;
		if (cutsceneCamera == null)
		{
			return;
		}
		if (playerCutsceneStates.TryGetValue(ownerClientId, out var value))
		{
			if (value.CutsceneScope == Scope.Private)
			{
				value.TransitionTo(cutsceneCamera);
			}
		}
		else
		{
			value = new GroupCutsceneState(cutsceneCamera, Scope.Private, playerContext, new List<ulong> { ownerClientId });
			playerCutsceneStates.Add(ownerClientId, value);
		}
	}

	public void ExitPrivateCutscene(ulong playerId, CutsceneCamera cutsceneCamera)
	{
		if (activeGlobalCutscene == null && playerCutsceneStates.TryGetValue(playerId, out var value) && value.CutsceneScope == Scope.Private && value.CurrentCutsceneCamera == cutsceneCamera)
		{
			value.End();
			playerCutsceneStates.Remove(playerId);
		}
	}

	public void EnterGlobalCutscene(CutsceneCamera cutsceneCamera)
	{
		if (!(cutsceneCamera == null))
		{
			if (activeGlobalCutscene != null)
			{
				activeGlobalCutscene.TransitionTo(cutsceneCamera);
				return;
			}
			activeGlobalCutscene = new GroupCutsceneState(cutsceneCamera, Scope.Global, Game.Instance.GetGameContext(), MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerIds());
			playerCutsceneStates.Clear();
			activeLocalCutscenes.Clear();
		}
	}

	public void ExitGlobalCutscene(CutsceneCamera cutsceneCamera)
	{
		if (activeGlobalCutscene != null && activeGlobalCutscene.CurrentCutsceneCamera == cutsceneCamera)
		{
			activeGlobalCutscene.End();
			activeGlobalCutscene = null;
		}
	}

	public bool CheckIfManagedCutsceneExists(Context managingContext)
	{
		if (activeGlobalCutscene != null)
		{
			return false;
		}
		return activeLocalCutscenes.ContainsKey(managingContext);
	}

	public void EnterLocalCutscene(Context context, CutsceneCamera cutsceneCamera, Context manager)
	{
		if (activeGlobalCutscene != null)
		{
			return;
		}
		bool flag = context?.IsPlayer() ?? false;
		ulong num = (flag ? context.WorldObject.NetworkObject.OwnerClientId : 0);
		if (activeLocalCutscenes.TryGetValue(manager, out var value))
		{
			if (flag)
			{
				if (value.IncludedPlayers.Contains(num))
				{
					if (!(cutsceneCamera == null))
					{
						value.TransitionTo(cutsceneCamera);
					}
				}
				else
				{
					value.AddPlayer(num);
					playerCutsceneStates.Add(num, value);
				}
			}
			else if (!(cutsceneCamera == null))
			{
				value.TransitionTo(cutsceneCamera);
			}
		}
		else if (!(cutsceneCamera == null) && flag)
		{
			value = new GroupCutsceneState(cutsceneCamera, Scope.Local, manager, new List<ulong> { num });
			activeLocalCutscenes.Add(manager, value);
			playerCutsceneStates.Add(num, value);
		}
	}

	public void ExitLocalCutscene(Context manager)
	{
		if (activeGlobalCutscene != null || !activeLocalCutscenes.TryGetValue(manager, out var value))
		{
			return;
		}
		value.End();
		activeLocalCutscenes.Remove(manager);
		foreach (ulong includedPlayer in value.IncludedPlayers)
		{
			playerCutsceneStates.Remove(includedPlayer);
		}
	}

	[ClientRpc]
	private void EnterCutscene_ClientRpc(NetworkObjectReference targetController, CutsceneCamera.TransitionInfo transition, InputSettings inputSettings, bool invulnerable, InProgressState inProgressState, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(638214693u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in targetController, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in transition, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in inputSettings, default(FastBufferWriter.ForEnums));
			bufferWriter.WriteValueSafe(in invulnerable, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in inProgressState, default(FastBufferWriter.ForNetworkSerializable));
			__endSendClientRpc(ref bufferWriter, 638214693u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if ((bool)PlayerReferenceManager.LocalInstance)
		{
			PlayerReferenceManager.LocalInstance.HittableComponent.IsDamageable = !invulnerable;
		}
		NetworkObject networkObject = targetController;
		if (networkObject != null)
		{
			CutsceneCamera componentInChildren = networkObject.GetComponentInChildren<CutsceneCamera>();
			if ((bool)componentInChildren)
			{
				MonoBehaviourSingleton<CameraController>.Instance.TransitionToCutsceneCamera(componentInChildren, transition.Type, transition.Duration, inProgressState.FollowInfo, inProgressState.LookAtInfo);
				componentInChildren.JoinInProgress(inProgressState);
				MonoBehaviourSingleton<InputManager>.Instance.SetCinematicInputState(blockInput: true);
				CurrentCutsceneInputSettings = inputSettings;
			}
			if (activeExitCutsceneCoroutine != null)
			{
				StopCoroutine(activeExitCutsceneCoroutine);
			}
			activeExitCutsceneCoroutine = null;
		}
	}

	[ClientRpc]
	private void ExitCutscene_ClientRpc(CutsceneCamera.TransitionInfo transition, ClientRpcParams rpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(3565536771u, rpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in transition, default(FastBufferWriter.ForNetworkSerializable));
			__endSendClientRpc(ref bufferWriter, 3565536771u, rpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (activeExitCutsceneCoroutine != null)
			{
				StopCoroutine(activeExitCutsceneCoroutine);
			}
			activeExitCutsceneCoroutine = StartCoroutine(ExitCutsceneCoroutine(transition));
		}
	}

	private IEnumerator ExitCutsceneCoroutine(CutsceneCamera.TransitionInfo transition)
	{
		MonoBehaviourSingleton<CameraController>.Instance.ExitCutscene(transition.Type, transition.Duration);
		yield return new WaitForSeconds(transition.Duration);
		if ((bool)PlayerReferenceManager.LocalInstance)
		{
			PlayerReferenceManager.LocalInstance.HittableComponent.IsDamageable = true;
		}
		MonoBehaviourSingleton<InputManager>.Instance.SetCinematicInputState(blockInput: false);
	}

	public void EndlessGameEnd()
	{
		MonoBehaviourSingleton<InputManager>.Instance.SetCinematicInputState(blockInput: false);
		if (!base.IsServer)
		{
			return;
		}
		if (activeGlobalCutscene != null)
		{
			activeGlobalCutscene.ResetDamageable();
		}
		foreach (KeyValuePair<ulong, GroupCutsceneState> playerCutsceneState in playerCutsceneStates)
		{
			playerCutsceneState.Value.ResetDamageable();
		}
		activeGlobalCutscene = null;
		playerCutsceneStates.Clear();
		activeLocalCutscenes.Clear();
		StopAllCoroutines();
		activeExitCutsceneCoroutine = null;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(638214693u, __rpc_handler_638214693, "EnterCutscene_ClientRpc");
		__registerRpc(3565536771u, __rpc_handler_3565536771, "ExitCutscene_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_638214693(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out NetworkObjectReference value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out CutsceneCamera.TransitionInfo value2, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out InputSettings value3, default(FastBufferWriter.ForEnums));
			reader.ReadValueSafe(out bool value4, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out InProgressState value5, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CutsceneManager)target).EnterCutscene_ClientRpc(value, value2, value3, value4, value5, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3565536771(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out CutsceneCamera.TransitionInfo value, default(FastBufferWriter.ForNetworkSerializable));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CutsceneManager)target).ExitCutscene_ClientRpc(value, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "CutsceneManager";
	}
}
