using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI;

public class UIIconDefinitionController : UIGameObject
{
	[SerializeField]
	private UIIconDefinitionView view;

	[SerializeField]
	private UIButton selectButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public UnityEvent<IconDefinition> SelectUnityEvent { get; } = new UnityEvent<IconDefinition>();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		selectButton.onClick.AddListener(OnSelect);
	}

	private void OnSelect()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelect");
		}
		SelectUnityEvent.Invoke(view.Model);
	}
}
