using System;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000F6 RID: 246
	public class UIEndlessParameterInfoListCellView : UIBaseListCellView<EndlessParameterInfo>
	{
		// Token: 0x06000403 RID: 1027 RVA: 0x000191F0 File Offset: 0x000173F0
		public override void View(UIBaseListView<EndlessParameterInfo> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.displayNameText.text = base.Model.DisplayName;
			Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(base.Model.DataType);
			string text = typeFromId.Name;
			if (typeFromId == typeof(float))
			{
				text = "Float";
			}
			else if (typeFromId == typeof(float[]))
			{
				text = "Float[]";
			}
			else if (typeFromId == typeof(int))
			{
				text = "Int";
			}
			else if (typeFromId == typeof(int[]))
			{
				text = "Int[]";
			}
			else if (typeFromId == typeof(bool))
			{
				text = "Bool";
			}
			else if (typeFromId == typeof(bool[]))
			{
				text = "Bool[]";
			}
			this.dataTypeText.text = text;
		}

		// Token: 0x04000415 RID: 1045
		[Header("UIEndlessParameterInfoListCellView")]
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x04000416 RID: 1046
		[SerializeField]
		private TextMeshProUGUI dataTypeText;
	}
}
