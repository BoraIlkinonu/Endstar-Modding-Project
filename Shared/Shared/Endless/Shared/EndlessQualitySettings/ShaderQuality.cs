using System;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000F9 RID: 249
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/ShaderQuality")]
	public class ShaderQuality : QualityOption
	{
		// Token: 0x060005FA RID: 1530 RVA: 0x00019160 File Offset: 0x00017360
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			ShaderQuality.CurrentQualityLevel = this.qualityLevel;
			ShaderQuality.OnShaderQualityLevelChanged.Invoke(ShaderQuality.CurrentQualityLevel);
		}

		// Token: 0x04000342 RID: 834
		public static UnityEvent<ShaderQuality.ShaderQualityLevel> OnShaderQualityLevelChanged = new UnityEvent<ShaderQuality.ShaderQualityLevel>();

		// Token: 0x04000343 RID: 835
		public static ShaderQuality.ShaderQualityLevel CurrentQualityLevel = ShaderQuality.ShaderQualityLevel.High;

		// Token: 0x04000344 RID: 836
		[SerializeField]
		private ShaderQuality.ShaderQualityLevel qualityLevel = ShaderQuality.ShaderQualityLevel.Low;

		// Token: 0x020000FA RID: 250
		public enum ShaderQualityLevel
		{
			// Token: 0x04000346 RID: 838
			High,
			// Token: 0x04000347 RID: 839
			Low
		}
	}
}
