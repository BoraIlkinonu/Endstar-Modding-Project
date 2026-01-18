using Endless.Gameplay.Screenshotting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class HideVisualsOnPlay : EndlessBehaviour, IGameEndSubscriber, IStartSubscriber
{
	[SerializeField]
	private Renderer[] renderersToManage;

	protected override void Start()
	{
		base.Start();
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.AddListener(ScreenshotStarted);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.AddListener(ScreenshotFinished);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnBeforeScreenshot.RemoveListener(ScreenshotStarted);
		MonoBehaviourSingleton<ScreenshotAPI>.Instance.OnAfterScreenshot.RemoveListener(ScreenshotFinished);
	}

	private void ScreenshotStarted()
	{
		SetVisuals(enabled: false);
	}

	private void ScreenshotFinished()
	{
		SetVisuals(enabled: true);
	}

	private void Reset()
	{
		if (renderersToManage == null || renderersToManage.Length == 0)
		{
			renderersToManage = GetComponentsInChildren<Renderer>();
		}
	}

	public void EndlessStart()
	{
		SetVisuals(enabled: false);
	}

	public void EndlessGameEnd()
	{
		SetVisuals(enabled: true);
	}

	private void SetVisuals(bool enabled)
	{
		Renderer[] array = renderersToManage;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = enabled;
		}
	}
}
