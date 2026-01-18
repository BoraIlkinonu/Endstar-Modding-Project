using System;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000284 RID: 644
	public class HitEffect : MonoBehaviour, IPoolableT
	{
		// Token: 0x17000291 RID: 657
		// (get) Token: 0x06000DD3 RID: 3539 RVA: 0x0004AC15 File Offset: 0x00048E15
		public ParticleSystem Particles
		{
			get
			{
				return this.particles;
			}
		}

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06000DD4 RID: 3540 RVA: 0x0004AC1D File Offset: 0x00048E1D
		// (set) Token: 0x06000DD5 RID: 3541 RVA: 0x0004AC25 File Offset: 0x00048E25
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x06000DD6 RID: 3542 RVA: 0x0004AC2E File Offset: 0x00048E2E
		public void OnSpawn()
		{
			base.transform.SetParent(null, true);
			this.isActive = true;
		}

		// Token: 0x06000DD7 RID: 3543 RVA: 0x0004AC44 File Offset: 0x00048E44
		public void OnDespawn()
		{
			this.particles.Stop();
		}

		// Token: 0x06000DD8 RID: 3544 RVA: 0x0004AC51 File Offset: 0x00048E51
		public void PlayEffect()
		{
			this.particles.Play();
		}

		// Token: 0x06000DD9 RID: 3545 RVA: 0x0004AC5E File Offset: 0x00048E5E
		private void OnParticleSystemStopped()
		{
			if (!this.isActive)
			{
				return;
			}
			this.isActive = false;
			if (ProjectileManager.UsePooling)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<HitEffect>(this);
				return;
			}
			global::UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x04000CB0 RID: 3248
		[SerializeField]
		private ParticleSystem particles;

		// Token: 0x04000CB2 RID: 3250
		private bool isActive;
	}
}
