using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorGroupNameView : UIBaseView<UIInspectorGroupName, UIInspectorGroupNameView.Styles>
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private TextMeshProUGUI groupNameText;

	[field: Header("UIInspectorGroupNameView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(UIInspectorGroupName model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model.GroupName);
		}
		groupNameText.text = model.GroupName;
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}
}
