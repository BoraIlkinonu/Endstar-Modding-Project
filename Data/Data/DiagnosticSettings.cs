using System;
using UnityEngine;

namespace Endless.Data
{
	// Token: 0x02000008 RID: 8
	public static class DiagnosticSettings
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000024 RID: 36 RVA: 0x00002BC0 File Offset: 0x00000DC0
		// (remove) Token: 0x06000025 RID: 37 RVA: 0x00002BF4 File Offset: 0x00000DF4
		public static event Action<bool> OnLatencyVisibilityChanged;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000026 RID: 38 RVA: 0x00002C28 File Offset: 0x00000E28
		// (remove) Token: 0x06000027 RID: 39 RVA: 0x00002C5C File Offset: 0x00000E5C
		public static event Action<bool> OnAverageFpsVisibilityChanged;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000028 RID: 40 RVA: 0x00002C90 File Offset: 0x00000E90
		// (remove) Token: 0x06000029 RID: 41 RVA: 0x00002CC4 File Offset: 0x00000EC4
		public static event Action<bool> OnFpsVisibilityChanged;

		// Token: 0x0600002A RID: 42 RVA: 0x00002CF7 File Offset: 0x00000EF7
		public static bool GetLatencyVisible()
		{
			return PlayerPrefs.GetInt("Diagnostic Latency Visible", 1) == 1;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002D07 File Offset: 0x00000F07
		public static bool GetAverageFpsVisible()
		{
			return PlayerPrefs.GetInt("Diagnostic Average FPS Visible", 1) == 1;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002D17 File Offset: 0x00000F17
		public static bool GetFpsVisible()
		{
			return PlayerPrefs.GetInt("Diagnostic FPS Visible", 1) == 1;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002D27 File Offset: 0x00000F27
		public static void SetLatencyVisible(bool isVisible)
		{
			PlayerPrefs.SetInt("Diagnostic Latency Visible", isVisible ? 1 : 0);
			Action<bool> onLatencyVisibilityChanged = DiagnosticSettings.OnLatencyVisibilityChanged;
			if (onLatencyVisibilityChanged == null)
			{
				return;
			}
			onLatencyVisibilityChanged(isVisible);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002D4A File Offset: 0x00000F4A
		public static void SetAverageFpsVisible(bool isVisible)
		{
			PlayerPrefs.SetInt("Diagnostic Average FPS Visible", isVisible ? 1 : 0);
			Action<bool> onAverageFpsVisibilityChanged = DiagnosticSettings.OnAverageFpsVisibilityChanged;
			if (onAverageFpsVisibilityChanged == null)
			{
				return;
			}
			onAverageFpsVisibilityChanged(isVisible);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002D6D File Offset: 0x00000F6D
		public static void SetFpsVisible(bool isVisible)
		{
			PlayerPrefs.SetInt("Diagnostic FPS Visible", isVisible ? 1 : 0);
			Action<bool> onFpsVisibilityChanged = DiagnosticSettings.OnFpsVisibilityChanged;
			if (onFpsVisibilityChanged == null)
			{
				return;
			}
			onFpsVisibilityChanged(isVisible);
		}
	}
}
