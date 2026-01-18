using System;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000013 RID: 19
	public class BouncePadReferences : BaseTypeReferences
	{
		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000050 RID: 80 RVA: 0x00002A20 File Offset: 0x00000C20
		// (set) Token: 0x06000051 RID: 81 RVA: 0x00002A28 File Offset: 0x00000C28
		public ColliderInfo WorldTriggerArea { get; private set; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000052 RID: 82 RVA: 0x00002A31 File Offset: 0x00000C31
		// (set) Token: 0x06000053 RID: 83 RVA: 0x00002A39 File Offset: 0x00000C39
		public ParticleSystem BounceParticleEffect { get; private set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000054 RID: 84 RVA: 0x00002A42 File Offset: 0x00000C42
		// (set) Token: 0x06000055 RID: 85 RVA: 0x00002A4A File Offset: 0x00000C4A
		public Transform BounceNormal { get; private set; }
	}
}
