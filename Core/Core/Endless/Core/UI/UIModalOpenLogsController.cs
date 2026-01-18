using System;
using Endless.Data;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Core.UI
{
	// Token: 0x0200006F RID: 111
	public class UIModalOpenLogsController : UIGameObject
	{
		// Token: 0x06000202 RID: 514 RVA: 0x0000B5EE File Offset: 0x000097EE
		private void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			this.playerInputActions = new PlayerInputActions();
		}

		// Token: 0x06000203 RID: 515 RVA: 0x0000B614 File Offset: 0x00009814
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.playerInputActions.Player.OpenLogsModal.performed += this.OpenLogsModal;
			this.playerInputActions.Player.OpenLogsModal.Enable();
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000B675 File Offset: 0x00009875
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.modalManager = MonoBehaviourSingleton<UIModalManager>.Instance;
		}

		// Token: 0x06000205 RID: 517 RVA: 0x0000B69C File Offset: 0x0000989C
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.playerInputActions.Player.OpenLogsModal.performed -= this.OpenLogsModal;
			this.playerInputActions.Player.OpenLogsModal.Disable();
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000B700 File Offset: 0x00009900
		private void OpenLogsModal(InputAction.CallbackContext callbackContext)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenLogsModal", new object[] { callbackContext.action.name });
			}
			this.modalManager.Display(this.logsModalSource, UIModalManagerStackActions.ClearStack, Array.Empty<object>());
		}

		// Token: 0x0400016F RID: 367
		[SerializeField]
		private UILogsModalView logsModalSource;

		// Token: 0x04000170 RID: 368
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000171 RID: 369
		private PlayerInputActions playerInputActions;

		// Token: 0x04000172 RID: 370
		private UIModalManager modalManager;
	}
}
