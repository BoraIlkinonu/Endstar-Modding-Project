using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000AE RID: 174
	public class AmbientSoundManager : NetworkBehaviourSingleton<AmbientSoundManager>, IStartSubscriber
	{
		// Token: 0x060002FC RID: 764 RVA: 0x0000FE4F File Offset: 0x0000E04F
		private void Start()
		{
			MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPool<PoolableAudioSource>(this.ambientSourcePrefab, 3);
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.HandleGameplayCleanup));
			MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
		}

		// Token: 0x060002FD RID: 765 RVA: 0x0000FE88 File Offset: 0x0000E088
		private void HandleGameplayCleanup()
		{
			if (base.IsServer)
			{
				this.playingAudioMap.Clear();
			}
			if (base.IsClient)
			{
				this.StopAllAudio(0f);
			}
		}

		// Token: 0x060002FE RID: 766 RVA: 0x0000FEB0 File Offset: 0x0000E0B0
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<EndlessLoop>.Instance)
			{
				MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
			}
		}

		// Token: 0x060002FF RID: 767 RVA: 0x0000FED0 File Offset: 0x0000E0D0
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

		// Token: 0x06000300 RID: 768 RVA: 0x0000FF00 File Offset: 0x0000E100
		public int PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, float volume, bool loop, float fadeInTime)
		{
			int num = this.GetNextAudioId();
			LateJoinAudioInfo lateJoinAudioInfo = new LateJoinAudioInfo
			{
				ActiveFileId = audioInfo.AudioAsset.AudioFileInstanceId,
				AudioType = audioInfo.AudioAsset.AudioType,
				Duration = audioInfo.AudioAsset.Duration,
				Loop = loop,
				FadeTime = fadeInTime,
				Volume = volume,
				TimeStamp = NetworkManager.Singleton.ServerTime.TimeAsFloat
			};
			this.playingAudioMap.Add(num, lateJoinAudioInfo);
			this.PlayAudio_ClientRpc(num, audioInfo.AudioAsset.AudioFileInstanceId, audioInfo.AudioAsset.AudioType, volume, loop, fadeInTime);
			return num;
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0000FFB5 File Offset: 0x0000E1B5
		public void StopAudio_ServerOnly(int audioId, float fadeOutTime = 0f)
		{
			if (this.playingAudioMap.Remove(audioId))
			{
				this.StopAudio_ClientRpc(audioId, fadeOutTime);
			}
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0000FFD0 File Offset: 0x0000E1D0
		[ClientRpc]
		private void StopAudio_ClientRpc(int audioId, float fadeTime)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3158898487U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, audioId);
				fastBufferWriter.WriteValueSafe<float>(in fadeTime, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 3158898487U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.StopAudio(audioId, fadeTime);
		}

		// Token: 0x06000303 RID: 771 RVA: 0x000100D4 File Offset: 0x0000E2D4
		private void StopAudio(int audioId, float fadeTime)
		{
			AmbientSoundManager.AudioSourceTrackingInfo audioSourceTrackingInfo;
			if (this.activeAudioSources.TryGetValue(audioId, out audioSourceTrackingInfo))
			{
				if (audioSourceTrackingInfo.FadeCoroutine != null)
				{
					base.StopCoroutine(audioSourceTrackingInfo.FadeCoroutine);
					audioSourceTrackingInfo.FadeCoroutine = null;
				}
				if (fadeTime > 0f)
				{
					audioSourceTrackingInfo.FadeCoroutine = base.StartCoroutine(this.FadeOut(fadeTime, audioId, audioSourceTrackingInfo));
					return;
				}
				this.ResetSource(audioId, audioSourceTrackingInfo);
			}
		}

		// Token: 0x06000304 RID: 772 RVA: 0x00010134 File Offset: 0x0000E334
		private void ResetSource(int audioId, AmbientSoundManager.AudioSourceTrackingInfo audioSourceTrackingInfo)
		{
			audioSourceTrackingInfo.PoolableAudioSource.AudioSource.Stop();
			audioSourceTrackingInfo.PoolableAudioSource.AudioSource.clip = null;
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<PoolableAudioSource>(audioSourceTrackingInfo.PoolableAudioSource);
			MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, this.activeAudioSources[audioId].FileId);
			this.activeAudioSources.Remove(audioId);
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0001019C File Offset: 0x0000E39C
		[ClientRpc]
		private void PlayAudio_ClientRpc(int audioId, int audioFileInstanceId, AudioType audioType, float volume, bool loop, float fadeTime)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3155401033U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, audioId);
				BytePacker.WriteValueBitPacked(fastBufferWriter, audioFileInstanceId);
				fastBufferWriter.WriteValueSafe<AudioType>(in audioType, default(FastBufferWriter.ForEnums));
				fastBufferWriter.WriteValueSafe<float>(in volume, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<bool>(in loop, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<float>(in fadeTime, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 3155401033U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.PlayAudio(audioId, audioFileInstanceId, audioType, volume, loop, fadeTime, 0f);
		}

		// Token: 0x06000306 RID: 774 RVA: 0x0001030C File Offset: 0x0000E50C
		private void PlayAudio(int audioId, int audioFileInstanceId, AudioType audioType, float volume, bool loop, float fadeTime, float fastForward = 0f)
		{
			AmbientSoundManager.AudioSourceTrackingInfo audioSourceTrackingInfo;
			if (this.activeAudioSources.TryGetValue(audioId, out audioSourceTrackingInfo))
			{
				if (audioSourceTrackingInfo.FadeCoroutine != null)
				{
					base.StopCoroutine(audioSourceTrackingInfo.FadeCoroutine);
					audioSourceTrackingInfo.FadeCoroutine = null;
				}
				audioSourceTrackingInfo.FadeCoroutine = base.StartCoroutine(this.PlayAudioSource(audioId, audioSourceTrackingInfo.PoolableAudioSource, fadeTime, volume, loop, audioFileInstanceId, audioType, fastForward));
				return;
			}
			PoolableAudioSource poolableAudioSource = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<PoolableAudioSource>(this.ambientSourcePrefab, default(Vector3), default(Quaternion), null);
			poolableAudioSource.transform.SetParent(base.transform);
			poolableAudioSource.AudioSource.volume = 0f;
			this.activeAudioSources.Add(audioId, new AmbientSoundManager.AudioSourceTrackingInfo(audioFileInstanceId, poolableAudioSource, base.StartCoroutine(this.PlayAudioSource(audioId, poolableAudioSource, fadeTime, volume, loop, audioFileInstanceId, audioType, fastForward))));
		}

		// Token: 0x06000307 RID: 775 RVA: 0x000103D8 File Offset: 0x0000E5D8
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
					float num = elapsedTime / fadeTime;
					float num2 = Mathf.Lerp(start, volume, num);
					poolableAudioSource.AudioSource.volume = num2;
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
					this.StopAudio_ServerOnly(audioId, 0f);
				}
			}
			else
			{
				this.activeAudioSources[audioId].FadeCoroutine = null;
			}
			yield break;
		}

		// Token: 0x06000308 RID: 776 RVA: 0x0001042F File Offset: 0x0000E62F
		private IEnumerator FadeOut(float fadeTime, int audioId, AmbientSoundManager.AudioSourceTrackingInfo audioSourceTrackingInfo)
		{
			float start = audioSourceTrackingInfo.PoolableAudioSource.AudioSource.volume;
			float end = 0f;
			for (float elapsedTime = 0f; elapsedTime < fadeTime; elapsedTime += Time.deltaTime)
			{
				float num = elapsedTime / fadeTime;
				float num2 = Mathf.Lerp(start, end, num);
				audioSourceTrackingInfo.PoolableAudioSource.AudioSource.volume = num2;
				yield return null;
			}
			this.ResetSource(audioId, audioSourceTrackingInfo);
			yield break;
		}

		// Token: 0x06000309 RID: 777 RVA: 0x00010454 File Offset: 0x0000E654
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			int num = 0;
			if (serializer.IsWriter)
			{
				num = this.playingAudioMap.Count;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			if (num > 0)
			{
				int[] array;
				LateJoinAudioInfo[] array2;
				if (serializer.IsWriter)
				{
					array = this.playingAudioMap.Keys.ToArray<int>();
					array2 = this.playingAudioMap.Values.ToArray<LateJoinAudioInfo>();
				}
				else
				{
					array = new int[num];
					array2 = new LateJoinAudioInfo[num];
					for (int i = 0; i < num; i++)
					{
						array2[i] = default(LateJoinAudioInfo);
					}
				}
				for (int j = 0; j < num; j++)
				{
					serializer.SerializeValue<int>(ref array[j], default(FastBufferWriter.ForPrimitives));
					serializer.SerializeValue<LateJoinAudioInfo>(ref array2[j], default(FastBufferWriter.ForNetworkSerializable));
					if (serializer.IsReader)
					{
						this.playingAudioMap.Add(array[j], array2[j]);
					}
				}
			}
		}

		// Token: 0x0600030A RID: 778 RVA: 0x0001054C File Offset: 0x0000E74C
		public void EndlessStart()
		{
			if (base.IsServer)
			{
				return;
			}
			foreach (KeyValuePair<int, LateJoinAudioInfo> keyValuePair in this.playingAudioMap)
			{
				LateJoinAudioInfo value = keyValuePair.Value;
				float num = NetworkManager.Singleton.ServerTime.TimeAsFloat - value.TimeStamp;
				if (value.Loop || num < value.Duration)
				{
					num %= value.Duration;
					float num2;
					if (num > 0f)
					{
						if (num > value.FadeTime)
						{
							num2 = 0f;
						}
						else
						{
							num2 = value.FadeTime - num;
						}
					}
					else
					{
						num2 = value.FadeTime;
					}
					this.PlayAudio(keyValuePair.Key, value.ActiveFileId, value.AudioType, value.Volume, value.Loop, num2, num);
				}
			}
			this.playingAudioMap.Clear();
		}

		// Token: 0x0600030B RID: 779 RVA: 0x00010648 File Offset: 0x0000E848
		public void StopAllAudio_ServerOnly(float fadeOutTime = 0f)
		{
			this.playingAudioMap.Clear();
			this.StopAllAudio_ClientRpc(fadeOutTime);
		}

		// Token: 0x0600030C RID: 780 RVA: 0x0001065C File Offset: 0x0000E85C
		[ClientRpc]
		private void StopAllAudio_ClientRpc(float fadeOutTime)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(674555753U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<float>(in fadeOutTime, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 674555753U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.StopAllAudio(fadeOutTime);
		}

		// Token: 0x0600030D RID: 781 RVA: 0x00010754 File Offset: 0x0000E954
		private void StopAllAudio(float fadeOutTime = 0f)
		{
			foreach (int num in ((IEnumerable<int>)this.activeAudioSources.Keys.ToArray<int>()))
			{
				this.StopAudio(num, fadeOutTime);
			}
		}

		// Token: 0x0600030F RID: 783 RVA: 0x000107CC File Offset: 0x0000E9CC
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000310 RID: 784 RVA: 0x000107E4 File Offset: 0x0000E9E4
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3158898487U, new NetworkBehaviour.RpcReceiveHandler(AmbientSoundManager.__rpc_handler_3158898487), "StopAudio_ClientRpc");
			base.__registerRpc(3155401033U, new NetworkBehaviour.RpcReceiveHandler(AmbientSoundManager.__rpc_handler_3155401033), "PlayAudio_ClientRpc");
			base.__registerRpc(674555753U, new NetworkBehaviour.RpcReceiveHandler(AmbientSoundManager.__rpc_handler_674555753), "StopAllAudio_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000311 RID: 785 RVA: 0x00010850 File Offset: 0x0000EA50
		private static void __rpc_handler_3158898487(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			float num2;
			reader.ReadValueSafe<float>(out num2, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((AmbientSoundManager)target).StopAudio_ClientRpc(num, num2);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000312 RID: 786 RVA: 0x000108D4 File Offset: 0x0000EAD4
		private static void __rpc_handler_3155401033(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			int num2;
			ByteUnpacker.ReadValueBitPacked(reader, out num2);
			AudioType audioType;
			reader.ReadValueSafe<AudioType>(out audioType, default(FastBufferWriter.ForEnums));
			float num3;
			reader.ReadValueSafe<float>(out num3, default(FastBufferWriter.ForPrimitives));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			float num4;
			reader.ReadValueSafe<float>(out num4, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((AmbientSoundManager)target).PlayAudio_ClientRpc(num, num2, audioType, num3, flag, num4);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000313 RID: 787 RVA: 0x000109C4 File Offset: 0x0000EBC4
		private static void __rpc_handler_674555753(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((AmbientSoundManager)target).StopAllAudio_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000314 RID: 788 RVA: 0x00010A34 File Offset: 0x0000EC34
		protected internal override string __getTypeName()
		{
			return "AmbientSoundManager";
		}

		// Token: 0x040002C6 RID: 710
		[SerializeField]
		private PoolableAudioSource ambientSourcePrefab;

		// Token: 0x040002C7 RID: 711
		private int nextAudioId;

		// Token: 0x040002C8 RID: 712
		private readonly Dictionary<int, LateJoinAudioInfo> playingAudioMap = new Dictionary<int, LateJoinAudioInfo>();

		// Token: 0x040002C9 RID: 713
		private readonly Dictionary<int, AmbientSoundManager.AudioSourceTrackingInfo> activeAudioSources = new Dictionary<int, AmbientSoundManager.AudioSourceTrackingInfo>();

		// Token: 0x020000AF RID: 175
		private class AudioSourceTrackingInfo
		{
			// Token: 0x06000315 RID: 789 RVA: 0x00010A3B File Offset: 0x0000EC3B
			public AudioSourceTrackingInfo(int fileId, PoolableAudioSource poolableAudioSource, Coroutine fadeCoroutine = null)
			{
				this.FileId = fileId;
				this.PoolableAudioSource = poolableAudioSource;
				this.FadeCoroutine = fadeCoroutine;
			}

			// Token: 0x040002CA RID: 714
			public Coroutine FadeCoroutine;

			// Token: 0x040002CB RID: 715
			public readonly PoolableAudioSource PoolableAudioSource;

			// Token: 0x040002CC RID: 716
			public readonly int FileId;
		}
	}
}
