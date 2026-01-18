using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000E7 RID: 231
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/FrameRateQuality")]
	public class FrameRateQuality : QualityOption
	{
		// Token: 0x060005A1 RID: 1441 RVA: 0x000182AC File Offset: 0x000164AC
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			if (this.frameRate == FrameRateQuality.FrameRate.Unlimited)
			{
				QualitySettings.vSyncCount = 0;
				Application.targetFrameRate = -1;
				return;
			}
			if (this.frameRate < FrameRateQuality.FrameRate.Frames30)
			{
				QualitySettings.vSyncCount = (int)this.frameRate;
				Application.targetFrameRate = -1;
				return;
			}
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = (int)this.frameRate;
		}

		// Token: 0x060005A2 RID: 1442 RVA: 0x00018300 File Offset: 0x00016500
		[return: TupleElementNames(new string[] { "VSyncCount", "TargetFrameRate" })]
		public ValueTuple<int, int> GetVSyncCountAndTargetFrameRate()
		{
			if (this.frameRate == FrameRateQuality.FrameRate.Unlimited)
			{
				return new ValueTuple<int, int>(0, -1);
			}
			if (this.frameRate < FrameRateQuality.FrameRate.Frames30)
			{
				return new ValueTuple<int, int>((int)this.frameRate, -1);
			}
			return new ValueTuple<int, int>(0, (int)this.frameRate);
		}

		// Token: 0x040002F5 RID: 757
		[SerializeField]
		private FrameRateQuality.FrameRate frameRate = FrameRateQuality.FrameRate.VSync1;

		// Token: 0x020000E8 RID: 232
		public enum FrameRate
		{
			// Token: 0x040002F7 RID: 759
			VSync1 = 1,
			// Token: 0x040002F8 RID: 760
			VSync2,
			// Token: 0x040002F9 RID: 761
			VSync3,
			// Token: 0x040002FA RID: 762
			VSync4,
			// Token: 0x040002FB RID: 763
			Frames30 = 30,
			// Token: 0x040002FC RID: 764
			Frames60 = 60,
			// Token: 0x040002FD RID: 765
			Frames90 = 90,
			// Token: 0x040002FE RID: 766
			Frames120 = 120,
			// Token: 0x040002FF RID: 767
			Unlimited = 1000
		}
	}
}
