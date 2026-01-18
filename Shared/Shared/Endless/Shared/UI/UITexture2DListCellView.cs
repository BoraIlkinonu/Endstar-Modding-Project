using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001BD RID: 445
	public class UITexture2DListCellView : UIBaseListCellView<Texture2D>
	{
		// Token: 0x06000B3A RID: 2874 RVA: 0x00030B7F File Offset: 0x0002ED7F
		public override void View(UIBaseListView<Texture2D> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.rawImage.texture = base.Model;
		}

		// Token: 0x0400072F RID: 1839
		[Header("UITexture2DListCellView")]
		[SerializeField]
		private RawImage rawImage;
	}
}
