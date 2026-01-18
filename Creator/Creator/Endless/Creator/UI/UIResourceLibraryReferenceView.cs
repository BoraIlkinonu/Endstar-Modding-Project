using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x0200023C RID: 572
	public class UIResourceLibraryReferenceView : UIBasePropLibraryReferenceView<ResourceLibraryReference, UIResourceLibraryReferenceView.Styles>
	{
		// Token: 0x1700012E RID: 302
		// (get) Token: 0x0600093E RID: 2366 RVA: 0x0002B728 File Offset: 0x00029928
		// (set) Token: 0x0600093F RID: 2367 RVA: 0x0002B730 File Offset: 0x00029930
		public override UIResourceLibraryReferenceView.Styles Style { get; protected set; }

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000940 RID: 2368 RVA: 0x0002B739 File Offset: 0x00029939
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.Resource;
			}
		}

		// Token: 0x06000941 RID: 2369 RVA: 0x0002B73D File Offset: 0x0002993D
		protected override string GetReferenceName(ResourceLibraryReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceName", "model", model), this);
			}
			return base.GetPropLibraryReferenceName(model);
		}

		// Token: 0x0200023D RID: 573
		public enum Styles
		{
			// Token: 0x040007A3 RID: 1955
			Default
		}
	}
}
