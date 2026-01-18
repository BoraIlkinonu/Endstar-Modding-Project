using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI;

public class UIRiflemanNpcCustomizationDataView : UINpcClassCustomizationDataView<RiflemanNpcCustomizationData>
{
	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}
}
