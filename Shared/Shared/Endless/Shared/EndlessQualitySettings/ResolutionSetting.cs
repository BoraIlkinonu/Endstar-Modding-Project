using System;
using UnityEngine;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000F7 RID: 247
	public class ResolutionSetting : QualityOption
	{
		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060005F1 RID: 1521 RVA: 0x0001901A File Offset: 0x0001721A
		public Resolution Resolution
		{
			get
			{
				return this.resolution;
			}
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x00019022 File Offset: 0x00017222
		public void Init(Resolution resolution)
		{
			this.resolution = resolution;
			this.displayName = ResolutionSetting.GetResolutionDisplayString(resolution);
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00019037 File Offset: 0x00017237
		public string GetResolutionDisplayString()
		{
			return ResolutionSetting.GetResolutionDisplayString(this.resolution);
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00019044 File Offset: 0x00017244
		public static string GetResolutionDisplayString(Resolution resolution)
		{
			if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
			{
				return string.Format("{0}x{1} {2} Hz", resolution.width, resolution.height, (int)Math.Round(resolution.refreshRateRatio.value));
			}
			return string.Format("{0}x{1}", resolution.width, resolution.height);
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x000190B8 File Offset: 0x000172B8
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			Debug.Log(string.Format("Setting resolution {0}{1}{2}", this.resolution.width, this.resolution.height, Screen.fullScreenMode));
			Screen.SetResolution(this.resolution.width, this.resolution.height, Screen.fullScreenMode, this.resolution.refreshRateRatio);
		}

		// Token: 0x04000340 RID: 832
		private Resolution resolution;
	}
}
