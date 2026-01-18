using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001E8 RID: 488
	public class UIInspectorGroupNameView : UIBaseView<UIInspectorGroupName, UIInspectorGroupNameView.Styles>
	{
		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000795 RID: 1941 RVA: 0x000259DB File Offset: 0x00023BDB
		// (set) Token: 0x06000796 RID: 1942 RVA: 0x000259E3 File Offset: 0x00023BE3
		public override UIInspectorGroupNameView.Styles Style { get; protected set; }

		// Token: 0x06000797 RID: 1943 RVA: 0x000259EC File Offset: 0x00023BEC
		public override void View(UIInspectorGroupName model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model.GroupName });
			}
			this.groupNameText.text = model.GroupName;
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x00025A21 File Offset: 0x00023C21
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}

		// Token: 0x040006CC RID: 1740
		[SerializeField]
		private TextMeshProUGUI groupNameText;

		// Token: 0x020001E9 RID: 489
		public enum Styles
		{
			// Token: 0x040006CE RID: 1742
			Default
		}
	}
}
