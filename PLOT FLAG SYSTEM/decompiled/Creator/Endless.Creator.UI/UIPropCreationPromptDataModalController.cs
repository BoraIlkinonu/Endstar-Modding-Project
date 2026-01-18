using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropCreationPromptDataModalController : UIGameObject
{
	[SerializeField]
	private UIPropCreationPromptDataModalView view;

	[SerializeField]
	private UIButton viewSdkButton;

	[SerializeField]
	private StringVariable sdkUrl;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		viewSdkButton.onClick.AddListener(ViewSdk);
	}

	private void ViewSdk()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewSdk");
		}
		Application.OpenURL(sdkUrl.Value);
	}
}
