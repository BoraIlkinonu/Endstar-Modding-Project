using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000240 RID: 576
	public readonly struct AnimationClipInfo
	{
		// Token: 0x06000BD4 RID: 3028 RVA: 0x00040D9F File Offset: 0x0003EF9F
		public AnimationClipInfo(AnimationClip clip, int clipIndex, uint frameLength)
		{
			this.Clip = clip;
			this.ClipIndex = clipIndex;
			this.FrameLength = frameLength;
		}

		// Token: 0x04000B08 RID: 2824
		public readonly AnimationClip Clip;

		// Token: 0x04000B09 RID: 2825
		public readonly int ClipIndex;

		// Token: 0x04000B0A RID: 2826
		public readonly uint FrameLength;
	}
}
