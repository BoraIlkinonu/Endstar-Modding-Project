using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Shared.UI
{
	// Token: 0x02000100 RID: 256
	[RequireComponent(typeof(RectTransform))]
	public class UIDragHandler : DragHandler
	{
		// Token: 0x17000103 RID: 259
		// (get) Token: 0x0600062C RID: 1580 RVA: 0x0001A210 File Offset: 0x00018410
		public UnityEvent<RectTransform> DragWithRectTransformUnityEvent { get; } = new UnityEvent<RectTransform>();

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x0600062D RID: 1581 RVA: 0x0001A218 File Offset: 0x00018418
		public UnityEvent<RectTransform> EndDragWithRectTransformUnityEvent { get; } = new UnityEvent<RectTransform>();

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x0600062E RID: 1582 RVA: 0x0001A220 File Offset: 0x00018420
		public RectTransform RectTransform
		{
			get
			{
				if (!this.rectTransform)
				{
					base.TryGetComponent<RectTransform>(out this.rectTransform);
				}
				return this.rectTransform;
			}
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0001A242 File Offset: 0x00018442
		public override void OnBeginDrag(PointerEventData eventData)
		{
			base.OnBeginDrag(eventData);
			if (!this.rectTransform)
			{
				base.TryGetComponent<RectTransform>(out this.rectTransform);
			}
			Action<RectTransform> beginDragWithRectTransformAction = UIDragHandler.BeginDragWithRectTransformAction;
			if (beginDragWithRectTransformAction == null)
			{
				return;
			}
			beginDragWithRectTransformAction(this.rectTransform);
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0001A27C File Offset: 0x0001847C
		public override void OnDrag(PointerEventData eventData)
		{
			base.OnDrag(eventData);
			if (!this.rectTransform)
			{
				base.TryGetComponent<RectTransform>(out this.rectTransform);
			}
			this.DragWithRectTransformUnityEvent.Invoke(this.rectTransform);
			Action<RectTransform> dragWithRectTransformAction = UIDragHandler.DragWithRectTransformAction;
			if (dragWithRectTransformAction == null)
			{
				return;
			}
			dragWithRectTransformAction(this.rectTransform);
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x0001A2D0 File Offset: 0x000184D0
		public override void OnEndDrag(PointerEventData eventData)
		{
			base.OnEndDrag(eventData);
			Action<RectTransform> endDragWithRectTransformAction = UIDragHandler.EndDragWithRectTransformAction;
			if (endDragWithRectTransformAction != null)
			{
				endDragWithRectTransformAction(this.rectTransform);
			}
			this.EndDragWithRectTransformUnityEvent.Invoke(this.rectTransform);
		}

		// Token: 0x04000368 RID: 872
		public static Action<RectTransform> BeginDragWithRectTransformAction;

		// Token: 0x04000369 RID: 873
		public static Action<RectTransform> DragWithRectTransformAction;

		// Token: 0x0400036A RID: 874
		public static Action<RectTransform> EndDragWithRectTransformAction;

		// Token: 0x0400036B RID: 875
		private RectTransform rectTransform;
	}
}
