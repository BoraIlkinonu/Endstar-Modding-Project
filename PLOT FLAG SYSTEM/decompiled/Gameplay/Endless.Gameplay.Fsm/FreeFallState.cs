using UnityEngine;

namespace Endless.Gameplay.Fsm;

public class FreeFallState : FsmState
{
	private const float rollThreshold = 0.5f;

	private const float splatThreshold = 1.4f;

	private float fallTime;

	private bool isInFreeFall;

	private bool entryTrigger;

	private bool freeFallTrigger;

	public override NpcEnum.FsmState State => NpcEnum.FsmState.FreeFall;

	public FreeFallState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		entryTrigger = true;
		fallTime = 0f;
		isInFreeFall = false;
		base.Components.CharacterController.enableOverlapRecovery = true;
		base.Components.Animator.ResetTrigger(NpcAnimator.PhysicsForceExit);
		base.Components.Animator.SetTrigger(NpcAnimator.LargePush);
		base.Components.Animator.SetBool(NpcAnimator.Grounded, value: false);
		base.Components.IndividualStateUpdater.OnUpdateState += HandleOnUpdateState;
	}

	private void HandleOnUpdateState(uint obj)
	{
		Vector3 forcesPlusGravity = GetForcesPlusGravity();
		if (!(forcesPlusGravity.y > 0f))
		{
			base.Components.Parameters.WillRoll = forcesPlusGravity.normalized.y > -0.5f;
			base.Components.Parameters.WillSplat = fallTime > 1.4f;
			if (!base.Components.Parameters.WillRoll && !isInFreeFall)
			{
				base.Components.Animator.SetTrigger(NpcAnimator.PhysicsForceExit);
				isInFreeFall = true;
				freeFallTrigger = true;
			}
		}
	}

	protected override void WriteState(ref NpcState npcState)
	{
		if (entryTrigger)
		{
			npcState.LargePush = true;
			entryTrigger = false;
		}
		if (freeFallTrigger)
		{
			npcState.PhysicsForceExit = true;
			freeFallTrigger = false;
		}
		npcState.isGrounded = false;
		npcState.fallTime = fallTime;
	}

	protected override void Update()
	{
		base.Update();
		Vector3 forcesPlusGravity = GetForcesPlusGravity();
		if ((double)forcesPlusGravity.normalized.y < -0.8)
		{
			Vector3 vector = base.Entity.transform.forward * 2f;
			forcesPlusGravity += vector;
		}
		base.Components.CharacterController.Move(forcesPlusGravity * Time.deltaTime);
		if (forcesPlusGravity.y < 0f)
		{
			fallTime += Time.deltaTime;
		}
		base.Components.Animator.SetFloat(NpcAnimator.FallTime, fallTime);
	}

	private Vector3 GetForcesPlusGravity()
	{
		Vector3 currentPhysics = base.Components.PhysicsTaker.CurrentPhysics;
		uint num = NetClock.CurrentFrame - base.Components.Grounding.LastGroundedFrame;
		float time = Mathf.InverseLerp(0f, base.Entity.Settings.FramesToTerminalVelocity, num);
		float num2 = base.Entity.Settings.GravityCurve.Evaluate(time);
		currentPhysics.y -= base.Entity.Settings.TerminalVelocity * num2;
		return currentPhysics;
	}

	protected override void Exit()
	{
		base.Exit();
		base.Components.CharacterController.enableOverlapRecovery = false;
		base.Components.Animator.SetBool(NpcAnimator.Grounded, value: true);
		base.Components.IndividualStateUpdater.OnUpdateState -= HandleOnUpdateState;
	}
}
