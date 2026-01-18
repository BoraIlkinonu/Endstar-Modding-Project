using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x0200049F RID: 1183
	public class AudioManager
	{
		// Token: 0x170005A6 RID: 1446
		// (get) Token: 0x06001D1D RID: 7453 RVA: 0x0007F073 File Offset: 0x0007D273
		internal static AudioManager Instance
		{
			get
			{
				if (AudioManager.instance == null)
				{
					AudioManager.instance = new AudioManager();
				}
				return AudioManager.instance;
			}
		}

		// Token: 0x06001D1E RID: 7454 RVA: 0x0007F08B File Offset: 0x0007D28B
		public AudioManager.SfxConfiguration GetSfxConfig(bool loop)
		{
			return new AudioManager.SfxConfiguration(loop);
		}

		// Token: 0x06001D1F RID: 7455 RVA: 0x0007F093 File Offset: 0x0007D293
		public AudioManager.SfxConfiguration GetSfxConfig()
		{
			return new AudioManager.SfxConfiguration(false);
		}

		// Token: 0x06001D20 RID: 7456 RVA: 0x0007F09C File Offset: 0x0007D29C
		public void PlayBackgroundMusic(Context instigator, AudioReference audioReference, float volume, bool loop, float fadeTime)
		{
			volume = Mathf.Clamp01(volume);
			fadeTime = Mathf.Clamp(fadeTime, 0f, 300f);
			RuntimeAudioInfo runtimeAudioInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out runtimeAudioInfo))
			{
				NetworkBehaviourSingleton<BackgroundAudioManager>.Instance.PlayAudio_ServerOnly(runtimeAudioInfo, volume, loop, fadeTime);
			}
		}

		// Token: 0x06001D21 RID: 7457 RVA: 0x0007F0ED File Offset: 0x0007D2ED
		public void StopBackgroundMusic(Context instigator, float fadeTime)
		{
			fadeTime = Mathf.Clamp(fadeTime, 0f, 300f);
			NetworkBehaviourSingleton<BackgroundAudioManager>.Instance.StopAudio_ServerOnly(fadeTime);
		}

		// Token: 0x06001D22 RID: 7458 RVA: 0x0007F10C File Offset: 0x0007D30C
		public int PlayAmbient(Context instigator, AudioReference audioReference, float volume, bool loop, float fadeTime)
		{
			volume = Mathf.Clamp01(volume);
			fadeTime = Mathf.Clamp(fadeTime, 0f, 1000f);
			RuntimeAudioInfo runtimeAudioInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out runtimeAudioInfo))
			{
				return NetworkBehaviourSingleton<AmbientSoundManager>.Instance.PlayAudio_ServerOnly(runtimeAudioInfo, volume, loop, fadeTime);
			}
			return -1;
		}

		// Token: 0x06001D23 RID: 7459 RVA: 0x0007F15F File Offset: 0x0007D35F
		public void StopAmbient(Context instigator, int audioId, float fadeOutTime)
		{
			if (audioId < 0)
			{
				return;
			}
			fadeOutTime = Mathf.Clamp(fadeOutTime, 0f, 1000f);
			NetworkBehaviourSingleton<AmbientSoundManager>.Instance.StopAudio_ServerOnly(audioId, fadeOutTime);
		}

		// Token: 0x06001D24 RID: 7460 RVA: 0x0007F184 File Offset: 0x0007D384
		public void StopAllAmbient(Context instigator, float fadeOutTime)
		{
			NetworkBehaviourSingleton<AmbientSoundManager>.Instance.StopAllAudio_ServerOnly(fadeOutTime);
		}

		// Token: 0x06001D25 RID: 7461 RVA: 0x0007F194 File Offset: 0x0007D394
		public int PlaySfx(Context instigator, AudioReference audioReference, Vector3 position, AudioManager.SfxConfiguration sfxConfiguration)
		{
			return this.PlaySfx(instigator, audioReference, position, sfxConfiguration.Volume, sfxConfiguration.Loop, sfxConfiguration.Pitch, sfxConfiguration.MinDistance, sfxConfiguration.MaxDistance);
		}

		// Token: 0x06001D26 RID: 7462 RVA: 0x0007F1D0 File Offset: 0x0007D3D0
		public int PlaySfx(Context instigator, AudioReference audioReference, Context source, AudioManager.SfxConfiguration sfxConfiguration)
		{
			return this.PlaySfx(instigator, audioReference, source, sfxConfiguration.Volume, sfxConfiguration.Loop, sfxConfiguration.Pitch, sfxConfiguration.MinDistance, sfxConfiguration.MaxDistance);
		}

		// Token: 0x06001D27 RID: 7463 RVA: 0x0007F20C File Offset: 0x0007D40C
		public int PlaySfx(Context instigator, AudioReference audioReference, Context source, string transformReference, AudioManager.SfxConfiguration sfxConfiguration)
		{
			return this.PlaySfx(instigator, audioReference, source, transformReference, sfxConfiguration.Volume, sfxConfiguration.Loop, sfxConfiguration.Pitch, sfxConfiguration.MinDistance, sfxConfiguration.MaxDistance);
		}

		// Token: 0x06001D28 RID: 7464 RVA: 0x0007F248 File Offset: 0x0007D448
		private int PlaySfx(Context instigator, AudioReference audioReference, Vector3 position, float volume, bool loop, float pitch, float minDistance, float maxDistance)
		{
			RuntimeAudioInfo runtimeAudioInfo;
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out runtimeAudioInfo))
			{
				volume = Mathf.Clamp01(volume);
				return NetworkBehaviourSingleton<SfxManager>.Instance.PlayAudio_ServerOnly(runtimeAudioInfo, position, volume, loop, pitch, minDistance, maxDistance);
			}
			return -1;
		}

		// Token: 0x06001D29 RID: 7465 RVA: 0x0007F290 File Offset: 0x0007D490
		private int PlaySfx(Context instigator, AudioReference audioReference, Context source, float volume, bool loop, float pitch, float minDistance, float maxDistance)
		{
			return this.PlaySfx(instigator, audioReference, source, null, volume, loop, pitch, minDistance, maxDistance);
		}

		// Token: 0x06001D2A RID: 7466 RVA: 0x0007F2B4 File Offset: 0x0007D4B4
		private int PlaySfx(Context instigator, AudioReference audioReference, Context source, string transformReference, float volume, bool loop, float pitch, float minDistance, float maxDistance)
		{
			volume = Mathf.Clamp01(volume);
			RuntimeAudioInfo runtimeAudioInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out runtimeAudioInfo))
			{
				return -1;
			}
			if (source.WorldObject.NetworkObject != null)
			{
				return NetworkBehaviourSingleton<SfxManager>.Instance.PlayAudio_ServerOnly(runtimeAudioInfo, source.WorldObject.NetworkObject.NetworkObjectId, new SerializableGuid?(transformReference), volume, loop, pitch, minDistance, maxDistance);
			}
			return NetworkBehaviourSingleton<SfxManager>.Instance.PlayAudio_ServerOnly(runtimeAudioInfo, source.WorldObject.InstanceId, new SerializableGuid?(transformReference), volume, loop, pitch, minDistance, maxDistance);
		}

		// Token: 0x06001D2B RID: 7467 RVA: 0x0007F356 File Offset: 0x0007D556
		public void StopSfx(Context instigator, int audioId)
		{
			if (audioId < 0)
			{
				return;
			}
			NetworkBehaviourSingleton<SfxManager>.Instance.StopAudio_ServerOnly(audioId);
		}

		// Token: 0x040016D3 RID: 5843
		private static AudioManager instance;

		// Token: 0x020004A0 RID: 1184
		public class SfxConfiguration
		{
			// Token: 0x06001D2D RID: 7469 RVA: 0x0007F368 File Offset: 0x0007D568
			internal SfxConfiguration(bool loop)
			{
				this.Volume = 1f;
				this.Pitch = 1f;
				this.Loop = loop;
				this.MinDistance = 5f;
				this.MaxDistance = 30f;
			}

			// Token: 0x040016D4 RID: 5844
			public float Volume = 1f;

			// Token: 0x040016D5 RID: 5845
			public float Pitch = 1f;

			// Token: 0x040016D6 RID: 5846
			public bool Loop;

			// Token: 0x040016D7 RID: 5847
			public float MinDistance = 1f;

			// Token: 0x040016D8 RID: 5848
			public float MaxDistance = 30f;
		}
	}
}
