using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UISpawnPointSelectionModalController : UIGameObject
{
	[SerializeField]
	private UISpawnPointSelectionModalView view;

	[SerializeField]
	private UIButton confirmButton;

	[SerializeField]
	private UISpawnPointListModel spawnPointListModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		confirmButton.onClick.AddListener(Confirm);
	}

	private void Confirm()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Confirm");
		}
		view.LevelDestinationPresenter.SetSpawnPoints(spawnPointListModel.SelectedTypedList);
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
	}
}
