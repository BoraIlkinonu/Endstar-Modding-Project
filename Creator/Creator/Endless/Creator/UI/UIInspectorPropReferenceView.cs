using System;
using Endless.Gameplay;

namespace Endless.Creator.UI
{
	// Token: 0x0200021F RID: 543
	public abstract class UIInspectorPropReferenceView<TModel, TViewStyle> : UIInspectorReferenceView<TModel, TViewStyle> where TModel : InspectorPropReference where TViewStyle : Enum
	{
		// Token: 0x17000110 RID: 272
		// (get) Token: 0x060008C2 RID: 2242
		protected abstract ReferenceFilter ReferenceFilter { get; }
	}
}
