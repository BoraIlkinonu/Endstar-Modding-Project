using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelAssetView : UIGameObject
{
	[SerializeField]
	private GameObject defaultVisual;

	[SerializeField]
	private UIScreenshotView mainScreenshot;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public void View(LevelAsset model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		bool flag = model.Screenshots.Count > 0;
		defaultVisual.SetActive(!flag);
		mainScreenshot.gameObject.SetActive(flag);
		if (flag)
		{
			mainScreenshot.Display(model.Screenshots[0]);
		}
		nameText.text = model.Name;
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		defaultVisual.SetActive(value: true);
		mainScreenshot.gameObject.SetActive(value: false);
		mainScreenshot.Clear();
	}
}
