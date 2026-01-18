using System;
using Endless.Core.UI;
using Endless.Data;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared
{
	// Token: 0x02000019 RID: 25
	[DefaultExecutionOrder(2147483647)]
	public class InstallationErrorHandler : MonoBehaviourSingleton<InstallationErrorHandler>
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600005A RID: 90 RVA: 0x00003E0A File Offset: 0x0000200A
		public UnityEvent OnErrorDetected { get; } = new UnityEvent();

		// Token: 0x0600005B RID: 91 RVA: 0x00003E12 File Offset: 0x00002012
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.CheckIfAppIsTranslocated();
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00003E34 File Offset: 0x00002034
		private void CheckIfAppIsTranslocated()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CheckIfAppIsTranslocated", Array.Empty<object>());
			}
			if (Application.platform != RuntimePlatform.OSXPlayer || !Application.dataPath.Contains("AppTranslocation"))
			{
				return;
			}
			this.OnErrorDetected.Invoke();
			MatchmakingClientController.Instance.Disconnect();
			DebugUtility.Log("Application.dataPath: " + Application.dataPath, this);
			ErrorCodes errorCodes = ErrorCodes.AppTranslocation;
			UIInstallationErrorScreenView.Display(new UIInstallationErrorScreenModel(errorCodes), UIScreenManager.DisplayStackActions.ClearAndPush);
			ErrorHandler.HandleError(errorCodes, new AppTranslocationException(), true, false);
		}

		// Token: 0x04000045 RID: 69
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
