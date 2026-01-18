using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200013E RID: 318
	public class NpcAnimationWriter
	{
		// Token: 0x06000779 RID: 1913 RVA: 0x00023257 File Offset: 0x00021457
		public NpcAnimationWriter(Animator animator, IndividualStateUpdater stateUpdater)
		{
			this.animator = animator;
			stateUpdater.OnWriteState += this.HandleOnWriteState;
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x00023278 File Offset: 0x00021478
		private void HandleOnWriteState(ref NpcState currentState)
		{
			currentState.zLock = this.animator.GetBool(NpcAnimator.ZLock);
			currentState.walking = this.animator.GetBool(NpcAnimator.Walking);
		}

		// Token: 0x040005DB RID: 1499
		private readonly Animator animator;
	}
}
