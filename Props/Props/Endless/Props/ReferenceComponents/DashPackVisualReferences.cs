using System;
using Endless.ParticleSystems.Components;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000018 RID: 24
	public class DashPackVisualReferences : ComponentReferences
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000073 RID: 115 RVA: 0x00002BAC File Offset: 0x00000DAC
		public SwappableParticleSystem LeftSideParticleSystem
		{
			get
			{
				return this.leftSideParticleSystem;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000074 RID: 116 RVA: 0x00002BB4 File Offset: 0x00000DB4
		public SwappableParticleSystem RightSideParticleSystem
		{
			get
			{
				return this.rightSideParticleSystem;
			}
		}

		// Token: 0x04000047 RID: 71
		[SerializeField]
		private SwappableParticleSystem leftSideParticleSystem;

		// Token: 0x04000048 RID: 72
		[SerializeField]
		private SwappableParticleSystem rightSideParticleSystem;
	}
}
