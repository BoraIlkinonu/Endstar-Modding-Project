using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001B4 RID: 436
	public class UISpriteListCellView : UIBaseListCellView<Sprite>
	{
		// Token: 0x06000B1C RID: 2844 RVA: 0x00030886 File Offset: 0x0002EA86
		public override void View(UIBaseListView<Sprite> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.image.sprite = base.Model;
		}

		// Token: 0x04000723 RID: 1827
		[Header("UISpriteListCellView")]
		[SerializeField]
		private Image image;
	}
}
