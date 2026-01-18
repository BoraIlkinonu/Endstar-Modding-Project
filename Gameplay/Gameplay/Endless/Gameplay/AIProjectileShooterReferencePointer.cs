using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200012A RID: 298
	public class AIProjectileShooterReferencePointer : MonoBehaviour
	{
		// Token: 0x17000130 RID: 304
		// (get) Token: 0x060006B9 RID: 1721 RVA: 0x00021297 File Offset: 0x0001F497
		// (set) Token: 0x060006BA RID: 1722 RVA: 0x0002129F File Offset: 0x0001F49F
		public Transform FirePoint { get; private set; }

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x060006BB RID: 1723 RVA: 0x000212A8 File Offset: 0x0001F4A8
		// (set) Token: 0x060006BC RID: 1724 RVA: 0x000212B0 File Offset: 0x0001F4B0
		public ParticleSystem MuzzleFlashEffect { get; private set; }

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x060006BD RID: 1725 RVA: 0x000212B9 File Offset: 0x0001F4B9
		// (set) Token: 0x060006BE RID: 1726 RVA: 0x000212C1 File Offset: 0x0001F4C1
		public ParticleSystem EjectionEffect { get; private set; }

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x060006BF RID: 1727 RVA: 0x000212CA File Offset: 0x0001F4CA
		// (set) Token: 0x060006C0 RID: 1728 RVA: 0x000212D2 File Offset: 0x0001F4D2
		[Obsolete("Use HitEffectPrefab")]
		public ParticleSystem HitEffect { get; private set; }

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x060006C1 RID: 1729 RVA: 0x000212DB File Offset: 0x0001F4DB
		// (set) Token: 0x060006C2 RID: 1730 RVA: 0x000212E3 File Offset: 0x0001F4E3
		public HitEffect HitEffectPrefab { get; private set; }
	}
}
