using System;
using System.Linq;
using Endless.Data;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000395 RID: 917
	public class UIDiagnosticView : MonoBehaviour
	{
		// Token: 0x06001755 RID: 5973 RVA: 0x00002DB0 File Offset: 0x00000FB0
		private void OnEnable()
		{
		}

		// Token: 0x06001756 RID: 5974 RVA: 0x0006CCB8 File Offset: 0x0006AEB8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.fpsHistoryArray = new float[this.frameCountToAverage];
			this.latencyVisible = DiagnosticSettings.GetLatencyVisible();
			this.averageFpsVisible = DiagnosticSettings.GetAverageFpsVisible();
			this.fpsVisible = DiagnosticSettings.GetFpsVisible();
			DiagnosticSettings.OnLatencyVisibilityChanged += this.SetLatencyVisible;
			DiagnosticSettings.OnAverageFpsVisibilityChanged += this.SetAverageFpsVisible;
			DiagnosticSettings.OnFpsVisibilityChanged += this.SetFpsVisible;
			this.UpdateVisibility();
		}

		// Token: 0x06001757 RID: 5975 RVA: 0x0006CD48 File Offset: 0x0006AF48
		private void OnDestroy()
		{
			DiagnosticSettings.OnLatencyVisibilityChanged -= this.SetLatencyVisible;
			DiagnosticSettings.OnAverageFpsVisibilityChanged -= this.SetAverageFpsVisible;
			DiagnosticSettings.OnFpsVisibilityChanged -= this.SetFpsVisible;
		}

		// Token: 0x06001758 RID: 5976 RVA: 0x0006CD80 File Offset: 0x0006AF80
		private void Update()
		{
			if (this.latencyVisible)
			{
				this.latencyText.text = string.Format("Latency: {0}ms", (int)NetClock.RoundTripTime);
			}
			this.fpsHistoryArrayIndex = (int)Mathf.Repeat((float)(this.fpsHistoryArrayIndex + 1), (float)this.fpsHistoryArray.Length);
			this.fpsHistoryArray[this.fpsHistoryArrayIndex] = Time.unscaledDeltaTime;
			if (this.averageFpsVisible)
			{
				int num = Mathf.RoundToInt(1f / (this.fpsHistoryArray.Sum() / (float)this.fpsHistoryArray.Length));
				this.averageFpsText.color = UIDiagnosticView.GetFpsColor(num);
				this.averageFpsText.text = string.Format("Average FPS: {0}", num);
			}
			if (this.fpsVisible)
			{
				int num2 = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
				this.fpsText.color = UIDiagnosticView.GetFpsColor(num2);
				this.fpsText.text = string.Format("FPS: {0}", num2);
			}
		}

		// Token: 0x06001759 RID: 5977 RVA: 0x0006CE80 File Offset: 0x0006B080
		private static Color GetFpsColor(int frameRate)
		{
			Color color;
			if (frameRate >= 30)
			{
				if (frameRate >= 60)
				{
					color = Color.white;
				}
				else
				{
					color = Color.yellow;
				}
			}
			else
			{
				color = Color.red;
			}
			return color;
		}

		// Token: 0x0600175A RID: 5978 RVA: 0x0006CEB0 File Offset: 0x0006B0B0
		private void UpdateVisibility()
		{
			this.latencyCanvas.enabled = this.latencyVisible;
			this.latencyText.gameObject.SetActive(this.latencyVisible);
			this.averageFpsCanvas.enabled = this.averageFpsVisible;
			this.averageFpsText.gameObject.SetActive(this.averageFpsVisible);
			this.fpsCanvas.enabled = this.fpsVisible;
			this.fpsText.gameObject.SetActive(this.fpsVisible);
			bool flag = this.latencyVisible || this.averageFpsVisible || this.fpsVisible;
			this.canvas.enabled = flag;
			base.enabled = flag;
		}

		// Token: 0x0600175B RID: 5979 RVA: 0x0006CF5F File Offset: 0x0006B15F
		private void SetLatencyVisible(bool isVisible)
		{
			this.latencyVisible = isVisible;
			this.UpdateVisibility();
		}

		// Token: 0x0600175C RID: 5980 RVA: 0x0006CF6E File Offset: 0x0006B16E
		private void SetAverageFpsVisible(bool isVisible)
		{
			this.averageFpsVisible = isVisible;
			this.UpdateVisibility();
		}

		// Token: 0x0600175D RID: 5981 RVA: 0x0006CF7D File Offset: 0x0006B17D
		private void SetFpsVisible(bool isVisible)
		{
			this.fpsVisible = isVisible;
			this.UpdateVisibility();
		}

		// Token: 0x040012BE RID: 4798
		[SerializeField]
		private Canvas canvas;

		// Token: 0x040012BF RID: 4799
		[SerializeField]
		private int frameCountToAverage = 30;

		// Token: 0x040012C0 RID: 4800
		[SerializeField]
		private Canvas latencyCanvas;

		// Token: 0x040012C1 RID: 4801
		[SerializeField]
		private TextMeshProUGUI latencyText;

		// Token: 0x040012C2 RID: 4802
		[SerializeField]
		private Canvas averageFpsCanvas;

		// Token: 0x040012C3 RID: 4803
		[SerializeField]
		private TextMeshProUGUI averageFpsText;

		// Token: 0x040012C4 RID: 4804
		[SerializeField]
		private Canvas fpsCanvas;

		// Token: 0x040012C5 RID: 4805
		[SerializeField]
		private TextMeshProUGUI fpsText;

		// Token: 0x040012C6 RID: 4806
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040012C7 RID: 4807
		private float[] fpsHistoryArray;

		// Token: 0x040012C8 RID: 4808
		private int fpsHistoryArrayIndex;

		// Token: 0x040012C9 RID: 4809
		private bool latencyVisible;

		// Token: 0x040012CA RID: 4810
		private bool averageFpsVisible;

		// Token: 0x040012CB RID: 4811
		private bool fpsVisible;
	}
}
