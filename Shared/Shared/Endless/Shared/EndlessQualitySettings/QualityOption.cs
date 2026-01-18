using System;
using UnityEngine;

namespace Endless.Shared.EndlessQualitySettings
{
	// Token: 0x020000F5 RID: 245
	public abstract class QualityOption : ScriptableObject
	{
		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x060005DA RID: 1498 RVA: 0x00018E04 File Offset: 0x00017004
		public string DisplayName
		{
			get
			{
				return this.displayName;
			}
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060005DB RID: 1499 RVA: 0x00018E0C File Offset: 0x0001700C
		public string SaveKey
		{
			get
			{
				return base.GetType().Name;
			}
		}

		// Token: 0x060005DC RID: 1500
		public abstract void ApplySetting(UniversalRenderPipelineHandler pipelineHandler);

		// Token: 0x04000335 RID: 821
		[SerializeField]
		protected string displayName = "Unnamed";
	}
}
