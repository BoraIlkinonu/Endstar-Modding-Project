using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Core
{
	// Token: 0x0200001B RID: 27
	public class CameraStateManager : MonoBehaviourSingleton<CameraStateManager>
	{
		// Token: 0x06000060 RID: 96 RVA: 0x00003EFC File Offset: 0x000020FC
		private void Start()
		{
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.HandleGameStateChanged));
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.HandleMenuOpened));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.HandleMenuClosed));
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003F64 File Offset: 0x00002164
		private void HandleGameStateChanged(GameState oldState, GameState newState)
		{
			if (newState == GameState.Gameplay)
			{
				this.inGame = true;
			}
			else
			{
				this.inGame = false;
			}
			this.Process();
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00003F80 File Offset: 0x00002180
		private void HandleMenuOpened()
		{
			this.inMenu = true;
			this.Process();
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00003F8F File Offset: 0x0000218F
		private void HandleMenuClosed()
		{
			this.inMenu = false;
			this.Process();
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003F9E File Offset: 0x0000219E
		private void Process()
		{
			if (MonoBehaviourSingleton<CameraController>.Instance)
			{
				if (this.inGame && !this.inMenu)
				{
					MonoBehaviourSingleton<CameraController>.Instance.HandleGameplayStarted();
					return;
				}
				MonoBehaviourSingleton<CameraController>.Instance.HandleGameplayStopped();
			}
		}

		// Token: 0x0400004C RID: 76
		private bool inGame;

		// Token: 0x0400004D RID: 77
		private bool inMenu;
	}
}
