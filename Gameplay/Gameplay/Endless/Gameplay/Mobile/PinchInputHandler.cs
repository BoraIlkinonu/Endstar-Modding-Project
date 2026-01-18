using System;
using System.Collections.Generic;
using Endless.Data;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Endless.Gameplay.Mobile
{
	// Token: 0x02000441 RID: 1089
	public class PinchInputHandler : MonoBehaviour
	{
		// Token: 0x14000041 RID: 65
		// (add) Token: 0x06001B3E RID: 6974 RVA: 0x0007BD38 File Offset: 0x00079F38
		// (remove) Token: 0x06001B3F RID: 6975 RVA: 0x0007BD6C File Offset: 0x00079F6C
		public static event Action OnFirstPinchStarted;

		// Token: 0x14000042 RID: 66
		// (add) Token: 0x06001B40 RID: 6976 RVA: 0x0007BDA0 File Offset: 0x00079FA0
		// (remove) Token: 0x06001B41 RID: 6977 RVA: 0x0007BDD4 File Offset: 0x00079FD4
		public static event Action OnLastPinchEnded;

		// Token: 0x17000581 RID: 1409
		// (get) Token: 0x06001B42 RID: 6978 RVA: 0x0007BE07 File Offset: 0x0007A007
		// (set) Token: 0x06001B43 RID: 6979 RVA: 0x0007BE0E File Offset: 0x0007A00E
		public static bool IsAnyInstancePinching { get; private set; }

		// Token: 0x17000582 RID: 1410
		// (get) Token: 0x06001B44 RID: 6980 RVA: 0x0007BE16 File Offset: 0x0007A016
		public UnityEvent<float> OnPinchUnityEvent { get; } = new UnityEvent<float>();

		// Token: 0x17000583 RID: 1411
		// (get) Token: 0x06001B45 RID: 6981 RVA: 0x0007BE1E File Offset: 0x0007A01E
		// (set) Token: 0x06001B46 RID: 6982 RVA: 0x0007BE28 File Offset: 0x0007A028
		public bool IsPinching
		{
			get
			{
				return this.isPinching;
			}
			private set
			{
				if (this.isPinching != value)
				{
					this.isPinching = value;
					PinchInputHandler.UpdateStaticPinchingState();
					if (this.isPinching && this.PinchCount == 1)
					{
						Action onFirstPinchStarted = PinchInputHandler.OnFirstPinchStarted;
						if (onFirstPinchStarted == null)
						{
							return;
						}
						onFirstPinchStarted();
						return;
					}
					else if (!this.isPinching && this.PinchCount == 0)
					{
						Action onLastPinchEnded = PinchInputHandler.OnLastPinchEnded;
						if (onLastPinchEnded == null)
						{
							return;
						}
						onLastPinchEnded();
					}
				}
			}
		}

		// Token: 0x06001B47 RID: 6983 RVA: 0x0007BE8A File Offset: 0x0007A08A
		private void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			PinchInputHandler.instances.Add(this);
			if (!MobileUtility.IsMobile)
			{
				base.enabled = false;
				return;
			}
			if (!EnhancedTouchSupport.enabled)
			{
				EnhancedTouchSupport.Enable();
			}
		}

		// Token: 0x06001B48 RID: 6984 RVA: 0x0007BECC File Offset: 0x0007A0CC
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!this.playerInputActionsCreated)
			{
				this.playerInputActions = new PlayerInputActions();
				this.playerInputActionsCreated = true;
			}
			this.playerInputActions.Player.Pinch.started += this.OnPinch;
			this.playerInputActions.Player.Pinch.performed += this.OnPinch;
			this.playerInputActions.Player.Pinch.canceled += this.OnPinch;
			this.playerInputActions.Player.Pinch.Enable();
		}

		// Token: 0x06001B49 RID: 6985 RVA: 0x0007BF90 File Offset: 0x0007A190
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.IsPinching = false;
			if (!this.playerInputActionsCreated)
			{
				return;
			}
			this.playerInputActions.Player.Pinch.started -= this.OnPinch;
			this.playerInputActions.Player.Pinch.performed -= this.OnPinch;
			this.playerInputActions.Player.Pinch.canceled -= this.OnPinch;
			this.playerInputActions.Player.Pinch.Disable();
		}

		// Token: 0x06001B4A RID: 6986 RVA: 0x0007C049 File Offset: 0x0007A249
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			PinchInputHandler.instances.Remove(this);
			PinchInputHandler.UpdateStaticPinchingState();
		}

		// Token: 0x06001B4B RID: 6987 RVA: 0x0007C074 File Offset: 0x0007A274
		private static void UpdateStaticPinchingState()
		{
			bool flag = false;
			for (int i = 0; i < PinchInputHandler.instances.Count; i++)
			{
				if (PinchInputHandler.instances[i].isPinching)
				{
					flag = true;
					break;
				}
			}
			PinchInputHandler.IsAnyInstancePinching = flag;
		}

		// Token: 0x17000584 RID: 1412
		// (get) Token: 0x06001B4C RID: 6988 RVA: 0x0007C0B4 File Offset: 0x0007A2B4
		private int PinchCount
		{
			get
			{
				int num = 0;
				using (List<PinchInputHandler>.Enumerator enumerator = PinchInputHandler.instances.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.isPinching)
						{
							num++;
						}
					}
				}
				return num;
			}
		}

		// Token: 0x06001B4D RID: 6989 RVA: 0x0007C10C File Offset: 0x0007A30C
		private void OnPinch(InputAction.CallbackContext callbackContext)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPinch", new object[] { callbackContext });
			}
			if (callbackContext.canceled)
			{
				this.IsPinching = false;
				return;
			}
			if (Touch.activeTouches.Count < 2)
			{
				this.IsPinching = false;
				return;
			}
			Touch touch = Touch.activeTouches[0];
			Touch touch2 = Touch.activeTouches[1];
			if (touch.history.Count < 1 || touch2.history.Count < 1)
			{
				this.IsPinching = false;
				return;
			}
			float num = Vector2.Distance(touch.screenPosition, touch2.screenPosition);
			float num2 = Vector2.Distance(touch.history[0].screenPosition, touch2.history[0].screenPosition);
			float num3 = (num - num2) * this.pinchSpeed;
			this.OnPinchUnityEvent.Invoke(num3);
			this.IsPinching = true;
		}

		// Token: 0x040015A0 RID: 5536
		[SerializeField]
		private float pinchSpeed = 0.00075f;

		// Token: 0x040015A1 RID: 5537
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040015A2 RID: 5538
		private bool playerInputActionsCreated;

		// Token: 0x040015A3 RID: 5539
		private PlayerInputActions playerInputActions;

		// Token: 0x040015A4 RID: 5540
		private static readonly List<PinchInputHandler> instances = new List<PinchInputHandler>();

		// Token: 0x040015A6 RID: 5542
		private bool isPinching;
	}
}
