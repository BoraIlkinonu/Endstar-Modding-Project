using System;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200018B RID: 395
	public interface IListModel
	{
		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x060009BA RID: 2490
		UnityEvent<SortOrders> SortOrderChangedUnityEvent { get; }

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x060009BB RID: 2491
		SortOrders SortOrder { get; }

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x060009BC RID: 2492
		bool DisplayAddButton { get; }

		// Token: 0x060009BD RID: 2493
		void SetSortOrder(SortOrders value);

		// Token: 0x060009BE RID: 2494
		void SetAddButtonIsInteractable(bool value);
	}
}
