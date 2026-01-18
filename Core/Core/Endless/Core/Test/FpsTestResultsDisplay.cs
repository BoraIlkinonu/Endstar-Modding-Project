using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.EndlessQualitySettings;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Endless.Core.Test
{
	// Token: 0x020000DC RID: 220
	public class FpsTestResultsDisplay : MonoBehaviour, IBackable
	{
		// Token: 0x060004EE RID: 1262 RVA: 0x0001808C File Offset: 0x0001628C
		private void Start()
		{
			if (this.displayOnStart)
			{
				this.Display();
			}
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x0001809C File Offset: 0x0001629C
		public void Display()
		{
			this.canvas.enabled = true;
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			foreach (string text in this.manager.SceneLabels)
			{
				FpsInfo[] fpsInfo = this.manager.GetFpsInfo(text);
				Debug.Log(string.Format("FpsTesting: Gathering reports. found {0} tests.", fpsInfo.Length));
				this.fpsReports.Add(new FpsReport(text + " (" + this.qualityMenu.GetCurrentQualityPresetName() + " quality)", fpsInfo));
			}
			this.UpdateFpsReportDisplay();
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x00018134 File Offset: 0x00016334
		public void NextReport()
		{
			if (this.currentIndex < this.fpsReports.Count - 1)
			{
				this.currentIndex++;
				this.UpdateFpsReportDisplay();
			}
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x0001815F File Offset: 0x0001635F
		public void PreviousReport()
		{
			if (this.currentIndex > 0)
			{
				this.currentIndex--;
				this.UpdateFpsReportDisplay();
			}
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x00018180 File Offset: 0x00016380
		private void UpdateFpsReportDisplay()
		{
			this.previousReportButton.interactable = this.currentIndex > 0;
			this.nextReportButton.interactable = this.currentIndex < this.fpsReports.Count - 1;
			if (this.fpsReports.Count == 0)
			{
				Debug.LogWarning("You must gather reports before running this scene!");
				return;
			}
			foreach (FpsReportEntry fpsReportEntry in this.spawnedEntries)
			{
				global::UnityEngine.Object.Destroy(fpsReportEntry.gameObject);
			}
			this.spawnedEntries.Clear();
			FpsReport fpsReport = this.fpsReports[this.currentIndex];
			this.sceneLabel.SetText(fpsReport.SceneLabel, true);
			foreach (FpsReportSection fpsReportSection in fpsReport.DisplayInfo)
			{
				FpsReportEntry fpsReportEntry2 = global::UnityEngine.Object.Instantiate<FpsReportEntry>(this.headerEntryPrefab, this.reportDisplayRoot);
				fpsReportEntry2.UpdateDisplay(new string[] { fpsReportSection.SectionTitle, fpsReportSection.SectionType });
				this.spawnedEntries.Add(fpsReportEntry2);
				for (int i = 0; i < fpsReportSection.SectionInfo.Count; i++)
				{
					fpsReportEntry2 = global::UnityEngine.Object.Instantiate<FpsReportEntry>(this.entryPrefab, this.reportDisplayRoot);
					fpsReportEntry2.UpdateDisplay(fpsReportSection.SectionInfo[i]);
					this.spawnedEntries.Add(fpsReportEntry2);
				}
			}
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x00018320 File Offset: 0x00016520
		public void Reload()
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x0001833F File Offset: 0x0001653F
		public void OnBack()
		{
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			this.Close();
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x00018352 File Offset: 0x00016552
		public void Close()
		{
			this.canvas.enabled = false;
			this.OnClosed.Invoke();
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x0001836C File Offset: 0x0001656C
		public void Clear()
		{
			this.fpsReports.Clear();
			foreach (FpsReportEntry fpsReportEntry in this.spawnedEntries)
			{
				global::UnityEngine.Object.Destroy(fpsReportEntry.gameObject);
			}
			this.spawnedEntries.Clear();
		}

		// Token: 0x04000358 RID: 856
		[SerializeField]
		private Button previousReportButton;

		// Token: 0x04000359 RID: 857
		[SerializeField]
		private Button nextReportButton;

		// Token: 0x0400035A RID: 858
		[SerializeField]
		private Transform reportDisplayRoot;

		// Token: 0x0400035B RID: 859
		[SerializeField]
		private FpsReportEntry headerEntryPrefab;

		// Token: 0x0400035C RID: 860
		[SerializeField]
		private FpsReportEntry entryPrefab;

		// Token: 0x0400035D RID: 861
		[SerializeField]
		private TextMeshProUGUI sceneLabel;

		// Token: 0x0400035E RID: 862
		[SerializeField]
		private Canvas canvas;

		// Token: 0x0400035F RID: 863
		[SerializeField]
		private bool displayOnStart = true;

		// Token: 0x04000360 RID: 864
		[SerializeField]
		private QualityMenu qualityMenu;

		// Token: 0x04000361 RID: 865
		[SerializeField]
		private FpsTestManager manager;

		// Token: 0x04000362 RID: 866
		[HideInInspector]
		public UnityEvent OnClosed = new UnityEvent();

		// Token: 0x04000363 RID: 867
		private int currentIndex;

		// Token: 0x04000364 RID: 868
		private List<FpsReport> fpsReports = new List<FpsReport>();

		// Token: 0x04000365 RID: 869
		private List<FpsReportEntry> spawnedEntries = new List<FpsReportEntry>();
	}
}
