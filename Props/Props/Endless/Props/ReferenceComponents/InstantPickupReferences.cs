using System;
using Endless.ParticleSystems.Components;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x0200001F RID: 31
	public class InstantPickupReferences : BaseTypeReferences
	{
		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600008A RID: 138 RVA: 0x00002C64 File Offset: 0x00000E64
		// (set) Token: 0x0600008B RID: 139 RVA: 0x00002C6C File Offset: 0x00000E6C
		public ColliderInfo WorldTriggerArea { get; private set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600008C RID: 140 RVA: 0x00002C75 File Offset: 0x00000E75
		// (set) Token: 0x0600008D RID: 141 RVA: 0x00002C7D File Offset: 0x00000E7D
		public SwappableParticleSystem CollectedParticleSystem { get; private set; }
	}
}
