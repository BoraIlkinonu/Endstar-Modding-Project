using System;
using Endless.Gameplay.UI;
using Endless.Shared;
using UnityEngine;

namespace Endless.Core
{
	// Token: 0x0200001A RID: 26
	[CreateAssetMenu(menuName = "ScriptableObject/Editor Tools/Automatic Match Starter Data", fileName = "Automatic Match Starter Data")]
	public class AutomaticMatchStarterData : ScriptableObject
	{
		// Token: 0x0600005E RID: 94 RVA: 0x00003ED0 File Offset: 0x000020D0
		public void TryToStartMatch()
		{
			MonoBehaviourSingleton<UIStartMatchHelper>.Instance.TryToStartMatch(this.GameId, this.GameVersion, this.LevelId, this.MainMenuGameContext);
		}

		// Token: 0x04000047 RID: 71
		[SerializeField]
		public bool Enabled;

		// Token: 0x04000048 RID: 72
		[SerializeField]
		public string GameId;

		// Token: 0x04000049 RID: 73
		[SerializeField]
		public string GameVersion;

		// Token: 0x0400004A RID: 74
		[SerializeField]
		public string LevelId;

		// Token: 0x0400004B RID: 75
		[SerializeField]
		public MainMenuGameContext MainMenuGameContext;
	}
}
