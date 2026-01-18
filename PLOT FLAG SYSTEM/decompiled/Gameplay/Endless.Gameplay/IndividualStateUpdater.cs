using System;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class IndividualStateUpdater : NpcComponent, IStartSubscriber
{
	private NpcState currentState;

	private readonly InterpolationRingBuffer<NpcState> aiStates = new InterpolationRingBuffer<NpcState>(30);

	public readonly RingBuffer<NpcState> ClientStates = new RingBuffer<NpcState>(30);

	private bool isBoundToEvents;

	private bool hasReceivedData;

	public event Action OnCleanupTriggers;

	public event Action<uint> OnCheckWorldTriggers;

	public event Action OnTickAi;

	public event Action OnProcessTransitions;

	public event Action<uint> OnUpdateState;

	public event UnifiedStateUpdater.ProcessAiState OnWriteState;

	public event UnifiedStateUpdater.ConsumeAiState OnReadState;

	public event Action<NpcState> OnStateInterpolated;

	public event Action OnCheckGroundState;

	private void WriteState(ref NpcState netState)
	{
		this.OnWriteState?.Invoke(ref netState);
	}

	private void ReadState(ref NpcState state)
	{
		this.OnReadState?.Invoke(ref state);
	}

	private void StateInterpolated(NpcState npcState)
	{
		this.OnStateInterpolated?.Invoke(npcState);
	}

	protected void OnDisable()
	{
		UnbindFromEvents();
	}

	private void UnbindFromEvents()
	{
		UnifiedStateUpdater.OnCheckWorldTriggers -= HandleOnCheckWorldTriggers;
		UnifiedStateUpdater.OnProcessTransitions -= HandleOnProcessTransitions;
		UnifiedStateUpdater.OnCleanupTriggers -= HandleOnCleanupTriggers;
		UnifiedStateUpdater.OnTickAi -= HandleOnTickAi;
		UnifiedStateUpdater.OnUpdateState -= HandleOnUpdateState;
		UnifiedStateUpdater.OnWriteState -= HandleOnWriteState;
		UnifiedStateUpdater.OnSendState -= HandleOnSendState;
		UnifiedStateUpdater.OnReadState -= HandleOnReadState;
		UnifiedStateUpdater.OnInterpolateState -= HandleOnInterpolateState;
		UnifiedStateUpdater.OnCheckGrounding -= HandleOnCheckGrounding;
	}

	private void HandleOnCheckGrounding()
	{
		this.OnCheckGroundState?.Invoke();
	}

	private void BindToServerEvents()
	{
		hasReceivedData = base.IsServer;
		UnifiedStateUpdater.OnCheckWorldTriggers += HandleOnCheckWorldTriggers;
		UnifiedStateUpdater.OnProcessTransitions += HandleOnProcessTransitions;
		UnifiedStateUpdater.OnCleanupTriggers += HandleOnCleanupTriggers;
		UnifiedStateUpdater.OnTickAi += HandleOnTickAi;
		UnifiedStateUpdater.OnUpdateState += HandleOnUpdateState;
		UnifiedStateUpdater.OnWriteState += HandleOnWriteState;
		UnifiedStateUpdater.OnSendState += HandleOnSendState;
		UnifiedStateUpdater.OnCheckGrounding += HandleOnCheckGrounding;
	}

	private void BindToClientEvents()
	{
		aiStates.OnStatesShifted.AddListener(HandleOnStatesShifted);
		UnifiedStateUpdater.OnUpdateState += HandleOnUpdateState;
		UnifiedStateUpdater.OnCheckGrounding += HandleOnCheckGrounding;
		UnifiedStateUpdater.OnInterpolateState += HandleOnInterpolateState;
		UnifiedStateUpdater.OnReadState += HandleOnReadState;
	}

	private void HandleOnStatesShifted(NpcState previousState, NpcState newState)
	{
		base.NpcEntity.Components.Animator.SetBool(NpcAnimator.Moving, newState.isMoving);
		base.NpcEntity.Components.Animator.SetFloat(NpcAnimator.FallTime, newState.fallTime);
		base.NpcEntity.Components.Animator.SetBool(NpcAnimator.Grounded, newState.isGrounded);
		if (newState.LargePush)
		{
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.LargePush);
		}
		if (newState.jumped)
		{
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Jump);
		}
		if (newState.landed)
		{
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Landed);
		}
		if (newState.PhysicsForceExit)
		{
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.PhysicsForceExit);
		}
		if (newState.SmallPush)
		{
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.SmallPush);
		}
		if (newState.EndSmallPush)
		{
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.EndSmallPush);
		}
		if (newState.LoopSmallPush)
		{
			base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.LoopSmallPush);
		}
		if (newState.ImminentlyAttacking)
		{
			base.NpcEntity.Components.AttackAlert.ImminentlyAttacking(0u);
		}
	}

	private void HandleOnCheckWorldTriggers(uint frame)
	{
		this.OnCheckWorldTriggers?.Invoke(frame);
	}

	private void HandleOnProcessTransitions()
	{
		this.OnProcessTransitions?.Invoke();
	}

	private void HandleOnCleanupTriggers()
	{
		this.OnCleanupTriggers?.Invoke();
	}

	private void HandleOnTickAi()
	{
		this.OnTickAi?.Invoke();
	}

	private void HandleOnUpdateState(uint frame)
	{
		this.OnUpdateState?.Invoke(frame);
	}

	private void HandleOnWriteState(uint frame)
	{
		ref NpcState reference = ref GetCurrentState();
		reference.NetFrame = frame;
		WriteState(ref reference);
		aiStates.UpdateValue(ref reference);
	}

	private void HandleOnSendState(uint obj)
	{
		GameplayMessagingManager.SendAiState(currentState, (uint)base.NpcEntity.WorldObject.NetworkObject.NetworkObjectId);
		currentState.Clear();
	}

	private void HandleOnReadState(uint frame)
	{
		ref NpcState referenceFromBuffer = ref aiStates.GetReferenceFromBuffer(frame);
		ReadState(ref referenceFromBuffer);
		referenceFromBuffer.Clear();
	}

	private void HandleOnInterpolateState(double interpolationTime)
	{
		if (hasReceivedData)
		{
			aiStates.ActiveInterpolationTime = interpolationTime;
			if (Vector3.Distance(aiStates.PastInterpolationState.Position, aiStates.NextInterpolationState.Position) > 1f)
			{
				aiStates.ActiveInterpolatedState.Position = aiStates.NextInterpolationState.Position;
			}
			else
			{
				aiStates.ActiveInterpolatedState.Position = Vector3.Lerp(aiStates.PastInterpolationState.Position, aiStates.NextInterpolationState.Position, aiStates.ActiveStateLerpTime);
			}
			aiStates.ActiveInterpolatedState.Rotation = Mathf.Lerp(aiStates.PastInterpolationState.Rotation, aiStates.NextInterpolationState.Rotation, aiStates.ActiveStateLerpTime);
			aiStates.ActiveInterpolatedState.slopeAngle = Mathf.Lerp(aiStates.PastInterpolationState.slopeAngle, aiStates.NextInterpolationState.slopeAngle, aiStates.ActiveStateLerpTime);
			aiStates.ActiveInterpolatedState.VelX = Mathf.Lerp(aiStates.PastInterpolationState.VelX, aiStates.NextInterpolationState.VelX, aiStates.ActiveStateLerpTime);
			aiStates.ActiveInterpolatedState.VelY = Mathf.Lerp(aiStates.PastInterpolationState.VelY, aiStates.NextInterpolationState.VelY, aiStates.ActiveStateLerpTime);
			aiStates.ActiveInterpolatedState.VelZ = Mathf.Lerp(aiStates.PastInterpolationState.VelZ, aiStates.NextInterpolationState.VelZ, aiStates.ActiveStateLerpTime);
			aiStates.ActiveInterpolatedState.AngularVelocity = Mathf.Lerp(aiStates.PastInterpolationState.AngularVelocity, aiStates.NextInterpolationState.AngularVelocity, aiStates.ActiveStateLerpTime);
			aiStates.ActiveInterpolatedState.HorizVelMagnitude = Mathf.Lerp(aiStates.PastInterpolationState.HorizVelMagnitude, aiStates.NextInterpolationState.HorizVelMagnitude, aiStates.ActiveStateLerpTime);
			StateInterpolated(aiStates.ActiveInterpolatedState);
		}
	}

	public static void ReceiveAiStates(uint key, NpcState simulatedState)
	{
		if (MonoBehaviourSingleton<NetworkedWorldObjectMap>.Instance.ObjectMap.TryGetValue(key, out var value))
		{
			value.GetUserComponent<NpcEntity>().Components.IndividualStateUpdater.UpdateStates(simulatedState);
		}
	}

	private void UpdateStates(NpcState simulatedState)
	{
		hasReceivedData = true;
		aiStates.UpdateValue(ref simulatedState);
	}

	public ref NpcState GetCurrentState()
	{
		return ref currentState;
	}

	public void EndlessStart()
	{
		if (base.IsServer)
		{
			BindToServerEvents();
		}
		else
		{
			BindToClientEvents();
		}
	}
}
