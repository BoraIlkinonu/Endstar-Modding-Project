using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay;

public static class InteractionAnimationExtensions
{
	public static string AnimationName(this InteractionAnimation interactionAnimation)
	{
		return interactionAnimation switch
		{
			InteractionAnimation.None => "", 
			InteractionAnimation.Default => "Interact", 
			InteractionAnimation.Hold => "", 
			InteractionAnimation.Revive => "Reviving", 
			_ => throw new ArgumentOutOfRangeException("interactionAnimation", interactionAnimation, null), 
		};
	}

	public static void Start(this InteractionAnimation interactionAnimation, Animator animator)
	{
		switch (interactionAnimation)
		{
		case InteractionAnimation.Default:
			animator.SetTrigger("Interact");
			break;
		case InteractionAnimation.Revive:
			animator.SetBool("Reviving", value: true);
			break;
		}
	}

	public static void Stop(this InteractionAnimation interactionAnimation, Animator animator)
	{
		if (interactionAnimation == InteractionAnimation.Revive)
		{
			animator.SetBool("Reviving", value: false);
		}
	}
}
