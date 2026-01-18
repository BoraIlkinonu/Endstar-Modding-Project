using Endless.Shared;
using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI;

public class UIGameplayPlayerListModel : UIPlayerReferenceManagerListModel
{
	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(base.Initialize);
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.AddListener(base.Uninitialize);
	}
}
