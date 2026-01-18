using System;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000131 RID: 305
	[RequireComponent(typeof(UIDragPositionController))]
	public class UIDragPositionResetter : UIGameObject
	{
		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000778 RID: 1912 RVA: 0x0001F6B2 File Offset: 0x0001D8B2
		public TweenPosition TweenPosition
		{
			get
			{
				if (!this.tweenPosition)
				{
					base.TryGetComponent<TweenPosition>(out this.tweenPosition);
				}
				if (!this.tweenPosition)
				{
					this.tweenPosition = base.gameObject.AddComponent<TweenPosition>();
				}
				return this.tweenPosition;
			}
		}

		// Token: 0x06000779 RID: 1913 RVA: 0x0001F6F4 File Offset: 0x0001D8F4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (!this.canvas)
			{
				this.canvas = base.GetComponentInParent<Canvas>();
			}
			if (!this.dragPositionController)
			{
				base.TryGetComponent<UIDragPositionController>(out this.dragPositionController);
			}
			this.dragPositionController.BeginDragUnityEvent.AddListener(new UnityAction(this.OnDragStarted));
			this.dragPositionController.DragEndedUnityEvent.AddListener(new UnityAction(this.OnDragEnded));
			this.TweenPosition.OnTweenComplete.AddListener(new UnityAction(this.OnTweenComplete));
		}

		// Token: 0x0600077A RID: 1914 RVA: 0x0001F7A0 File Offset: 0x0001D9A0
		public void SetInteractionBlockerSource(UIPooledInteractionBlocker newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractionBlockerSource", new object[] { newValue });
			}
			this.interactionBlockerSource = newValue;
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x0001F7C8 File Offset: 0x0001D9C8
		public void SetPositionBackToOriginalPosition()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetPositionBackToOriginalPosition", Array.Empty<object>());
			}
			if (this.parentUnderCanvasWhileDragging)
			{
				base.RectTransform.SetParent(this.originalParent);
			}
			base.RectTransform.position = this.originalPosition;
		}

		// Token: 0x0600077C RID: 1916 RVA: 0x0001F818 File Offset: 0x0001DA18
		public void TweenBackToOriginalPosition(Action onTweenCompleteTemp = null)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("TweenBackToOriginalPosition ( onTweenCompleteTemp: " + onTweenCompleteTemp.DebugIsNull() + " )", this);
			}
			if (this.parentUnderCanvasWhileDragging)
			{
				base.RectTransform.SetParent(this.originalParent);
			}
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIPooledInteractionBlocker uipooledInteractionBlocker = this.interactionBlockerSource;
			Transform transform = base.transform;
			this.spawnedInteractionBlocker = instance.Spawn<UIPooledInteractionBlocker>(uipooledInteractionBlocker, default(Vector3), default(Quaternion), transform);
			this.TweenPosition.To = this.originalPosition;
			this.TweenPosition.Tween(onTweenCompleteTemp);
		}

		// Token: 0x0600077D RID: 1917 RVA: 0x0001F8AE File Offset: 0x0001DAAE
		public void SetOriginalPosition(Vector3 value)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetOriginalPosition", new object[] { value });
			}
			this.originalPosition = value;
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x0001F8DC File Offset: 0x0001DADC
		public void OnDragStarted()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDragStarted", Array.Empty<object>());
			}
			this.originalPosition = base.RectTransform.position;
			if (!this.parentUnderCanvasWhileDragging)
			{
				return;
			}
			this.originalParent = base.RectTransform.parent;
			base.RectTransform.SetParent(this.canvas.transform);
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x0001F944 File Offset: 0x0001DB44
		private void OnDragEnded()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDragEnded", Array.Empty<object>());
			}
			if (!this.tweenBackToOriginalPositionOnDragEnded)
			{
				if (this.verboseLogging)
				{
					DebugUtility.LogMethodWithAppension(this, "OnDragEnded", "returned due to tweenBackToOriginalPositionOnDragEnded being false", Array.Empty<object>());
				}
				DebugUtility.LogMethodWithAppension(this, "OnDragEnded", "returned due to tweenBackToOriginalPositionOnDragEnded being false", Array.Empty<object>());
				return;
			}
			if (this.TweenPosition.IsTweening)
			{
				if (this.verboseLogging)
				{
					DebugUtility.LogMethodWithAppension(this, "OnDragEnded", "returned due to this already tweening", Array.Empty<object>());
				}
				return;
			}
			this.TweenBackToOriginalPosition(null);
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x0001F9D6 File Offset: 0x0001DBD6
		private void OnTweenComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnTweenComplete", Array.Empty<object>());
			}
			if (this.spawnedInteractionBlocker == null)
			{
				return;
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIPooledInteractionBlocker>(this.spawnedInteractionBlocker);
			this.spawnedInteractionBlocker = null;
		}

		// Token: 0x04000467 RID: 1127
		[SerializeField]
		private bool parentUnderCanvasWhileDragging;

		// Token: 0x04000468 RID: 1128
		[Tooltip("If left null, this will recursively search for one upward until found!")]
		[SerializeField]
		private Canvas canvas;

		// Token: 0x04000469 RID: 1129
		[SerializeField]
		private bool tweenBackToOriginalPositionOnDragEnded = true;

		// Token: 0x0400046A RID: 1130
		[SerializeField]
		private UIDragPositionController dragPositionController;

		// Token: 0x0400046B RID: 1131
		[SerializeField]
		private TweenPosition tweenPosition;

		// Token: 0x0400046C RID: 1132
		[SerializeField]
		private UIPooledInteractionBlocker interactionBlockerSource;

		// Token: 0x0400046D RID: 1133
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400046E RID: 1134
		private Transform originalParent;

		// Token: 0x0400046F RID: 1135
		private Vector3 originalPosition = Vector3.zero;

		// Token: 0x04000470 RID: 1136
		private UIPooledInteractionBlocker spawnedInteractionBlocker;
	}
}
