using System;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x020001FE RID: 510
	public class UIEndlessParameterInfoView : UIBasePropertyView<EndlessParameterInfo, UIEndlessParameterInfoView.Styles>
	{
		// Token: 0x170000EE RID: 238
		// (get) Token: 0x06000803 RID: 2051 RVA: 0x00027BF2 File Offset: 0x00025DF2
		// (set) Token: 0x06000804 RID: 2052 RVA: 0x00027BFA File Offset: 0x00025DFA
		public override UIEndlessParameterInfoView.Styles Style { get; protected set; }

		// Token: 0x06000805 RID: 2053 RVA: 0x00027C04 File Offset: 0x00025E04
		public override void View(EndlessParameterInfo model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			Type referencedType = model.GetReferencedType();
			object obj = MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(referencedType);
			this.ViewProperty(model.DisplayName, obj, model.DataType, "", Array.Empty<ClampValue>());
		}

		// Token: 0x06000806 RID: 2054 RVA: 0x00027C60 File Offset: 0x00025E60
		protected override object ExtractModel(object model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "ExtractModel", (model == null) ? "null" : model.GetType().Name, new object[] { model });
			}
			EndlessParameterInfo endlessParameterInfo = model as EndlessParameterInfo;
			if (endlessParameterInfo == null)
			{
				return model;
			}
			Type referencedType = endlessParameterInfo.GetReferencedType();
			return MonoBehaviourSingleton<UIDynamicTypeFactory>.Instance.Create(referencedType);
		}

		// Token: 0x020001FF RID: 511
		public enum Styles
		{
			// Token: 0x0400071F RID: 1823
			Default
		}
	}
}
