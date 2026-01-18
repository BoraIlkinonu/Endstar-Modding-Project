using UnityEngine;

namespace Endless.Gameplay;

public class NpcAnimationReader
{
	private readonly Animator animator;

	public NpcAnimationReader(Animator animator, IndividualStateUpdater stateUpdater)
	{
		this.animator = animator;
		stateUpdater.OnStateInterpolated += HandleOnStateInterpolated;
		stateUpdater.OnReadState += HandleOnReadState;
	}

	private void HandleOnStateInterpolated(NpcState state)
	{
		animator.SetFloat(NpcAnimator.VelX, state.VelX);
		animator.SetFloat(NpcAnimator.VelY, state.VelY);
		animator.SetFloat(NpcAnimator.VelZ, state.VelZ);
		animator.SetFloat(NpcAnimator.AngularVelocity, state.AngularVelocity);
		animator.SetFloat(NpcAnimator.HorizVelMagnitude, state.HorizVelMagnitude);
		animator.SetFloat(NpcAnimator.SlopeAngle, state.slopeAngle);
	}

	private void HandleOnReadState(ref NpcState state)
	{
		if (state.jumped)
		{
			animator.SetTrigger(NpcAnimator.Jump);
		}
		animator.SetBool(NpcAnimator.Walking, state.walking);
		if (state.fidget > 0)
		{
			animator.SetTrigger(NpcAnimator.Fidget);
			animator.SetInteger(NpcAnimator.FidgetInt, state.fidget - 1);
		}
		if (state.taunt > 0)
		{
			animator.SetTrigger(NpcAnimator.Taunt);
			animator.SetInteger(NpcAnimator.TauntInt, state.taunt - 1);
		}
		animator.SetBool(NpcAnimator.ZLock, state.zLock);
	}
}
