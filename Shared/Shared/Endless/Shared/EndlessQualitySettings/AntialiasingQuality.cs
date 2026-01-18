using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000E6 RID: 230
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/AntialiasingQuality")]
	public class AntialiasingQuality : QualityOption
	{
		// Token: 0x0600059E RID: 1438 RVA: 0x00018265 File Offset: 0x00016465
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			AntialiasingQuality.CurrentAntialiasingMode = this.antialiasingMode;
			AntialiasingQuality.CurrentAntialiasingQuality = this.antialiasingQuality;
			AntialiasingQuality.AntialiasingQualityChanged.Invoke(AntialiasingQuality.CurrentAntialiasingMode, AntialiasingQuality.CurrentAntialiasingQuality);
		}

		// Token: 0x040002F0 RID: 752
		[SerializeField]
		private AntialiasingMode antialiasingMode;

		// Token: 0x040002F1 RID: 753
		[SerializeField]
		private AntialiasingQuality antialiasingQuality = AntialiasingQuality.High;

		// Token: 0x040002F2 RID: 754
		public static AntialiasingMode CurrentAntialiasingMode;

		// Token: 0x040002F3 RID: 755
		public static AntialiasingQuality CurrentAntialiasingQuality;

		// Token: 0x040002F4 RID: 756
		public static UnityEvent<AntialiasingMode, AntialiasingQuality> AntialiasingQualityChanged = new UnityEvent<AntialiasingMode, AntialiasingQuality>();
	}
}
