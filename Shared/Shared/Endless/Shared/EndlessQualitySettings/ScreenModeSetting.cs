using System;
using UnityEngine;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000F8 RID: 248
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/ScreenModeSetting")]
	public class ScreenModeSetting : QualityOption
	{
		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060005F7 RID: 1527 RVA: 0x00019131 File Offset: 0x00017331
		public FullScreenMode FullScreenMode
		{
			get
			{
				return this.fullScreenMode;
			}
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x00019139 File Offset: 0x00017339
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			Debug.Log(string.Format("Setting resolution {0}", this.fullScreenMode));
			Screen.fullScreenMode = this.fullScreenMode;
		}

		// Token: 0x04000341 RID: 833
		[SerializeField]
		private FullScreenMode fullScreenMode;
	}
}
