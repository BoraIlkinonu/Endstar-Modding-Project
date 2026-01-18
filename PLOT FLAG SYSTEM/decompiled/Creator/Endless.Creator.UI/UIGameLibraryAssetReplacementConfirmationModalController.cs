using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

[RequireComponent(typeof(UIGameLibraryAssetReplacementConfirmationModalView))]
public class UIGameLibraryAssetReplacementConfirmationModalController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	[SerializeField]
	private UIGameLibraryAssetReplacementConfirmationModalView view;

	[SerializeField]
	private UIButton noButton;

	[SerializeField]
	private UIButton yesButton;

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
		noButton.onClick.AddListener(Cancel);
		yesButton.onClick.AddListener(RemoveAsset);
		TryGetComponent<UIGameLibraryAssetReplacementConfirmationModalView>(out view);
	}

	private void Cancel()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Cancel");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}

	private async void RemoveAsset()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "RemoveAsset", "ToRemove: " + view.ToRemove.AssetID + ", ToReplace: " + view.ToReplace.AssetID);
		}
		OnLoadingStarted.Invoke();
		if ((await MonoBehaviourSingleton<GameEditor>.Instance.RemoveTerrainEntryFromGameLibrary(view.ToRemove.AssetID, view.ToReplace.AssetID)).Success)
		{
			if (verboseLogging)
			{
				DebugUtility.Log("Removal success!", this);
			}
		}
		else
		{
			DebugUtility.LogError("Could not replace " + view.ToRemove.AssetID + " with " + view.ToReplace.AssetID + "!", this);
		}
		OnLoadingEnded.Invoke();
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
	}
}
