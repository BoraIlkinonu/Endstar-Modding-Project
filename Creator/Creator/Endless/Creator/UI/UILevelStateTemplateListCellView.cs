using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200013E RID: 318
	public class UILevelStateTemplateListCellView : UIBaseListCellView<LevelStateTemplateSourceBase>
	{
		// Token: 0x060004F2 RID: 1266 RVA: 0x0001BC33 File Offset: 0x00019E33
		public override void View(UIBaseListView<LevelStateTemplateSourceBase> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.Setup();
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x0001BC44 File Offset: 0x00019E44
		private async void Setup()
		{
			TextMeshProUGUI textMeshProUGUI = this.nameText;
			string text = await base.Model.GetDisplayName();
			textMeshProUGUI.text = text;
			textMeshProUGUI = null;
			Image image = this.spriteImage;
			image.sprite = await base.Model.GetDisplaySprite();
			image = null;
		}

		// Token: 0x0400048E RID: 1166
		[Header("UILevelStateTemplateListCellView")]
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x0400048F RID: 1167
		[SerializeField]
		private Image spriteImage;
	}
}
