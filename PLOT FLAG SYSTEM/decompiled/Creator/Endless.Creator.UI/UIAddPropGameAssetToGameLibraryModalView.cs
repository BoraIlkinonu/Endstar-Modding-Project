using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIAddPropGameAssetToGameLibraryModalView : UIBaseModalView
{
	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnBack", this);
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}
}
