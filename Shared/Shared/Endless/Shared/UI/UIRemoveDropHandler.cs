using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001A5 RID: 421
	public abstract class UIRemoveDropHandler : UIDropHandler
	{
		// Token: 0x06000AE8 RID: 2792 RVA: 0x0002FF7C File Offset: 0x0002E17C
		protected override void Start()
		{
			base.Start();
			this.DropWithGameObjectUnityEvent.AddListener(new UnityAction<GameObject>(this.OnDroppedGameObject));
		}

		// Token: 0x06000AE9 RID: 2793 RVA: 0x0002FF9C File Offset: 0x0002E19C
		protected override void OnEnable()
		{
			base.OnEnable();
			if (this.DropEventSource == UIDropHandler.DropEventSources.DragHandlerEndDragAction)
			{
				UIDragHandler.BeginDragWithRectTransformAction = (Action<RectTransform>)Delegate.Combine(UIDragHandler.BeginDragWithRectTransformAction, new Action<RectTransform>(this.OnBeginDrag));
				UIDragHandler.DragWithRectTransformAction = (Action<RectTransform>)Delegate.Combine(UIDragHandler.DragWithRectTransformAction, new Action<RectTransform>(this.OnDrag));
				return;
			}
			if (this.DropEventSource == UIDropHandler.DropEventSources.DragInstanceHandlerInstanceEndDragEvent)
			{
				UIDragInstanceHandler.InstanceBeginDragAction = (Action<RectTransform>)Delegate.Combine(UIDragInstanceHandler.InstanceBeginDragAction, new Action<RectTransform>(this.OnBeginDrag));
				UIListCellInstanceUpdateHandler.UpdateWithRectTransform = (Action<RectTransform>)Delegate.Combine(UIListCellInstanceUpdateHandler.UpdateWithRectTransform, new Action<RectTransform>(this.OnDrag));
			}
		}

		// Token: 0x06000AEA RID: 2794 RVA: 0x00030044 File Offset: 0x0002E244
		protected override void OnDisable()
		{
			base.OnDisable();
			if (this.DropEventSource == UIDropHandler.DropEventSources.DragHandlerEndDragAction)
			{
				UIDragHandler.BeginDragWithRectTransformAction = (Action<RectTransform>)Delegate.Remove(UIDragHandler.BeginDragWithRectTransformAction, new Action<RectTransform>(this.OnBeginDrag));
				UIDragHandler.DragWithRectTransformAction = (Action<RectTransform>)Delegate.Remove(UIDragHandler.DragWithRectTransformAction, new Action<RectTransform>(this.OnDrag));
			}
			else if (this.DropEventSource == UIDropHandler.DropEventSources.DragInstanceHandlerInstanceEndDragEvent)
			{
				UIDragInstanceHandler.InstanceBeginDragAction = (Action<RectTransform>)Delegate.Remove(UIDragInstanceHandler.InstanceBeginDragAction, new Action<RectTransform>(this.OnBeginDrag));
				UIListCellInstanceUpdateHandler.UpdateWithRectTransform = (Action<RectTransform>)Delegate.Remove(UIListCellInstanceUpdateHandler.UpdateWithRectTransform, new Action<RectTransform>(this.OnDrag));
			}
			this.isHovering = false;
		}

		// Token: 0x06000AEB RID: 2795 RVA: 0x000300F4 File Offset: 0x0002E2F4
		protected virtual void OnDroppedGameObject(GameObject droppedGameObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDroppedGameObject ( droppedGameObject: " + droppedGameObject.DebugSafeName(true) + " )", this);
			}
			RectTransform rectTransform;
			if (droppedGameObject.TryGetComponent<RectTransform>(out rectTransform) && base.WouldBeValidDrop(rectTransform))
			{
				this.onValidDroppedGameLibraryObjectTweens.Tween();
			}
			this.onDropTweens.Tween();
		}

		// Token: 0x06000AEC RID: 2796 RVA: 0x0003014E File Offset: 0x0002E34E
		private void OnBeginDrag(RectTransform draggedObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnBeginDrag ( draggedObject: " + draggedObject.DebugSafeName(true) + " )", this);
			}
			this.onDraggableTweens.Tween();
		}

		// Token: 0x06000AED RID: 2797 RVA: 0x00030180 File Offset: 0x0002E380
		private void OnDrag(RectTransform draggedObject)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.Log("OnDrag ( draggedObject: " + draggedObject.DebugSafeName(true) + " )", this);
			}
			bool flag = draggedObject.Overlaps(base.RectTransform);
			if (!this.isHovering && flag)
			{
				this.isHovering = true;
				this.onHoverTweens.Tween();
				return;
			}
			if (this.isHovering && !flag)
			{
				this.isHovering = false;
				this.onUnhoverTweens.Tween();
			}
		}

		// Token: 0x04000707 RID: 1799
		[Header("UIRemoveDropHandler")]
		[SerializeField]
		private TweenCollection onDraggableTweens;

		// Token: 0x04000708 RID: 1800
		[SerializeField]
		private TweenCollection onHoverTweens;

		// Token: 0x04000709 RID: 1801
		[SerializeField]
		private TweenCollection onUnhoverTweens;

		// Token: 0x0400070A RID: 1802
		[SerializeField]
		private TweenCollection onValidDroppedGameLibraryObjectTweens;

		// Token: 0x0400070B RID: 1803
		[SerializeField]
		private TweenCollection onDropTweens;

		// Token: 0x0400070C RID: 1804
		private bool isHovering;
	}
}
