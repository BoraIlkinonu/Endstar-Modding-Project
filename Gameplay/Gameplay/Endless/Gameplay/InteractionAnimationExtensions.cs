using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000DA RID: 218
	public static class InteractionAnimationExtensions
	{
		// Token: 0x06000489 RID: 1161 RVA: 0x00017D08 File Offset: 0x00015F08
		public static string AnimationName(this InteractionAnimation interactionAnimation)
		{
			string text;
			switch (interactionAnimation)
			{
			case InteractionAnimation.Default:
				text = "Interact";
				break;
			case InteractionAnimation.Hold:
				text = "";
				break;
			case InteractionAnimation.Revive:
				text = "Reviving";
				break;
			case InteractionAnimation.None:
				text = "";
				break;
			default:
				throw new ArgumentOutOfRangeException("interactionAnimation", interactionAnimation, null);
			}
			return text;
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x00017D60 File Offset: 0x00015F60
		public static void Start(this InteractionAnimation interactionAnimation, Animator animator)
		{
			if (interactionAnimation == InteractionAnimation.Default)
			{
				animator.SetTrigger("Interact");
				return;
			}
			if (interactionAnimation != InteractionAnimation.Revive)
			{
				return;
			}
			animator.SetBool("Reviving", true);
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x00017D82 File Offset: 0x00015F82
		public static void Stop(this InteractionAnimation interactionAnimation, Animator animator)
		{
			if (interactionAnimation == InteractionAnimation.Revive)
			{
				animator.SetBool("Reviving", false);
			}
		}
	}
}
