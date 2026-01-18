using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UIWireView))]
[RequireComponent(typeof(UILineRenderer))]
public class UIWireController : UIGameObject
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIWireView wireView;

	private UIWireView WireView
	{
		get
		{
			if (!wireView)
			{
				TryGetComponent<UIWireView>(out wireView);
			}
			return wireView;
		}
	}

	private UIWiringManager WiringManager => MonoBehaviourSingleton<UIWiringManager>.Instance;

	public void OnSelect()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelect");
		}
		WiringManager.WireEditorController.EditWire(WireView);
	}
}
