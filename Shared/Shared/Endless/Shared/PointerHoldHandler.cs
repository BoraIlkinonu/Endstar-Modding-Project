using System;
using System.Collections;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared
{
	// Token: 0x02000056 RID: 86
	[RequireComponent(typeof(PointerDownAndUpHandler))]
	public class PointerHoldHandler : MonoBehaviour
	{
		// Token: 0x17000065 RID: 101
		// (get) Token: 0x060002B7 RID: 695 RVA: 0x0000DE02 File Offset: 0x0000C002
		// (set) Token: 0x060002B8 RID: 696 RVA: 0x0000DE0A File Offset: 0x0000C00A
		[Tooltip("In seconds")]
		public float HoldDuration { get; private set; } = 1f;

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x060002B9 RID: 697 RVA: 0x0000DE13 File Offset: 0x0000C013
		// (set) Token: 0x060002BA RID: 698 RVA: 0x0000DE1B File Offset: 0x0000C01B
		protected bool VerboseLogging { get; set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x060002BB RID: 699 RVA: 0x0000DE24 File Offset: 0x0000C024
		public bool IsTrackingHold
		{
			get
			{
				return this.checkForHoldCoroutine != null;
			}
		}

		// Token: 0x060002BC RID: 700 RVA: 0x0000DE30 File Offset: 0x0000C030
		private void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			if (this.HoldDuration <= 0f)
			{
				this.HoldDuration = 0.25f;
			}
			base.TryGetComponent<PointerDownAndUpHandler>(out this.pointerUpAndDownHandler);
			this.pointerUpAndDownHandler.PointerDownUnityEvent.AddListener(new UnityAction(this.StartHoldTracking));
			this.pointerUpAndDownHandler.PointerUpUnityEvent.AddListener(new UnityAction(this.OnPointerUp));
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000DEB4 File Offset: 0x0000C0B4
		public void CancelHoldTracking()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelHoldTracking", Array.Empty<object>());
			}
			if (this.isHeld)
			{
				this.HoldEndUnityEvent.Invoke();
				this.isHeld = false;
			}
			if (this.checkForHoldCoroutine != null)
			{
				base.StopCoroutine(this.checkForHoldCoroutine);
				this.checkForHoldCoroutine = null;
			}
			this.BeginHoldTrackingTweenCollection.Cancel();
			this.HoldStartTweenCollection.Cancel();
			this.HoldEndOrCanceledTweenCollection.Tween();
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000DF30 File Offset: 0x0000C130
		private void StartHoldTracking()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "StartHoldTracking", Array.Empty<object>());
			}
			this.checkForHoldCoroutine = base.StartCoroutine(this.CheckForHold());
			this.BeginHoldTrackingUnityEvent.Invoke();
			this.BeginHoldTrackingTweenCollection.Tween();
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000DF7D File Offset: 0x0000C17D
		private IEnumerator CheckForHold()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CheckForHold", Array.Empty<object>());
			}
			yield return new WaitForSecondsRealtime(this.HoldDuration);
			this.isHeld = true;
			this.HoldStartUnityEvent.Invoke();
			this.HoldStartTweenCollection.Tween();
			yield break;
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000DF8C File Offset: 0x0000C18C
		private void OnPointerUp()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", Array.Empty<object>());
			}
			this.CancelHoldTracking();
		}

		// Token: 0x0400016A RID: 362
		public UnityEvent BeginHoldTrackingUnityEvent = new UnityEvent();

		// Token: 0x0400016B RID: 363
		public UnityEvent HoldStartUnityEvent = new UnityEvent();

		// Token: 0x0400016C RID: 364
		public UnityEvent HoldEndUnityEvent = new UnityEvent();

		// Token: 0x0400016D RID: 365
		public TweenCollection BeginHoldTrackingTweenCollection;

		// Token: 0x0400016E RID: 366
		public TweenCollection HoldStartTweenCollection;

		// Token: 0x0400016F RID: 367
		public TweenCollection HoldEndOrCanceledTweenCollection;

		// Token: 0x04000172 RID: 370
		private PointerDownAndUpHandler pointerUpAndDownHandler;

		// Token: 0x04000173 RID: 371
		private Coroutine checkForHoldCoroutine;

		// Token: 0x04000174 RID: 372
		private bool isHeld;
	}
}
