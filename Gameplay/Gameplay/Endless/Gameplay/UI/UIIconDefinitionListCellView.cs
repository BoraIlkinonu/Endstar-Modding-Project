using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003C7 RID: 967
	public class UIIconDefinitionListCellView : UIBaseListCellView<IconDefinition>
	{
		// Token: 0x06001895 RID: 6293 RVA: 0x000722B5 File Offset: 0x000704B5
		public override void View(UIBaseListView<IconDefinition> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.icon.View(base.Model);
		}

		// Token: 0x040013BD RID: 5053
		[Header("UIIconDefinitionListCellView")]
		[SerializeField]
		private UIIconDefinitionView icon;
	}
}
