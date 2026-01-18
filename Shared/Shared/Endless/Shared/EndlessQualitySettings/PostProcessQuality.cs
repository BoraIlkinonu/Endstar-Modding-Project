using System;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000EC RID: 236
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/PostProcessQuality")]
	public class PostProcessQuality : QualityOption
	{
		// Token: 0x060005AC RID: 1452 RVA: 0x000184E0 File Offset: 0x000166E0
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			PostProcessQuality.CurrentQualityLevel = this.qualityLevel;
			PostProcessQuality.PostProcessQualityLevelChanged.Invoke(PostProcessQuality.CurrentQualityLevel);
		}

		// Token: 0x0400030E RID: 782
		public static UnityEvent<PostProcessQuality.PostProcessQualityLevel> PostProcessQualityLevelChanged = new UnityEvent<PostProcessQuality.PostProcessQualityLevel>();

		// Token: 0x0400030F RID: 783
		public static PostProcessQuality.PostProcessQualityLevel CurrentQualityLevel = PostProcessQuality.PostProcessQualityLevel.High;

		// Token: 0x04000310 RID: 784
		[SerializeField]
		private PostProcessQuality.PostProcessQualityLevel qualityLevel = PostProcessQuality.PostProcessQualityLevel.Low;

		// Token: 0x020000ED RID: 237
		public enum PostProcessQualityLevel
		{
			// Token: 0x04000312 RID: 786
			Disabled,
			// Token: 0x04000313 RID: 787
			Low,
			// Token: 0x04000314 RID: 788
			Medium,
			// Token: 0x04000315 RID: 789
			High
		}
	}
}
