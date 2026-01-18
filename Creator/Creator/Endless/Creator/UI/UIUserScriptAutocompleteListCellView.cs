using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000174 RID: 372
	public class UIUserScriptAutocompleteListCellView : UIBaseListCellView<UIUserScriptAutocompleteListModelItem>
	{
		// Token: 0x06000588 RID: 1416 RVA: 0x0001D6F4 File Offset: 0x0001B8F4
		public override void View(UIBaseListView<UIUserScriptAutocompleteListModelItem> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.iconImage.sprite = this.tokenGroupTypeSpriteDictionary[base.Model.Type];
			this.autoCompleteText.text = base.Model.Value;
		}

		// Token: 0x040004E7 RID: 1255
		[Header("UIUserScriptAutocompleteListCellView")]
		[SerializeField]
		private Image iconImage;

		// Token: 0x040004E8 RID: 1256
		[SerializeField]
		private UITokenGroupTypeSpriteDictionary tokenGroupTypeSpriteDictionary;

		// Token: 0x040004E9 RID: 1257
		[SerializeField]
		private TextMeshProUGUI autoCompleteText;
	}
}
