using Endless.Props.Assets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIAudioAssetView : UIBaseView<AudioAsset, UIAudioAssetView.Styles>
{
	public enum Styles
	{
		Default
	}

	[field: Header("UIAudioAssetView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(AudioAsset model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}
}
