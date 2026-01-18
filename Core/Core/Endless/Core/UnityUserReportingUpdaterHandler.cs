using System;
using Endless.Core.UI;
using Endless.Data;
using Endless.Shared.Debugging;
using Unity.Cloud.UserReporting.Client;
using Unity.Cloud.UserReporting.Plugin;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Core
{
	// Token: 0x0200002E RID: 46
	public class UnityUserReportingUpdaterHandler : MonoBehaviour
	{
		// Token: 0x060000C7 RID: 199 RVA: 0x000060F9 File Offset: 0x000042F9
		private void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			this.playerInputActions = new PlayerInputActions();
			UnityUserReporting.Configure(new UserReportingClientConfiguration());
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x00006128 File Offset: 0x00004328
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.playerInputActions.Player.OpenUserReportModal.started += this.OpenUserReportModal;
			this.playerInputActions.Player.Enable();
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00006184 File Offset: 0x00004384
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.playerInputActions.Player.OpenUserReportModal.started -= this.OpenUserReportModal;
			this.playerInputActions.Player.Disable();
		}

		// Token: 0x060000CA RID: 202 RVA: 0x000061E0 File Offset: 0x000043E0
		private void Update()
		{
			this.unityUserReportingUpdater.Reset();
			base.StartCoroutine(this.unityUserReportingUpdater);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x000061FC File Offset: 0x000043FC
		private void OpenUserReportModal(InputAction.CallbackContext callbackContext)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenUserReportModal", new object[] { callbackContext.action.name });
			}
			if (this.userReportWindowView.IsDisplaying)
			{
				return;
			}
			this.userReportWindowController.TakeScreenshots();
			this.userReportWindowView.Display();
		}

		// Token: 0x0400007A RID: 122
		[SerializeField]
		private UIUserReportWindowController userReportWindowController;

		// Token: 0x0400007B RID: 123
		[SerializeField]
		private UIUserReportWindowView userReportWindowView;

		// Token: 0x0400007C RID: 124
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400007D RID: 125
		private PlayerInputActions playerInputActions;

		// Token: 0x0400007E RID: 126
		private readonly UnityUserReportingUpdater unityUserReportingUpdater = new UnityUserReportingUpdater();
	}
}
