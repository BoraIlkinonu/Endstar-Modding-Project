using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Shared
{
	// Token: 0x02000052 RID: 82
	public class PointerDownAndUpHandler : BaseEventSystemHandler<PointerDownAndUpHandler>, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
	{
		// Token: 0x060002AC RID: 684 RVA: 0x0000DAD4 File Offset: 0x0000BCD4
		public void SetBlockNextOnDownEvent(bool newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetBlockNextOnDownEvent", new object[] { newValue });
			}
			this.blockNextOnDownEvent = newValue;
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0000DAFF File Offset: 0x0000BCFF
		public void SetBlockNextOnPointerUpEvent(bool newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetBlockNextOnPointerUpEvent", new object[] { newValue });
			}
			this.blockNextOnUpEvent = newValue;
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0000DB2C File Offset: 0x0000BD2C
		public void OnPointerDown(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerDown", new object[] { eventData.pointerId });
			}
			if (this.blockNextOnDownEvent)
			{
				this.blockNextOnDownEvent = false;
				return;
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((PointerDownAndUpHandler)this.Intercepter).OnPointerDown(eventData);
				return;
			}
			this.PointerDownUnityEvent.Invoke();
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0000DBB0 File Offset: 0x0000BDB0
		public void OnPointerUp(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", new object[] { eventData.pointerId });
			}
			if (this.blockNextOnUpEvent)
			{
				this.blockNextOnUpEvent = false;
				return;
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((PointerDownAndUpHandler)this.Intercepter).OnPointerUp(eventData);
				return;
			}
			this.PointerUpUnityEvent.Invoke();
		}

		// Token: 0x04000162 RID: 354
		[Header("PointerDownAndUpHandler")]
		public UnityEvent PointerDownUnityEvent = new UnityEvent();

		// Token: 0x04000163 RID: 355
		public UnityEvent PointerUpUnityEvent = new UnityEvent();

		// Token: 0x04000164 RID: 356
		[SerializeField]
		private bool blockNextOnDownEvent;

		// Token: 0x04000165 RID: 357
		[SerializeField]
		private bool blockNextOnUpEvent;
	}
}
