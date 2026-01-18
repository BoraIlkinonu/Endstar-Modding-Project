using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core
{
	// Token: 0x02000022 RID: 34
	public class TriggerIfGameState : MonoBehaviour, IValidatable
	{
		// Token: 0x06000077 RID: 119 RVA: 0x00004398 File Offset: 0x00002598
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			foreach (TriggerIfGameState.TriggerIfGameStateEntry triggerIfGameStateEntry in this.entries)
			{
				this.dictionary.Add(triggerIfGameStateEntry.Trigger, triggerIfGameStateEntry.ToTrigger);
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00004408 File Offset: 0x00002608
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			HashSet<GameState> hashSet = new HashSet<GameState>();
			foreach (TriggerIfGameState.TriggerIfGameStateEntry triggerIfGameStateEntry in this.entries)
			{
				if (!hashSet.Add(triggerIfGameStateEntry.Trigger))
				{
					DebugUtility.LogException(new Exception(string.Format("{0} already has a {1} of {2}. Each {3} must be unique!", new object[] { "TriggerIfGameState", "Trigger", triggerIfGameStateEntry.Trigger, "Trigger" })), this);
				}
			}
		}

		// Token: 0x06000079 RID: 121 RVA: 0x0000449C File Offset: 0x0000269C
		private void OnGameStateChanged(GameState previousState, GameState currentState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { previousState, currentState });
			}
			DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { previousState, currentState });
			if (!this.dictionary.ContainsKey(currentState))
			{
				return;
			}
			UnityEvent[] array = this.dictionary[currentState];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Invoke();
			}
		}

		// Token: 0x04000052 RID: 82
		[SerializeField]
		private TriggerIfGameState.TriggerIfGameStateEntry[] entries = Array.Empty<TriggerIfGameState.TriggerIfGameStateEntry>();

		// Token: 0x04000053 RID: 83
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000054 RID: 84
		private readonly Dictionary<GameState, UnityEvent[]> dictionary = new Dictionary<GameState, UnityEvent[]>();

		// Token: 0x02000023 RID: 35
		[Serializable]
		private class TriggerIfGameStateEntry
		{
			// Token: 0x04000055 RID: 85
			public GameState Trigger = GameState.Gameplay;

			// Token: 0x04000056 RID: 86
			public UnityEvent[] ToTrigger = Array.Empty<UnityEvent>();
		}
	}
}
