using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000239 RID: 569
	public class UIPropLibraryReferenceView : UIBasePropLibraryReferenceView<PropLibraryReference, UIPropLibraryReferenceView.Styles>
	{
		// Token: 0x1700012A RID: 298
		// (get) Token: 0x06000934 RID: 2356 RVA: 0x0002B63B File Offset: 0x0002983B
		// (set) Token: 0x06000935 RID: 2357 RVA: 0x0002B643 File Offset: 0x00029843
		public override UIPropLibraryReferenceView.Styles Style { get; protected set; }

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x06000936 RID: 2358 RVA: 0x0001BF89 File Offset: 0x0001A189
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.None;
			}
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x0002B64C File Offset: 0x0002984C
		protected override string GetReferenceName(PropLibraryReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceName", "model", model), this);
			}
			return base.GetPropLibraryReferenceName(model);
		}

		// Token: 0x0200023A RID: 570
		public enum Styles
		{
			// Token: 0x040007A0 RID: 1952
			Default
		}
	}
}
