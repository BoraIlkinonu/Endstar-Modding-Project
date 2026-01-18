using System;
using Endless.ParticleSystems.Components;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000019 RID: 25
	public class JetpackVisualReferences : ComponentReferences
	{
		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000076 RID: 118 RVA: 0x00002BC4 File Offset: 0x00000DC4
		public SwappableParticleSystem InUseParticleSystem
		{
			get
			{
				return this.inUseParticleSystem;
			}
		}

		// Token: 0x04000049 RID: 73
		[SerializeField]
		private SwappableParticleSystem inUseParticleSystem;
	}
}
