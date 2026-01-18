using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI;

public class UIBlankNpcCustomizationDataView : UINpcClassCustomizationDataView<BlankNpcCustomizationData>
{
	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}
}
