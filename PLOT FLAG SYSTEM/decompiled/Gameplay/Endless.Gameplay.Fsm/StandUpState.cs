using Endless.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay.Fsm;

public class StandUpState : FsmState
{
	private uint exitFrame;

	private static readonly Collider[] colliders = new Collider[5];

	public override NpcEnum.FsmState State => NpcEnum.FsmState.StandUp;

	public StandUpState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.Components.Animator.SetFloat(NpcAnimator.FallTime, 1.5f);
		base.Components.Animator.SetTrigger(NpcAnimator.Landed);
		ref NpcState currentState = ref base.Entity.Components.IndividualStateUpdater.GetCurrentState();
		currentState.fallTime = 1.5f;
		currentState.landed = true;
		base.Components.Parameters.WillSplat = false;
		exitFrame = NetClock.CurrentFrame + 65;
		base.Entity.Components.IndividualStateUpdater.OnTickAi += HandleOnTickAi;
	}

	protected override void Exit()
	{
		base.Exit();
		base.Entity.Components.IndividualStateUpdater.OnTickAi -= HandleOnTickAi;
		base.Components.Animator.ResetTrigger(NpcAnimator.Landed);
	}

	private void HandleOnTickAi()
	{
		if (NetClock.CurrentFrame >= exitFrame)
		{
			if (!base.Components.Agent.enabled)
			{
				base.Components.Agent.enabled = true;
			}
			else
			{
				SetAppropriateTrigger();
			}
		}
	}

	private void SetAppropriateTrigger()
	{
		Vector3 position;
		if (base.Entity.Components.Grounding.IsGrounded)
		{
			base.Components.Parameters.StandUpCompleteTrigger = true;
		}
		else if (CanJumpToNavMesh(base.Entity, out position))
		{
			base.Components.Parameters.JumpTrigger = true;
			base.Entity.NpcBlackboard.Set(NpcBlackboard.Key.JumpPosition, position);
		}
		else if (MonoBehaviourSingleton<Pathfinding>.Instance.NumSections > 0)
		{
			base.Components.Parameters.WarpTrigger = true;
		}
	}

	private static bool CanJumpToNavMesh(NpcEntity entity, out Vector3 position)
	{
		foreach (Vector3 item in MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(entity.FootPosition, 5f))
		{
			if (NavMesh.SamplePosition(item + Vector3.up * 0.5f, out var hit, 1f, -1) && Physics.OverlapSphereNonAlloc(hit.position, 0.1f, colliders, entity.Settings.CharacterCollisionMask) <= 0)
			{
				position = hit.position;
				return true;
			}
		}
		position = Vector3.zero;
		return false;
	}

	protected override void Update()
	{
		base.Update();
		base.Components.CharacterController.SimpleMove(Vector3.zero);
	}
}
