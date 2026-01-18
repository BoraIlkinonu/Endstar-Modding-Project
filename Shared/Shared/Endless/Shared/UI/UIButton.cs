using System;
using System.Collections;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200010E RID: 270
	[RequireComponent(typeof(RectTransform))]
	public class UIButton : Button, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IValidatable
	{
		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000672 RID: 1650 RVA: 0x0001B9E5 File Offset: 0x00019BE5
		// (set) Token: 0x06000673 RID: 1651 RVA: 0x0001B9ED File Offset: 0x00019BED
		protected bool VerboseLogging { get; set; }

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000674 RID: 1652 RVA: 0x0001B9F6 File Offset: 0x00019BF6
		// (set) Token: 0x06000675 RID: 1653 RVA: 0x0001B9FE File Offset: 0x00019BFE
		protected bool SuperVerboseLogging { get; set; }

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000676 RID: 1654 RVA: 0x0001BA07 File Offset: 0x00019C07
		public UnityEvent PointerDownUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000677 RID: 1655 RVA: 0x0001BA0F File Offset: 0x00019C0F
		public UnityEvent PointerUpUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000678 RID: 1656 RVA: 0x0001BA17 File Offset: 0x00019C17
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

		// Token: 0x06000679 RID: 1657 RVA: 0x0001BA3C File Offset: 0x00019C3C
		protected override void Start()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Start", this);
			}
			if (this.triggerType == UIButton.TriggerTypes.LongPress)
			{
				this.longPressTweenCollection.SetInSeconds(this.longPressTime);
				this.longPressTweenCollection.OnAllTweenCompleted.AddListener(new UnityAction(this.longPressCompleteTweenCollection.Tween));
			}
			base.Start();
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x0001BAA0 File Offset: 0x00019CA0
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.VerboseLogging)
			{
				Debug.Log("Validate", this);
			}
			if (this.triggerType == UIButton.TriggerTypes.LongPress)
			{
				this.longPressTweenCollection.ValidateForNumberOfTweens(1);
			}
			if (!base.targetGraphic)
			{
				return;
			}
			if (base.targetGraphic.raycastTarget)
			{
				return;
			}
			DebugUtility.LogWarning(base.gameObject.name + "'s targetGraphic '" + base.targetGraphic.gameObject.name + "' at  must have raycastTarget enabled!", base.targetGraphic);
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x0001BB26 File Offset: 0x00019D26
		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (this.SuperVerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnPointerEnter", "eventData", eventData), this);
			}
			base.OnPointerEnter(eventData);
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x0001BB54 File Offset: 0x00019D54
		public override void OnPointerDown(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnPointerDown", "eventData", eventData), this);
			}
			if (this.triggerType == UIButton.TriggerTypes.LongPress)
			{
				this.waitForLongPressCoroutine = base.StartCoroutine(this.WaitForLongPress(eventData));
				return;
			}
			base.OnPointerDown(eventData);
			this.PointerDownUnityEvent.Invoke();
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x0001BBB3 File Offset: 0x00019DB3
		public override void OnSelect(BaseEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnSelect", "eventData", eventData), this);
			}
			base.OnSelect(eventData);
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x0001BBE0 File Offset: 0x00019DE0
		public override void OnPointerUp(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnPointerUp", "eventData", eventData), this);
			}
			if (this.triggerType == UIButton.TriggerTypes.LongPress)
			{
				if (this.isPointerDown)
				{
					this.longPressTweenCollection.Cancel();
					base.StopCoroutine(this.waitForLongPressCoroutine);
					this.waitForLongPressCoroutine = null;
					this.isPointerDown = false;
				}
				this.hideLongPressTweenCollection.Tween();
			}
			base.OnPointerUp(eventData);
			this.PointerUpUnityEvent.Invoke();
		}

		// Token: 0x0600067F RID: 1663 RVA: 0x0001BC63 File Offset: 0x00019E63
		public override void OnPointerClick(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnPointerUp", "eventData", eventData), this);
			}
			base.OnPointerClick(eventData);
			MonoBehaviourSingleton<UiAudioManager>.Instance.PlayUiAudio(this.soundType);
		}

		// Token: 0x06000680 RID: 1664 RVA: 0x0001BC9F File Offset: 0x00019E9F
		public override void OnPointerExit(PointerEventData eventData)
		{
			if (this.SuperVerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnPointerExit", "eventData", eventData), this);
			}
			base.OnPointerExit(eventData);
		}

		// Token: 0x06000681 RID: 1665 RVA: 0x0001BCCB File Offset: 0x00019ECB
		public override void OnDeselect(BaseEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnDeselect", "eventData", eventData), this);
			}
			base.OnDeselect(eventData);
		}

		// Token: 0x06000682 RID: 1666 RVA: 0x0001BCF7 File Offset: 0x00019EF7
		public override void OnSubmit(BaseEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnSubmit", "eventData", eventData), this);
			}
			base.OnSubmit(eventData);
		}

		// Token: 0x06000683 RID: 1667 RVA: 0x0001BD23 File Offset: 0x00019F23
		public override void OnMove(AxisEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnMove", "eventData", eventData), this);
			}
			base.OnMove(eventData);
		}

		// Token: 0x06000684 RID: 1668 RVA: 0x0001BD4F File Offset: 0x00019F4F
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnBeginDrag", "eventData", eventData), this);
			}
			if (!this.passDragEvents)
			{
				return;
			}
			this.targetBeginDragHandler.Interface.OnBeginDrag(eventData);
		}

		// Token: 0x06000685 RID: 1669 RVA: 0x0001BD8E File Offset: 0x00019F8E
		public void OnDrag(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnDrag", "eventData", eventData), this);
			}
			if (!this.passDragEvents)
			{
				return;
			}
			this.targetDragHandler.Interface.OnDrag(eventData);
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x0001BDCD File Offset: 0x00019FCD
		public void OnEndDrag(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnEndDrag", "eventData", eventData), this);
			}
			if (!this.passDragEvents)
			{
				return;
			}
			this.targetEndDragHandler.Interface.OnEndDrag(eventData);
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x0001BE0C File Offset: 0x0001A00C
		[ContextMenu("SetChildGraphicsToChildren")]
		public void SetChildGraphicsToChildren()
		{
			if (this.SuperVerboseLogging)
			{
				Debug.Log("SetChildGraphicsToChildren", this);
			}
			this.childGraphics = base.GetComponentsInChildren<Graphic>(true);
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x0001BE2E File Offset: 0x0001A02E
		protected override void InstantClearState()
		{
			if (this.SuperVerboseLogging)
			{
				Debug.Log("InstantClearState", this);
			}
			base.InstantClearState();
			if (base.transition == Selectable.Transition.ColorTint)
			{
				this.StartColorTween(Color.white, true);
			}
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x0001BE60 File Offset: 0x0001A060
		protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
		{
			if (this.SuperVerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "DoStateTransition", "state", state, "instant", instant }), this);
			}
			base.DoStateTransition(state, instant);
			if (base.transition != Selectable.Transition.ColorTint)
			{
				return;
			}
			Color color;
			switch (state)
			{
			case Selectable.SelectionState.Normal:
				color = base.colors.normalColor;
				break;
			case Selectable.SelectionState.Highlighted:
				color = base.colors.highlightedColor;
				break;
			case Selectable.SelectionState.Pressed:
				color = base.colors.pressedColor;
				break;
			case Selectable.SelectionState.Selected:
				color = base.colors.selectedColor;
				break;
			case Selectable.SelectionState.Disabled:
				color = base.colors.disabledColor;
				break;
			default:
				color = Color.black;
				break;
			}
			this.StartColorTween(color * base.colors.colorMultiplier, instant);
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x0001BF59 File Offset: 0x0001A159
		private IEnumerator WaitForLongPress(PointerEventData eventData)
		{
			if (this.VerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "WaitForLongPress", "eventData", eventData), this);
			}
			this.isPointerDown = true;
			this.longPressTweenCollection.Tween();
			yield return new WaitForSeconds(this.longPressTime);
			base.OnPointerDown(eventData);
			this.PointerDownUnityEvent.Invoke();
			this.isPointerDown = false;
			this.waitForLongPressCoroutine = null;
			yield break;
		}

		// Token: 0x0600068B RID: 1675 RVA: 0x0001BF70 File Offset: 0x0001A170
		private void StartColorTween(Color targetColor, bool instant)
		{
			if (this.SuperVerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "StartColorTween", "targetColor", targetColor, "instant", instant }), this);
			}
			float num = (instant ? 0f : base.colors.fadeDuration);
			Graphic[] array = this.childGraphics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CrossFadeColor(targetColor, num, true, true);
			}
		}

		// Token: 0x040003B8 RID: 952
		[Header("UIButton")]
		[SerializeField]
		private UIButton.TriggerTypes triggerType;

		// Token: 0x040003B9 RID: 953
		[SerializeField]
		private NormalUiSoundType soundType;

		// Token: 0x040003BA RID: 954
		[Header("LongPress")]
		[SerializeField]
		[Min(0.01f)]
		private float longPressTime = 1f;

		// Token: 0x040003BB RID: 955
		[SerializeField]
		private TweenCollection longPressTweenCollection;

		// Token: 0x040003BC RID: 956
		[SerializeField]
		private TweenCollection longPressCompleteTweenCollection;

		// Token: 0x040003BD RID: 957
		[SerializeField]
		private TweenCollection hideLongPressTweenCollection;

		// Token: 0x040003BE RID: 958
		[Header("childGraphics")]
		[SerializeField]
		private Graphic[] childGraphics = Array.Empty<Graphic>();

		// Token: 0x040003BF RID: 959
		[Header("Drag Intercept Handling")]
		[SerializeField]
		private bool passDragEvents;

		// Token: 0x040003C0 RID: 960
		[SerializeField]
		private InterfaceReference<IBeginDragHandler> targetBeginDragHandler;

		// Token: 0x040003C1 RID: 961
		[SerializeField]
		private InterfaceReference<IDragHandler> targetDragHandler;

		// Token: 0x040003C2 RID: 962
		[SerializeField]
		private InterfaceReference<IEndDragHandler> targetEndDragHandler;

		// Token: 0x040003C5 RID: 965
		private bool isPointerDown;

		// Token: 0x040003C6 RID: 966
		private RectTransform rectTransform;

		// Token: 0x040003C7 RID: 967
		private Coroutine waitForLongPressCoroutine;

		// Token: 0x0200010F RID: 271
		public enum TriggerTypes
		{
			// Token: 0x040003CB RID: 971
			Immediate,
			// Token: 0x040003CC RID: 972
			LongPress
		}
	}
}
