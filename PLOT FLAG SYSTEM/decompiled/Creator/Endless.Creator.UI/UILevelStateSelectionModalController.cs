using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UILevelStateSelectionModalView))]
public class UILevelStateSelectionModalController : UIGameObject
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UILevelStateSelectionModalView view;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		TryGetComponent<UILevelStateSelectionModalView>(out view);
	}
}
