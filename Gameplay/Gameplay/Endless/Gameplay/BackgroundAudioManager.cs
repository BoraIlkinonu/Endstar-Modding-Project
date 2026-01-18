using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.FileManagement;
using Endless.Gameplay.LevelEditing;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000B2 RID: 178
	public class BackgroundAudioManager : NetworkBehaviourSingleton<BackgroundAudioManager>, IStartSubscriber
	{
		// Token: 0x06000322 RID: 802 RVA: 0x00010DE8 File Offset: 0x0000EFE8
		protected override void Awake()
		{
			base.Awake();
			this.activeFileIds = new int[this.sources.Length];
			for (int i = 0; i < this.activeFileIds.Length; i++)
			{
				this.activeFileIds[i] = -1;
			}
		}

		// Token: 0x06000323 RID: 803 RVA: 0x00010E2A File Offset: 0x0000F02A
		private void Start()
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.ResetAudio));
			this.ResetAudio();
			MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
		}

		// Token: 0x06000324 RID: 804 RVA: 0x00010E58 File Offset: 0x0000F058
		public void EndlessStart()
		{
			if (this.lateJoinInfo.ActiveFileId != -1 && base.IsClient)
			{
				float num = NetworkManager.Singleton.ServerTime.TimeAsFloat - this.lateJoinInfo.TimeStamp;
				if (this.lateJoinInfo.Loop || num < this.lateJoinInfo.Duration)
				{
					num %= this.lateJoinInfo.Duration;
					float num2;
					if (num > 0f)
					{
						if (num > this.lateJoinInfo.FadeTime)
						{
							num2 = 0f;
						}
						else
						{
							num2 = this.lateJoinInfo.FadeTime - num;
						}
					}
					else
					{
						num2 = this.lateJoinInfo.FadeTime;
					}
					this.PlayAudio(this.lateJoinInfo.ActiveFileId, this.lateJoinInfo.AudioType, this.lateJoinInfo.Volume, this.lateJoinInfo.Loop, num2, num);
				}
			}
		}

		// Token: 0x06000325 RID: 805 RVA: 0x00010F37 File Offset: 0x0000F137
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<EndlessLoop>.Instance)
			{
				MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
			}
		}

		// Token: 0x06000326 RID: 806 RVA: 0x00010F58 File Offset: 0x0000F158
		protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
		{
			base.OnSynchronize<T>(ref serializer);
			serializer.SerializeValue<LateJoinAudioInfo>(ref this.lateJoinInfo, default(FastBufferWriter.ForNetworkSerializable));
		}

		// Token: 0x06000327 RID: 807 RVA: 0x00010F84 File Offset: 0x0000F184
		private void ResetAudio()
		{
			this.lateJoinInfo = default(LateJoinAudioInfo);
			if (this.tokenSource != null)
			{
				this.tokenSource.Cancel();
			}
			this.tokenSource = new CancellationTokenSource();
			this.currentState = BackgroundAudioManager.AudioState.None;
			if (this.activeCoroutine != null)
			{
				base.StopCoroutine(this.activeCoroutine);
			}
			AudioSource[] array = this.sources;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop();
			}
			for (int j = 0; j < this.activeFileIds.Length; j++)
			{
				if (this.activeFileIds[j] != -1)
				{
					MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, this.activeFileIds[j]);
					this.activeFileIds[j] = -1;
				}
			}
		}

		// Token: 0x06000328 RID: 808 RVA: 0x00011030 File Offset: 0x0000F230
		public void PlayAudio_ServerOnly(RuntimeAudioInfo audioInfo, float volume, bool loop, float fadeTime)
		{
			BackgroundAudioManager.<PlayAudio_ServerOnly>d__17 <PlayAudio_ServerOnly>d__;
			<PlayAudio_ServerOnly>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<PlayAudio_ServerOnly>d__.<>4__this = this;
			<PlayAudio_ServerOnly>d__.audioInfo = audioInfo;
			<PlayAudio_ServerOnly>d__.volume = volume;
			<PlayAudio_ServerOnly>d__.loop = loop;
			<PlayAudio_ServerOnly>d__.fadeTime = fadeTime;
			<PlayAudio_ServerOnly>d__.<>1__state = -1;
			<PlayAudio_ServerOnly>d__.<>t__builder.Start<BackgroundAudioManager.<PlayAudio_ServerOnly>d__17>(ref <PlayAudio_ServerOnly>d__);
		}

		// Token: 0x06000329 RID: 809 RVA: 0x00011088 File Offset: 0x0000F288
		public void StopAudio_ServerOnly(float fadeOutTime)
		{
			this.targetFileId = -1;
			this.lateJoinInfo = default(LateJoinAudioInfo);
			this.StopAudio_ClientRpc(fadeOutTime);
		}

		// Token: 0x0600032A RID: 810 RVA: 0x000110A4 File Offset: 0x0000F2A4
		[ClientRpc]
		private void StopAudio_ClientRpc(float fadeOutTime)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3836156267U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<float>(in fadeOutTime, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 3836156267U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.activeCoroutine != null)
			{
				base.StopCoroutine(this.activeCoroutine);
			}
			this.currentState = BackgroundAudioManager.AudioState.Transitioning;
			this.activeCoroutine = base.StartCoroutine(this.StopMusic(fadeOutTime));
		}

		// Token: 0x0600032B RID: 811 RVA: 0x000111C0 File Offset: 0x0000F3C0
		private IEnumerator StopMusic(float fadeOutTime)
		{
			if (fadeOutTime > 0f)
			{
				int num = this.currentActiveSource + 1;
				if (num >= this.sources.Length)
				{
					num = 0;
				}
				bool fadeOutInProgress = true;
				bool fadeOutTwoInProgress = true;
				IEnumerator fadeOut = this.FadeSourceMixer(this.currentActiveSource, 0f, fadeOutTime);
				IEnumerator fadeOut2 = this.FadeSourceMixer(num, 0f, fadeOutTime);
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
				fadeOut = null;
				fadeOut2 = null;
			}
			this.ResetAudio();
			yield break;
		}

		// Token: 0x0600032C RID: 812 RVA: 0x000111D8 File Offset: 0x0000F3D8
		[ClientRpc]
		private void PlayAudio_ClientRpc(int audioFileInstanceId, AudioType audioType, float volume, bool loop, float fadeTime)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2452794245U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, audioFileInstanceId);
				fastBufferWriter.WriteValueSafe<AudioType>(in audioType, default(FastBufferWriter.ForEnums));
				fastBufferWriter.WriteValueSafe<float>(in volume, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<bool>(in loop, default(FastBufferWriter.ForPrimitives));
				fastBufferWriter.WriteValueSafe<float>(in fadeTime, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2452794245U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.PlayAudio(audioFileInstanceId, audioType, volume, loop, fadeTime, 0f);
		}

		// Token: 0x0600032D RID: 813 RVA: 0x00011338 File Offset: 0x0000F538
		private void PlayAudio(int audioFileInstanceId, AudioType audioType, float volume, bool loop, float fadeTime, float fastForward = 0f)
		{
			if (this.activeCoroutine != null)
			{
				base.StopCoroutine(this.activeCoroutine);
			}
			this.currentState = BackgroundAudioManager.AudioState.Transitioning;
			this.activeCoroutine = base.StartCoroutine(this.SwitchTracks(fadeTime, volume, loop, audioFileInstanceId, audioType, fastForward));
		}

		// Token: 0x0600032E RID: 814 RVA: 0x0001137B File Offset: 0x0000F57B
		private IEnumerator SwitchTracks(float fadeTime, float volume, bool loopNew, int audioFileInstanceId, AudioType audioType, float fastForward = 0f)
		{
			int previousSourceIndex = this.currentActiveSource;
			int nextSourceIndex = this.currentActiveSource + 1;
			if (nextSourceIndex >= this.sources.Length)
			{
				nextSourceIndex = 0;
			}
			this.activeFileIds[nextSourceIndex] = audioFileInstanceId;
			Task<AudioClip> getFileTask = MonoBehaviourSingleton<LoadedFileManager>.Instance.GetAudioFileAsync(this, audioFileInstanceId, audioType);
			while (!getFileTask.IsCompleted)
			{
				yield return null;
			}
			AudioClip result = getFileTask.Result;
			if (result == null)
			{
				Debug.LogError(string.Format("Failed to load music file {0}", audioFileInstanceId));
				yield break;
			}
			this.sources[nextSourceIndex].Stop();
			if (fadeTime > 0f)
			{
				this.audioMixer.SetFloat(this.mixerVolumeParamNames[nextSourceIndex], AudioUtility.VolumeToDecibel(0f));
			}
			else
			{
				this.audioMixer.SetFloat(this.mixerVolumeParamNames[nextSourceIndex], AudioUtility.VolumeToDecibel(1f));
			}
			this.currentActiveSource = nextSourceIndex;
			this.sources[nextSourceIndex].clip = result;
			this.sources[nextSourceIndex].loop = loopNew;
			this.sources[nextSourceIndex].volume = volume;
			this.sources[nextSourceIndex].Play();
			if (fastForward > 0f)
			{
				this.sources[nextSourceIndex].time = fastForward;
			}
			if (fadeTime > 0f)
			{
				bool fadeInHappening = true;
				bool fadeOutHappening = true;
				IEnumerator fadeOut = this.FadeSourceMixer(previousSourceIndex, 0f, fadeTime);
				IEnumerator fadeIn = this.FadeSourceMixer(nextSourceIndex, 1f, fadeTime);
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
				fadeOut = null;
				fadeIn = null;
			}
			this.sources[previousSourceIndex].Stop();
			if (this.activeFileIds[previousSourceIndex] != -1)
			{
				MonoBehaviourSingleton<LoadedFileManager>.Instance.ReleaseAccess(this, this.activeFileIds[previousSourceIndex]);
				this.activeFileIds[previousSourceIndex] = -1;
			}
			this.currentState = BackgroundAudioManager.AudioState.Active;
			yield break;
		}

		// Token: 0x0600032F RID: 815 RVA: 0x000113B7 File Offset: 0x0000F5B7
		private IEnumerator FadeSourceMixer(int sourceIndex, float targetVolume, float duration)
		{
			string exposedParam = this.mixerVolumeParamNames[sourceIndex];
			float startVolume;
			this.audioMixer.GetFloat(exposedParam, out startVolume);
			startVolume = AudioUtility.DecibelToVolume(startVolume);
			float targetValue = Mathf.Clamp01(targetVolume);
			for (float elapsedTime = 0f; elapsedTime < duration; elapsedTime += Time.deltaTime)
			{
				float num = elapsedTime / duration;
				float num2 = AudioUtility.VolumeToDecibel(Mathf.Lerp(startVolume, targetValue, num));
				this.audioMixer.SetFloat(exposedParam, num2);
				yield return null;
			}
			this.audioMixer.SetFloat(exposedParam, AudioUtility.VolumeToDecibel(targetVolume));
			if (targetValue == 0f)
			{
				this.sources[sourceIndex].Stop();
			}
			yield break;
		}

		// Token: 0x06000331 RID: 817 RVA: 0x000113EC File Offset: 0x0000F5EC
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000332 RID: 818 RVA: 0x00011404 File Offset: 0x0000F604
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3836156267U, new NetworkBehaviour.RpcReceiveHandler(BackgroundAudioManager.__rpc_handler_3836156267), "StopAudio_ClientRpc");
			base.__registerRpc(2452794245U, new NetworkBehaviour.RpcReceiveHandler(BackgroundAudioManager.__rpc_handler_2452794245), "PlayAudio_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000333 RID: 819 RVA: 0x00011454 File Offset: 0x0000F654
		private static void __rpc_handler_3836156267(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((BackgroundAudioManager)target).StopAudio_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000334 RID: 820 RVA: 0x000114C4 File Offset: 0x0000F6C4
		private static void __rpc_handler_2452794245(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			AudioType audioType;
			reader.ReadValueSafe<AudioType>(out audioType, default(FastBufferWriter.ForEnums));
			float num2;
			reader.ReadValueSafe<float>(out num2, default(FastBufferWriter.ForPrimitives));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			float num3;
			reader.ReadValueSafe<float>(out num3, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((BackgroundAudioManager)target).PlayAudio_ClientRpc(num, audioType, num2, flag, num3);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000335 RID: 821 RVA: 0x000115A2 File Offset: 0x0000F7A2
		protected internal override string __getTypeName()
		{
			return "BackgroundAudioManager";
		}

		// Token: 0x040002E6 RID: 742
		[SerializeField]
		private AudioMixer audioMixer;

		// Token: 0x040002E7 RID: 743
		[SerializeField]
		private AudioSource[] sources;

		// Token: 0x040002E8 RID: 744
		[SerializeField]
		private string[] mixerVolumeParamNames;

		// Token: 0x040002E9 RID: 745
		private int[] activeFileIds;

		// Token: 0x040002EA RID: 746
		private BackgroundAudioManager.AudioState currentState;

		// Token: 0x040002EB RID: 747
		private int currentActiveSource;

		// Token: 0x040002EC RID: 748
		private int targetFileId = -1;

		// Token: 0x040002ED RID: 749
		private Coroutine activeCoroutine;

		// Token: 0x040002EE RID: 750
		private CancellationTokenSource tokenSource;

		// Token: 0x040002EF RID: 751
		private LateJoinAudioInfo lateJoinInfo;

		// Token: 0x020000B3 RID: 179
		private enum AudioState
		{
			// Token: 0x040002F1 RID: 753
			None,
			// Token: 0x040002F2 RID: 754
			Transitioning,
			// Token: 0x040002F3 RID: 755
			Active
		}
	}
}
