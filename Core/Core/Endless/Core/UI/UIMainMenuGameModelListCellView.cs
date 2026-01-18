using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.UI;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x0200005A RID: 90
	public class UIMainMenuGameModelListCellView : UIBaseListCellView<MainMenuGameModel>
	{
		// Token: 0x060001A2 RID: 418 RVA: 0x0000A451 File Offset: 0x00008651
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.mainMenuGameView.Clear();
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x0000A464 File Offset: 0x00008664
		public override void View(UIBaseListView<MainMenuGameModel> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			MainMenuGameModel mainMenuGameModel = listView.Model[dataIndex];
			UIMainMenuGameModelPaginatedListModel uimainMenuGameModelPaginatedListModel = listView.Model as UIMainMenuGameModelPaginatedListModel;
			if (uimainMenuGameModelPaginatedListModel != null)
			{
				this.mainMenuGameView.View(mainMenuGameModel, uimainMenuGameModelPaginatedListModel.MainMenuGameContext);
				return;
			}
			this.mainMenuGameView.View(mainMenuGameModel, MainMenuGameContext.Play);
		}

		// Token: 0x0400012E RID: 302
		[Header("UIMainMenuGameModelListCellView")]
		[SerializeField]
		private UIMainMenuGameView mainMenuGameView;
	}
}
