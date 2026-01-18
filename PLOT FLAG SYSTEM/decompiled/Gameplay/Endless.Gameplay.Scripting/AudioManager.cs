using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class AudioManager
{
	public class SfxConfiguration
	{
		public float Volume = 1f;

		public float Pitch = 1f;

		public bool Loop;

		public float MinDistance = 1f;

		public float MaxDistance = 30f;

		internal SfxConfiguration(bool loop)
		{
			Volume = 1f;
			Pitch = 1f;
			Loop = loop;
			MinDistance = 5f;
			MaxDistance = 30f;
		}
	}

	private static AudioManager instance;

	internal static AudioManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new AudioManager();
			}
			return instance;
		}
	}

	public SfxConfiguration GetSfxConfig(bool loop)
	{
		return new SfxConfiguration(loop);
	}

	public SfxConfiguration GetSfxConfig()
	{
		return new SfxConfiguration(loop: false);
	}

	public void PlayBackgroundMusic(Context instigator, AudioReference audioReference, float volume, bool loop, float fadeTime)
	{
		volume = Mathf.Clamp01(volume);
		fadeTime = Mathf.Clamp(fadeTime, 0f, 300f);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out var metadata))
		{
			NetworkBehaviourSingleton<BackgroundAudioManager>.Instance.PlayAudio_ServerOnly(metadata, volume, loop, fadeTime);
		}
	}

	public void StopBackgroundMusic(Context instigator, float fadeTime)
	{
		fadeTime = Mathf.Clamp(fadeTime, 0f, 300f);
		NetworkBehaviourSingleton<BackgroundAudioManager>.Instance.StopAudio_ServerOnly(fadeTime);
	}

	public int PlayAmbient(Context instigator, AudioReference audioReference, float volume, bool loop, float fadeTime)
	{
		volume = Mathf.Clamp01(volume);
		fadeTime = Mathf.Clamp(fadeTime, 0f, 1000f);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out var metadata))
		{
			return NetworkBehaviourSingleton<AmbientSoundManager>.Instance.PlayAudio_ServerOnly(metadata, volume, loop, fadeTime);
		}
		return -1;
	}

	public void StopAmbient(Context instigator, int audioId, float fadeOutTime)
	{
		if (audioId >= 0)
		{
			fadeOutTime = Mathf.Clamp(fadeOutTime, 0f, 1000f);
			NetworkBehaviourSingleton<AmbientSoundManager>.Instance.StopAudio_ServerOnly(audioId, fadeOutTime);
		}
	}

	public void StopAllAmbient(Context instigator, float fadeOutTime)
	{
		NetworkBehaviourSingleton<AmbientSoundManager>.Instance.StopAllAudio_ServerOnly(fadeOutTime);
	}

	public int PlaySfx(Context instigator, AudioReference audioReference, UnityEngine.Vector3 position, SfxConfiguration sfxConfiguration)
	{
		return PlaySfx(instigator, audioReference, position, sfxConfiguration.Volume, sfxConfiguration.Loop, sfxConfiguration.Pitch, sfxConfiguration.MinDistance, sfxConfiguration.MaxDistance);
	}

	public int PlaySfx(Context instigator, AudioReference audioReference, Context source, SfxConfiguration sfxConfiguration)
	{
		return PlaySfx(instigator, audioReference, source, sfxConfiguration.Volume, sfxConfiguration.Loop, sfxConfiguration.Pitch, sfxConfiguration.MinDistance, sfxConfiguration.MaxDistance);
	}

	public int PlaySfx(Context instigator, AudioReference audioReference, Context source, string transformReference, SfxConfiguration sfxConfiguration)
	{
		return PlaySfx(instigator, audioReference, source, transformReference, sfxConfiguration.Volume, sfxConfiguration.Loop, sfxConfiguration.Pitch, sfxConfiguration.MinDistance, sfxConfiguration.MaxDistance);
	}

	private int PlaySfx(Context instigator, AudioReference audioReference, UnityEngine.Vector3 position, float volume, bool loop, float pitch, float minDistance, float maxDistance)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out var metadata))
		{
			volume = Mathf.Clamp01(volume);
			return NetworkBehaviourSingleton<SfxManager>.Instance.PlayAudio_ServerOnly(metadata, position, volume, loop, pitch, minDistance, maxDistance);
		}
		return -1;
	}

	private int PlaySfx(Context instigator, AudioReference audioReference, Context source, float volume, bool loop, float pitch, float minDistance, float maxDistance)
	{
		return PlaySfx(instigator, audioReference, source, null, volume, loop, pitch, minDistance, maxDistance);
	}

	private int PlaySfx(Context instigator, AudioReference audioReference, Context source, string transformReference, float volume, bool loop, float pitch, float minDistance, float maxDistance)
	{
		volume = Mathf.Clamp01(volume);
		if (MonoBehaviourSingleton<StageManager>.Instance.ActiveAudioLibrary.TryGetRuntimeAudioInfo(audioReference.Id, out var metadata))
		{
			if (source.WorldObject.NetworkObject != null)
			{
				return NetworkBehaviourSingleton<SfxManager>.Instance.PlayAudio_ServerOnly(metadata, source.WorldObject.NetworkObject.NetworkObjectId, transformReference, volume, loop, pitch, minDistance, maxDistance);
			}
			return NetworkBehaviourSingleton<SfxManager>.Instance.PlayAudio_ServerOnly(metadata, source.WorldObject.InstanceId, transformReference, volume, loop, pitch, minDistance, maxDistance);
		}
		return -1;
	}

	public void StopSfx(Context instigator, int audioId)
	{
		if (audioId >= 0)
		{
			NetworkBehaviourSingleton<SfxManager>.Instance.StopAudio_ServerOnly(audioId);
		}
	}
}
