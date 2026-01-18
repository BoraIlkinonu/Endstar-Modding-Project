using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI;

public class UICreatorPlayerListModel : UIPlayerReferenceManagerListModel
{
	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(base.Initialize);
		NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(base.Uninitialize);
	}
}
