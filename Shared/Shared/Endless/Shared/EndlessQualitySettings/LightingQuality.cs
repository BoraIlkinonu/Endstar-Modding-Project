using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000EA RID: 234
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/LightingQuality")]
	public class LightingQuality : QualityOption
	{
		// Token: 0x060005A8 RID: 1448 RVA: 0x000183A4 File Offset: 0x000165A4
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			pipelineHandler.MainLightCastShadows = this.castShadows;
			pipelineHandler.MainLightShadowResolution = this.shadowResolution;
			pipelineHandler.AdditionalLightsRenderingMode = this.additionalLightsRenderingMode;
			pipelineHandler.AdditionalLightsCastShadows = this.castShadows && this.additionalLightsCastShadows;
			pipelineHandler.AdditionalLightsShadowResolution = this.shadowAtlasResolution;
			pipelineHandler.SoftShadowsEnabled = this.softShadows;
			pipelineHandler.ShadowDistance = this.shadowDistance;
			pipelineHandler.ShadowCascades = this.shadowCascades;
			pipelineHandler.Cascade2Split = this.cascade2Split;
			pipelineHandler.Cascade3Split = this.cascade3Split;
			pipelineHandler.Cascade4Split = this.cascade4Split;
		}

		// Token: 0x04000301 RID: 769
		[Header("Main Light")]
		[SerializeField]
		private bool castShadows;

		// Token: 0x04000302 RID: 770
		[SerializeField]
		private global::UnityEngine.Rendering.Universal.ShadowResolution shadowResolution = global::UnityEngine.Rendering.Universal.ShadowResolution._256;

		// Token: 0x04000303 RID: 771
		[Header("Additional Lights")]
		[SerializeField]
		private LightRenderingMode additionalLightsRenderingMode;

		// Token: 0x04000304 RID: 772
		[SerializeField]
		private bool additionalLightsCastShadows;

		// Token: 0x04000305 RID: 773
		[SerializeField]
		private global::UnityEngine.Rendering.Universal.ShadowResolution shadowAtlasResolution = global::UnityEngine.Rendering.Universal.ShadowResolution._256;

		// Token: 0x04000306 RID: 774
		[Header("Shadows")]
		[SerializeField]
		private bool softShadows;

		// Token: 0x04000307 RID: 775
		[SerializeField]
		private float shadowDistance = 50f;

		// Token: 0x04000308 RID: 776
		[Range(1f, 4f)]
		[SerializeField]
		private int shadowCascades = 4;

		// Token: 0x04000309 RID: 777
		[SerializeField]
		private float cascade2Split = 0.25f;

		// Token: 0x0400030A RID: 778
		[SerializeField]
		private Vector2 cascade3Split = new Vector2(0.1f, 0.3f);

		// Token: 0x0400030B RID: 779
		[SerializeField]
		private Vector3 cascade4Split = new Vector3(0.034f, 0.316f, 0.572f);
	}
}
