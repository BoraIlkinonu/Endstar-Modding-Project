using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000233 RID: 563
	public class UIInventoryLibraryReferenceView : UIBaseInventoryLibraryReferenceView<InventoryLibraryReference, UIInventoryLibraryReferenceView.Styles>
	{
		// Token: 0x17000122 RID: 290
		// (get) Token: 0x06000922 RID: 2338 RVA: 0x0002B54B File Offset: 0x0002974B
		// (set) Token: 0x06000923 RID: 2339 RVA: 0x0002B553 File Offset: 0x00029753
		public override UIInventoryLibraryReferenceView.Styles Style { get; protected set; }

		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000924 RID: 2340 RVA: 0x0002B55C File Offset: 0x0002975C
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.InventoryItem;
			}
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x0002B55F File Offset: 0x0002975F
		protected override string GetReferenceName(InventoryLibraryReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetReferenceName", new object[] { model });
			}
			return base.GetPropLibraryReferenceName(model);
		}

		// Token: 0x02000234 RID: 564
		public enum Styles
		{
			// Token: 0x0400079A RID: 1946
			Default
		}
	}
}
