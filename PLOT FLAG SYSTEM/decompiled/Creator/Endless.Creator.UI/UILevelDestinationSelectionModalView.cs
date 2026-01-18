using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UILevelDestinationSelectionModalView : UIEscapableModalView
{
	private UILevelDestinationPresenter levelDestinationPresenterToApplyTo;

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		levelDestinationPresenterToApplyTo = modalData[0] as UILevelDestinationPresenter;
	}

	public void ApplyToProperty(LevelDestination levelDestination)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ApplyToProperty", levelDestination.TargetLevelId);
		}
		levelDestinationPresenterToApplyTo.SetLevelDestination(levelDestination);
	}
}
