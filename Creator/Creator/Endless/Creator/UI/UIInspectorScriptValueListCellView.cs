using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200011F RID: 287
	public class UIInspectorScriptValueListCellView : UIBaseListCellView<InspectorScriptValue>
	{
		// Token: 0x06000486 RID: 1158 RVA: 0x0001A667 File Offset: 0x00018867
		public override void View(UIBaseListView<InspectorScriptValue> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.nameText.text = base.Model.Name;
			this.removeButton.gameObject.SetActive(base.ListModel.UserCanRemove);
		}

		// Token: 0x0400044D RID: 1101
		[Header("UIInspectorScriptValueListCellView")]
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x0400044E RID: 1102
		[SerializeField]
		private UIButton removeButton;
	}
}
