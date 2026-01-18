using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200012D RID: 301
	public class AnimationEvents : MonoBehaviour
	{
		// Token: 0x060006CA RID: 1738 RVA: 0x000214FE File Offset: 0x0001F6FE
		public void AnimationComplete(string animationName)
		{
			Action<string> onAnimationComplete = this.OnAnimationComplete;
			if (onAnimationComplete == null)
			{
				return;
			}
			onAnimationComplete(animationName);
		}

		// Token: 0x04000590 RID: 1424
		public Action<string> OnAnimationComplete;
	}
}
