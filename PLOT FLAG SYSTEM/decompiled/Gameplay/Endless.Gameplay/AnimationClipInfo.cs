using UnityEngine;

namespace Endless.Gameplay;

public readonly struct AnimationClipInfo
{
	public readonly AnimationClip Clip;

	public readonly int ClipIndex;

	public readonly uint FrameLength;

	public AnimationClipInfo(AnimationClip clip, int clipIndex, uint frameLength)
	{
		Clip = clip;
		ClipIndex = clipIndex;
		FrameLength = frameLength;
	}
}
