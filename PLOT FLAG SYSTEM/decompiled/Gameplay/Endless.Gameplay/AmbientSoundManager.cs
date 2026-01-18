using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class AmbientSoundManager : NetworkBehaviourSingleton<AmbientSoundManager>, IStartSubscriber
{
	private class AudioSourceTrackingInfo
	{
		public Coroutine FadeCoroutine;

		public readonly PoolableAudioSource PoolableAudioSource;

		public readonly int FileId;

		public AudioSourceTrackingInfo(int fileId, PoolableAudioSource poolableAudioSource, Coroutine fadeCoroutine = null)
		{
			FileId = fileId;
			PoolableAudioSource = poolableAudioSource;
			FadeCoroutine = fadeCoroutine;
		}
	}

	[SerializeField]
	private PoolableAudioSource ambientSourcePrefab;

	private int nextAudioId;

	private readonly Dictionary<int, LateJoinAudioInfo> playingAudioMap = new Dictionary<int, LateJoinAudioInfo>();

	private readonly Dictionary<int, AudioSourceTrackingInfo> activeAudioSources = new Dictionary<int, AudioSourceTrackingInfo>();

	private void Start()
	{
		MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool(ambientSourcePrefab, 3);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(HandleGameplayCleanup);
		MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
	}

	private void HandleGameplayCleanup()
	{
		if (base.IsServer)
		{
			playingAudioMap.Clear();
		}
		if (base.IsClient)
		{
			StopAllAudio();
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)MonoBehaviourSingleton<EndlessLoop>.Instance)
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
		}
	}

	private int GetNextAudioId()
	{
		int result = nextAudioId++;
		if (nextAudioId == int.MaxValue)
		{
			result = 0;
		}
		return result;
	}

	public int PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, float volume, bool loop, float fadeInTime)
	{
		int num = GetNextAudioId();
		LateJoinAudioInfo value = new LateJoinAudioInfo
		{
			ActiveFileId = audioInfo.AudioAsset.AudioFileInstanceId,
			AudioType = audioInfo.AudioAsset.AudioType,
			Duration = audioInfo.AudioAsset.Duration,
			Loop = loop,
			FadeTime = fadeInTime,
			Volume = volume,
			TimeStamp = NetworkManager.Singleton.ServerTime.TimeAsFloat
		};
		playingAudioMap.Add(num, value);
		PlayAudio_ClientRpc(num, audioInfo.AudioAsset.AudioFileInstanceId, audioInfo.AudioAsset.AudioType, volume, loop, fadeInTime);
		return num;
	}

	public void StopAudio_ServerOnly(int audioId, float fadeOutTime = 0f)
	{
		if (playingAudioMap.Remove(audioId))
		{
			StopAudio_ClientRpc(audioId, fadeOutTime);
		}
	}

	[ClientRpc]
	private void StopAudio_ClientRpc(int audioId, float fadeTime)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(3158898487u, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(bufferWriter, audioId);
				bufferWriter.WriteValueSafe(in fadeTime, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 3158898487u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				StopAudio(audioId, fadeTime);
			}
		}
	}

	private void StopAudio(int audioId, float fadeTime)
	{
		if (activeAudioSources.TryGetValue(audioId, out var value))
		{
			if (value.FadeCoroutine != null)
			{
				StopCoroutine(value.FadeCoroutine);
				value.FadeCoroutine = null;
			}
			if (fadeTime > 0f)
			{
				value.FadeCoroutine = StartCoroutine(FadeOut(fadeTime, audioId, value));
			}
			else
			{
				ResetSource(audioId, value);
			}
		}
	}

	private void ResetSource(int audioId, AudioSourceTrackingInfo audioSourceTrackingInfo)
	{
		audioSourceTrackingInfo.PoolableAudioSource.AudioSource.Stop();
		audioSourceTrackingInfo.PoolableAudioSource.AudioSource.clip = null;
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(audioSourceTrackingInfo.PoolableAudioSource);
		MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, activeAudioSources[audioId].FileId);
		activeAudioSources.Remove(audioId);
	}

	[ClientRpc]
	private void PlayAudio_ClientRpc(int audioId, int audioFileInstanceId, AudioType audioType, float volume, bool loop, float fadeTime)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(3155401033u, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(bufferWriter, audioId);
				BytePacker.WriteValueBitPacked(bufferWriter, audioFileInstanceId);
				bufferWriter.WriteValueSafe(in audioType, default(FastBufferWriter.ForEnums));
				bufferWriter.WriteValueSafe(in volume, default(FastBufferWriter.ForPrimitives));
				bufferWriter.WriteValueSafe(in loop, default(FastBufferWriter.ForPrimitives));
				bufferWriter.WriteValueSafe(in fadeTime, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 3155401033u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				PlayAudio(audioId, audioFileInstanceId, audioType, volume, loop, fadeTime);
			}
		}
	}

	private void PlayAudio(int audioId, int audioFileInstanceId, AudioType audioType, float volume, bool loop, float fadeTime, float fastForward = 0f)
	{
		if (activeAudioSources.TryGetValue(audioId, out var value))
		{
			if (value.FadeCoroutine != null)
			{
				StopCoroutine(value.FadeCoroutine);
				value.FadeCoroutine = null;
			}
			value.FadeCoroutine = StartCoroutine(PlayAudioSource(audioId, value.PoolableAudioSource, fadeTime, volume, loop, audioFileInstanceId, audioType, fastForward));
		}
		else
		{
			PoolableAudioSource poolableAudioSource = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(ambientSourcePrefab);
			poolableAudioSource.transform.SetParent(base.transform);
			poolableAudioSource.AudioSource.volume = 0f;
			activeAudioSources.Add(audioId, new AudioSourceTrackingInfo(audioFileInstanceId, poolableAudioSource, StartCoroutine(PlayAudioSource(audioId, poolableAudioSource, fadeTime, volume, loop, audioFileInstanceId, audioType, fastForward))));
		}
	}

	private IEnumerator PlayAudioSource(int audioId, PoolableAudioSource poolableAudioSource, float fadeTime, float volume, bool loop, int audioFileInstanceId, AudioType audioType, float fastForward = 0f)
	{
		Task<AudioClip> getFileTask = MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAudioFileAsync(this, audioFileInstanceId, audioType);
		while (!getFileTask.IsCompleted)
		{
			yield return null;
		}
		AudioClip audioClip = getFileTask.Result;
		poolableAudioSource.AudioSource.clip = audioClip;
		poolableAudioSource.AudioSource.loop = loop;
		if (fastForward > 0f && fastForward < audioClip.length)
		{
			poolableAudioSource.AudioSource.time = fastForward;
		}
		if (fadeTime <= 0f)
		{
			poolableAudioSource.AudioSource.volume = volume;
		}
		else
		{
			poolableAudioSource.AudioSource.volume = 0f;
		}
		poolableAudioSource.AudioSource.Play();
		if (fadeTime > 0f)
		{
			float start = poolableAudioSource.AudioSource.volume;
			for (float elapsedTime = 0f; elapsedTime < fadeTime; elapsedTime += Time.deltaTime)
			{
				float t = elapsedTime / fadeTime;
				float volume2 = Mathf.Lerp(start, volume, t);
				poolableAudioSource.AudioSource.volume = volume2;
				yield return null;
			}
			poolableAudioSource.AudioSource.volume = volume;
		}
		yield return null;
		if (!loop)
		{
			yield return new WaitForSeconds(audioClip.length - fastForward);
			if (base.IsServer)
			{
				StopAudio_ServerOnly(audioId);
			}
		}
		else
		{
			activeAudioSources[audioId].FadeCoroutine = null;
		}
	}

	private IEnumerator FadeOut(float fadeTime, int audioId, AudioSourceTrackingInfo audioSourceTrackingInfo)
	{
		float start = audioSourceTrackingInfo.PoolableAudioSource.AudioSource.volume;
		float end = 0f;
		for (float elapsedTime = 0f; elapsedTime < fadeTime; elapsedTime += Time.deltaTime)
		{
			float t = elapsedTime / fadeTime;
			float volume = Mathf.Lerp(start, end, t);
			audioSourceTrackingInfo.PoolableAudioSource.AudioSource.volume = volume;
			yield return null;
		}
		ResetSource(audioId, audioSourceTrackingInfo);
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		base.OnSynchronize(ref serializer);
		int value = 0;
		if (serializer.IsWriter)
		{
			value = playingAudioMap.Count;
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		if (value <= 0)
		{
			return;
		}
		int[] array;
		LateJoinAudioInfo[] array2;
		if (serializer.IsWriter)
		{
			array = playingAudioMap.Keys.ToArray();
			array2 = playingAudioMap.Values.ToArray();
		}
		else
		{
			array = new int[value];
			array2 = new LateJoinAudioInfo[value];
			for (int i = 0; i < value; i++)
			{
				array2[i] = default(LateJoinAudioInfo);
			}
		}
		for (int j = 0; j < value; j++)
		{
			serializer.SerializeValue(ref array[j], default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref array2[j], default(FastBufferWriter.ForNetworkSerializable));
			if (serializer.IsReader)
			{
				playingAudioMap.Add(array[j], array2[j]);
			}
		}
	}

	public void EndlessStart()
	{
		if (base.IsServer)
		{
			return;
		}
		foreach (KeyValuePair<int, LateJoinAudioInfo> item in playingAudioMap)
		{
			LateJoinAudioInfo value = item.Value;
			float num = NetworkManager.Singleton.ServerTime.TimeAsFloat - value.TimeStamp;
			if (value.Loop || num < value.Duration)
			{
				num %= value.Duration;
				PlayAudio(fadeTime: (!(num > 0f)) ? value.FadeTime : ((!(num > value.FadeTime)) ? (value.FadeTime - num) : 0f), audioId: item.Key, audioFileInstanceId: value.ActiveFileId, audioType: value.AudioType, volume: value.Volume, loop: value.Loop, fastForward: num);
			}
		}
		playingAudioMap.Clear();
	}

	public void StopAllAudio_ServerOnly(float fadeOutTime = 0f)
	{
		playingAudioMap.Clear();
		StopAllAudio_ClientRpc(fadeOutTime);
	}

	[ClientRpc]
	private void StopAllAudio_ClientRpc(float fadeOutTime)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(674555753u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in fadeOutTime, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 674555753u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				StopAllAudio(fadeOutTime);
			}
		}
	}

	private void StopAllAudio(float fadeOutTime = 0f)
	{
		foreach (int item in (IEnumerable<int>)activeAudioSources.Keys.ToArray())
		{
			StopAudio(item, fadeOutTime);
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3158898487u, __rpc_handler_3158898487, "StopAudio_ClientRpc");
		__registerRpc(3155401033u, __rpc_handler_3155401033, "PlayAudio_ClientRpc");
		__registerRpc(674555753u, __rpc_handler_674555753, "StopAllAudio_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3158898487(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			reader.ReadValueSafe(out float value2, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((AmbientSoundManager)target).StopAudio_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3155401033(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			reader.ReadValueSafe(out AudioType value3, default(FastBufferWriter.ForEnums));
			reader.ReadValueSafe(out float value4, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out bool value5, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value6, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((AmbientSoundManager)target).PlayAudio_ClientRpc(value, value2, value3, value4, value5, value6);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_674555753(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out float value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((AmbientSoundManager)target).StopAllAudio_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "AmbientSoundManager";
	}
}
