using System;
using Endless.Data;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIGenericPropCreationModalController : UIBasePropCreationModalController
{
	private UIGenericPropCreationModalView typedView;

	protected override void Start()
	{
		base.Start();
		typedView = view as UIGenericPropCreationModalView;
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
			Prop prop = await typedView.GenericPropCreationScreenData.UploadProp(base.Name, base.Description, base.GrantEditRightsToCollaborators);
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
}
