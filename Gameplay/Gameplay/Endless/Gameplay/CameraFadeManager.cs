using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay
{
	// Token: 0x02000078 RID: 120
	public class CameraFadeManager : EndlessNetworkBehaviourSingleton<CameraFadeManager>, IStartSubscriber, IGameEndSubscriber
	{
		// Token: 0x0600021E RID: 542 RVA: 0x0000C49B File Offset: 0x0000A69B
		public void EndlessStart()
		{
			this.playing = true;
		}

		// Token: 0x0600021F RID: 543 RVA: 0x0000C4A4 File Offset: 0x0000A6A4
		public void EndlessGameEnd()
		{
			this.playing = false;
			this.currentFadeColor = Color.clear;
			this.fadeImage.color = this.currentFadeColor;
			base.StopAllCoroutines();
		}

		// Token: 0x06000220 RID: 544 RVA: 0x0000C4D0 File Offset: 0x0000A6D0
		public void SendFadeIn(ulong clientId, float duration)
		{
			if (!base.IsServer)
			{
				return;
			}
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[] { clientId }
				}
			};
			this.FadeIn_ClientRpc(duration, clientRpcParams);
		}

		// Token: 0x06000221 RID: 545 RVA: 0x0000C51C File Offset: 0x0000A71C
		public void SendFadeOut(ulong clientId, float duration)
		{
			if (!base.IsServer)
			{
				return;
			}
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[] { clientId }
				}
			};
			this.FadeOut_ClientRpc(duration, clientRpcParams);
		}

		// Token: 0x06000222 RID: 546 RVA: 0x0000C568 File Offset: 0x0000A768
		public void SendFadeInGlobal(float duration)
		{
			if (!base.IsServer)
			{
				return;
			}
			this.FadeIn_ClientRpc(duration, default(ClientRpcParams));
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0000C590 File Offset: 0x0000A790
		public void SendFadeOutGlobal(float duration)
		{
			if (!base.IsServer)
			{
				return;
			}
			this.FadeOut_ClientRpc(duration, default(ClientRpcParams));
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0000C5B8 File Offset: 0x0000A7B8
		[ClientRpc]
		private void FadeIn_ClientRpc(float duration, ClientRpcParams clientRpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2197127512U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<float>(in duration, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2197127512U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.FadeIn(duration, null);
		}

		// Token: 0x06000225 RID: 549 RVA: 0x0000C6B0 File Offset: 0x0000A8B0
		[ClientRpc]
		private void FadeOut_ClientRpc(float duration, ClientRpcParams clientRpcParams = default(ClientRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(2314910796U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<float>(in duration, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 2314910796U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.FadeOut(duration, null);
		}

		// Token: 0x06000226 RID: 550 RVA: 0x0000C7A8 File Offset: 0x0000A9A8
		public void FadeOutIn(float duration, Action cutCallback = null, Action finishedCallback = null)
		{
			CameraFadeManager.<>c__DisplayClass13_0 CS$<>8__locals1 = new CameraFadeManager.<>c__DisplayClass13_0();
			CS$<>8__locals1.cutCallback = cutCallback;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.finishedCallback = finishedCallback;
			if (!this.playing)
			{
				return;
			}
			CS$<>8__locals1.blackOutDuration = duration * 0.1f;
			CS$<>8__locals1.fadeLength = (duration - CS$<>8__locals1.blackOutDuration) / 2f;
			int num;
			if (!this.currentlyFadedOut)
			{
				CameraFadeManager.<>c__DisplayClass13_1 CS$<>8__locals2 = new CameraFadeManager.<>c__DisplayClass13_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CameraFadeManager.<>c__DisplayClass13_1 CS$<>8__locals3 = CS$<>8__locals2;
				num = this.activeFadeIndex + 1;
				this.activeFadeIndex = num;
				CS$<>8__locals3.currentFadeIndex = num;
				base.StartCoroutine(this.FadeOutRoutine(CS$<>8__locals2.CS$<>8__locals1.fadeLength, CS$<>8__locals2.currentFadeIndex, delegate
				{
					Action cutCallback3 = CS$<>8__locals2.CS$<>8__locals1.cutCallback;
					if (cutCallback3 != null)
					{
						cutCallback3();
					}
					MonoBehaviour <>4__this = CS$<>8__locals2.CS$<>8__locals1.<>4__this;
					CameraFadeManager <>4__this2 = CS$<>8__locals2.CS$<>8__locals1.<>4__this;
					float blackOutDuration = CS$<>8__locals2.CS$<>8__locals1.blackOutDuration;
					int currentFadeIndex = CS$<>8__locals2.currentFadeIndex;
					Action action;
					if ((action = CS$<>8__locals2.<>9__1) == null)
					{
						action = (CS$<>8__locals2.<>9__1 = delegate
						{
							CS$<>8__locals2.CS$<>8__locals1.<>4__this.StartCoroutine(CS$<>8__locals2.CS$<>8__locals1.<>4__this.FadeInRoutine(CS$<>8__locals2.CS$<>8__locals1.fadeLength, CS$<>8__locals2.currentFadeIndex, CS$<>8__locals2.CS$<>8__locals1.finishedCallback));
						});
					}
					<>4__this.StartCoroutine(<>4__this2.BlackOutRoutine(blackOutDuration, currentFadeIndex, action));
				}));
				return;
			}
			CameraFadeManager.<>c__DisplayClass13_2 CS$<>8__locals4 = new CameraFadeManager.<>c__DisplayClass13_2();
			CS$<>8__locals4.CS$<>8__locals2 = CS$<>8__locals1;
			CameraFadeManager.<>c__DisplayClass13_2 CS$<>8__locals5 = CS$<>8__locals4;
			num = this.activeFadeIndex + 1;
			this.activeFadeIndex = num;
			CS$<>8__locals5.currentFadeIndex = num;
			Action cutCallback2 = CS$<>8__locals4.CS$<>8__locals2.cutCallback;
			if (cutCallback2 != null)
			{
				cutCallback2();
			}
			base.StartCoroutine(this.BlackOutRoutine(CS$<>8__locals4.CS$<>8__locals2.fadeLength + CS$<>8__locals4.CS$<>8__locals2.blackOutDuration, CS$<>8__locals4.currentFadeIndex, delegate
			{
				CS$<>8__locals4.CS$<>8__locals2.<>4__this.StartCoroutine(CS$<>8__locals4.CS$<>8__locals2.<>4__this.FadeInRoutine(CS$<>8__locals4.CS$<>8__locals2.fadeLength, CS$<>8__locals4.currentFadeIndex, CS$<>8__locals4.CS$<>8__locals2.finishedCallback));
			}));
		}

		// Token: 0x06000227 RID: 551 RVA: 0x0000C8C4 File Offset: 0x0000AAC4
		public void FadeIn(float duration, Action callback = null)
		{
			if (!this.playing)
			{
				return;
			}
			int num = this.activeFadeIndex + 1;
			this.activeFadeIndex = num;
			base.StartCoroutine(this.FadeInRoutine(duration, num, callback));
		}

		// Token: 0x06000228 RID: 552 RVA: 0x0000C8FC File Offset: 0x0000AAFC
		public void FadeOut(float duration, Action callback = null)
		{
			if (!this.playing)
			{
				return;
			}
			int num = this.activeFadeIndex + 1;
			this.activeFadeIndex = num;
			base.StartCoroutine(this.FadeOutRoutine(duration, num, callback));
		}

		// Token: 0x06000229 RID: 553 RVA: 0x0000C932 File Offset: 0x0000AB32
		private IEnumerator BlackOutRoutine(float duration, int fadeIndex, Action callback = null)
		{
			if (this.activeFadeIndex == fadeIndex)
			{
				this.currentlyFadedOut = true;
				this.currentFadeColor = Color.black;
				this.fadeImage.color = this.currentFadeColor;
			}
			yield return new WaitForSeconds(duration);
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		// Token: 0x0600022A RID: 554 RVA: 0x0000C956 File Offset: 0x0000AB56
		private IEnumerator FadeInRoutine(float duration, int fadeIndex, Action callback = null)
		{
			if (fadeIndex == this.activeFadeIndex)
			{
				this.currentlyFadedOut = false;
				this.currentFadeColor = Color.black;
				this.fadeImage.color = this.currentFadeColor;
			}
			float elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				elapsedTime += Time.deltaTime;
				if (fadeIndex == this.activeFadeIndex)
				{
					this.currentFadeColor.a = this.currentFadeColor.a - Time.deltaTime / duration;
					this.fadeImage.color = this.currentFadeColor;
				}
				yield return null;
			}
			if (fadeIndex == this.activeFadeIndex)
			{
				this.currentFadeColor = Color.clear;
				this.fadeImage.color = this.currentFadeColor;
			}
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0000C97A File Offset: 0x0000AB7A
		private IEnumerator FadeOutRoutine(float duration, int fadeIndex, Action callback = null)
		{
			if (fadeIndex == this.activeFadeIndex)
			{
				this.currentlyFadedOut = true;
				this.currentFadeColor = Color.clear;
				this.fadeImage.color = this.currentFadeColor;
			}
			float elapsedTime = 0f;
			while (elapsedTime < duration)
			{
				elapsedTime += Time.deltaTime;
				if (fadeIndex == this.activeFadeIndex)
				{
					this.currentFadeColor.a = this.currentFadeColor.a + Time.deltaTime / duration;
					this.fadeImage.color = this.currentFadeColor;
				}
				yield return null;
			}
			if (fadeIndex == this.activeFadeIndex)
			{
				this.currentFadeColor = Color.black;
				this.fadeImage.color = this.currentFadeColor;
			}
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0000C9B4 File Offset: 0x0000ABB4
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x0600022E RID: 558 RVA: 0x0000C9CC File Offset: 0x0000ABCC
		protected override void __initializeRpcs()
		{
			base.__registerRpc(2197127512U, new NetworkBehaviour.RpcReceiveHandler(CameraFadeManager.__rpc_handler_2197127512), "FadeIn_ClientRpc");
			base.__registerRpc(2314910796U, new NetworkBehaviour.RpcReceiveHandler(CameraFadeManager.__rpc_handler_2314910796), "FadeOut_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x0600022F RID: 559 RVA: 0x0000CA1C File Offset: 0x0000AC1C
		private static void __rpc_handler_2197127512(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CameraFadeManager)target).FadeIn_ClientRpc(num, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000230 RID: 560 RVA: 0x0000CA9C File Offset: 0x0000AC9C
		private static void __rpc_handler_2314910796(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			float num;
			reader.ReadValueSafe<float>(out num, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CameraFadeManager)target).FadeOut_ClientRpc(num, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000231 RID: 561 RVA: 0x0000CB1A File Offset: 0x0000AD1A
		protected internal override string __getTypeName()
		{
			return "CameraFadeManager";
		}

		// Token: 0x04000219 RID: 537
		[SerializeField]
		private Image fadeImage;

		// Token: 0x0400021A RID: 538
		private Color currentFadeColor = Color.clear;

		// Token: 0x0400021B RID: 539
		private bool playing;

		// Token: 0x0400021C RID: 540
		private bool currentlyFadedOut;

		// Token: 0x0400021D RID: 541
		private int activeFadeIndex;
	}
}
