using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;

namespace Endless.Core.UI
{
	// Token: 0x0200007B RID: 123
	public class UIGameInspectorScreenModel
	{
		// Token: 0x0600026C RID: 620 RVA: 0x0000D739 File Offset: 0x0000B939
		public UIGameInspectorScreenModel(MainMenuGameModel mainMenuGameModel, MainMenuGameContext mainMenuGameContext)
		{
			this.MainMenuGameModel = mainMenuGameModel;
			this.MainMenuGameContext = mainMenuGameContext;
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x0600026D RID: 621 RVA: 0x0000D74F File Offset: 0x0000B94F
		// (set) Token: 0x0600026E RID: 622 RVA: 0x0000D757 File Offset: 0x0000B957
		public MainMenuGameModel MainMenuGameModel { get; private set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600026F RID: 623 RVA: 0x0000D760 File Offset: 0x0000B960
		public MainMenuGameContext MainMenuGameContext { get; }

		// Token: 0x06000270 RID: 624 RVA: 0x0000D768 File Offset: 0x0000B968
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3} }}", new object[] { "MainMenuGameModel", this.MainMenuGameModel, "MainMenuGameContext", this.MainMenuGameContext });
		}
	}
}
