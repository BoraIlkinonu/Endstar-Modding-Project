using System;

namespace Endless.Shared.UI
{
	// Token: 0x0200018D RID: 397
	public abstract class UIBaseAssetCloudPaginatedListModel<T> : UIBaseCloudPaginatedListModel<T>
	{
		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x060009C2 RID: 2498
		protected abstract string AssetType { get; }

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x060009C3 RID: 2499 RVA: 0x00029883 File Offset: 0x00027A83
		protected virtual string AssetFilter
		{
			get
			{
				return string.Concat(new string[] { "(Name: \"*", this.StringFilter, "*\", ", base.SortQuery, ")" });
			}
		}
	}
}
