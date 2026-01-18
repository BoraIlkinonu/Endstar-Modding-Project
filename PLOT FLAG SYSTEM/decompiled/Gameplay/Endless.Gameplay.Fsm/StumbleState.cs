using UnityEngine;

namespace Endless.Gameplay.Fsm;

public class StumbleState : FsmState
{
	private const float STOPPING_THRESHOLD = 0.4f;

	private bool isStopping;

	private bool loopTrigger;

	private bool endTrigger;

	public override NpcEnum.FsmState State => NpcEnum.FsmState.Stumble;

	public StumbleState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.Components.CharacterController.enableOverlapRecovery = true;
		base.Components.Animator.SetTrigger(NpcAnimator.SmallPush);
		base.Entity.Components.IndividualStateUpdater.GetCurrentState().SmallPush = true;
		isStopping = false;
	}

	protected override void WriteState(ref NpcState npcState)
	{
		if (loopTrigger)
		{
			npcState.LoopSmallPush = true;
			loopTrigger = false;
		}
		if (endTrigger)
		{
			npcState.EndSmallPush = true;
			endTrigger = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		Vector3 currentPhysics = base.Components.PhysicsTaker.CurrentPhysics;
		base.Components.CharacterController.Move(currentPhysics * Time.deltaTime);
		if (currentPhysics.magnitude < 0.4f && !isStopping)
		{
			isStopping = true;
			base.Components.Animator.SetTrigger(NpcAnimator.EndSmallPush);
			endTrigger = true;
		}
		if (currentPhysics.magnitude > 0.4f && isStopping)
		{
			isStopping = false;
			base.Components.Animator.SetTrigger(NpcAnimator.LoopSmallPush);
			loopTrigger = true;
		}
	}

	protected override void Exit()
	{
		base.Exit();
		base.Components.CharacterController.enableOverlapRecovery = false;
		base.Components.Animator.SetTrigger(NpcAnimator.PhysicsForceExit);
		base.Entity.Components.IndividualStateUpdater.GetCurrentState().PhysicsForceExit = true;
	}
}
