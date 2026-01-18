using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIVersionView : UIBaseView<UIVersion, UIVersionView.Styles>
{
	public enum Styles
	{
		Default,
		Publish
	}

	[SerializeField]
	private UIText versionText;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(UIVersion model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		versionText.Value = model.UserFacingVersion;
	}

	public override void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		versionText.Clear();
	}
}
