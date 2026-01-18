using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x0200001E RID: 30
	public class HittableReferences : ComponentReferences
	{
		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000087 RID: 135 RVA: 0x00002C4C File Offset: 0x00000E4C
		public IReadOnlyList<ColliderInfo> HittableColliders
		{
			get
			{
				return this.hittableColliders;
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000088 RID: 136 RVA: 0x00002C54 File Offset: 0x00000E54
		public Renderer[] HitFlashRenderers
		{
			get
			{
				return this.hitFlashRenderers;
			}
		}

		// Token: 0x04000055 RID: 85
		[SerializeField]
		private ColliderInfo[] hittableColliders;

		// Token: 0x04000056 RID: 86
		[SerializeField]
		private Renderer[] hitFlashRenderers;
	}
}
