using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000E9 RID: 233
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/HighDynamicRangeQuality")]
	public class HighDynamicRangeQuality : QualityOption
	{
		// Token: 0x170000DF RID: 223
		// (get) Token: 0x060005A4 RID: 1444 RVA: 0x00018349 File Offset: 0x00016549
		// (set) Token: 0x060005A5 RID: 1445 RVA: 0x00018351 File Offset: 0x00016551
		public bool Enabled { get; private set; } = true;

		// Token: 0x060005A6 RID: 1446 RVA: 0x0001835C File Offset: 0x0001655C
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			UniversalRenderPipelineAsset universalRenderPipelineAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
			if (universalRenderPipelineAsset == null)
			{
				DebugUtility.LogError("Could not cast QualitySettings.renderPipeline as UniversalRenderPipelineAsset!", this);
				return;
			}
			universalRenderPipelineAsset.supportsHDR = this.Enabled;
		}
	}
}
