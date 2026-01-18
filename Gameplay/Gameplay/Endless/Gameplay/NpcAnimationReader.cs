using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200013D RID: 317
	public class NpcAnimationReader
	{
		// Token: 0x06000776 RID: 1910 RVA: 0x000230DD File Offset: 0x000212DD
		public NpcAnimationReader(Animator animator, IndividualStateUpdater stateUpdater)
		{
			this.animator = animator;
			stateUpdater.OnStateInterpolated += this.HandleOnStateInterpolated;
			stateUpdater.OnReadState += this.HandleOnReadState;
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x00023110 File Offset: 0x00021310
		private void HandleOnStateInterpolated(NpcState state)
		{
			this.animator.SetFloat(NpcAnimator.VelX, state.VelX);
			this.animator.SetFloat(NpcAnimator.VelY, state.VelY);
			this.animator.SetFloat(NpcAnimator.VelZ, state.VelZ);
			this.animator.SetFloat(NpcAnimator.AngularVelocity, state.AngularVelocity);
			this.animator.SetFloat(NpcAnimator.HorizVelMagnitude, state.HorizVelMagnitude);
			this.animator.SetFloat(NpcAnimator.SlopeAngle, state.slopeAngle);
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x000231A4 File Offset: 0x000213A4
		private void HandleOnReadState(ref NpcState state)
		{
			if (state.jumped)
			{
				this.animator.SetTrigger(NpcAnimator.Jump);
			}
			this.animator.SetBool(NpcAnimator.Walking, state.walking);
			if (state.fidget > 0)
			{
				this.animator.SetTrigger(NpcAnimator.Fidget);
				this.animator.SetInteger(NpcAnimator.FidgetInt, state.fidget - 1);
			}
			if (state.taunt > 0)
			{
				this.animator.SetTrigger(NpcAnimator.Taunt);
				this.animator.SetInteger(NpcAnimator.TauntInt, state.taunt - 1);
			}
			this.animator.SetBool(NpcAnimator.ZLock, state.zLock);
		}

		// Token: 0x040005DA RID: 1498
		private readonly Animator animator;
	}
}
