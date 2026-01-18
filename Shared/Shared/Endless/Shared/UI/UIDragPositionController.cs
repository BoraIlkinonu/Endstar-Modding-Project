using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000130 RID: 304
	[RequireComponent(typeof(UIDragHandler))]
	public class UIDragPositionController : UIGameObject
	{
		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000772 RID: 1906 RVA: 0x0001F4B7 File Offset: 0x0001D6B7
		private Canvas Canvas
		{
			get
			{
				if (!this.canvas)
				{
					this.canvas = base.GetComponentInParent<Canvas>();
				}
				return this.canvas;
			}
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x0001F4D8 File Offset: 0x0001D6D8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!this.rectTransformToMove)
			{
				this.rectTransformToMove = base.RectTransform;
			}
			base.TryGetComponent<DragHandler>(out this.dragHandler);
			this.dragHandler.BeginDragUnityEvent.AddListener(new UnityAction(this.OnBeginDrag));
			this.dragHandler.DragWithPointerEventDataUnityEvent.AddListener(new UnityAction<PointerEventData>(this.OnDrag));
			this.dragHandler.EndDragUnityEvent.AddListener(new UnityAction(this.OnEndDrag));
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x0001F578 File Offset: 0x0001D778
		public void OnBeginDrag()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBeginDrag", Array.Empty<object>());
			}
			Graphic[] array = this.graphicsToToggleRaycastTargetDuringDrag;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].raycastTarget = false;
			}
			this.BeginDragUnityEvent.Invoke();
			this.BeginDragWithSelfUnityEvent.Invoke(base.gameObject);
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x0001F5D8 File Offset: 0x0001D7D8
		public void OnDrag(PointerEventData eventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDrag", new object[] { eventData });
			}
			this.rectTransformToMove.anchoredPosition += eventData.delta / this.Canvas.scaleFactor;
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x0001F630 File Offset: 0x0001D830
		public void OnEndDrag()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEndDrag", Array.Empty<object>());
			}
			Graphic[] array = this.graphicsToToggleRaycastTargetDuringDrag;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].raycastTarget = true;
			}
			this.DragEndedUnityEvent.Invoke();
		}

		// Token: 0x0400045F RID: 1119
		public UnityEvent BeginDragUnityEvent = new UnityEvent();

		// Token: 0x04000460 RID: 1120
		public UnityEvent<GameObject> BeginDragWithSelfUnityEvent = new UnityEvent<GameObject>();

		// Token: 0x04000461 RID: 1121
		public UnityEvent DragEndedUnityEvent = new UnityEvent();

		// Token: 0x04000462 RID: 1122
		[Tooltip("Defaults to self if not set")]
		[SerializeField]
		private RectTransform rectTransformToMove;

		// Token: 0x04000463 RID: 1123
		[Tooltip("Used to prevent the dragged object from blocking a drop event on another object")]
		[SerializeField]
		private Graphic[] graphicsToToggleRaycastTargetDuringDrag = Array.Empty<Graphic>();

		// Token: 0x04000464 RID: 1124
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000465 RID: 1125
		private DragHandler dragHandler;

		// Token: 0x04000466 RID: 1126
		private Canvas canvas;
	}
}
