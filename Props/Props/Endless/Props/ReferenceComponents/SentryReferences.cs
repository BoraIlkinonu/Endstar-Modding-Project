using System;
using Endless.ParticleSystems.Components;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x0200002C RID: 44
	public class SentryReferences : BaseTypeReferences
	{
		// Token: 0x04000072 RID: 114
		[SerializeField]
		public Transform SwivelTransform;

		// Token: 0x04000073 RID: 115
		[SerializeField]
		public Transform LaserPoint;

		// Token: 0x04000074 RID: 116
		[SerializeField]
		public Transform[] ShootPointTransformList;

		// Token: 0x04000075 RID: 117
		[SerializeField]
		public LineRenderer TrackLaser;

		// Token: 0x04000076 RID: 118
		[SerializeField]
		public LineRenderer ShootLaser;

		// Token: 0x04000077 RID: 119
		[SerializeField]
		public float ShootLaserFlashDuration;

		// Token: 0x04000078 RID: 120
		[SerializeField]
		public SwappableParticleSystem ShootFlashEffect;

		// Token: 0x04000079 RID: 121
		[SerializeField]
		public SwappableParticleSystem ShootHitEffect;

		// Token: 0x0400007A RID: 122
		[SerializeField]
		public SwappableParticleSystem SlightlyDamagedParticle;

		// Token: 0x0400007B RID: 123
		[SerializeField]
		public SwappableParticleSystem CriticallyDamagedPartical;

		// Token: 0x0400007C RID: 124
		[SerializeField]
		public SwappableParticleSystem DestroyedParticle;
	}
}
