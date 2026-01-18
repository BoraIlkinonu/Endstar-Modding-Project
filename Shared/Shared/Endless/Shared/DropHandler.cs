using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Shared
{
	// Token: 0x02000051 RID: 81
	public class DropHandler : BaseEventSystemHandler<DropHandler>, IDropHandler, IEventSystemHandler
	{
		// Token: 0x060002AA RID: 682 RVA: 0x0000DA1C File Offset: 0x0000BC1C
		public virtual void OnDrop(PointerEventData eventData)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDrop", new object[] { eventData });
			}
			if (base.BlockAllEvents)
			{
				return;
			}
			if (base.HasIntercepter)
			{
				eventData.pointerDrag = this.Intercepter.gameObject;
				((DropHandler)this.Intercepter).OnDrop(eventData);
				return;
			}
			GameObject pointerDrag = eventData.pointerDrag;
			if (base.VerboseLogging)
			{
				Debug.Log(pointerDrag.name ?? "", pointerDrag);
			}
			this.DropWithGameObjectUnityEvent.Invoke(pointerDrag);
			this.DropWithPointerEventDataUnityEvent.Invoke(eventData);
		}

		// Token: 0x04000160 RID: 352
		[Header("DropHandler")]
		public UnityEvent<GameObject> DropWithGameObjectUnityEvent = new UnityEvent<GameObject>();

		// Token: 0x04000161 RID: 353
		public UnityEvent<PointerEventData> DropWithPointerEventDataUnityEvent = new UnityEvent<PointerEventData>();
	}
}
