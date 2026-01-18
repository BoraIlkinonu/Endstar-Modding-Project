using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Shared
{
	// Token: 0x02000050 RID: 80
	public class DragHandler : BaseEventSystemHandler<DragHandler>, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
	{
		// Token: 0x17000064 RID: 100
		// (get) Token: 0x060002A4 RID: 676 RVA: 0x0000D794 File Offset: 0x0000B994
		// (set) Token: 0x060002A5 RID: 677 RVA: 0x0000D79C File Offset: 0x0000B99C
		public PointerEventData LastPointerEventData { get; private set; }

		// Token: 0x060002A6 RID: 678 RVA: 0x0000D7A8 File Offset: 0x0000B9A8
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}", new object[]
				{
					"OnBeginDrag",
					"eventData",
					eventData.selectedObject.DebugSafeName(true),
					"BlockAllEvents",
					base.BlockAllEvents
				}), this);
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((DragHandler)this.Intercepter).OnBeginDrag(eventData);
				return;
			}
			this.LastPointerEventData = eventData;
			this.BeginDragUnityEvent.Invoke();
			this.BeginDragWithPointerEventDataUnityEvent.Invoke(eventData);
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0000D85C File Offset: 0x0000BA5C
		public virtual void OnDrag(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}", new object[]
				{
					"OnDrag",
					"eventData",
					eventData.selectedObject.DebugSafeName(true),
					"BlockAllEvents",
					base.BlockAllEvents
				}), this);
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((DragHandler)this.Intercepter).OnDrag(eventData);
				return;
			}
			this.LastPointerEventData = eventData;
			this.DragUnityEvent.Invoke();
			this.DragWithPointerEventDataUnityEvent.Invoke(eventData);
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000D910 File Offset: 0x0000BB10
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}", new object[]
				{
					"OnEndDrag",
					"eventData",
					eventData.selectedObject.DebugSafeName(true),
					"BlockAllEvents",
					base.BlockAllEvents
				}), this);
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((DragHandler)this.Intercepter).OnEndDrag(eventData);
				return;
			}
			this.LastPointerEventData = eventData;
			this.EndDragUnityEvent.Invoke();
			this.EndDragWithPointerEventDataUnityEvent.Invoke(eventData);
		}

		// Token: 0x04000159 RID: 345
		[Header("BaseEventSystemHandler")]
		public UnityEvent BeginDragUnityEvent = new UnityEvent();

		// Token: 0x0400015A RID: 346
		public UnityEvent<PointerEventData> BeginDragWithPointerEventDataUnityEvent = new UnityEvent<PointerEventData>();

		// Token: 0x0400015B RID: 347
		public UnityEvent DragUnityEvent = new UnityEvent();

		// Token: 0x0400015C RID: 348
		public UnityEvent<PointerEventData> DragWithPointerEventDataUnityEvent = new UnityEvent<PointerEventData>();

		// Token: 0x0400015D RID: 349
		public UnityEvent EndDragUnityEvent = new UnityEvent();

		// Token: 0x0400015E RID: 350
		public UnityEvent<PointerEventData> EndDragWithPointerEventDataUnityEvent = new UnityEvent<PointerEventData>();
	}
}
