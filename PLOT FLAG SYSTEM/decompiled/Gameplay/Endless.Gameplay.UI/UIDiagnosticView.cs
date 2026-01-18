using System.Linq;
using Endless.Data;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIDiagnosticView : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private int frameCountToAverage = 30;

	[SerializeField]
	private Canvas latencyCanvas;

	[SerializeField]
	private TextMeshProUGUI latencyText;

	[SerializeField]
	private Canvas averageFpsCanvas;

	[SerializeField]
	private TextMeshProUGUI averageFpsText;

	[SerializeField]
	private Canvas fpsCanvas;

	[SerializeField]
	private TextMeshProUGUI fpsText;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private float[] fpsHistoryArray;

	private int fpsHistoryArrayIndex;

	private bool latencyVisible;

	private bool averageFpsVisible;

	private bool fpsVisible;

	private void OnEnable()
	{
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		fpsHistoryArray = new float[frameCountToAverage];
		latencyVisible = DiagnosticSettings.GetLatencyVisible();
		averageFpsVisible = DiagnosticSettings.GetAverageFpsVisible();
		fpsVisible = DiagnosticSettings.GetFpsVisible();
		DiagnosticSettings.OnLatencyVisibilityChanged += SetLatencyVisible;
		DiagnosticSettings.OnAverageFpsVisibilityChanged += SetAverageFpsVisible;
		DiagnosticSettings.OnFpsVisibilityChanged += SetFpsVisible;
		UpdateVisibility();
	}

	private void OnDestroy()
	{
		DiagnosticSettings.OnLatencyVisibilityChanged -= SetLatencyVisible;
		DiagnosticSettings.OnAverageFpsVisibilityChanged -= SetAverageFpsVisible;
		DiagnosticSettings.OnFpsVisibilityChanged -= SetFpsVisible;
	}

	private void Update()
	{
		if (latencyVisible)
		{
			latencyText.text = $"Latency: {(int)NetClock.RoundTripTime}ms";
		}
		fpsHistoryArrayIndex = (int)Mathf.Repeat(fpsHistoryArrayIndex + 1, fpsHistoryArray.Length);
		fpsHistoryArray[fpsHistoryArrayIndex] = Time.unscaledDeltaTime;
		if (averageFpsVisible)
		{
			int num = Mathf.RoundToInt(1f / (fpsHistoryArray.Sum() / (float)fpsHistoryArray.Length));
			averageFpsText.color = GetFpsColor(num);
			averageFpsText.text = $"Average FPS: {num}";
		}
		if (fpsVisible)
		{
			int num2 = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
			fpsText.color = GetFpsColor(num2);
			fpsText.text = $"FPS: {num2}";
		}
	}

	private static Color GetFpsColor(int frameRate)
	{
		if (frameRate >= 30)
		{
			if (frameRate < 60)
			{
				return Color.yellow;
			}
			return Color.white;
		}
		return Color.red;
	}

	private void UpdateVisibility()
	{
		latencyCanvas.enabled = latencyVisible;
		latencyText.gameObject.SetActive(latencyVisible);
		averageFpsCanvas.enabled = averageFpsVisible;
		averageFpsText.gameObject.SetActive(averageFpsVisible);
		fpsCanvas.enabled = fpsVisible;
		fpsText.gameObject.SetActive(fpsVisible);
		bool flag = latencyVisible || averageFpsVisible || fpsVisible;
		canvas.enabled = flag;
		base.enabled = flag;
	}

	private void SetLatencyVisible(bool isVisible)
	{
		latencyVisible = isVisible;
		UpdateVisibility();
	}

	private void SetAverageFpsVisible(bool isVisible)
	{
		averageFpsVisible = isVisible;
		UpdateVisibility();
	}

	private void SetFpsVisible(bool isVisible)
	{
		fpsVisible = isVisible;
		UpdateVisibility();
	}
}
