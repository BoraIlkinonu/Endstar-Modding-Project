using System;
using Endless.Gameplay.Mobile;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003D9 RID: 985
	public class UIOnScreenStickZone : UIGameObject, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
	{
		// Token: 0x060018E6 RID: 6374 RVA: 0x000739F8 File Offset: 0x00071BF8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.onScreenStickDisplayAndHideHandler.SetToHideEnd(false);
			if (this.blockInputIfPinchInputHandlerIsPinching)
			{
				PinchInputHandler.OnFirstPinchStarted += this.Cancel;
			}
		}

		// Token: 0x060018E7 RID: 6375 RVA: 0x00073A37 File Offset: 0x00071C37
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			if (this.blockInputIfPinchInputHandlerIsPinching)
			{
				PinchInputHandler.OnFirstPinchStarted -= this.Cancel;
			}
		}

		// Token: 0x060018E8 RID: 6376 RVA: 0x00073A6C File Offset: 0x00071C6C
		public void OnPointerDown(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerDown", new object[] { pointerEventData });
			}
			if (this.blockInputIfPinchInputHandlerIsPinching && PinchInputHandler.IsAnyInstancePinching)
			{
				return;
			}
			if (this.activePointerId != null)
			{
				return;
			}
			this.activePointerId = new int?(pointerEventData.pointerId);
			this.lastPosition = pointerEventData.position;
			this.onScreenStickTransform.position = pointerEventData.position;
			this.onScreenStickDisplayAndHideHandler.Display();
			this.onScreenStick.OnPointerDown(pointerEventData);
		}

		// Token: 0x060018E9 RID: 6377 RVA: 0x00073B00 File Offset: 0x00071D00
		public void OnDrag(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDrag", new object[] { pointerEventData });
			}
			if (this.blockInputIfPinchInputHandlerIsPinching && PinchInputHandler.IsAnyInstancePinching)
			{
				return;
			}
			int? num = this.activePointerId;
			int pointerId = pointerEventData.pointerId;
			if (!((num.GetValueOrDefault() == pointerId) & (num != null)))
			{
				return;
			}
			this.lastPosition = pointerEventData.position;
			this.onScreenStick.OnDrag(pointerEventData);
		}

		// Token: 0x060018EA RID: 6378 RVA: 0x00073B78 File Offset: 0x00071D78
		public void OnPointerUp(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", new object[] { pointerEventData });
			}
			if (this.blockInputIfPinchInputHandlerIsPinching && PinchInputHandler.IsAnyInstancePinching)
			{
				return;
			}
			int? num = this.activePointerId;
			int pointerId = pointerEventData.pointerId;
			if (!((num.GetValueOrDefault() == pointerId) & (num != null)))
			{
				return;
			}
			this.onScreenStick.OnPointerUp(pointerEventData);
			this.onScreenStickDisplayAndHideHandler.Hide();
			this.activePointerId = null;
		}

		// Token: 0x060018EB RID: 6379 RVA: 0x00073BF8 File Offset: 0x00071DF8
		private void Cancel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Cancel", Array.Empty<object>());
			}
			if (this.activePointerId == null)
			{
				return;
			}
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
			{
				pointerId = this.activePointerId.Value,
				position = this.lastPosition,
				button = PointerEventData.InputButton.Left
			};
			this.onScreenStick.OnPointerUp(pointerEventData);
			this.onScreenStickDisplayAndHideHandler.Hide();
			this.activePointerId = null;
		}

		// Token: 0x04001405 RID: 5125
		[SerializeField]
		private Transform onScreenStickTransform;

		// Token: 0x04001406 RID: 5126
		[SerializeField]
		private UIDisplayAndHideHandler onScreenStickDisplayAndHideHandler;

		// Token: 0x04001407 RID: 5127
		[SerializeField]
		private OnScreenStick onScreenStick;

		// Token: 0x04001408 RID: 5128
		[SerializeField]
		private bool blockInputIfPinchInputHandlerIsPinching;

		// Token: 0x04001409 RID: 5129
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400140A RID: 5130
		private int? activePointerId;

		// Token: 0x0400140B RID: 5131
		private Vector2 lastPosition;
	}
}
