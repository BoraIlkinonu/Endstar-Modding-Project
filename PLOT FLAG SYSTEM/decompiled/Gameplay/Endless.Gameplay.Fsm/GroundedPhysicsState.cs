using UnityEngine;

namespace Endless.Gameplay.Fsm;

public class GroundedPhysicsState : FsmState
{
	private float cachedSpeed;

	public override NpcEnum.FsmState State => NpcEnum.FsmState.GroundedPhysics;

	public GroundedPhysicsState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.Entity.Components.IndividualStateUpdater.GetCurrentState().isGrounded = true;
		base.Entity.NpcBlackboard.Set(NpcBlackboard.Key.OverrideSpeed, 1f);
	}

	protected override void Update()
	{
		base.Update();
		Vector3 currentPhysics = base.Components.PhysicsTaker.CurrentPhysics;
		base.Components.CharacterController.SimpleMove(currentPhysics);
	}

	protected override void WriteState(ref NpcState npcState)
	{
		npcState.isGrounded = true;
	}

	protected override void Exit()
	{
		base.Exit();
		base.Components.Animator.SetTrigger(NpcAnimator.PhysicsForceExit);
		base.Entity.Components.IndividualStateUpdater.GetCurrentState().PhysicsForceExit = true;
		base.Entity.NpcBlackboard.Clear<float>(NpcBlackboard.Key.OverrideSpeed);
	}
}
