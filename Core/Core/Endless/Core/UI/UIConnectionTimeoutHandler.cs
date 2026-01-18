using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000043 RID: 67
	public class UIConnectionTimeoutHandler : UIGameObject
	{
		// Token: 0x06000147 RID: 327 RVA: 0x000088F0 File Offset: 0x00006AF0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.waitForConnectionCoroutine = base.StartCoroutine(this.WaitForConnection());
			NetworkManager.Singleton.OnServerStarted += this.ServerStartedCallback;
			NetworkManager.Singleton.OnClientConnectedCallback += this.ClientConnectedCallback;
			NetworkManager.Singleton.OnClientDisconnectCallback += this.ClientDisconnectedCallback;
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00008969 File Offset: 0x00006B69
		private IEnumerator WaitForConnection()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "WaitForConnection", Array.Empty<object>());
			}
			yield return new WaitForSeconds(this.timeOutModalDisplay);
			this.waitForConnectionCoroutine = null;
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal("", null, "Connection Time Out!", UIModalManagerStackActions.ClearStack, new UIModalGenericViewAction[]
			{
				new UIModalGenericViewAction(this.waitButtonColor, "Wait Some More", delegate
				{
					this.waitForConnectionCoroutine = base.StartCoroutine(this.WaitForConnection());
					MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				}),
				new UIModalGenericViewAction(this.exitButtonColor, "Exit", new Action(Application.Quit))
			});
			yield break;
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00008978 File Offset: 0x00006B78
		private void ServerStartedCallback()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ServerStartedCallback", Array.Empty<object>());
			}
			if (this.waitForConnectionCoroutine != null)
			{
				this.ClearWaitForConnectionCoroutine();
			}
		}

		// Token: 0x0600014A RID: 330 RVA: 0x000089A0 File Offset: 0x00006BA0
		private void ClientConnectedCallback(ulong clientID)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClientConnectedCallback", new object[] { clientID });
			}
			if (this.waitForConnectionCoroutine != null)
			{
				this.ClearWaitForConnectionCoroutine();
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x000089D2 File Offset: 0x00006BD2
		private void ClientDisconnectedCallback(ulong clientID)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClientDisconnectedCallback", new object[] { clientID });
			}
			if (this.waitForConnectionCoroutine == null)
			{
				this.waitForConnectionCoroutine = base.StartCoroutine(this.WaitForConnection());
			}
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00008A10 File Offset: 0x00006C10
		private void ClearWaitForConnectionCoroutine()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearWaitForConnectionCoroutine", Array.Empty<object>());
			}
			base.StopCoroutine(this.waitForConnectionCoroutine);
			this.waitForConnectionCoroutine = null;
		}

		// Token: 0x040000DA RID: 218
		[SerializeField]
		[Min(1f)]
		private float timeOutModalDisplay = 15f;

		// Token: 0x040000DB RID: 219
		[SerializeField]
		private Color waitButtonColor = Color.blue;

		// Token: 0x040000DC RID: 220
		[SerializeField]
		private Color exitButtonColor = Color.red;

		// Token: 0x040000DD RID: 221
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000DE RID: 222
		private Coroutine waitForConnectionCoroutine;
	}
}
