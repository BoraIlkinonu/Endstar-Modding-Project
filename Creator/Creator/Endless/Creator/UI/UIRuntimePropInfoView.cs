using System;
using Endless.Gameplay.LevelEditing;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200024A RID: 586
	public class UIRuntimePropInfoView : UIBaseView<PropLibrary.RuntimePropInfo, UIRuntimePropInfoView.Styles>
	{
		// Token: 0x17000138 RID: 312
		// (get) Token: 0x0600097E RID: 2430 RVA: 0x0002C592 File Offset: 0x0002A792
		// (set) Token: 0x0600097F RID: 2431 RVA: 0x0002C59A File Offset: 0x0002A79A
		public override UIRuntimePropInfoView.Styles Style { get; protected set; }

		// Token: 0x06000980 RID: 2432 RVA: 0x0002C5A4 File Offset: 0x0002A7A4
		public override void View(PropLibrary.RuntimePropInfo model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.nameText.text = model.PropData.Name;
			this.iconImage.sprite = model.Icon;
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x0002C5F5 File Offset: 0x0002A7F5
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.iconImage.sprite = null;
		}

		// Token: 0x040007DF RID: 2015
		[SerializeField]
		private Image iconImage;

		// Token: 0x040007E0 RID: 2016
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x0200024B RID: 587
		public enum Styles
		{
			// Token: 0x040007E2 RID: 2018
			Default
		}
	}
}
