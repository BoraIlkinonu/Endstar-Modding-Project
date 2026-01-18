using System;
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
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000C7 RID: 199
	public class SfxManager : NetworkBehaviourSingleton<SfxManager>, IGameEndSubscriber, IStartSubscriber
	{
		// Token: 0x060003CA RID: 970 RVA: 0x00014CF1 File Offset: 0x00012EF1
		private void Start()
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool<PoolableSfxSource>(this.sfxSourcePrefab, 10);
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.HandleGameplayCleanup));
			MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
		}

		// Token: 0x060003CB RID: 971 RVA: 0x00014D2B File Offset: 0x00012F2B
		private void HandleGameplayCleanup()
		{
			this.ResetAudio();
		}

		// Token: 0x060003CC RID: 972 RVA: 0x00014D34 File Offset: 0x00012F34
		public void EndlessStart()
		{
			if (base.IsClient)
			{
				foreach (int num in this.lateJoinAudioMap.Keys)
				{
					PositionalAudioInfo positionalAudioInfo = this.lateJoinAudioMap[num];
					float num2 = NetworkManager.Singleton.ServerTime.TimeAsFloat - positionalAudioInfo.TimeStamp;
					if (positionalAudioInfo.Loop || num2 < positionalAudioInfo.Duration)
					{
						num2 %= positionalAudioInfo.Duration;
					}
					Coroutine coroutine = base.StartCoroutine(this.PlayAudio(positionalAudioInfo.ActiveFileId, positionalAudioInfo, num2));
					if (coroutine != null)
					{
						this.activeAudioRoutines[num] = coroutine;
					}
				}
			}
			this.lateJoinAudioMap.Clear();
		}

		// Token: 0x060003CD RID: 973 RVA: 0x00014D2B File Offset: 0x00012F2B
		public void EndlessGameEnd()
		{
			this.ResetAudio();
		}

		// Token: 0x060003CE RID: 974 RVA: 0x00014E08 File Offset: 0x00013008
		private void ResetAudio()
		{
			foreach (int num in this.activeAudioRoutines.Keys)
			{
				base.StopCoroutine(this.activeAudioRoutines[num]);
			}
			foreach (int num2 in this.activeAudioSources.Keys)
			{
				SfxManager.CleanupAudioSource(this.activeAudioSources[num2]);
			}
			this.activeAudioRoutines.Clear();
			this.activeAudioSources.Clear();
			this.lateJoinAudioMap.Clear();
		}

		// Token: 0x060003CF RID: 975 RVA: 0x00014EE0 File Offset: 0x000130E0
		private static void CleanupAudioSource(PoolableSfxSource poolableAudioSource)
		{
			poolableAudioSource.AudioSource.Stop();
			poolableAudioSource.AudioSource.clip = null;
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAllAccesses(poolableAudioSource);
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<PoolableSfxSource>(poolableAudioSource);
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x00014F10 File Offset: 0x00013110
		private int GetNextAudioId()
		{
			int num = this.nextAudioId;
			this.nextAudioId = num + 1;
			int num2 = num;
			if (this.nextAudioId == 2147483647)
			{
				num2 = 0;
			}
			return num2;
		}

		// Token: 0x060003D1 RID: 977 RVA: 0x00014F40 File Offset: 0x00013140
		public int PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, Vector3 position, float volume, bool loop, float pitch, float minDistance, float maxDistance)
		{
			int num = this.GetNextAudioId();
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
				this.lateJoinAudioMap.Add(num, positionalAudioInfo);
			}
			this.PlayAudio_ClientRpc(num, positionalAudioInfo);
			return num;
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x0001502C File Offset: 0x0001322C
		public int PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, ulong networkId, SerializableGuid? transformId, float volume, bool loop, float pitch, float minDistance, float maxDistance)
		{
			int num = this.GetNextAudioId();
			float duration = audioInfo.AudioAsset.Duration;
			SerializableGuid serializableGuid = ((transformId != null) ? transformId.Value : SerializableGuid.Empty);
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
				TransformId = serializableGuid
			};
			if (loop || duration > 60f)
			{
				this.lateJoinAudioMap.Add(num, positionalAudioInfo);
			}
			this.PlayAudio_ClientRpc(num, positionalAudioInfo);
			return num;
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x0001512C File Offset: 0x0001332C
		public int PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, SerializableGuid instanceId, SerializableGuid? transformId, float volume, bool loop, float pitch, float minDistance, float maxDistance)
		{
			int num = this.GetNextAudioId();
			float duration = audioInfo.AudioAsset.Duration;
			SerializableGuid serializableGuid = ((transformId != null) ? transformId.Value : SerializableGuid.Empty);
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
				TransformId = serializableGuid
			};
			if (loop || duration > 60f)
			{
				this.lateJoinAudioMap.Add(num, positionalAudioInfo);
			}
			this.PlayAudio_ClientRpc(num, positionalAudioInfo);
			return num;
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x0001522C File Offset: 0x0001342C
		[ClientRpc]
		private void PlayAudio_ClientRpc(int audioId, PositionalAudioInfo positionalAudioInfo)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1772686040U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, audioId);
				fastBufferWriter.WriteValueSafe<PositionalAudioInfo>(in positionalAudioInfo, default(FastBufferWriter.ForNetworkSerializable));
				base.__endSendClientRpc(ref fastBufferWriter, 1772686040U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			Coroutine coroutine = base.StartCoroutine(this.PlayAudio(audioId, positionalAudioInfo, 0f));
			if (coroutine != null)
			{
				this.activeAudioRoutines[audioId] = coroutine;
			}
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x0001534B File Offset: 0x0001354B
		private IEnumerator PlayAudio(int audioId, PositionalAudioInfo audioInfo, float elapsedTime = 0f)
		{
			Transform followTarget = null;
			Vector3 vector;
			if (!audioInfo.InstanceId.IsEmpty || audioInfo.NetworkId != 0UL)
			{
				GameObject gameObject;
				if (audioInfo.NetworkId != 0UL)
				{
					gameObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[audioInfo.NetworkId].gameObject;
				}
				else
				{
					gameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(audioInfo.InstanceId);
				}
				WorldObject component = gameObject.GetComponent<WorldObject>();
				if (audioInfo.TransformId.IsEmpty || !component.EndlessProp.TransformMap.TryGetValue(audioInfo.TransformId, out followTarget))
				{
					followTarget = component.BaseTypeComponent.transform;
				}
				vector = followTarget.position;
			}
			else
			{
				vector = audioInfo.Position;
			}
			PoolableSfxSource poolableAudioSource = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<PoolableSfxSource>(this.sfxSourcePrefab, vector, default(Quaternion), null);
			Task<AudioClip> getFileTask = MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAudioFileAsync(poolableAudioSource, audioInfo.ActiveFileId, audioInfo.AudioType);
			this.activeAudioSources[audioId] = poolableAudioSource;
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
			poolableAudioSource.AudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, SfxManager.Logarithmic(audioInfo.MinDistance / audioInfo.MaxDistance, 1f, 1f));
			if (elapsedTime > 0f && elapsedTime < result.length)
			{
				poolableAudioSource.AudioSource.time = elapsedTime;
			}
			poolableAudioSource.transform.SetParent(base.transform);
			if (followTarget != null)
			{
				poolableAudioSource.SetAudioIdAndFollowTarget(audioId, followTarget);
				poolableAudioSource.OnSelfDisabled.AddListener(new UnityAction<int>(this.HandleSfxSelfDisabled));
			}
			poolableAudioSource.AudioSource.Play();
			if (!audioInfo.Loop)
			{
				yield return new WaitForSeconds(audioInfo.Duration);
				SfxManager.CleanupAudioSource(this.activeAudioSources[audioId]);
				this.activeAudioSources.Remove(audioId);
			}
			this.activeAudioRoutines.Remove(audioId);
			yield break;
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x00015370 File Offset: 0x00013570
		private static AnimationCurve Logarithmic(float timeStart, float timeEnd, float logBase)
		{
			List<Keyframe> list = new List<Keyframe>();
			float num = 2f;
			timeStart = Mathf.Max(timeStart, 0.0001f);
			bool flag = true;
			float num3;
			float num4;
			float num5;
			for (float num2 = timeStart; num2 < timeEnd; num2 *= num)
			{
				if (flag)
				{
					num3 = SfxManager.LogarithmicValue(num2, timeStart, logBase);
					flag = false;
				}
				else
				{
					num3 = SfxManager.LogarithmicValue(num2, timeStart, logBase) - timeStart;
				}
				num4 = num2 / 50f;
				num5 = (SfxManager.LogarithmicValue(num2 + num4, timeStart, logBase) - SfxManager.LogarithmicValue(num2 - num4, timeStart, logBase)) / (num4 * 2f);
				list.Add(new Keyframe(num2, num3, num5, num5));
			}
			num3 = SfxManager.LogarithmicValue(timeEnd, timeStart, logBase);
			num4 = timeEnd / 50f;
			num5 = (SfxManager.LogarithmicValue(timeEnd + num4, timeStart, logBase) - SfxManager.LogarithmicValue(timeEnd - num4, timeStart, logBase)) / (num4 * 2f);
			list.Add(new Keyframe(timeEnd, 0f, num5, num5));
			return new AnimationCurve(list.ToArray());
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x00015450 File Offset: 0x00013650
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

		// Token: 0x060003D8 RID: 984 RVA: 0x0001547F File Offset: 0x0001367F
		public void StopAudio_ServerOnly(int audioId)
		{
			this.lateJoinAudioMap.Remove(audioId);
			this.StopAudio_ClientRpc(audioId);
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x00015498 File Offset: 0x00013698
		[ClientRpc]
		private void StopAudio_ClientRpc(int audioId)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2879044725U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, audioId);
				base.__endSendClientRpc(ref fastBufferWriter, 2879044725U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.IsServer)
			{
				this.lateJoinAudioMap.Remove(audioId);
			}
			Coroutine coroutine;
			if (this.activeAudioRoutines.TryGetValue(audioId, out coroutine))
			{
				base.StopCoroutine(coroutine);
				this.activeAudioRoutines.Remove(audioId);
			}
			PoolableSfxSource poolableSfxSource;
			if (this.activeAudioSources.TryGetValue(audioId, out poolableSfxSource))
			{
				SfxManager.CleanupAudioSource(poolableSfxSource);
				this.activeAudioSources.Remove(audioId);
			}
		}

		// Token: 0x060003DA RID: 986 RVA: 0x000155D4 File Offset: 0x000137D4
		private void HandleSfxSelfDisabled(int audioId)
		{
			if (base.IsServer)
			{
				this.lateJoinAudioMap.Remove(audioId);
				this.StopAudio_ClientRpc(audioId);
			}
		}

		// Token: 0x060003DB RID: 987 RVA: 0x000155F4 File Offset: 0x000137F4
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			int num = 0;
			if (serializer.IsWriter)
			{
				num = this.lateJoinAudioMap.Count;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			if (num > 0)
			{
				int[] array;
				PositionalAudioInfo[] array2;
				if (serializer.IsWriter)
				{
					array = this.lateJoinAudioMap.Keys.ToArray<int>();
					array2 = this.lateJoinAudioMap.Values.ToArray<PositionalAudioInfo>();
				}
				else
				{
					array = new int[num];
					array2 = new PositionalAudioInfo[num];
					for (int i = 0; i < num; i++)
					{
						array2[i] = default(PositionalAudioInfo);
					}
				}
				for (int j = 0; j < num; j++)
				{
					serializer.SerializeValue<int>(ref array[j], default(FastBufferWriter.ForPrimitives));
					serializer.SerializeValue<PositionalAudioInfo>(ref array2[j], default(FastBufferWriter.ForNetworkSerializable));
					if (serializer.IsReader)
					{
						this.lateJoinAudioMap.Add(array[j], array2[j]);
					}
				}
			}
		}

		// Token: 0x060003DD RID: 989 RVA: 0x00015718 File Offset: 0x00013918
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060003DE RID: 990 RVA: 0x00015730 File Offset: 0x00013930
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1772686040U, new NetworkBehaviour.RpcReceiveHandler(SfxManager.__rpc_handler_1772686040), "PlayAudio_ClientRpc");
			base.__registerRpc(2879044725U, new NetworkBehaviour.RpcReceiveHandler(SfxManager.__rpc_handler_2879044725), "StopAudio_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060003DF RID: 991 RVA: 0x00015780 File Offset: 0x00013980
		private static void __rpc_handler_1772686040(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			PositionalAudioInfo positionalAudioInfo;
			reader.ReadValueSafe<PositionalAudioInfo>(out positionalAudioInfo, default(FastBufferWriter.ForNetworkSerializable));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((SfxManager)target).PlayAudio_ClientRpc(num, positionalAudioInfo);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x00015804 File Offset: 0x00013A04
		private static void __rpc_handler_2879044725(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((SfxManager)target).StopAudio_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x00015866 File Offset: 0x00013A66
		protected internal override string __getTypeName()
		{
			return "SfxManager";
		}

		// Token: 0x04000377 RID: 887
		[SerializeField]
		private PoolableSfxSource sfxSourcePrefab;

		// Token: 0x04000378 RID: 888
		private Dictionary<int, Coroutine> activeAudioRoutines = new Dictionary<int, Coroutine>();

		// Token: 0x04000379 RID: 889
		private Dictionary<int, PoolableSfxSource> activeAudioSources = new Dictionary<int, PoolableSfxSource>();

		// Token: 0x0400037A RID: 890
		private int nextAudioId;

		// Token: 0x0400037B RID: 891
		private const int LATE_JOIN_DURATION_CUTOFF = 60;

		// Token: 0x0400037C RID: 892
		private Dictionary<int, PositionalAudioInfo> lateJoinAudioMap = new Dictionary<int, PositionalAudioInfo>();
	}
}
