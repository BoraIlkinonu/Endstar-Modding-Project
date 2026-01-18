using System;
using Endless.Core;
using Endless.Shared;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Runtime.Core
{
	// Token: 0x02000008 RID: 8
	public class InputTimeoutDetection : MonoBehaviour, IObserver<InputControl>
	{
		// Token: 0x0600001D RID: 29 RVA: 0x00002696 File Offset: 0x00000896
		private void Start()
		{
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.HandleGameStateChanged));
			InputSystem.onAnyButtonPress.Subscribe(this);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000026BF File Offset: 0x000008BF
		private void HandleGameStateChanged(GameState oldState, GameState newState)
		{
			this.trackInputForKicking = newState == GameState.Creator || newState == GameState.Gameplay;
			this.lastInputTime = Time.time;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000026DD File Offset: 0x000008DD
		private void Update()
		{
			if (this.trackInputForKicking && Time.time - this.lastInputTime >= this.GetInputTimeWindow())
			{
				this.trackInputForKicking = false;
				MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch(null);
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002710 File Offset: 0x00000910
		private float GetInputTimeWindow()
		{
			float num = 60f;
			return 30f * num;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000229D File Offset: 0x0000049D
		public void OnCompleted()
		{
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000229D File Offset: 0x0000049D
		public void OnError(Exception error)
		{
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000272A File Offset: 0x0000092A
		public void OnNext(InputControl value)
		{
			if (!this.trackInputForKicking)
			{
				return;
			}
			this.lastInputTime = Time.time;
		}

		// Token: 0x04000018 RID: 24
		private float lastInputTime = -1f;

		// Token: 0x04000019 RID: 25
		private bool trackInputForKicking;
	}
}
