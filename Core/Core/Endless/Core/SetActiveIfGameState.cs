using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core
{
	// Token: 0x0200001C RID: 28
	public class SetActiveIfGameState : BaseSetActiveIf
	{
		// Token: 0x06000066 RID: 102 RVA: 0x00003FDC File Offset: 0x000021DC
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			foreach (SetActiveIfGameState.SetActiveIfGameStateDictionaryEntry setActiveIfGameStateDictionaryEntry in this.entries)
			{
				this.dictionary.Add(setActiveIfGameStateDictionaryEntry.Key, setActiveIfGameStateDictionaryEntry.Value);
			}
			NetworkBehaviourSingleton<GameStateManager>.Instance.OnGameStateChanged.AddListener(new UnityAction<GameState, GameState>(this.OnGameStateChanged));
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00004050 File Offset: 0x00002250
		public override void Validate()
		{
			base.Validate();
			HashSet<GameState> hashSet = new HashSet<GameState>();
			foreach (SetActiveIfGameState.SetActiveIfGameStateDictionaryEntry setActiveIfGameStateDictionaryEntry in this.entries)
			{
				if (!hashSet.Add(setActiveIfGameStateDictionaryEntry.Key))
				{
					DebugUtility.LogException(new Exception(string.Format("{0} already has a {1} of {2}. Each {3} must be unique!", new object[] { "SetActiveIfGameState", "Key", setActiveIfGameStateDictionaryEntry.Key, "Key" })), this);
				}
			}
		}

		// Token: 0x06000068 RID: 104 RVA: 0x000040D8 File Offset: 0x000022D8
		private void OnGameStateChanged(GameState previousState, GameState currentState)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGameStateChanged", new object[] { previousState, currentState });
			}
			bool flag;
			if (this.dictionary.TryGetValue(currentState, out flag))
			{
				base.SetActive(flag);
			}
		}

		// Token: 0x0400004E RID: 78
		[SerializeField]
		private SetActiveIfGameState.SetActiveIfGameStateDictionaryEntry[] entries = Array.Empty<SetActiveIfGameState.SetActiveIfGameStateDictionaryEntry>();

		// Token: 0x0400004F RID: 79
		private readonly Dictionary<GameState, bool> dictionary = new Dictionary<GameState, bool>();

		// Token: 0x0200001D RID: 29
		[Serializable]
		private struct SetActiveIfGameStateDictionaryEntry
		{
			// Token: 0x04000050 RID: 80
			public GameState Key;

			// Token: 0x04000051 RID: 81
			public bool Value;
		}
	}
}
