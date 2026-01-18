using System;
using Endless.Data;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001C8 RID: 456
	public class UIGenericPropCreationModalController : UIBasePropCreationModalController
	{
		// Token: 0x060006D1 RID: 1745 RVA: 0x00022A77 File Offset: 0x00020C77
		protected override void Start()
		{
			base.Start();
			this.typedView = this.view as UIGenericPropCreationModalView;
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x00022A90 File Offset: 0x00020C90
		protected override async void Create()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Create", Array.Empty<object>());
			}
			if (base.ValidatePropCreation())
			{
				base.OnLoadingStarted.Invoke();
				try
				{
					Prop prop = await this.typedView.GenericPropCreationScreenData.UploadProp(base.Name, base.Description, base.GrantEditRightsToCollaborators);
					this.propTool.UpdateSelectedAssetId(prop.AssetID);
					base.OnLoadingEnded.Invoke();
					MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
				}
				catch (Exception ex)
				{
					base.OnLoadingEnded.Invoke();
					ErrorHandler.HandleError(ErrorCodes.AbstractPropCreationFailure, ex, true, false);
				}
			}
		}

		// Token: 0x0400061E RID: 1566
		private UIGenericPropCreationModalView typedView;
	}
}
