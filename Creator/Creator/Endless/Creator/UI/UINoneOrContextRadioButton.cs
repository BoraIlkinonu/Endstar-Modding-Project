using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000274 RID: 628
	public class UINoneOrContextRadioButton : UIBaseRadioButton<NoneOrContext>
	{
		// Token: 0x06000A66 RID: 2662 RVA: 0x00030A9C File Offset: 0x0002EC9C
		public override void Initialize(UIBaseRadio<NoneOrContext> radio, NoneOrContext value)
		{
			base.Initialize(radio, value);
			this.displayNameText.text = value.ToString();
		}

		// Token: 0x040008A6 RID: 2214
		[Header("UIScriptDataTypeRadioButton")]
		[SerializeField]
		private TextMeshProUGUI displayNameText;
	}
}
