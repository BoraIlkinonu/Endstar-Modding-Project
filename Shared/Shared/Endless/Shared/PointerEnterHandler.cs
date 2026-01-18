using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Shared
{
	// Token: 0x02000054 RID: 84
	public class PointerEnterHandler : BaseEventSystemHandler<PointerEnterHandler>, IPointerEnterHandler, IEventSystemHandler
	{
		// Token: 0x060002B3 RID: 691 RVA: 0x0000DCF4 File Offset: 0x0000BEF4
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerEnter", new object[] { eventData.pointerId });
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((PointerEnterHandler)this.Intercepter).OnPointerEnter(eventData);
				return;
			}
			this.PointerEnterUnityEvent.Invoke();
		}

		// Token: 0x04000168 RID: 360
		[Header("PointerEnterHandler")]
		public UnityEvent PointerEnterUnityEvent = new UnityEvent();
	}
}
