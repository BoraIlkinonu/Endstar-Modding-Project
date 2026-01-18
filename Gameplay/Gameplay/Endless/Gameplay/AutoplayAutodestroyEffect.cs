using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200006E RID: 110
	public class AutoplayAutodestroyEffect : MonoBehaviour
	{
		// Token: 0x060001C5 RID: 453 RVA: 0x0000A9C4 File Offset: 0x00008BC4
		private void Start()
		{
			this.targetParticleSystem = base.GetComponent<ParticleSystem>();
			if (this.targetParticleSystem && !this.targetParticleSystem.isPlaying)
			{
				this.targetParticleSystem.Play();
			}
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000A9F7 File Offset: 0x00008BF7
		private void Update()
		{
			if (this.targetParticleSystem && !this.targetParticleSystem.IsAlive())
			{
				global::UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x0400019C RID: 412
		private ParticleSystem targetParticleSystem;
	}
}
