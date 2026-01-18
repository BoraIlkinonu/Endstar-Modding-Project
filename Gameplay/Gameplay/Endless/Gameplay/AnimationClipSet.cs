using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000239 RID: 569
	[CreateAssetMenu(menuName = "ScriptableObject/AnimationClipSet", fileName = "AnimationClipSet")]
	public class AnimationClipSet : ScriptableObject
	{
		// Token: 0x06000BC5 RID: 3013 RVA: 0x00040A48 File Offset: 0x0003EC48
		public AnimationClipInfo GetRandomClip()
		{
			int num = global::UnityEngine.Random.Range(0, this.clips.Count);
			AnimationClip animationClip = this.clips[num];
			return new AnimationClipInfo(animationClip, num, (uint)Mathf.RoundToInt(animationClip.length / NetClock.FixedDeltaTime));
		}

		// Token: 0x04000AFC RID: 2812
		[SerializeField]
		private List<AnimationClip> clips;
	}
}
