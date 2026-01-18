using System;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared.Debugging;
using Newtonsoft.Json;

namespace Endless.Creator.UI
{
	// Token: 0x02000205 RID: 517
	public class UIStoredParameterPackageView : UIBasePropertyView<UIStoredParameterPackage, UIStoredParameterPackageView.Styles>
	{
		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000817 RID: 2071 RVA: 0x00027F22 File Offset: 0x00026122
		// (set) Token: 0x06000818 RID: 2072 RVA: 0x00027F2A File Offset: 0x0002612A
		public override UIStoredParameterPackageView.Styles Style { get; protected set; }

		// Token: 0x06000819 RID: 2073 RVA: 0x00027F34 File Offset: 0x00026134
		public override void View(UIStoredParameterPackage model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(model.StoredParameter.DataType);
			object obj = JsonConvert.DeserializeObject(model.StoredParameter.JsonData, typeFromId);
			this.ViewProperty(model.Name, obj, model.StoredParameter.DataType, "", Array.Empty<ClampValue>());
		}

		// Token: 0x0600081A RID: 2074 RVA: 0x00027FA8 File Offset: 0x000261A8
		protected override object ExtractModel(object model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "ExtractModel", (model == null) ? "null" : model.GetType().Name, new object[] { model });
			}
			UIStoredParameterPackage uistoredParameterPackage = model as UIStoredParameterPackage;
			if (uistoredParameterPackage == null)
			{
				return model;
			}
			Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(uistoredParameterPackage.StoredParameter.DataType);
			return JsonConvert.DeserializeObject(uistoredParameterPackage.StoredParameter.JsonData, typeFromId);
		}

		// Token: 0x02000206 RID: 518
		public enum Styles
		{
			// Token: 0x0400072A RID: 1834
			Default
		}
	}
}
