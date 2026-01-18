using System;
using Endless.ParticleSystems.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000012 RID: 18
	public class BasicLevelGateReferences : BaseTypeReferences
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000045 RID: 69 RVA: 0x000029C3 File Offset: 0x00000BC3
		// (set) Token: 0x06000046 RID: 70 RVA: 0x000029CB File Offset: 0x00000BCB
		public Image TimerFillImage { get; private set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000047 RID: 71 RVA: 0x000029D4 File Offset: 0x00000BD4
		// (set) Token: 0x06000048 RID: 72 RVA: 0x000029DC File Offset: 0x00000BDC
		public SwappableParticleSystem PartialReadyParticles { get; private set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000049 RID: 73 RVA: 0x000029E5 File Offset: 0x00000BE5
		// (set) Token: 0x0600004A RID: 74 RVA: 0x000029ED File Offset: 0x00000BED
		public SwappableParticleSystem ReadyParticles { get; private set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600004B RID: 75 RVA: 0x000029F6 File Offset: 0x00000BF6
		// (set) Token: 0x0600004C RID: 76 RVA: 0x000029FE File Offset: 0x00000BFE
		public SwappableParticleSystem StartingParticles { get; private set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600004D RID: 77 RVA: 0x00002A07 File Offset: 0x00000C07
		// (set) Token: 0x0600004E RID: 78 RVA: 0x00002A0F File Offset: 0x00000C0F
		public Transform[] SpawnPoints { get; private set; }
	}
}
