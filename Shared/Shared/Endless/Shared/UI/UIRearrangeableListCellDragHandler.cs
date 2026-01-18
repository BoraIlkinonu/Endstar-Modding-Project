using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Shared.UI
{
	// Token: 0x0200017E RID: 382
	public class UIRearrangeableListCellDragHandler : UIDragHandler
	{
		// Token: 0x06000976 RID: 2422 RVA: 0x00028875 File Offset: 0x00026A75
		public override void OnDrag(PointerEventData eventData)
		{
			base.OnDrag(eventData);
			Action<RectTransform> onDragAction = UIRearrangeableListCellDragHandler.OnDragAction;
			if (onDragAction == null)
			{
				return;
			}
			onDragAction(base.RectTransform);
		}

		// Token: 0x040005F7 RID: 1527
		public static Action<RectTransform> OnDragAction;
	}
}
