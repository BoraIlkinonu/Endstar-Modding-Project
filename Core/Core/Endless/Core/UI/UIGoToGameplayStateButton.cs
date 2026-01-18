using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200003D RID: 61
	[RequireComponent(typeof(UIButton))]
	public class UIGoToGameplayStateButton : UIGameObject
	{
		// Token: 0x06000126 RID: 294 RVA: 0x000080A4 File Offset: 0x000062A4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIButton uibutton;
			base.TryGetComponent<UIButton>(out uibutton);
			uibutton.onClick.AddListener(new UnityAction(this.GoToGameplay));
		}

		// Token: 0x06000127 RID: 295 RVA: 0x000080EC File Offset: 0x000062EC
		private void GoToGameplay()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GoToGameplay", Array.Empty<object>());
			}
			if (NetworkManager.Singleton.IsServer)
			{
				NetworkBehaviourSingleton<GameStateManager>.Instance.FlipGameState();
				return;
			}
			NetworkBehaviourSingleton<CoreMessagingManager>.Instance.RequestGoToGameplay_ServerRpc(default(ServerRpcParams));
		}

		// Token: 0x040000C3 RID: 195
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
