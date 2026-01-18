using System;
using UnityEngine;

namespace Endless.Gameplay;

public class AnimationEvents : MonoBehaviour
{
	public Action<string> OnAnimationComplete;

	public void AnimationComplete(string animationName)
	{
		OnAnimationComplete?.Invoke(animationName);
	}
}
