using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

namespace Endless.Gameplay;

public class BackgroundAudioManager : NetworkBehaviourSingleton<BackgroundAudioManager>, IStartSubscriber
{
	private enum AudioState
	{
		None,
		Transitioning,
		Active
	}

	[SerializeField]
	private AudioMixer audioMixer;

	[SerializeField]
	private AudioSource[] sources;

	[SerializeField]
	private string[] mixerVolumeParamNames;

	private int[] activeFileIds;

	private AudioState currentState;

	private int currentActiveSource;

	private int targetFileId = -1;

	private Coroutine activeCoroutine;

	private CancellationTokenSource tokenSource;

	private LateJoinAudioInfo lateJoinInfo;

	protected override void Awake()
	{
		base.Awake();
		activeFileIds = new int[sources.Length];
		for (int i = 0; i < activeFileIds.Length; i++)
		{
			activeFileIds[i] = -1;
		}
	}

	private void Start()
	{
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(ResetAudio);
		ResetAudio();
		MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
	}

	public void EndlessStart()
	{
		if (lateJoinInfo.ActiveFileId != -1 && base.IsClient)
		{
			float num = NetworkManager.Singleton.ServerTime.TimeAsFloat - lateJoinInfo.TimeStamp;
			if (lateJoinInfo.Loop || num < lateJoinInfo.Duration)
			{
				num %= lateJoinInfo.Duration;
				PlayAudio(fadeTime: (!(num > 0f)) ? lateJoinInfo.FadeTime : ((!(num > lateJoinInfo.FadeTime)) ? (lateJoinInfo.FadeTime - num) : 0f), audioFileInstanceId: lateJoinInfo.ActiveFileId, audioType: lateJoinInfo.AudioType, volume: lateJoinInfo.Volume, loop: lateJoinInfo.Loop, fastForward: num);
			}
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

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		base.OnSynchronize(ref serializer);
		serializer.SerializeValue(ref lateJoinInfo, default(FastBufferWriter.ForNetworkSerializable));
	}

	private void ResetAudio()
	{
		lateJoinInfo = default(LateJoinAudioInfo);
		if (tokenSource != null)
		{
			tokenSource.Cancel();
		}
		tokenSource = new CancellationTokenSource();
		currentState = AudioState.None;
		if (activeCoroutine != null)
		{
			StopCoroutine(activeCoroutine);
		}
		AudioSource[] array = sources;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Stop();
		}
		for (int j = 0; j < activeFileIds.Length; j++)
		{
			if (activeFileIds[j] != -1)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, activeFileIds[j]);
				activeFileIds[j] = -1;
			}
		}
	}

	public async void PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, float volume, bool loop, float fadeTime)
	{
		int audioFileInstanceId = audioInfo.AudioAsset.AudioFileInstanceId;
		CancellationToken token = tokenSource.Token;
		targetFileId = audioFileInstanceId;
		while (currentState == AudioState.Transitioning && !token.IsCancellationRequested)
		{
			await Task.Yield();
		}
		if (targetFileId == audioFileInstanceId && !token.IsCancellationRequested)
		{
			lateJoinInfo = new LateJoinAudioInfo
			{
				ActiveFileId = audioFileInstanceId,
				AudioType = audioInfo.AudioAsset.AudioType,
				Duration = audioInfo.AudioAsset.Duration,
				Loop = loop,
				FadeTime = fadeTime,
				Volume = volume,
				TimeStamp = NetworkManager.Singleton.ServerTime.TimeAsFloat
			};
			PlayAudio_ClientRpc(audioFileInstanceId, audioInfo.AudioAsset.AudioType, volume, loop, fadeTime);
		}
	}

	public void StopAudio_ServerOnly(float fadeOutTime)
	{
		targetFileId = -1;
		lateJoinInfo = default(LateJoinAudioInfo);
		StopAudio_ClientRpc(fadeOutTime);
	}

	[ClientRpc]
	private void StopAudio_ClientRpc(float fadeOutTime)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3836156267u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in fadeOutTime, default(FastBufferWriter.ForPrimitives));
			__endSendClientRpc(ref bufferWriter, 3836156267u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (activeCoroutine != null)
			{
				StopCoroutine(activeCoroutine);
			}
			currentState = AudioState.Transitioning;
			activeCoroutine = StartCoroutine(StopMusic(fadeOutTime));
		}
	}

	private IEnumerator StopMusic(float fadeOutTime)
	{
		if (fadeOutTime > 0f)
		{
			int num = currentActiveSource + 1;
			if (num >= sources.Length)
			{
				num = 0;
			}
			bool fadeOutInProgress = true;
			bool fadeOutTwoInProgress = true;
			IEnumerator fadeOut = FadeSourceMixer(currentActiveSource, 0f, fadeOutTime);
			IEnumerator fadeOut2 = FadeSourceMixer(num, 0f, fadeOutTime);
			while (fadeOutInProgress || fadeOutTwoInProgress)
			{
				if (fadeOutInProgress)
				{
					fadeOutInProgress = fadeOut.MoveNext();
				}
				if (fadeOutTwoInProgress)
				{
					fadeOutTwoInProgress = fadeOut2.MoveNext();
				}
				yield return null;
			}
		}
		ResetAudio();
	}

	[ClientRpc]
	private void PlayAudio_ClientRpc(int audioFileInstanceId, AudioType audioType, float volume, bool loop, float fadeTime)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(2452794245u, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(bufferWriter, audioFileInstanceId);
				bufferWriter.WriteValueSafe(in audioType, default(FastBufferWriter.ForEnums));
				bufferWriter.WriteValueSafe(in volume, default(FastBufferWriter.ForPrimitives));
				bufferWriter.WriteValueSafe(in loop, default(FastBufferWriter.ForPrimitives));
				bufferWriter.WriteValueSafe(in fadeTime, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 2452794245u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				PlayAudio(audioFileInstanceId, audioType, volume, loop, fadeTime);
			}
		}
	}

	private void PlayAudio(int audioFileInstanceId, AudioType audioType, float volume, bool loop, float fadeTime, float fastForward = 0f)
	{
		if (activeCoroutine != null)
		{
			StopCoroutine(activeCoroutine);
		}
		currentState = AudioState.Transitioning;
		activeCoroutine = StartCoroutine(SwitchTracks(fadeTime, volume, loop, audioFileInstanceId, audioType, fastForward));
	}

	private IEnumerator SwitchTracks(float fadeTime, float volume, bool loopNew, int audioFileInstanceId, AudioType audioType, float fastForward = 0f)
	{
		int previousSourceIndex = currentActiveSource;
		int nextSourceIndex = currentActiveSource + 1;
		if (nextSourceIndex >= sources.Length)
		{
			nextSourceIndex = 0;
		}
		activeFileIds[nextSourceIndex] = audioFileInstanceId;
		Task<AudioClip> getFileTask = MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAudioFileAsync(this, audioFileInstanceId, audioType);
		while (!getFileTask.IsCompleted)
		{
			yield return null;
		}
		AudioClip result = getFileTask.Result;
		if (result == null)
		{
			Debug.LogError($"Failed to load music file {audioFileInstanceId}");
			yield break;
		}
		sources[nextSourceIndex].Stop();
		if (fadeTime > 0f)
		{
			audioMixer.SetFloat(mixerVolumeParamNames[nextSourceIndex], AudioUtility.VolumeToDecibel(0f));
		}
		else
		{
			audioMixer.SetFloat(mixerVolumeParamNames[nextSourceIndex], AudioUtility.VolumeToDecibel(1f));
		}
		currentActiveSource = nextSourceIndex;
		sources[nextSourceIndex].clip = result;
		sources[nextSourceIndex].loop = loopNew;
		sources[nextSourceIndex].volume = volume;
		sources[nextSourceIndex].Play();
		if (fastForward > 0f)
		{
			sources[nextSourceIndex].time = fastForward;
		}
		if (fadeTime > 0f)
		{
			bool fadeInHappening = true;
			bool fadeOutHappening = true;
			IEnumerator fadeOut = FadeSourceMixer(previousSourceIndex, 0f, fadeTime);
			IEnumerator fadeIn = FadeSourceMixer(nextSourceIndex, 1f, fadeTime);
			while (fadeInHappening || fadeOutHappening)
			{
				if (fadeInHappening)
				{
					fadeInHappening = fadeIn.MoveNext();
				}
				if (fadeOutHappening)
				{
					fadeOutHappening = fadeOut.MoveNext();
				}
				yield return null;
			}
		}
		sources[previousSourceIndex].Stop();
		if (activeFileIds[previousSourceIndex] != -1)
		{
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, activeFileIds[previousSourceIndex]);
			activeFileIds[previousSourceIndex] = -1;
		}
		currentState = AudioState.Active;
	}

	private IEnumerator FadeSourceMixer(int sourceIndex, float targetVolume, float duration)
	{
		string exposedParam = mixerVolumeParamNames[sourceIndex];
		audioMixer.GetFloat(exposedParam, out var startVolume);
		startVolume = AudioUtility.DecibelToVolume(startVolume);
		float targetValue = Mathf.Clamp01(targetVolume);
		for (float elapsedTime = 0f; elapsedTime < duration; elapsedTime += Time.deltaTime)
		{
			float t = elapsedTime / duration;
			float value = AudioUtility.VolumeToDecibel(Mathf.Lerp(startVolume, targetValue, t));
			audioMixer.SetFloat(exposedParam, value);
			yield return null;
		}
		audioMixer.SetFloat(exposedParam, AudioUtility.VolumeToDecibel(targetVolume));
		if (targetValue == 0f)
		{
			sources[sourceIndex].Stop();
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3836156267u, __rpc_handler_3836156267, "StopAudio_ClientRpc");
		__registerRpc(2452794245u, __rpc_handler_2452794245, "PlayAudio_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3836156267(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out float value, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((BackgroundAudioManager)target).StopAudio_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2452794245(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			reader.ReadValueSafe(out AudioType value2, default(FastBufferWriter.ForEnums));
			reader.ReadValueSafe(out float value3, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out bool value4, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value5, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((BackgroundAudioManager)target).PlayAudio_ClientRpc(value, value2, value3, value4, value5);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "BackgroundAudioManager";
	}
}
