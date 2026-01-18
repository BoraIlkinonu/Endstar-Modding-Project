using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core
{
	// Token: 0x02000024 RID: 36
	public class TweenIfGameState : MonoBehaviour, IValidatable
	{
		// Token: 0x0600007C RID: 124 RVA: 0x00004564 File Offset: 0x00002764
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			foreach (TweenIfGameState.TweenIfGameStateEntry tweenIfGameStateEntry in this.entries)
			{
				this.dictionary.Add(tweenIfGameStateEntry.Trigger, tweenIfGameStateEntry.ToTween);
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
			this.OnGameStateChanged(NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState, NetworkBehaviourSingleton<GameStateManager>.Instance.CurrentState);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000045F0 File Offset: 0x000027F0
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			HashSet<GameState> hashSet = new HashSet<GameState>();
			foreach (TweenIfGameState.TweenIfGameStateEntry tweenIfGameStateEntry in this.entries)
			{
				if (!hashSet.Add(tweenIfGameStateEntry.Trigger))
				{
					DebugUtility.LogException(new Exception(string.Format("{0} already has a {1} of {2}. Each {3} must be unique!", new object[] { "TweenIfGameState", "Trigger", tweenIfGameStateEntry.Trigger, "Trigger" })), this);
				}
			}
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00004684 File Offset: 0x00002884
		private void OnGameStateChanged(GameState previousState, GameState currentState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { previousState, currentState });
			}
			if (this.dictionary.ContainsKey(currentState))
			{
				TweenCollection[] array = this.dictionary[currentState];
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Tween();
				}
			}
		}

		// Token: 0x04000057 RID: 87
		[SerializeField]
		private TweenIfGameState.TweenIfGameStateEntry[] entries = Array.Empty<TweenIfGameState.TweenIfGameStateEntry>();

		// Token: 0x04000058 RID: 88
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000059 RID: 89
		private readonly Dictionary<GameState, TweenCollection[]> dictionary = new Dictionary<GameState, TweenCollection[]>();

		// Token: 0x02000025 RID: 37
		[Serializable]
		private class TweenIfGameStateEntry
		{
			// Token: 0x0400005A RID: 90
			public GameState Trigger = GameState.Gameplay;

			// Token: 0x0400005B RID: 91
			public TweenCollection[] ToTween = Array.Empty<TweenCollection>();
		}
	}
}
