using UnityEngine;

namespace Endless.Gameplay;

public class NpcAnimationWriter
{
	private readonly Animator animator;

	public NpcAnimationWriter(Animator animator, IndividualStateUpdater stateUpdater)
	{
		this.animator = animator;
		stateUpdater.OnWriteState += HandleOnWriteState;
	}

	private void HandleOnWriteState(ref NpcState currentState)
	{
		currentState.zLock = animator.GetBool(NpcAnimator.ZLock);
		currentState.walking = animator.GetBool(NpcAnimator.Walking);
	}
}
