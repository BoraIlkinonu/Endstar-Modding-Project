using System;
using Endless.ParticleSystems.Components;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x0200002A RID: 42
	public class ResourcePickupReferences : BaseTypeReferences
	{
		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00002FD1 File Offset: 0x000011D1
		// (set) Token: 0x060000AE RID: 174 RVA: 0x00002FD9 File Offset: 0x000011D9
		public ColliderInfo WorldTriggerArea { get; private set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000AF RID: 175 RVA: 0x00002FE2 File Offset: 0x000011E2
		// (set) Token: 0x060000B0 RID: 176 RVA: 0x00002FEA File Offset: 0x000011EA
		public GameObject OneVisuals { get; private set; }

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x00002FF3 File Offset: 0x000011F3
		// (set) Token: 0x060000B2 RID: 178 RVA: 0x00002FFB File Offset: 0x000011FB
		public GameObject FiveVisuals { get; private set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x00003004 File Offset: 0x00001204
		// (set) Token: 0x060000B4 RID: 180 RVA: 0x0000300C File Offset: 0x0000120C
		public GameObject TenVisuals { get; private set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x00003015 File Offset: 0x00001215
		// (set) Token: 0x060000B6 RID: 182 RVA: 0x0000301D File Offset: 0x0000121D
		public SwappableParticleSystem OneCollectedParticleSystem { get; private set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x00003026 File Offset: 0x00001226
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x0000302E File Offset: 0x0000122E
		public SwappableParticleSystem FiveCollectedParticleSystem { get; private set; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x00003037 File Offset: 0x00001237
		// (set) Token: 0x060000BA RID: 186 RVA: 0x0000303F File Offset: 0x0000123F
		public SwappableParticleSystem TenCollectedParticleSystem { get; private set; }
	}
}
