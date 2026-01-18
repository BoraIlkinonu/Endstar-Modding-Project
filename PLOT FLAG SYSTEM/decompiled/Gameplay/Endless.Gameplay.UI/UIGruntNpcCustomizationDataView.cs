using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI;

public class UIGruntNpcCustomizationDataView : UINpcClassCustomizationDataView<GruntNpcCustomizationData>
{
	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}
}
