using System;
using UnityEngine;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000FB RID: 251
	[CreateAssetMenu(menuName = "ScriptableObject/Quality Settings/TextureQuality")]
	public class TextureQuality : QualityOption
	{
		// Token: 0x060005FD RID: 1533 RVA: 0x0001919D File Offset: 0x0001739D
		public override void ApplySetting(UniversalRenderPipelineHandler pipelineHandler)
		{
			QualitySettings.globalTextureMipmapLimit = this.textureQuality;
		}

		// Token: 0x04000348 RID: 840
		[Range(0f, 3f)]
		[Tooltip("0 = Full Res, 1 = half, 2 = quarter, 3 = eighth")]
		[SerializeField]
		private int textureQuality;
	}
}
