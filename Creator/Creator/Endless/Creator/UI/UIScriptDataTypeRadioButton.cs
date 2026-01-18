using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000276 RID: 630
	public class UIScriptDataTypeRadioButton : UIBaseRadioButton<ScriptDataTypes>
	{
		// Token: 0x06000A69 RID: 2665 RVA: 0x00030AD0 File Offset: 0x0002ECD0
		public override void Initialize(UIBaseRadio<ScriptDataTypes> radio, ScriptDataTypes value)
		{
			base.Initialize(radio, value);
			DisplayNameAndDescription displayNameAndDescription = this.scriptDataTypeInfoDictionary[value];
			this.displayNameText.text = displayNameAndDescription.DisplayName;
		}

		// Token: 0x040008A7 RID: 2215
		[Header("UIScriptDataTypeRadioButton")]
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x040008A8 RID: 2216
		[SerializeField]
		private ScriptDataTypeInfoDictionary scriptDataTypeInfoDictionary;
	}
}
