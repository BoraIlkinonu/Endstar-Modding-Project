using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000236 RID: 566
	public class UIPhysicsObjectLibraryReferenceView : UIBasePropLibraryReferenceView<PhysicsObjectLibraryReference, UIPhysicsObjectLibraryReferenceView.Styles>
	{
		// Token: 0x17000126 RID: 294
		// (get) Token: 0x0600092B RID: 2347 RVA: 0x0002B5C0 File Offset: 0x000297C0
		// (set) Token: 0x0600092C RID: 2348 RVA: 0x0002B5C8 File Offset: 0x000297C8
		public override UIPhysicsObjectLibraryReferenceView.Styles Style { get; protected set; }

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x0600092D RID: 2349 RVA: 0x0002B5D1 File Offset: 0x000297D1
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.PhysicsObject;
			}
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x0002B5D4 File Offset: 0x000297D4
		protected override string GetReferenceName(PhysicsObjectLibraryReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceName", "model", model), this);
			}
			return base.GetPropLibraryReferenceName(model);
		}

		// Token: 0x02000237 RID: 567
		public enum Styles
		{
			// Token: 0x0400079D RID: 1949
			Default
		}
	}
}
