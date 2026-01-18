using System;
using UnityEngine;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000EB RID: 235
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/ModelQuality")]
	public class ModelQuality : QualityOption
	{
		// Token: 0x060005AA RID: 1450 RVA: 0x000184B5 File Offset: 0x000166B5
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			QualitySettings.lodBias = this.lodBias;
			QualitySettings.maximumLODLevel = this.maximumLodLevel;
		}

		// Token: 0x0400030C RID: 780
		[SerializeField]
		private float lodBias = 1f;

		// Token: 0x0400030D RID: 781
		[SerializeField]
		private int maximumLodLevel;
	}
}
