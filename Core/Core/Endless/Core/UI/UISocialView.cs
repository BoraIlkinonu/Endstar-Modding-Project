using System;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200009D RID: 157
	public class UISocialView : UIGameObject, IValidatable
	{
		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600034A RID: 842 RVA: 0x00011823 File Offset: 0x0000FA23
		// (set) Token: 0x0600034B RID: 843 RVA: 0x0001182B File Offset: 0x0000FA2B
		public bool IsOpen { get; private set; }

		// Token: 0x0600034C RID: 844 RVA: 0x00011834 File Offset: 0x0000FA34
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayButton.gameObject.SetActive(false);
			this.userGroupView.Initialize(this);
			this.displayButtonHideTweens.SetToEnd();
			this.containerHideTweens.SetToEnd();
			this.displayButtonHideTweens.OnAllTweenCompleted.AddListener(new UnityAction(this.DeactivatedDisplayButton));
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationProcessSuccessful));
			MatchmakingClientController.OnDisconnectedFromServer += this.OnDisconnectedFromServer;
		}

		// Token: 0x0600034D RID: 845 RVA: 0x000118F4 File Offset: 0x0000FAF4
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.displayButtonHideTweens.OnAllTweenCompleted.RemoveListener(new UnityAction(this.DeactivatedDisplayButton));
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.RemoveListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationProcessSuccessful));
			MatchmakingClientController.OnDisconnectedFromServer -= this.OnDisconnectedFromServer;
		}

		// Token: 0x0600034E RID: 846 RVA: 0x00011981 File Offset: 0x0000FB81
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (this.displayButtonHideTweens)
			{
				this.displayButtonHideTweens.ValidateForNumberOfTweens(1);
			}
		}

		// Token: 0x0600034F RID: 847 RVA: 0x000119B4 File Offset: 0x0000FBB4
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			this.containerDisplayTweens.Tween();
			this.userGroupView.View();
			this.IsOpen = true;
		}

		// Token: 0x06000350 RID: 848 RVA: 0x000119EB File Offset: 0x0000FBEB
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.containerHideTweens.Tween();
			this.IsOpen = false;
		}

		// Token: 0x06000351 RID: 849 RVA: 0x00011A17 File Offset: 0x0000FC17
		private void DeactivatedDisplayButton()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DeactivatedDisplayButton", Array.Empty<object>());
			}
			this.displayButton.gameObject.SetActive(false);
		}

		// Token: 0x06000352 RID: 850 RVA: 0x00011A42 File Offset: 0x0000FC42
		private void OnGameStateChanged(GameState previousGameState, GameState newGameState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { previousGameState, newGameState });
			}
			if (newGameState != GameState.Default && this.IsOpen)
			{
				this.Hide();
			}
		}

		// Token: 0x06000353 RID: 851 RVA: 0x00011A80 File Offset: 0x0000FC80
		private void OnAuthenticationProcessSuccessful(ClientData clientData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAuthenticationProcessSuccessful", new object[] { clientData.ToPrettyString() });
			}
			this.displayButton.gameObject.SetActive(true);
			this.displayButtonDisplayTweens.Tween();
		}

		// Token: 0x06000354 RID: 852 RVA: 0x00011AC0 File Offset: 0x0000FCC0
		private void OnDisconnectedFromServer(string error)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisconnectedFromServer", new object[] { error });
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			this.displayButtonHideTweens.Tween();
			this.Hide();
		}

		// Token: 0x0400026B RID: 619
		[SerializeField]
		private UIButton displayButton;

		// Token: 0x0400026C RID: 620
		[SerializeField]
		private UIUserGroupView userGroupView;

		// Token: 0x0400026D RID: 621
		[Header("Tweens")]
		[SerializeField]
		private TweenCollection displayButtonDisplayTweens;

		// Token: 0x0400026E RID: 622
		[SerializeField]
		private TweenCollection displayButtonHideTweens;

		// Token: 0x0400026F RID: 623
		[SerializeField]
		private TweenCollection containerDisplayTweens;

		// Token: 0x04000270 RID: 624
		[SerializeField]
		private TweenCollection containerHideTweens;

		// Token: 0x04000271 RID: 625
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
