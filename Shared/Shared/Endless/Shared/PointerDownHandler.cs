using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Shared
{
	// Token: 0x02000053 RID: 83
	public class PointerDownHandler : BaseEventSystemHandler<PointerDownHandler>, IPointerDownHandler, IEventSystemHandler
	{
		// Token: 0x060002B1 RID: 689 RVA: 0x0000DC54 File Offset: 0x0000BE54
		public void OnPointerDown(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerDown", new object[] { eventData.pointerId });
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((PointerDownHandler)this.Intercepter).OnPointerDown(eventData);
				return;
			}
			this.PointerDownUnityEvent.Invoke();
			this.PointerDownEventWitgPointerEventDataUnityEvent.Invoke(eventData);
		}

		// Token: 0x04000166 RID: 358
		[Header("PointerDownHandler")]
		public UnityEvent PointerDownUnityEvent = new UnityEvent();

		// Token: 0x04000167 RID: 359
		public UnityEvent<PointerEventData> PointerDownEventWitgPointerEventDataUnityEvent = new UnityEvent<PointerEventData>();
	}
}
