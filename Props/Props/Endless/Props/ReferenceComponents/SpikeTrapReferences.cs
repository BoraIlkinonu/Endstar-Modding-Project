using System;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000030 RID: 48
	public class SpikeTrapReferences : BaseTypeReferences
	{
		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060000C5 RID: 197 RVA: 0x000030ED File Offset: 0x000012ED
		// (set) Token: 0x060000C6 RID: 198 RVA: 0x000030F5 File Offset: 0x000012F5
		public ColliderInfo WorldTriggerArea { get; private set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060000C7 RID: 199 RVA: 0x000030FE File Offset: 0x000012FE
		// (set) Token: 0x060000C8 RID: 200 RVA: 0x00003106 File Offset: 0x00001306
		public Transform SpikeTransform { get; private set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060000C9 RID: 201 RVA: 0x0000310F File Offset: 0x0000130F
		// (set) Token: 0x060000CA RID: 202 RVA: 0x00003117 File Offset: 0x00001317
		public Transform RetractedPosition { get; private set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060000CB RID: 203 RVA: 0x00003120 File Offset: 0x00001320
		// (set) Token: 0x060000CC RID: 204 RVA: 0x00003128 File Offset: 0x00001328
		public Transform StabPosition { get; private set; }
	}
}
