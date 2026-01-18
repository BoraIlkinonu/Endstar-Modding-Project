using System;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000201 RID: 513
	public class UIInspectorPropertyView : UIBasePropertyView<UIInspectorPropertyModel, UIInspectorPropertyView.Styles>
	{
		// Token: 0x170000EF RID: 239
		// (get) Token: 0x0600080B RID: 2059 RVA: 0x00027DE2 File Offset: 0x00025FE2
		// (set) Token: 0x0600080C RID: 2060 RVA: 0x00027DEA File Offset: 0x00025FEA
		public override UIInspectorPropertyView.Styles Style { get; protected set; }

		// Token: 0x0600080D RID: 2061 RVA: 0x00027DF4 File Offset: 0x00025FF4
		public override void View(UIInspectorPropertyModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.ViewProperty(model.Name, model.GetModel(), model.MemberChange.DataType, model.Description, model.ClampValues);
		}

		// Token: 0x0600080E RID: 2062 RVA: 0x00027E48 File Offset: 0x00026048
		protected override object ExtractModel(object model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "ExtractModel", (model == null) ? "null" : model.GetType().Name, new object[] { model });
			}
			UIInspectorPropertyModel uiinspectorPropertyModel = model as UIInspectorPropertyModel;
			if (uiinspectorPropertyModel == null)
			{
				return model;
			}
			return uiinspectorPropertyModel.GetModel();
		}

		// Token: 0x0600080F RID: 2063 RVA: 0x00027E9C File Offset: 0x0002609C
		protected override void ViewProperty(string name, object model, int dataTypeId, string description, ClampValue[] clampValues)
		{
			base.ViewProperty(name, model, dataTypeId, description, clampValues);
			bool flag = !description.IsNullOrEmptyOrWhiteSpace();
			this.descriptionTooltip.enabled = flag;
			this.descriptionTooltip.SetTooltip(description);
		}

		// Token: 0x04000722 RID: 1826
		[SerializeField]
		private UITooltip descriptionTooltip;

		// Token: 0x02000202 RID: 514
		public enum Styles
		{
			// Token: 0x04000725 RID: 1829
			Default
		}
	}
}
