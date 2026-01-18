using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.UI;

namespace Endless.Core.UI
{
	// Token: 0x0200005D RID: 93
	public class UIMainMenuGameModelListModel : UIBaseLocalFilterableListModel<MainMenuGameModel>
	{
		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060001B5 RID: 437 RVA: 0x0000A855 File Offset: 0x00008A55
		protected override Comparison<MainMenuGameModel> DefaultSort
		{
			get
			{
				return (MainMenuGameModel x, MainMenuGameModel y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
			}
		}
	}
}
