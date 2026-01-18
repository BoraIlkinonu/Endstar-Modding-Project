using System;
using Endless.Shared;
using Endless.Shared.Audio;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000AB RID: 171
	public class FootstepController : MonoBehaviour
	{
		// Token: 0x060002EC RID: 748 RVA: 0x0000FC30 File Offset: 0x0000DE30
		public void UpdateFootsteps(float horizontalVelocityMagnitude, bool walking)
		{
			this.footstepAccumulation += horizontalVelocityMagnitude * Time.deltaTime;
			float num = (walking ? this.walkingFootstepTriggerThreshold : this.runningFootstepTriggerThreshold);
			if (this.footstepAccumulation > num)
			{
				this.footstepAccumulation %= num;
				PoolableAudioSource poolableAudioSource = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<PoolableAudioSource>(this.footstepAudioSourcePrefab, base.transform.position, default(Quaternion), null);
				poolableAudioSource.transform.SetParent(null);
				poolableAudioSource.AudioSource.volume = (walking ? this.walkingVolume : this.runningVolume);
				this.footstepGroup.PlayWithManagedPool(this, poolableAudioSource);
			}
		}

		// Token: 0x040002B9 RID: 697
		[SerializeField]
		private float runningFootstepTriggerThreshold = 0.3f;

		// Token: 0x040002BA RID: 698
		[SerializeField]
		private float walkingFootstepTriggerThreshold = 0.15f;

		// Token: 0x040002BB RID: 699
		[SerializeField]
		private PoolableAudioSource footstepAudioSourcePrefab;

		// Token: 0x040002BC RID: 700
		[SerializeField]
		private AudioGroup footstepGroup;

		// Token: 0x040002BD RID: 701
		[SerializeField]
		private float runningVolume = 0.5f;

		// Token: 0x040002BE RID: 702
		[SerializeField]
		private float walkingVolume = 0.25f;

		// Token: 0x040002BF RID: 703
		private float footstepAccumulation;
	}
}
