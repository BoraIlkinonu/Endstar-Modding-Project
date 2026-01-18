using System;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Endless.Shared
{
	// Token: 0x02000068 RID: 104
	public class InputManager : MonoBehaviourSingleton<InputManager>
	{
		// Token: 0x17000089 RID: 137
		// (get) Token: 0x0600034E RID: 846 RVA: 0x0000FB04 File Offset: 0x0000DD04
		public static bool InputUnrestricted
		{
			get
			{
				return MonoBehaviourSingleton<InputManager>.Instance != null && MonoBehaviourSingleton<InputManager>.Instance.currentInputState == InputManager.InputState.Free && !MonoBehaviourSingleton<InputManager>.Instance.screenIsOpen;
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x0600034F RID: 847 RVA: 0x0000FB2F File Offset: 0x0000DD2F
		public static InputManager.InputState CurrentInputState
		{
			get
			{
				if (!(MonoBehaviourSingleton<InputManager>.Instance != null))
				{
					return InputManager.InputState.Free;
				}
				return MonoBehaviourSingleton<InputManager>.Instance.currentInputState;
			}
		}

		// Token: 0x06000350 RID: 848 RVA: 0x0000FB4A File Offset: 0x0000DD4A
		protected override void Awake()
		{
			base.Awake();
			this.endlessSharedInputActions = new EndlessSharedInputActions();
		}

		// Token: 0x06000351 RID: 849 RVA: 0x0000FB60 File Offset: 0x0000DD60
		private void OnEnable()
		{
			this.endlessSharedInputActions.UI.SelectNextUI.performed += this.SelectNextUI;
			this.endlessSharedInputActions.UI.SelectNextUI.Enable();
		}

		// Token: 0x06000352 RID: 850 RVA: 0x0000FBAC File Offset: 0x0000DDAC
		private void OnDisable()
		{
			this.endlessSharedInputActions.UI.SelectNextUI.performed -= this.SelectNextUI;
			this.endlessSharedInputActions.UI.SelectNextUI.Disable();
		}

		// Token: 0x06000353 RID: 851 RVA: 0x0000FBF8 File Offset: 0x0000DDF8
		private void SelectNextUI(InputAction.CallbackContext callbackContext)
		{
			EventSystem current = EventSystem.current;
			if (!current.currentSelectedGameObject)
			{
				return;
			}
			if (this.currentInputField && !this.currentInputField.CanSelectNextUiOnTab)
			{
				return;
			}
			Selectable selectable;
			if (current.currentSelectedGameObject.TryGetComponent<Selectable>(out selectable))
			{
				Selectable selectable2 = selectable.FindSelectableOnDown();
				if (selectable2)
				{
					selectable2.Select();
				}
			}
		}

		// Token: 0x06000354 RID: 852 RVA: 0x0000FC58 File Offset: 0x0000DE58
		private void Start()
		{
			if (MonoBehaviourSingleton<UIScreenManager>.Instance)
			{
				UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.OnScreenDisplay));
				UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.OnClosingScreen));
				return;
			}
			Debug.LogWarning("Could not find UIScreenManager!", this);
		}

		// Token: 0x06000355 RID: 853 RVA: 0x0000FCBD File Offset: 0x0000DEBD
		public void ReleaseInputField(UIInputField inputField)
		{
			if (this.currentInputField == inputField)
			{
				this.currentInputField = null;
				this.UpdateInputState();
			}
		}

		// Token: 0x06000356 RID: 854 RVA: 0x0000FCDA File Offset: 0x0000DEDA
		public void SetInputField(UIInputField inputField)
		{
			if (this.currentInputField != inputField && inputField != null)
			{
				this.currentInputField = inputField;
				this.UpdateInputState();
			}
		}

		// Token: 0x06000357 RID: 855 RVA: 0x0000FD00 File Offset: 0x0000DF00
		public void SetCinematicInputState(bool blockInput)
		{
			this.cinematicBlockingInput = blockInput;
			this.UpdateInputState();
		}

		// Token: 0x06000358 RID: 856 RVA: 0x0000FD0F File Offset: 0x0000DF0F
		private void UpdateInputState()
		{
			if (this.currentInputField != null)
			{
				this.currentInputState = InputManager.InputState.InputField;
				return;
			}
			if (this.cinematicBlockingInput)
			{
				this.currentInputState = InputManager.InputState.Cinematic;
				return;
			}
			this.currentInputState = InputManager.InputState.Free;
		}

		// Token: 0x06000359 RID: 857 RVA: 0x0000FD3E File Offset: 0x0000DF3E
		private void OnScreenDisplay()
		{
			this.screenIsOpen = true;
		}

		// Token: 0x0600035A RID: 858 RVA: 0x0000FD47 File Offset: 0x0000DF47
		private void OnClosingScreen()
		{
			this.screenIsOpen = false;
		}

		// Token: 0x04000199 RID: 409
		private InputManager.InputState currentInputState = InputManager.InputState.Free;

		// Token: 0x0400019A RID: 410
		private UIInputField currentInputField;

		// Token: 0x0400019B RID: 411
		private bool screenIsOpen;

		// Token: 0x0400019C RID: 412
		private EndlessSharedInputActions endlessSharedInputActions;

		// Token: 0x0400019D RID: 413
		private bool cinematicBlockingInput;

		// Token: 0x02000069 RID: 105
		public enum InputState
		{
			// Token: 0x0400019F RID: 415
			InputField,
			// Token: 0x040001A0 RID: 416
			Cinematic,
			// Token: 0x040001A1 RID: 417
			Free
		}
	}
}
