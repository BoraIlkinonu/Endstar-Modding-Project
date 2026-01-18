using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200003B RID: 59
	[RequireComponent(typeof(UIButton))]
	public class UIGoToCreatorStateButton : UIGameObject
	{
		// Token: 0x0600011B RID: 283 RVA: 0x00007E5C File Offset: 0x0000605C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			NetworkManager.Singleton.OnClientConnectedCallback += this.HandleVisibility;
			UIButton uibutton;
			base.TryGetComponent<UIButton>(out uibutton);
			uibutton.onClick.AddListener(new UnityAction(this.GoToCreator));
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00007EB8 File Offset: 0x000060B8
		private void HandleVisibility(ulong clientId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleVisibility", new object[] { clientId });
				Debug.Log(string.Format("{0}: {1}", "IsHost", NetworkManager.Singleton.IsHost), this);
				Debug.Log(string.Format("{0}: {1}", "IsEditEnabledSession", NetworkBehaviourSingleton<GameStateManager>.Instance.IsEditEnabledSession), this);
			}
			MonoBehaviourSingleton<UICoroutineManager>.Instance.StartThisCoroutine(this.HandleVisibilityCoroutine(clientId));
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00007F41 File Offset: 0x00006141
		private IEnumerator HandleVisibilityCoroutine(ulong clientId)
		{
			yield return new WaitForEndOfFrame();
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleVisibilityCoroutine", new object[] { clientId });
				Debug.Log(string.Format("{0}: {1}", "IsHost", NetworkManager.Singleton.IsHost), this);
				Debug.Log(string.Format("{0}: {1}", "IsEditEnabledSession", NetworkBehaviourSingleton<GameStateManager>.Instance.IsEditEnabledSession), this);
			}
			base.gameObject.SetActive(NetworkManager.Singleton.IsHost && NetworkBehaviourSingleton<GameStateManager>.Instance.IsEditEnabledSession);
			yield break;
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00007F58 File Offset: 0x00006158
		private void GoToCreator()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GoToCreator", Array.Empty<object>());
			}
			if (NetworkManager.Singleton.IsServer)
			{
				NetworkBehaviourSingleton<GameStateManager>.Instance.FlipGameState();
				return;
			}
			NetworkBehaviourSingleton<CoreMessagingManager>.Instance.RequestGoToCreator_ServerRpc(default(ServerRpcParams));
		}

		// Token: 0x040000BE RID: 190
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
