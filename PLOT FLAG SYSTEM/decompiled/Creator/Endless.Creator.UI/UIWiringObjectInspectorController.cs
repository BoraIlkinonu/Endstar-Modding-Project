using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UIWiringObjectInspectorView))]
public class UIWiringObjectInspectorController : UIGameObject
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIWiringObjectInspectorView wiringObjectInspectorView;

	private UIWiringObjectInspectorView WiringObjectInspectorView
	{
		get
		{
			if (!wiringObjectInspectorView)
			{
				TryGetComponent<UIWiringObjectInspectorView>(out wiringObjectInspectorView);
			}
			return wiringObjectInspectorView;
		}
	}

	private UIWiringManager WiringManager => MonoBehaviourSingleton<UIWiringManager>.Instance;

	public void Close()
	{
		if (verboseLogging)
		{
			Debug.Log("Close", this);
		}
		WiringManager.HideWiringInspector(WiringObjectInspectorView, displayToolPrompt: true);
	}
}
