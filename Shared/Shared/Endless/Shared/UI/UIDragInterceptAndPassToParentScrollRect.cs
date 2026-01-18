using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000258 RID: 600
	[RequireComponent(typeof(UIDragHandler))]
	public class UIDragInterceptAndPassToParentScrollRect : UIGameObject
	{
		// Token: 0x06000F25 RID: 3877 RVA: 0x00041470 File Offset: 0x0003F670
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UIDragHandler>(out this.dragHandler);
			this.scrollRect = base.GetComponentInParent<ScrollRect>();
			if (!this.scrollRect)
			{
				return;
			}
			this.dragHandler.BeginDragWithPointerEventDataUnityEvent.AddListener(new UnityAction<PointerEventData>(this.OnBeginDrag));
			this.dragHandler.DragWithPointerEventDataUnityEvent.AddListener(new UnityAction<PointerEventData>(this.scrollRect.OnDrag));
			this.dragHandler.EndDragWithPointerEventDataUnityEvent.AddListener(new UnityAction<PointerEventData>(this.scrollRect.OnEndDrag));
			this.initialized = true;
		}

		// Token: 0x06000F26 RID: 3878 RVA: 0x00041524 File Offset: 0x0003F724
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			if (!this.initialized)
			{
				return;
			}
			this.dragHandler.BeginDragWithPointerEventDataUnityEvent.RemoveListener(new UnityAction<PointerEventData>(this.OnBeginDrag));
			this.dragHandler.DragWithPointerEventDataUnityEvent.RemoveListener(new UnityAction<PointerEventData>(this.scrollRect.OnDrag));
			this.dragHandler.EndDragWithPointerEventDataUnityEvent.RemoveListener(new UnityAction<PointerEventData>(this.scrollRect.OnEndDrag));
		}

		// Token: 0x06000F27 RID: 3879 RVA: 0x000415B4 File Offset: 0x0003F7B4
		private void OnBeginDrag(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBeginDrag", new object[] { pointerEventData, pointerEventData.pointerId });
			}
			this.scrollRect.OnBeginDrag(pointerEventData);
			if (this.pointerUpHandlerToBlockNextOnBeginDrag)
			{
				this.pointerUpHandlerToBlockNextOnBeginDrag.BlockNextOnPointerUpEvent = true;
			}
		}

		// Token: 0x0400099D RID: 2461
		[SerializeField]
		private PointerUpHandler pointerUpHandlerToBlockNextOnBeginDrag;

		// Token: 0x0400099E RID: 2462
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400099F RID: 2463
		private UIDragHandler dragHandler;

		// Token: 0x040009A0 RID: 2464
		private ScrollRect scrollRect;

		// Token: 0x040009A1 RID: 2465
		private bool initialized;
	}
}
