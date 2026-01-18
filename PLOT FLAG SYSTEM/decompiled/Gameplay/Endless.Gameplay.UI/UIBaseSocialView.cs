using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI;

public abstract class UIBaseSocialView<T> : UIBaseView<T, UIBaseSocialView<T>.Styles>
{
	public enum Styles
	{
		LineItem,
		Card,
		PortraitOnly
	}

	public override Styles Style { get; protected set; }

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Clear", this);
		}
	}
}
