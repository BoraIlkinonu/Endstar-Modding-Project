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
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001C3 RID: 451
	public class UIAbstractPropCreationModalController : UIBasePropCreationModalController
	{
		// Token: 0x060006AF RID: 1711 RVA: 0x000224AF File Offset: 0x000206AF
		protected override void Start()
		{
			base.Start();
			this.selectIconButton.onClick.AddListener(new UnityAction(this.SelectIcon));
			this.typedView = this.view as UIAbstractPropCreationModalView;
		}

		// Token: 0x060006B0 RID: 1712 RVA: 0x000224E4 File Offset: 0x000206E4
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.iconDefinitionSelectorWindow)
			{
				this.iconDefinitionSelectorWindow.Close();
			}
		}

		// Token: 0x060006B1 RID: 1713 RVA: 0x00022518 File Offset: 0x00020718
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
					Prop prop = await this.typedView.AbstractPropCreationScreenData.UploadProp(base.Name, base.Description, base.GrantEditRightsToCollaborators, this.typedView.IconId, this.typedView.FinalTexture);
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

		// Token: 0x060006B2 RID: 1714 RVA: 0x00022550 File Offset: 0x00020750
		private void SelectIcon()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SelectIcon", Array.Empty<object>());
			}
			IconDefinition iconDefinition = this.iconList.Definitions.FirstOrDefault((IconDefinition item) => item.IconId == this.typedView.IconId);
			this.iconDefinitionSelectorWindow = UIIconDefinitionSelectorWindowView.Display(iconDefinition, new Action<IconDefinition>(this.SetCapturedIconTexture), base.transform);
			this.iconDefinitionSelectorWindow.CloseUnityEvent.AddListener(new UnityAction(this.ClearIconDefinitionSelectorWindowHookups));
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x000225CC File Offset: 0x000207CC
		private void SetCapturedIconTexture(IconDefinition icon)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetCapturedIconTexture", new object[] { icon });
			}
			this.typedView.IconId = icon.IconId;
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x000225FC File Offset: 0x000207FC
		private void ClearIconDefinitionSelectorWindowHookups()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearIconDefinitionSelectorWindowHookups", Array.Empty<object>());
			}
			this.iconDefinitionSelectorWindow.CloseUnityEvent.RemoveListener(new UnityAction(this.ClearIconDefinitionSelectorWindowHookups));
			this.iconDefinitionSelectorWindow = null;
		}

		// Token: 0x04000604 RID: 1540
		[Header("UIAbstractPropCreationModalController")]
		[SerializeField]
		private UIButton selectIconButton;

		// Token: 0x04000605 RID: 1541
		[SerializeField]
		private IconList iconList;

		// Token: 0x04000606 RID: 1542
		private UIAbstractPropCreationModalView typedView;

		// Token: 0x04000607 RID: 1543
		private UIIconDefinitionSelectorWindowView iconDefinitionSelectorWindow;
	}
}
