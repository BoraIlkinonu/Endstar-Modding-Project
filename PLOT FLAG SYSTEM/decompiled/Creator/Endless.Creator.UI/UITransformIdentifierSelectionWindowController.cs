using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UITransformIdentifierSelectionWindowController : UIWindowController
{
	[Header("UITransformIdentifierSelectionWindowController")]
	[SerializeField]
	private UITransformIdentifierListModel transformIdentifierListModel;

	private UITransformIdentifierSelectionWindowView TypedWindowView => (UITransformIdentifierSelectionWindowView)BaseWindowView;

	protected override void Start()
	{
		base.Start();
		transformIdentifierListModel.SelectionChangedUnityEvent.AddListener(OnSelect);
	}

	private void OnSelect(int index, bool selected)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelect", index, selected);
		}
		TypedWindowView.OnTransformIdentifierSelect?.Invoke(transformIdentifierListModel[index].TransformIdentifier);
		TypedWindowView.Close();
	}
}
