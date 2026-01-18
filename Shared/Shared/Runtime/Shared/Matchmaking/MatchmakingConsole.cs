using System;
using System.Collections.Generic;
using MatchmakingClientSDK.Diagnostics;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000013 RID: 19
	public class MatchmakingConsole
	{
		// Token: 0x060000B2 RID: 178 RVA: 0x000053B0 File Offset: 0x000035B0
		public MatchmakingConsole()
		{
			this.diagnosticsClient = new DiagnosticsClient();
			this.diagnosticsClient.ReportEvent += this.OnDiagnosticsReport;
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000053E8 File Offset: 0x000035E8
		public void OnGUI()
		{
			if (!this.showConsole && GUILayout.Button("▼", new GUILayoutOption[] { GUILayout.Width(32f) }))
			{
				this.showConsole = !this.showConsole;
			}
			else if (this.showConsole && GUILayout.Button("▲", new GUILayoutOption[] { GUILayout.Width(32f) }))
			{
				this.showConsole = !this.showConsole;
			}
			if (this.showConsole)
			{
				if (!this.diagnosticsInProgress && GUILayout.Button("Run Diagnostics", Array.Empty<GUILayoutOption>()))
				{
					this.RunDiagnostics();
				}
				if (this.diagnosticsInProgress)
				{
					GUILayout.Label("Diagnostics in progress...", Array.Empty<GUILayoutOption>());
				}
				if (this.diagnosticsOutput.Count > 0)
				{
					GUILayout.Label("Diagnostics report:", Array.Empty<GUILayoutOption>());
					foreach (MatchmakingConsole.DiagnosticsReport diagnosticsReport in this.diagnosticsOutput.Values)
					{
						Color color = GUI.color;
						if (diagnosticsReport.IsError)
						{
							GUI.color = Color.red;
						}
						GUILayout.Label(string.Format("[{0}] {1}", diagnosticsReport.Id, diagnosticsReport.ShortMsg), Array.Empty<GUILayoutOption>());
						GUILayout.Label("  [INFO]: " + diagnosticsReport.LongMsg, Array.Empty<GUILayoutOption>());
						GUI.color = color;
					}
				}
			}
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00005564 File Offset: 0x00003764
		private async void RunDiagnostics()
		{
			this.diagnosticsInProgress = true;
			await this.diagnosticsClient.StartDiagnostics();
			this.diagnosticsInProgress = false;
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x0000559C File Offset: 0x0000379C
		private void OnDiagnosticsReport(string shortMsg, bool isError, string longMsg, int id)
		{
			this.diagnosticsOutput[id] = new MatchmakingConsole.DiagnosticsReport
			{
				Id = id,
				ShortMsg = shortMsg,
				IsError = isError,
				LongMsg = longMsg
			};
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x000055E0 File Offset: 0x000037E0
		public void Dispose()
		{
			this.diagnosticsClient.Dispose();
			this.diagnosticsOutput.Clear();
		}

		// Token: 0x0400002F RID: 47
		private bool showConsole;

		// Token: 0x04000030 RID: 48
		private readonly Dictionary<int, MatchmakingConsole.DiagnosticsReport> diagnosticsOutput = new Dictionary<int, MatchmakingConsole.DiagnosticsReport>();

		// Token: 0x04000031 RID: 49
		private bool diagnosticsInProgress;

		// Token: 0x04000032 RID: 50
		private readonly DiagnosticsClient diagnosticsClient;

		// Token: 0x02000014 RID: 20
		private struct DiagnosticsReport
		{
			// Token: 0x04000033 RID: 51
			public int Id;

			// Token: 0x04000034 RID: 52
			public string ShortMsg;

			// Token: 0x04000035 RID: 53
			public bool IsError;

			// Token: 0x04000036 RID: 54
			public string LongMsg;
		}
	}
}
