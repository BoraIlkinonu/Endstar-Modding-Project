using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Shared.UI
{
	// Token: 0x02000101 RID: 257
	[RequireComponent(typeof(RectTransform))]
	public class UIDropHandler : DropHandler
	{
		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06000633 RID: 1587 RVA: 0x0001A31E File Offset: 0x0001851E
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

		// Token: 0x06000634 RID: 1588 RVA: 0x0001A340 File Offset: 0x00018540
		protected virtual void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			base.TryGetComponent<RectTransform>(out this.rectTransform);
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x0001A364 File Offset: 0x00018564
		protected virtual void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			if (this.DropEventSource == UIDropHandler.DropEventSources.DragHandlerEndDragAction)
			{
				UIDragHandler.EndDragWithRectTransformAction = (Action<RectTransform>)Delegate.Combine(UIDragHandler.EndDragWithRectTransformAction, new Action<RectTransform>(this.HandleDrop));
				return;
			}
			if (this.DropEventSource == UIDropHandler.DropEventSources.DragInstanceHandlerInstanceEndDragEvent)
			{
				UIDragInstanceHandler.InstanceEndDragAction = (Action<RectTransform>)Delegate.Combine(UIDragInstanceHandler.InstanceEndDragAction, new Action<RectTransform>(this.HandleDrop));
			}
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x0001A3D8 File Offset: 0x000185D8
		protected override void OnDisable()
		{
			base.OnDisable();
			if (this.DropEventSource == UIDropHandler.DropEventSources.DragHandlerEndDragAction)
			{
				UIDragHandler.EndDragWithRectTransformAction = (Action<RectTransform>)Delegate.Remove(UIDragHandler.EndDragWithRectTransformAction, new Action<RectTransform>(this.HandleDrop));
				return;
			}
			if (this.DropEventSource == UIDropHandler.DropEventSources.DragInstanceHandlerInstanceEndDragEvent)
			{
				UIDragInstanceHandler.InstanceEndDragAction = (Action<RectTransform>)Delegate.Remove(UIDragInstanceHandler.InstanceEndDragAction, new Action<RectTransform>(this.HandleDrop));
			}
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x000050BB File Offset: 0x000032BB
		public override void OnDrop(PointerEventData eventData)
		{
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x0001A440 File Offset: 0x00018640
		protected bool WouldBeValidDrop(RectTransform droppedRectTransform)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("WouldBeValidDrop ( droppedRectTransform: " + droppedRectTransform.DebugSafeName(true) + " )", this);
			}
			RectTransform rectTransform = ((droppedRectTransform.Area() > this.rectTransform.Area()) ? droppedRectTransform : this.rectTransform);
			float num = RectTransformUtility.OverlapPercentage((rectTransform == droppedRectTransform) ? this.rectTransform : droppedRectTransform, rectTransform);
			float num2 = this.dropEventIfOverlapPercentageIsEqualToOrOver / 100f;
			bool flag = num > num2;
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1:P0}, ", "overlapPercentage", num) + string.Format("{0}: {1:P0}, ", "triggerPercentage", num2) + string.Format("{0}: {1}", "wouldBeValidDrop", flag), this);
			}
			return flag;
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x0001A50C File Offset: 0x0001870C
		private void HandleDrop(RectTransform droppedRectTransform)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("HandleDrop ( droppedRectTransform: " + droppedRectTransform.name + " )", this);
			}
			if (!this.WouldBeValidDrop(droppedRectTransform))
			{
				return;
			}
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
			{
				pointerDrag = droppedRectTransform.gameObject
			};
			base.OnDrop(pointerEventData);
		}

		// Token: 0x0400036E RID: 878
		[Header("UIDropHandler")]
		[SerializeField]
		protected UIDropHandler.DropEventSources DropEventSource;

		// Token: 0x0400036F RID: 879
		[Range(0f, 100f)]
		[SerializeField]
		private float dropEventIfOverlapPercentageIsEqualToOrOver = 50f;

		// Token: 0x04000370 RID: 880
		private RectTransform rectTransform;

		// Token: 0x02000102 RID: 258
		protected enum DropEventSources
		{
			// Token: 0x04000372 RID: 882
			DragHandlerEndDragAction,
			// Token: 0x04000373 RID: 883
			DragInstanceHandlerInstanceEndDragEvent
		}
	}
}
