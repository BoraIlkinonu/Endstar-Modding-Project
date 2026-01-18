using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class SfxManager : NetworkBehaviourSingleton<SfxManager>, IGameEndSubscriber, IStartSubscriber
{
	[SerializeField]
	private PoolableSfxSource sfxSourcePrefab;

	private Dictionary<int, Coroutine> activeAudioRoutines = new Dictionary<int, Coroutine>();

	private Dictionary<int, PoolableSfxSource> activeAudioSources = new Dictionary<int, PoolableSfxSource>();

	private int nextAudioId;

	private const int LATE_JOIN_DURATION_CUTOFF = 60;

	private Dictionary<int, PositionalAudioInfo> lateJoinAudioMap = new Dictionary<int, PositionalAudioInfo>();

	private void Start()
	{
		MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool(sfxSourcePrefab, 10);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(HandleGameplayCleanup);
		MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
	}

	private void HandleGameplayCleanup()
	{
		ResetAudio();
	}

	public void EndlessStart()
	{
		if (base.IsClient)
		{
			foreach (int key in lateJoinAudioMap.Keys)
			{
				PositionalAudioInfo audioInfo = lateJoinAudioMap[key];
				float num = NetworkManager.Singleton.ServerTime.TimeAsFloat - audioInfo.TimeStamp;
				if (audioInfo.Loop || num < audioInfo.Duration)
				{
					num %= audioInfo.Duration;
				}
				Coroutine coroutine = StartCoroutine(PlayAudio(audioInfo.ActiveFileId, audioInfo, num));
				if (coroutine != null)
				{
					activeAudioRoutines[key] = coroutine;
				}
			}
		}
		lateJoinAudioMap.Clear();
	}

	public void EndlessGameEnd()
	{
		ResetAudio();
	}

	private void ResetAudio()
	{
		foreach (int key in activeAudioRoutines.Keys)
		{
			StopCoroutine(activeAudioRoutines[key]);
		}
		foreach (int key2 in activeAudioSources.Keys)
		{
			CleanupAudioSource(activeAudioSources[key2]);
		}
		activeAudioRoutines.Clear();
		activeAudioSources.Clear();
		lateJoinAudioMap.Clear();
	}

	private static void CleanupAudioSource(PoolableSfxSource poolableAudioSource)
	{
		poolableAudioSource.AudioSource.Stop();
		poolableAudioSource.AudioSource.clip = null;
		MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAllAccesses(poolableAudioSource);
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(poolableAudioSource);
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

	public int PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, Vector3 position, float volume, bool loop, float pitch, float minDistance, float maxDistance)
	{
		int num = GetNextAudioId();
		float duration = audioInfo.AudioAsset.Duration;
		PositionalAudioInfo positionalAudioInfo = new PositionalAudioInfo
		{
			ActiveFileId = audioInfo.AudioAsset.AudioFileInstanceId,
			AudioType = audioInfo.AudioAsset.AudioType,
			Duration = duration,
			Loop = loop,
			FadeTime = 0f,
			TimeStamp = NetworkManager.Singleton.ServerTime.TimeAsFloat,
			Position = position,
			Pitch = pitch,
			MinDistance = minDistance,
			MaxDistance = maxDistance,
			Volume = volume,
			InstanceId = SerializableGuid.Empty,
			TransformId = SerializableGuid.Empty
		};
		if (loop || duration > 60f)
		{
			lateJoinAudioMap.Add(num, positionalAudioInfo);
		}
		PlayAudio_ClientRpc(num, positionalAudioInfo);
		return num;
	}

	public int PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, ulong networkId, SerializableGuid? transformId, float volume, bool loop, float pitch, float minDistance, float maxDistance)
	{
		int num = GetNextAudioId();
		float duration = audioInfo.AudioAsset.Duration;
		SerializableGuid transformId2 = (transformId.HasValue ? transformId.Value : SerializableGuid.Empty);
		PositionalAudioInfo positionalAudioInfo = new PositionalAudioInfo
		{
			ActiveFileId = audioInfo.AudioAsset.AudioFileInstanceId,
			AudioType = audioInfo.AudioAsset.AudioType,
			Duration = duration,
			Loop = loop,
			FadeTime = 0f,
			TimeStamp = NetworkManager.Singleton.ServerTime.TimeAsFloat,
			Position = Vector3.zero,
			Volume = volume,
			Pitch = pitch,
			MinDistance = minDistance,
			MaxDistance = maxDistance,
			NetworkId = networkId,
			TransformId = transformId2
		};
		if (loop || duration > 60f)
		{
			lateJoinAudioMap.Add(num, positionalAudioInfo);
		}
		PlayAudio_ClientRpc(num, positionalAudioInfo);
		return num;
	}

	public int PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, SerializableGuid instanceId, SerializableGuid? transformId, float volume, bool loop, float pitch, float minDistance, float maxDistance)
	{
		int num = GetNextAudioId();
		float duration = audioInfo.AudioAsset.Duration;
		SerializableGuid transformId2 = (transformId.HasValue ? transformId.Value : SerializableGuid.Empty);
		PositionalAudioInfo positionalAudioInfo = new PositionalAudioInfo
		{
			ActiveFileId = audioInfo.AudioAsset.AudioFileInstanceId,
			AudioType = audioInfo.AudioAsset.AudioType,
			Duration = duration,
			Loop = loop,
			FadeTime = 0f,
			TimeStamp = NetworkManager.Singleton.ServerTime.TimeAsFloat,
			Position = Vector3.zero,
			Volume = volume,
			Pitch = pitch,
			MinDistance = minDistance,
			MaxDistance = maxDistance,
			InstanceId = instanceId,
			TransformId = transformId2
		};
		if (loop || duration > 60f)
		{
			lateJoinAudioMap.Add(num, positionalAudioInfo);
		}
		PlayAudio_ClientRpc(num, positionalAudioInfo);
		return num;
	}

	[ClientRpc]
	private void PlayAudio_ClientRpc(int audioId, PositionalAudioInfo positionalAudioInfo)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(1772686040u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, audioId);
			bufferWriter.WriteValueSafe(in positionalAudioInfo, default(FastBufferWriter.ForNetworkSerializable));
			__endSendClientRpc(ref bufferWriter, 1772686040u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			Coroutine coroutine = StartCoroutine(PlayAudio(audioId, positionalAudioInfo));
			if (coroutine != null)
			{
				activeAudioRoutines[audioId] = coroutine;
			}
		}
	}

	private IEnumerator PlayAudio(int audioId, PositionalAudioInfo audioInfo, float elapsedTime = 0f)
	{
		Transform followTarget = null;
		Vector3 position;
		if (!audioInfo.InstanceId.IsEmpty || audioInfo.NetworkId != 0L)
		{
			GameObject gameObject = ((audioInfo.NetworkId == 0L) ? MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(audioInfo.InstanceId) : NetworkManager.Singleton.SpawnManager.SpawnedObjects[audioInfo.NetworkId].gameObject);
			WorldObject component = gameObject.GetComponent<WorldObject>();
			if (audioInfo.TransformId.IsEmpty || !component.EndlessProp.TransformMap.TryGetValue(audioInfo.TransformId, out followTarget))
			{
				followTarget = component.BaseTypeComponent.transform;
			}
			position = followTarget.position;
		}
		else
		{
			position = audioInfo.Position;
		}
		PoolableSfxSource poolableAudioSource = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(sfxSourcePrefab, position);
		Task<AudioClip> getFileTask = MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAudioFileAsync(poolableAudioSource, audioInfo.ActiveFileId, audioInfo.AudioType);
		activeAudioSources[audioId] = poolableAudioSource;
		while (!getFileTask.IsCompleted)
		{
			yield return null;
		}
		AudioClip result = getFileTask.Result;
		poolableAudioSource.AudioSource.clip = result;
		poolableAudioSource.AudioSource.loop = audioInfo.Loop;
		poolableAudioSource.AudioSource.pitch = audioInfo.Pitch;
		poolableAudioSource.AudioSource.volume = audioInfo.Volume;
		poolableAudioSource.AudioSource.minDistance = audioInfo.MinDistance;
		poolableAudioSource.AudioSource.maxDistance = audioInfo.MaxDistance;
		poolableAudioSource.AudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, Logarithmic(audioInfo.MinDistance / audioInfo.MaxDistance, 1f, 1f));
		if (elapsedTime > 0f && elapsedTime < result.length)
		{
			poolableAudioSource.AudioSource.time = elapsedTime;
		}
		poolableAudioSource.transform.SetParent(base.transform);
		if (followTarget != null)
		{
			poolableAudioSource.SetAudioIdAndFollowTarget(audioId, followTarget);
			poolableAudioSource.OnSelfDisabled.AddListener(HandleSfxSelfDisabled);
		}
		poolableAudioSource.AudioSource.Play();
		if (!audioInfo.Loop)
		{
			yield return new WaitForSeconds(audioInfo.Duration);
			CleanupAudioSource(activeAudioSources[audioId]);
			activeAudioSources.Remove(audioId);
		}
		activeAudioRoutines.Remove(audioId);
	}

	private static AnimationCurve Logarithmic(float timeStart, float timeEnd, float logBase)
	{
		List<Keyframe> list = new List<Keyframe>();
		float num = 2f;
		timeStart = Mathf.Max(timeStart, 0.0001f);
		bool flag = true;
		float value;
		float num3;
		float num4;
		for (float num2 = timeStart; num2 < timeEnd; num2 *= num)
		{
			if (flag)
			{
				value = LogarithmicValue(num2, timeStart, logBase);
				flag = false;
			}
			else
			{
				value = LogarithmicValue(num2, timeStart, logBase) - timeStart;
			}
			num3 = num2 / 50f;
			num4 = (LogarithmicValue(num2 + num3, timeStart, logBase) - LogarithmicValue(num2 - num3, timeStart, logBase)) / (num3 * 2f);
			list.Add(new Keyframe(num2, value, num4, num4));
		}
		value = LogarithmicValue(timeEnd, timeStart, logBase);
		num3 = timeEnd / 50f;
		num4 = (LogarithmicValue(timeEnd + num3, timeStart, logBase) - LogarithmicValue(timeEnd - num3, timeStart, logBase)) / (num3 * 2f);
		list.Add(new Keyframe(timeEnd, 0f, num4, num4));
		return new AnimationCurve(list.ToArray());
	}

	private static float LogarithmicValue(float distance, float minDistance, float rolloffScale)
	{
		if (distance > minDistance && rolloffScale != 1f)
		{
			distance -= minDistance;
			distance *= rolloffScale;
			distance += minDistance;
		}
		if (distance < 1E-06f)
		{
			distance = 1E-06f;
		}
		return minDistance / distance;
	}

	public void StopAudio_ServerOnly(int audioId)
	{
		lateJoinAudioMap.Remove(audioId);
		StopAudio_ClientRpc(audioId);
	}

	[ClientRpc]
	private void StopAudio_ClientRpc(int audioId)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2879044725u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, audioId);
			__endSendClientRpc(ref bufferWriter, 2879044725u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.IsServer)
			{
				lateJoinAudioMap.Remove(audioId);
			}
			if (activeAudioRoutines.TryGetValue(audioId, out var value))
			{
				StopCoroutine(value);
				activeAudioRoutines.Remove(audioId);
			}
			if (activeAudioSources.TryGetValue(audioId, out var value2))
			{
				CleanupAudioSource(value2);
				activeAudioSources.Remove(audioId);
			}
		}
	}

	private void HandleSfxSelfDisabled(int audioId)
	{
		if (base.IsServer)
		{
			lateJoinAudioMap.Remove(audioId);
			StopAudio_ClientRpc(audioId);
		}
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		base.OnSynchronize(ref serializer);
		int value = 0;
		if (serializer.IsWriter)
		{
			value = lateJoinAudioMap.Count;
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		if (value <= 0)
		{
			return;
		}
		int[] array;
		PositionalAudioInfo[] array2;
		if (serializer.IsWriter)
		{
			array = lateJoinAudioMap.Keys.ToArray();
			array2 = lateJoinAudioMap.Values.ToArray();
		}
		else
		{
			array = new int[value];
			array2 = new PositionalAudioInfo[value];
			for (int i = 0; i < value; i++)
			{
				array2[i] = default(PositionalAudioInfo);
			}
		}
		for (int j = 0; j < value; j++)
		{
			serializer.SerializeValue(ref array[j], default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref array2[j], default(FastBufferWriter.ForNetworkSerializable));
			if (serializer.IsReader)
			{
				lateJoinAudioMap.Add(array[j], array2[j]);
			}
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1772686040u, __rpc_handler_1772686040, "PlayAudio_ClientRpc");
		__registerRpc(2879044725u, __rpc_handler_2879044725, "StopAudio_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1772686040(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			reader.ReadValueSafe(out PositionalAudioInfo value2, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((SfxManager)target).PlayAudio_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2879044725(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out int value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((SfxManager)target).StopAudio_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "SfxManager";
	}
}
