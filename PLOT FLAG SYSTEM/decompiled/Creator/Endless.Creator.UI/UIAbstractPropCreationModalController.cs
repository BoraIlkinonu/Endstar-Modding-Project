using System;
using System.Linq;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.UI;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIAbstractPropCreationModalController : UIBasePropCreationModalController
{
	[Header("UIAbstractPropCreationModalController")]
	[SerializeField]
	private UIButton selectIconButton;

	[SerializeField]
	private IconList iconList;

	private UIAbstractPropCreationModalView typedView;

	private UIIconDefinitionSelectorWindowView iconDefinitionSelectorWindow;

	protected override void Start()
	{
		base.Start();
		selectIconButton.onClick.AddListener(SelectIcon);
		typedView = view as UIAbstractPropCreationModalView;
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if ((bool)iconDefinitionSelectorWindow)
		{
			iconDefinitionSelectorWindow.Close();
		}
	}

	protected override async void Create()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Create");
		}
		if (!ValidatePropCreation())
		{
			return;
		}
		base.OnLoadingStarted.Invoke();
		try
		{
			Prop prop = await typedView.AbstractPropCreationScreenData.UploadProp(base.Name, base.Description, base.GrantEditRightsToCollaborators, typedView.IconId, typedView.FinalTexture);
			propTool.UpdateSelectedAssetId(prop.AssetID);
			base.OnLoadingEnded.Invoke();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
		}
		catch (Exception exception)
		{
			base.OnLoadingEnded.Invoke();
			ErrorHandler.HandleError(ErrorCodes.AbstractPropCreationFailure, exception);
		}
	}

	private void SelectIcon()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SelectIcon");
		}
		IconDefinition currentSelection = iconList.Definitions.FirstOrDefault((IconDefinition item) => item.IconId == typedView.IconId);
		iconDefinitionSelectorWindow = UIIconDefinitionSelectorWindowView.Display(currentSelection, SetCapturedIconTexture, base.transform);
		iconDefinitionSelectorWindow.CloseUnityEvent.AddListener(ClearIconDefinitionSelectorWindowHookups);
	}

	private void SetCapturedIconTexture(IconDefinition icon)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetCapturedIconTexture", icon);
		}
		typedView.IconId = icon.IconId;
	}

	private void ClearIconDefinitionSelectorWindowHookups()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ClearIconDefinitionSelectorWindowHookups");
		}
		iconDefinitionSelectorWindow.CloseUnityEvent.RemoveListener(ClearIconDefinitionSelectorWindowHookups);
		iconDefinitionSelectorWindow = null;
	}
}
