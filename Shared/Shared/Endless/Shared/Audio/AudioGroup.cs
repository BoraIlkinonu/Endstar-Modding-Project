using System;
using System.Collections;
using UnityEngine;

namespace Endless.Shared.Audio
{
	// Token: 0x020002AA RID: 682
	[Serializable]
	public class AudioGroup
	{
		// Token: 0x060010D5 RID: 4309 RVA: 0x000479CF File Offset: 0x00045BCF
		public float PlayAudio(AudioSource audioSource)
		{
			return 0.25f;
		}

		// Token: 0x060010D6 RID: 4310 RVA: 0x000479D8 File Offset: 0x00045BD8
		public void SpawnAndPlayWithManagedPool(MonoBehaviour managingScript, PoolableAudioSource audioSourcePrefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
		{
			PoolableAudioSource poolableAudioSource = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<PoolableAudioSource>(audioSourcePrefab, position, rotation, null);
			poolableAudioSource.transform.SetParent(null);
			float num = this.PlayAudio(poolableAudioSource.AudioSource);
			managingScript.StartCoroutine(this.ReturnToPoolAfterDelay(poolableAudioSource, num));
		}

		// Token: 0x060010D7 RID: 4311 RVA: 0x00047A20 File Offset: 0x00045C20
		public void PlayWithManagedPool(MonoBehaviour managingScript, PoolableAudioSource pooledAudioSource)
		{
			float num = this.PlayAudio(pooledAudioSource.AudioSource);
			managingScript.StartCoroutine(this.ReturnToPoolAfterDelay(pooledAudioSource, num));
		}

		// Token: 0x060010D8 RID: 4312 RVA: 0x00047A49 File Offset: 0x00045C49
		private IEnumerator ReturnToPoolAfterDelay(PoolableAudioSource pooledAudioSource, float duration)
		{
			yield return new WaitForSeconds(duration);
			if (MonoBehaviourSingleton<PoolManagerT>.Instance != null)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<PoolableAudioSource>(pooledAudioSource);
			}
			yield break;
		}

		// Token: 0x04000A9A RID: 2714
		public AudioClip[] AvailableClips;

		// Token: 0x04000A9B RID: 2715
		public bool RandomizePitch = true;

		// Token: 0x04000A9C RID: 2716
		public Vector2 PitchVariation = new Vector2(0.95f, 1.05f);

		// Token: 0x04000A9D RID: 2717
		public bool RandomizeVolume = true;

		// Token: 0x04000A9E RID: 2718
		public Vector2 VolumeVariation = new Vector2(0.95f, 1.05f);
	}
}
