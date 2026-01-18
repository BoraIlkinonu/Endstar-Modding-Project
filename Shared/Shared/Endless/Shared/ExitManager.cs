using System;
using UnityEngine;
using UnityEngine.CrashReportHandler;

namespace Endless.Shared
{
	// Token: 0x020000A0 RID: 160
	public static class ExitManager
	{
		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x06000479 RID: 1145 RVA: 0x00013672 File Offset: 0x00011872
		// (set) Token: 0x0600047A RID: 1146 RVA: 0x00013679 File Offset: 0x00011879
		public static bool IsQuitting { get; private set; }

		// Token: 0x1400002F RID: 47
		// (add) Token: 0x0600047B RID: 1147 RVA: 0x00013684 File Offset: 0x00011884
		// (remove) Token: 0x0600047C RID: 1148 RVA: 0x000136B8 File Offset: 0x000118B8
		public static event Action OnQuitting;

		// Token: 0x0600047D RID: 1149 RVA: 0x000136EB File Offset: 0x000118EB
		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			Application.quitting += ExitManager.HandleQuitting;
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x000136FE File Offset: 0x000118FE
		private static void HandleQuitting()
		{
			CrashReportHandler.enableCaptureExceptions = false;
			Debug.Log("Application quitting");
			CrashReportHandler.enableCaptureExceptions = false;
			ExitManager.IsQuitting = true;
			Action onQuitting = ExitManager.OnQuitting;
			if (onQuitting == null)
			{
				return;
			}
			onQuitting();
		}
	}
}
