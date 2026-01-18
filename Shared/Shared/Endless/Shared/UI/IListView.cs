using System;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x0200018C RID: 396
	public interface IListView
	{
		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x060009BF RID: 2495
		ListCellSizeTypes ListCellSizeType { get; }

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x060009C0 RID: 2496
		UnityEvent<ListCellSizeTypes> ListCellSizeTypeChangedUnityEvent { get; }

		// Token: 0x060009C1 RID: 2497
		void SetListCellSizeType(ListCellSizeTypes value);
	}
}
