using System;
using System.Collections;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000259 RID: 601
	[RequireComponent(typeof(RectTransform))]
	public class UIScrollRect : ScrollRect
	{
		// Token: 0x1400004F RID: 79
		// (add) Token: 0x06000F29 RID: 3881 RVA: 0x00041614 File Offset: 0x0003F814
		// (remove) Token: 0x06000F2A RID: 3882 RVA: 0x0004164C File Offset: 0x0003F84C
		public event Action OnScrolled;

		// Token: 0x14000050 RID: 80
		// (add) Token: 0x06000F2B RID: 3883 RVA: 0x00041684 File Offset: 0x0003F884
		// (remove) Token: 0x06000F2C RID: 3884 RVA: 0x000416BC File Offset: 0x0003F8BC
		public event Action OnScrollStarted;

		// Token: 0x14000051 RID: 81
		// (add) Token: 0x06000F2D RID: 3885 RVA: 0x000416F4 File Offset: 0x0003F8F4
		// (remove) Token: 0x06000F2E RID: 3886 RVA: 0x0004172C File Offset: 0x0003F92C
		public event Action OnScrollEnded;

		// Token: 0x14000052 RID: 82
		// (add) Token: 0x06000F2F RID: 3887 RVA: 0x00041764 File Offset: 0x0003F964
		// (remove) Token: 0x06000F30 RID: 3888 RVA: 0x0004179C File Offset: 0x0003F99C
		public event Action OnScrolledToStart;

		// Token: 0x14000053 RID: 83
		// (add) Token: 0x06000F31 RID: 3889 RVA: 0x000417D4 File Offset: 0x0003F9D4
		// (remove) Token: 0x06000F32 RID: 3890 RVA: 0x0004180C File Offset: 0x0003FA0C
		public event Action OnScrolledToEnd;

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x06000F33 RID: 3891 RVA: 0x00041841 File Offset: 0x0003FA41
		// (set) Token: 0x06000F34 RID: 3892 RVA: 0x00041848 File Offset: 0x0003FA48
		public static float GlobalScrollSensitivity { get; set; }

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06000F35 RID: 3893 RVA: 0x00041850 File Offset: 0x0003FA50
		// (set) Token: 0x06000F36 RID: 3894 RVA: 0x00041858 File Offset: 0x0003FA58
		public Direction LastScrollDirection { get; private set; }

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06000F37 RID: 3895 RVA: 0x00041861 File Offset: 0x0003FA61
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

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06000F38 RID: 3896 RVA: 0x00041883 File Offset: 0x0003FA83
		// (set) Token: 0x06000F39 RID: 3897 RVA: 0x0004188B File Offset: 0x0003FA8B
		public bool IsScrolling { get; private set; }

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06000F3A RID: 3898 RVA: 0x00041894 File Offset: 0x0003FA94
		public bool IsAtStart
		{
			get
			{
				if (!base.vertical)
				{
					return base.horizontalNormalizedPosition <= 0.001f;
				}
				return base.verticalNormalizedPosition >= 1f;
			}
		}

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06000F3B RID: 3899 RVA: 0x000418BF File Offset: 0x0003FABF
		public bool IsAtEnd
		{
			get
			{
				if (!base.vertical)
				{
					return base.horizontalNormalizedPosition >= 1f;
				}
				return base.verticalNormalizedPosition <= 0.001f;
			}
		}

		// Token: 0x06000F3C RID: 3900 RVA: 0x000418EC File Offset: 0x0003FAEC
		protected override void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.Start();
			this.isMobile = MobileUtility.IsMobile;
			base.movementType = (this.isMobile ? this.mobileMovementType : this.desktopMovementType);
			this.upwardScrollRect = base.transform.parent.GetComponentInParent<UIScrollRect>(true);
			this.hasUpwardScrollRect = this.upwardScrollRect != null;
		}

		// Token: 0x06000F3D RID: 3901 RVA: 0x00041967 File Offset: 0x0003FB67
		protected override void OnEnable()
		{
			base.OnEnable();
			this.checkIfScrollEndedCoroutine = null;
			this.isSmoothScrolling = false;
			this.IsScrolling = false;
		}

		// Token: 0x06000F3E RID: 3902 RVA: 0x00041984 File Offset: 0x0003FB84
		protected override void OnDisable()
		{
			base.OnDisable();
			if (this.checkIfScrollEndedCoroutine != null)
			{
				base.StopCoroutine(this.checkIfScrollEndedCoroutine);
				this.checkIfScrollEndedCoroutine = null;
			}
			ValueTween.CancelAndNull(ref this.smoothScrollTween);
			this.isSmoothScrolling = false;
			this.IsScrolling = false;
		}

		// Token: 0x06000F3F RID: 3903 RVA: 0x000419C0 File Offset: 0x0003FBC0
		protected override void LateUpdate()
		{
			base.LateUpdate();
			if (base.velocity != Vector2.zero)
			{
				if (base.vertical)
				{
					this.LastScrollDirection = ((base.velocity.y > 0f) ? Direction.Forward : Direction.Backward);
				}
				else if (base.horizontal)
				{
					this.LastScrollDirection = ((base.velocity.x > 0f) ? Direction.Forward : Direction.Backward);
				}
			}
			if (Mathf.Abs(base.velocity.x) < 25f && Mathf.Abs(base.velocity.y) < 25f)
			{
				base.velocity = Vector2.zero;
			}
		}

		// Token: 0x06000F40 RID: 3904 RVA: 0x00041A68 File Offset: 0x0003FC68
		public override void OnScroll(PointerEventData data)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScroll", new object[] { data.delta });
			}
			bool isAtStart = this.IsAtStart;
			bool isAtEnd = this.IsAtEnd;
			if (this.SendToUpwardScrollRect((data.scrollDelta.y > 0f) ? UIScrollRect.Directions.Up : UIScrollRect.Directions.Down))
			{
				this.upwardScrollRect.OnScroll(data);
				return;
			}
			Vector2 normalizedPosition = base.normalizedPosition;
			if (this.smooth)
			{
				this.isSmoothScrolling = false;
				ValueTween.CancelAndNull(ref this.smoothScrollTween);
			}
			float globalScrollSensitivity = UIScrollRect.GlobalScrollSensitivity;
			if (this.useGlobalScrollSensitivity && !Mathf.Approximately(globalScrollSensitivity, 1f))
			{
				PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
				{
					scrollDelta = data.scrollDelta * globalScrollSensitivity
				};
				base.OnScroll(pointerEventData);
			}
			else
			{
				base.OnScroll(data);
			}
			if (this.smooth && MonoBehaviourSingleton<TweenManager>.Instance != null)
			{
				this.targetNormalizedPosition = base.normalizedPosition;
				base.normalizedPosition = normalizedPosition;
				this.isSmoothScrolling = true;
				this.smoothScrollTween = MonoBehaviourSingleton<TweenManager>.Instance.TweenValue(this.smoothScrollInSeconds, new Action<float>(this.TweenToTargetNormalizedPosition), null, TweenTimeMode.Unscaled, null);
			}
			Action onScrolled = this.OnScrolled;
			if (onScrolled != null)
			{
				onScrolled();
			}
			if (!isAtStart && this.IsAtStart)
			{
				Action onScrolledToStart = this.OnScrolledToStart;
				if (onScrolledToStart != null)
				{
					onScrolledToStart();
				}
			}
			if (!isAtEnd && this.IsAtEnd)
			{
				Action onScrolledToEnd = this.OnScrolledToEnd;
				if (onScrolledToEnd != null)
				{
					onScrolledToEnd();
				}
			}
			if (this.checkIfScrollEndedCoroutine != null)
			{
				base.StopCoroutine(this.checkIfScrollEndedCoroutine);
				this.checkIfScrollEndedCoroutine = null;
			}
			if (base.gameObject.activeInHierarchy)
			{
				this.checkIfScrollEndedCoroutine = base.StartCoroutine(this.CheckIfScrollEnded());
			}
		}

		// Token: 0x06000F41 RID: 3905 RVA: 0x00041C1E File Offset: 0x0003FE1E
		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInitializePotentialDrag", new object[] { eventData.delta });
			}
			if (!this.interactable)
			{
				return;
			}
			base.OnInitializePotentialDrag(eventData);
		}

		// Token: 0x06000F42 RID: 3906 RVA: 0x00041C58 File Offset: 0x0003FE58
		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBeginDrag", new object[] { eventData.delta });
			}
			if (base.vertical)
			{
				if (this.SendToUpwardScrollRect((eventData.delta.y < 0f) ? UIScrollRect.Directions.Up : UIScrollRect.Directions.Down))
				{
					this.RouteOnBeginDrag(eventData);
					return;
				}
			}
			else if (this.SendToUpwardScrollRect((eventData.delta.x < 0f) ? UIScrollRect.Directions.Right : UIScrollRect.Directions.Left))
			{
				this.RouteOnBeginDrag(eventData);
				return;
			}
			base.OnBeginDrag(eventData);
			Action onScrollStarted = this.OnScrollStarted;
			if (onScrollStarted == null)
			{
				return;
			}
			onScrollStarted();
		}

		// Token: 0x06000F43 RID: 3907 RVA: 0x00041D00 File Offset: 0x0003FF00
		public override void OnDrag(PointerEventData eventData)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDrag", new object[] { eventData.delta });
			}
			if (!this.interactable || !this.isMobile)
			{
				return;
			}
			if (base.vertical)
			{
				if (this.SendToUpwardScrollRect((eventData.delta.y < 0f) ? UIScrollRect.Directions.Up : UIScrollRect.Directions.Down))
				{
					this.RouteOnDrag(eventData);
					return;
				}
			}
			else if (this.SendToUpwardScrollRect((eventData.delta.x < 0f) ? UIScrollRect.Directions.Right : UIScrollRect.Directions.Left))
			{
				this.RouteOnDrag(eventData);
				return;
			}
			base.OnDrag(eventData);
		}

		// Token: 0x06000F44 RID: 3908 RVA: 0x00041DA8 File Offset: 0x0003FFA8
		public override void OnEndDrag(PointerEventData eventData)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEndDrag", new object[] { eventData.delta });
			}
			if (!this.interactable || !this.isMobile)
			{
				return;
			}
			if (base.vertical)
			{
				if (this.SendToUpwardScrollRect((eventData.delta.y < 0f) ? UIScrollRect.Directions.Up : UIScrollRect.Directions.Down))
				{
					this.upwardScrollRect.OnEndDrag(eventData);
				}
			}
			else if (this.SendToUpwardScrollRect((eventData.delta.x < 0f) ? UIScrollRect.Directions.Right : UIScrollRect.Directions.Left))
			{
				this.upwardScrollRect.OnEndDrag(eventData);
			}
			base.OnEndDrag(eventData);
			this.upwardScrollRectDragInitialized = false;
			Action onScrollEnded = this.OnScrollEnded;
			if (onScrollEnded == null)
			{
				return;
			}
			onScrollEnded();
		}

		// Token: 0x06000F45 RID: 3909 RVA: 0x00041E74 File Offset: 0x00040074
		public void ResetPosition()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ResetPosition", Array.Empty<object>());
			}
			Vector2 vector = (base.vertical ? Vector2.up : Vector2.left);
			if (base.normalizedPosition == vector)
			{
				return;
			}
			base.normalizedPosition = vector;
		}

		// Token: 0x06000F46 RID: 3910 RVA: 0x00041EC4 File Offset: 0x000400C4
		private void TweenToTargetNormalizedPosition(float delta)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "TweenToTargetNormalizedPosition", new object[] { delta });
			}
			if (!this.isSmoothScrolling || !this || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			Vector2 vector = Vector2.Lerp(base.normalizedPosition, this.targetNormalizedPosition, delta);
			base.normalizedPosition = vector;
		}

		// Token: 0x06000F47 RID: 3911 RVA: 0x00041F2B File Offset: 0x0004012B
		private IEnumerator CheckIfScrollEnded()
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "CheckIfScrollEnded", Array.Empty<object>());
			}
			if (!this.IsScrolling)
			{
				Action onScrollStarted = this.OnScrollStarted;
				if (onScrollStarted != null)
				{
					onScrollStarted();
				}
			}
			this.IsScrolling = true;
			while (base.velocity != Vector2.zero)
			{
				yield return null;
			}
			this.IsScrolling = false;
			this.checkIfScrollEndedCoroutine = null;
			Action onScrollEnded = this.OnScrollEnded;
			if (onScrollEnded != null)
			{
				onScrollEnded();
			}
			yield break;
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x00041F3A File Offset: 0x0004013A
		private void RouteOnBeginDrag(PointerEventData eventData)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBeginDrag", new object[] { eventData.delta });
			}
			this.upwardScrollRect.OnBeginDrag(eventData);
			this.upwardScrollRectDragInitialized = true;
		}

		// Token: 0x06000F49 RID: 3913 RVA: 0x00041F78 File Offset: 0x00040178
		private void RouteOnDrag(PointerEventData eventData)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "RouteOnDrag", new object[] { eventData.delta });
			}
			if (!this.upwardScrollRectDragInitialized)
			{
				this.upwardScrollRect.OnBeginDrag(eventData);
				this.upwardScrollRectDragInitialized = true;
			}
			this.upwardScrollRect.OnDrag(eventData);
		}

		// Token: 0x06000F4A RID: 3914 RVA: 0x00041FD4 File Offset: 0x000401D4
		private bool SendToUpwardScrollRect(UIScrollRect.Directions scrollDirection)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "SendToUpwardScrollRect", new object[] { scrollDirection });
			}
			bool flag = false;
			if (!this.hasUpwardScrollRect || !this.sendEventsUpward)
			{
				return flag;
			}
			switch (scrollDirection)
			{
			case UIScrollRect.Directions.Up:
				flag = this.IsAtStart;
				break;
			case UIScrollRect.Directions.Down:
				flag = this.IsAtEnd;
				break;
			case UIScrollRect.Directions.Left:
				flag = this.IsAtStart;
				break;
			case UIScrollRect.Directions.Right:
				flag = this.IsAtEnd;
				break;
			default:
				DebugUtility.LogError(this, "SendToUpwardScrollRect", "No support for a scrollDirection of this value!", new object[] { scrollDirection });
				break;
			}
			return flag;
		}

		// Token: 0x040009A2 RID: 2466
		private const float VELOCITY_CLAMP = 25f;

		// Token: 0x040009A8 RID: 2472
		[Header("UIScrollRect")]
		[SerializeField]
		private bool interactable = true;

		// Token: 0x040009A9 RID: 2473
		[SerializeField]
		private bool sendEventsUpward;

		// Token: 0x040009AA RID: 2474
		[SerializeField]
		private bool smooth = true;

		// Token: 0x040009AB RID: 2475
		[SerializeField]
		private float smoothScrollInSeconds = 0.08f;

		// Token: 0x040009AC RID: 2476
		[SerializeField]
		private bool useGlobalScrollSensitivity = true;

		// Token: 0x040009AD RID: 2477
		[Header("MovementType")]
		[SerializeField]
		private ScrollRect.MovementType desktopMovementType = ScrollRect.MovementType.Clamped;

		// Token: 0x040009AE RID: 2478
		[SerializeField]
		private ScrollRect.MovementType mobileMovementType = ScrollRect.MovementType.Clamped;

		// Token: 0x040009AF RID: 2479
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040009B0 RID: 2480
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x040009B1 RID: 2481
		private RectTransform rectTransform;

		// Token: 0x040009B2 RID: 2482
		private bool isMobile;

		// Token: 0x040009B3 RID: 2483
		private bool hasUpwardScrollRect;

		// Token: 0x040009B4 RID: 2484
		private UIScrollRect upwardScrollRect;

		// Token: 0x040009B5 RID: 2485
		private bool upwardScrollRectDragInitialized;

		// Token: 0x040009B6 RID: 2486
		private Coroutine checkIfScrollEndedCoroutine;

		// Token: 0x040009B7 RID: 2487
		private bool isSmoothScrolling;

		// Token: 0x040009B8 RID: 2488
		private Vector2 targetNormalizedPosition;

		// Token: 0x040009B9 RID: 2489
		private ValueTween smoothScrollTween;

		// Token: 0x0200025A RID: 602
		private enum Directions
		{
			// Token: 0x040009BE RID: 2494
			Up,
			// Token: 0x040009BF RID: 2495
			Down,
			// Token: 0x040009C0 RID: 2496
			Left,
			// Token: 0x040009C1 RID: 2497
			Right
		}
	}
}
