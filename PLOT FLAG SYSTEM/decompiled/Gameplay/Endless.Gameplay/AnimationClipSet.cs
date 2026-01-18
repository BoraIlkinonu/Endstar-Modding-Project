using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/AnimationClipSet", fileName = "AnimationClipSet")]
public class AnimationClipSet : ScriptableObject
{
	[SerializeField]
	private List<AnimationClip> clips;

	public AnimationClipInfo GetRandomClip()
	{
		int num = Random.Range(0, clips.Count);
		AnimationClip animationClip = clips[num];
		return new AnimationClipInfo(animationClip, num, (uint)Mathf.RoundToInt(animationClip.length / NetClock.FixedDeltaTime));
	}
}
