using System;
using System.Linq;
using Endless.Assets;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIInspectorToolPanelController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	public static Action OnCancelEyeDrop;

	[SerializeField]
	private UIInspectorToolPanelView view;

	[SerializeField]
	private UIInputField label;

	[SerializeField]
	private UIButton cancelEyeDropButton;

	[SerializeField]
	private UIButton reAddToGameLibraryButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private InspectorTool inspectorTool;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		label.onSubmit.AddListener(UpdateLabel);
		label.onEndEdit.AddListener(UpdateLabel);
		cancelEyeDropButton.onClick.AddListener(CancelEyeDrop);
		reAddToGameLibraryButton.onClick.AddListener(ReAddToGameLibrary);
		inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<InspectorTool>();
	}

	private void UpdateLabel(string newLabel)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateLabel", newLabel);
		}
		inspectorTool.HandleLabelChange(view.InstanceId, newLabel);
	}

	private void CancelEyeDrop()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CancelEyeDrop");
		}
		inspectorTool.CancelEyeDrop();
		inspectorTool.SetStateToInspect();
	}

	private async void ReAddToGameLibrary()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ReAddToGameLibrary");
		}
		OnLoadingStarted.Invoke();
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(view.AssetId);
		if (graphQlResult.HasErrors)
		{
			Exception errorMessage = graphQlResult.GetErrorMessage();
			ErrorHandler.HandleError(ErrorCodes.UIInspectorToolPanelController_GetVersionsToReAddToGameLibrary, errorMessage);
			OnLoadingEnded.Invoke();
			return;
		}
		string[] parsedAndOrderedVersions = VersionUtilities.GetParsedAndOrderedVersions(graphQlResult.GetDataMember());
		if (parsedAndOrderedVersions.Length == 0)
		{
			Exception errorMessage2 = graphQlResult.GetErrorMessage();
			ErrorHandler.HandleError(ErrorCodes.UIInspectorToolPanelController_NoVersions, errorMessage2);
			OnLoadingEnded.Invoke();
		}
		string assetVersion = parsedAndOrderedVersions[0];
		AssetReference propReference = new AssetReference
		{
			AssetID = view.AssetId,
			AssetVersion = assetVersion,
			AssetType = "prop"
		};
		if (!MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences.Any((AssetReference reference) => reference.AssetID == propReference.AssetID))
		{
			await MonoBehaviourSingleton<GameEditor>.Instance.AddPropToGameLibrary(propReference);
		}
		else
		{
			await MonoBehaviourSingleton<GameEditor>.Instance.SetPropVersionInGameLibrary(propReference.AssetID, propReference.AssetVersion);
		}
		OnLoadingEnded.Invoke();
		if (inspectorTool.IsActive)
		{
			view.Review();
		}
	}
}
