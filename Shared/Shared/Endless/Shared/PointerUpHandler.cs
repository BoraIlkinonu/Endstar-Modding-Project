using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Shared
{
	// Token: 0x02000058 RID: 88
	public class PointerUpHandler : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
	{
		// Token: 0x060002C8 RID: 712 RVA: 0x0000E07A File Offset: 0x0000C27A
		public void OnPointerDown(PointerEventData eventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerDown", new object[] { eventData.ToString() });
			}
		}

		// Token: 0x060002C9 RID: 713 RVA: 0x0000E09E File Offset: 0x0000C29E
		public void OnPointerUp(PointerEventData eventData)
		{
			if (this.BlockNextOnPointerUpEvent)
			{
				this.BlockNextOnPointerUpEvent = false;
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", new object[] { eventData.ToString() });
			}
			this.PointerUpUnityEvent.Invoke();
		}

		// Token: 0x060002CA RID: 714 RVA: 0x0000E0DD File Offset: 0x0000C2DD
		public void SetBlockNextOnPointerUpEventToTrue()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetBlockNextOnPointerUpEventToTrue", Array.Empty<object>());
			}
			this.BlockNextOnPointerUpEvent = true;
		}

		// Token: 0x04000178 RID: 376
		public UnityEvent PointerUpUnityEvent = new UnityEvent();

		// Token: 0x04000179 RID: 377
		public bool BlockNextOnPointerUpEvent;

		// Token: 0x0400017A RID: 378
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
