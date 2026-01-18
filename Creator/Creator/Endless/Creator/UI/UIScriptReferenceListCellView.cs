using System;
using Endless.Props.Assets;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200017A RID: 378
	public class UIScriptReferenceListCellView : UIBaseListCellView<ScriptReference>
	{
		// Token: 0x06000595 RID: 1429 RVA: 0x0001D80F File Offset: 0x0001BA0F
		public override void View(UIBaseListView<ScriptReference> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.displayNameText.text = base.Model.NameInCode;
			this.removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		}

		// Token: 0x040004EF RID: 1263
		[Header("UIScriptReferenceListCellView")]
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x040004F0 RID: 1264
		[SerializeField]
		private UIButton removeButton;
	}
}
