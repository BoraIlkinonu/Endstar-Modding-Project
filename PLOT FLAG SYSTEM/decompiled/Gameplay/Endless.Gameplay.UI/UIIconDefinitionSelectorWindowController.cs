using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIIconDefinitionSelectorWindowController : UIDraggableWindowController
{
	[Header("UIIconDefinitionSelectorWindowController")]
	[SerializeField]
	private UIIconDefinitionSelectorWindowView view;

	[SerializeField]
	private UIIconDefinitionSelector selector;

	protected override void Start()
	{
		base.Start();
		selector.OnSelectedUnityEvent.AddListener(SetSelection);
	}

	private void SetSelection(IconDefinition item)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetSelection", item);
		}
		view.OnSelect(item);
		Close();
	}
}
