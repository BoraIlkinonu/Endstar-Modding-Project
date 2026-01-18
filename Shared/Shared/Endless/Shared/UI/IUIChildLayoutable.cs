using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200014F RID: 335
	public interface IUIChildLayoutable : IUILayoutable
	{
		// Token: 0x0600082D RID: 2093
		void CollectChildLayoutItems();

		// Token: 0x0600082E RID: 2094
		void AddChildLayoutItem(RectTransform newChildLayoutItem, int? siblingIndex = null);

		// Token: 0x0600082F RID: 2095
		void RemoveChildLayoutItem(RectTransform childLayoutItem);
	}
}
