using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003D8 RID: 984
	public class UIOnScreenStickHandler : UIGameObject, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
	{
		// Token: 0x17000518 RID: 1304
		// (get) Token: 0x060018DD RID: 6365 RVA: 0x00073878 File Offset: 0x00071A78
		public UnityEvent OnPointerDownUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000519 RID: 1305
		// (get) Token: 0x060018DE RID: 6366 RVA: 0x00073880 File Offset: 0x00071A80
		public UnityEvent OnPointerUpUnityEvent { get; } = new UnityEvent();

		// Token: 0x060018DF RID: 6367 RVA: 0x00073888 File Offset: 0x00071A88
		public void SetMovementRange(float movementRange)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetMovementRange", new object[] { movementRange });
			}
			this.onScreenStick.movementRange = movementRange;
		}

		// Token: 0x060018E0 RID: 6368 RVA: 0x000738B8 File Offset: 0x00071AB8
		public void OnPointerDown(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerDown", new object[] { pointerEventData });
			}
			this.ScalePointerEventData(pointerEventData);
			this.onScreenStick.OnPointerDown(pointerEventData);
			this.OnPointerDownUnityEvent.Invoke();
		}

		// Token: 0x060018E1 RID: 6369 RVA: 0x000738F5 File Offset: 0x00071AF5
		public void OnPointerUp(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", new object[] { pointerEventData });
			}
			this.onScreenStick.OnPointerUp(pointerEventData);
			this.OnPointerUpUnityEvent.Invoke();
		}

		// Token: 0x060018E2 RID: 6370 RVA: 0x0007392B File Offset: 0x00071B2B
		public void OnDrag(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDrag", new object[] { pointerEventData });
			}
			this.ScalePointerEventData(pointerEventData);
			this.onScreenStick.OnDrag(pointerEventData);
		}

		// Token: 0x060018E3 RID: 6371 RVA: 0x0007395D File Offset: 0x00071B5D
		public void SetInteractable(bool interactable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.graphic.raycastTarget = interactable;
			this.selectable.interactable = interactable;
		}

		// Token: 0x060018E4 RID: 6372 RVA: 0x00073999 File Offset: 0x00071B99
		private void ScalePointerEventData(PointerEventData pointerEventData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ScalePointerEventData", new object[] { pointerEventData });
			}
			pointerEventData.position *= this.sensitivity;
		}

		// Token: 0x040013FE RID: 5118
		[SerializeField]
		private Graphic graphic;

		// Token: 0x040013FF RID: 5119
		[SerializeField]
		private Selectable selectable;

		// Token: 0x04001400 RID: 5120
		[SerializeField]
		private OnScreenStick onScreenStick;

		// Token: 0x04001401 RID: 5121
		[Header("Sensitivity")]
		[SerializeField]
		private float sensitivity = 1f;

		// Token: 0x04001402 RID: 5122
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
