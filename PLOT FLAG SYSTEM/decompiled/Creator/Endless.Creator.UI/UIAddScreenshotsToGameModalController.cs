using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIAddScreenshotsToGameModalController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	[SerializeField]
	private UIAddScreenshotsToGameModalModel model;

	[SerializeField]
	private UIAddScreenshotsToGameModalView view;

	[SerializeField]
	private UIButton doneButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		doneButton.onClick.AddListener(Done);
	}

	private async void Done()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Done");
		}
		if (model.ScreenshotsToAdd.Count == 0)
		{
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			return;
		}
		OnLoadingStarted.Invoke();
		bool num = await MonoBehaviourSingleton<GameEditor>.Instance.AddScreenshotsToGame(model.ScreenshotsToAdd);
		OnLoadingEnded.Invoke();
		if (num)
		{
			view.OnScreenshotsToAdded.Invoke(model.ScreenshotsToAdd);
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}
	}
}
