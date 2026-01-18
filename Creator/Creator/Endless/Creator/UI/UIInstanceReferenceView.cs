using System;
using Endless.Gameplay;

namespace Endless.Creator.UI
{
	// Token: 0x02000225 RID: 549
	public class UIInstanceReferenceView : UIBaseInstanceReferenceView<InstanceReference, UIInstanceReferenceView.Styles>
	{
		// Token: 0x17000115 RID: 277
		// (get) Token: 0x060008EE RID: 2286 RVA: 0x0002AE1C File Offset: 0x0002901C
		// (set) Token: 0x060008EF RID: 2287 RVA: 0x0002AE24 File Offset: 0x00029024
		public override UIInstanceReferenceView.Styles Style { get; protected set; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x060008F0 RID: 2288 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.NonStatic;
			}
		}

		// Token: 0x02000226 RID: 550
		public enum Styles
		{
			// Token: 0x0400078A RID: 1930
			Default,
			// Token: 0x0400078B RID: 1931
			NoneOrContext
		}
	}
}
