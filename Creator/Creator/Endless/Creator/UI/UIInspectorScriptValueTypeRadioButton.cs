using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000271 RID: 625
	public class UIInspectorScriptValueTypeRadioButton : UIBaseRadioButton<Type>
	{
		// Token: 0x06000A63 RID: 2659 RVA: 0x00030A54 File Offset: 0x0002EC54
		public override void Initialize(UIBaseRadio<Type> radio, Type value)
		{
			base.Initialize(radio, value);
			DisplayNameAndDescription displayNameAndDescription = this.inspectorScriptValueTypeInfoDictionary[value.Name];
			this.displayNameText.text = displayNameAndDescription.DisplayName;
		}

		// Token: 0x040008A1 RID: 2209
		[Header("UIInspectorScriptValueTypeRadioButton")]
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x040008A2 RID: 2210
		[SerializeField]
		private InspectorScriptValueTypeInfoDictionary inspectorScriptValueTypeInfoDictionary;
	}
}
