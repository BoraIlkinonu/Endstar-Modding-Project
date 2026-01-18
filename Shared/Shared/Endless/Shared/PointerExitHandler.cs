using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Shared
{
	// Token: 0x02000055 RID: 85
	public class PointerExitHandler : BaseEventSystemHandler<PointerExitHandler>, IPointerExitHandler, IEventSystemHandler
	{
		// Token: 0x060002B5 RID: 693 RVA: 0x0000DD7C File Offset: 0x0000BF7C
		public void OnPointerExit(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerExit", new object[] { eventData.pointerId });
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((PointerExitHandler)this.Intercepter).OnPointerExit(eventData);
				return;
			}
			this.PointerExitUnityEvent.Invoke();
		}

		// Token: 0x04000169 RID: 361
		[Header("PointerExitHandler")]
		public UnityEvent PointerExitUnityEvent = new UnityEvent();
	}
}
