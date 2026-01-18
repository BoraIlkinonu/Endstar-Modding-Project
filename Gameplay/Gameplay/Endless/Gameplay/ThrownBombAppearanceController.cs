using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002CF RID: 719
	public class ThrownBombAppearanceController : NetworkRigidbodyAppearanceController
	{
		// Token: 0x06001054 RID: 4180 RVA: 0x00052D14 File Offset: 0x00050F14
		protected override void AfterUpdate()
		{
			base.AfterUpdate();
			if (!this.detonated && this.detonateFrame > 0U && this.stateRingBuffer.NextInterpolationState.NetFrame <= this.detonateFrame)
			{
				this.detonated = true;
				global::UnityEngine.Object.Instantiate<ParticleSystem>(this.explosionObjectPrefab, base.transform.position, Quaternion.identity).Play();
			}
		}

		// Token: 0x06001055 RID: 4181 RVA: 0x00052D7A File Offset: 0x00050F7A
		public void SetDetonateFrame(uint frame)
		{
			this.detonateFrame = frame;
		}

		// Token: 0x04000E05 RID: 3589
		[SerializeField]
		private ParticleSystem explosionObjectPrefab;

		// Token: 0x04000E06 RID: 3590
		private bool detonated;

		// Token: 0x04000E07 RID: 3591
		private uint detonateFrame;
	}
}
