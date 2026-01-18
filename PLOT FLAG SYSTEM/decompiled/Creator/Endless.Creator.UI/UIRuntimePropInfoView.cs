using Endless.Gameplay.LevelEditing;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoView : UIBaseView<PropLibrary.RuntimePropInfo, UIRuntimePropInfoView.Styles>
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[field: Header("UIRuntimePropInfoView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(PropLibrary.RuntimePropInfo model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		nameText.text = model.PropData.Name;
		iconImage.sprite = model.Icon;
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		iconImage.sprite = null;
	}
}
