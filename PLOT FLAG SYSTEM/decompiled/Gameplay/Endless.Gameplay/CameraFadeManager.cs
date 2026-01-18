using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay;

public class CameraFadeManager : EndlessNetworkBehaviourSingleton<CameraFadeManager>, IStartSubscriber, IGameEndSubscriber
{
	[SerializeField]
	private Image fadeImage;

	private Color currentFadeColor = Color.clear;

	private bool playing;

	private bool currentlyFadedOut;

	private int activeFadeIndex;

	public void EndlessStart()
	{
		playing = true;
	}

	public void EndlessGameEnd()
	{
		playing = false;
		currentFadeColor = Color.clear;
		fadeImage.color = currentFadeColor;
		StopAllCoroutines();
	}

	public void SendFadeIn(ulong clientId, float duration)
	{
		if (base.IsServer)
		{
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[1] { clientId }
				}
			};
			FadeIn_ClientRpc(duration, clientRpcParams);
		}
	}

	public void SendFadeOut(ulong clientId, float duration)
	{
		if (base.IsServer)
		{
			ClientRpcParams clientRpcParams = new ClientRpcParams
			{
				Send = new ClientRpcSendParams
				{
					TargetClientIds = new ulong[1] { clientId }
				}
			};
			FadeOut_ClientRpc(duration, clientRpcParams);
		}
	}

	public void SendFadeInGlobal(float duration)
	{
		if (base.IsServer)
		{
			FadeIn_ClientRpc(duration);
		}
	}

	public void SendFadeOutGlobal(float duration)
	{
		if (base.IsServer)
		{
			FadeOut_ClientRpc(duration);
		}
	}

	[ClientRpc]
	private void FadeIn_ClientRpc(float duration, ClientRpcParams clientRpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendClientRpc(2197127512u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in duration, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 2197127512u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				FadeIn(duration);
			}
		}
	}

	[ClientRpc]
	private void FadeOut_ClientRpc(float duration, ClientRpcParams clientRpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter bufferWriter = __beginSendClientRpc(2314910796u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in duration, default(FastBufferWriter.ForPrimitives));
				__endSendClientRpc(ref bufferWriter, 2314910796u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				FadeOut(duration);
			}
		}
	}

	public void FadeOutIn(float duration, Action cutCallback = null, Action finishedCallback = null)
	{
		if (!playing)
		{
			return;
		}
		float blackOutDuration = duration * 0.1f;
		float fadeLength = (duration - blackOutDuration) / 2f;
		if (!currentlyFadedOut)
		{
			int currentFadeIndex = ++activeFadeIndex;
			StartCoroutine(FadeOutRoutine(fadeLength, currentFadeIndex, delegate
			{
				cutCallback?.Invoke();
				StartCoroutine(BlackOutRoutine(blackOutDuration, currentFadeIndex, delegate
				{
					StartCoroutine(FadeInRoutine(fadeLength, currentFadeIndex, finishedCallback));
				}));
			}));
		}
		else
		{
			int currentFadeIndex2 = ++activeFadeIndex;
			cutCallback?.Invoke();
			StartCoroutine(BlackOutRoutine(fadeLength + blackOutDuration, currentFadeIndex2, delegate
			{
				StartCoroutine(FadeInRoutine(fadeLength, currentFadeIndex2, finishedCallback));
			}));
		}
	}

	public void FadeIn(float duration, Action callback = null)
	{
		if (playing)
		{
			StartCoroutine(FadeInRoutine(duration, ++activeFadeIndex, callback));
		}
	}

	public void FadeOut(float duration, Action callback = null)
	{
		if (playing)
		{
			StartCoroutine(FadeOutRoutine(duration, ++activeFadeIndex, callback));
		}
	}

	private IEnumerator BlackOutRoutine(float duration, int fadeIndex, Action callback = null)
	{
		if (activeFadeIndex == fadeIndex)
		{
			currentlyFadedOut = true;
			currentFadeColor = Color.black;
			fadeImage.color = currentFadeColor;
		}
		yield return new WaitForSeconds(duration);
		callback?.Invoke();
	}

	private IEnumerator FadeInRoutine(float duration, int fadeIndex, Action callback = null)
	{
		if (fadeIndex == activeFadeIndex)
		{
			currentlyFadedOut = false;
			currentFadeColor = Color.black;
			fadeImage.color = currentFadeColor;
		}
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			if (fadeIndex == activeFadeIndex)
			{
				currentFadeColor.a -= Time.deltaTime / duration;
				fadeImage.color = currentFadeColor;
			}
			yield return null;
		}
		if (fadeIndex == activeFadeIndex)
		{
			currentFadeColor = Color.clear;
			fadeImage.color = currentFadeColor;
		}
		callback?.Invoke();
	}

	private IEnumerator FadeOutRoutine(float duration, int fadeIndex, Action callback = null)
	{
		if (fadeIndex == activeFadeIndex)
		{
			currentlyFadedOut = true;
			currentFadeColor = Color.clear;
			fadeImage.color = currentFadeColor;
		}
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			if (fadeIndex == activeFadeIndex)
			{
				currentFadeColor.a += Time.deltaTime / duration;
				fadeImage.color = currentFadeColor;
			}
			yield return null;
		}
		if (fadeIndex == activeFadeIndex)
		{
			currentFadeColor = Color.black;
			fadeImage.color = currentFadeColor;
		}
		callback?.Invoke();
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2197127512u, __rpc_handler_2197127512, "FadeIn_ClientRpc");
		__registerRpc(2314910796u, __rpc_handler_2314910796, "FadeOut_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2197127512(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out float value, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CameraFadeManager)target).FadeIn_ClientRpc(value, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2314910796(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out float value, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CameraFadeManager)target).FadeOut_ClientRpc(value, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "CameraFadeManager";
	}
}
