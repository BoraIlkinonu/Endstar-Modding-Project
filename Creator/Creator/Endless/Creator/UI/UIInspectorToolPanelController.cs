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

namespace Endless.Creator.UI
{
	// Token: 0x020002A4 RID: 676
	public class UIInspectorToolPanelController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000B43 RID: 2883 RVA: 0x00034B81 File Offset: 0x00032D81
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000B44 RID: 2884 RVA: 0x00034B89 File Offset: 0x00032D89
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000B45 RID: 2885 RVA: 0x00034B94 File Offset: 0x00032D94
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.label.onSubmit.AddListener(new UnityAction<string>(this.UpdateLabel));
			this.label.onEndEdit.AddListener(new UnityAction<string>(this.UpdateLabel));
			this.cancelEyeDropButton.onClick.AddListener(new UnityAction(this.CancelEyeDrop));
			this.reAddToGameLibraryButton.onClick.AddListener(new UnityAction(this.ReAddToGameLibrary));
			this.inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<InspectorTool>();
		}

		// Token: 0x06000B46 RID: 2886 RVA: 0x00034C39 File Offset: 0x00032E39
		private void UpdateLabel(string newLabel)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateLabel", new object[] { newLabel });
			}
			this.inspectorTool.HandleLabelChange(this.view.InstanceId, newLabel);
		}

		// Token: 0x06000B47 RID: 2887 RVA: 0x00034C6F File Offset: 0x00032E6F
		private void CancelEyeDrop()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelEyeDrop", Array.Empty<object>());
			}
			this.inspectorTool.CancelEyeDrop();
			this.inspectorTool.SetStateToInspect();
		}

		// Token: 0x06000B48 RID: 2888 RVA: 0x00034CA0 File Offset: 0x00032EA0
		private async void ReAddToGameLibrary()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ReAddToGameLibrary", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetVersionsAsync(this.view.AssetId, false);
			if (graphQlResult.HasErrors)
			{
				ErrorHandler.HandleError(ErrorCodes.UIInspectorToolPanelController_GetVersionsToReAddToGameLibrary, graphQlResult.GetErrorMessage(0), true, false);
				this.OnLoadingEnded.Invoke();
			}
			else
			{
				string[] parsedAndOrderedVersions = VersionUtilities.GetParsedAndOrderedVersions(graphQlResult.GetDataMember());
				if (parsedAndOrderedVersions.Length == 0)
				{
					ErrorHandler.HandleError(ErrorCodes.UIInspectorToolPanelController_NoVersions, graphQlResult.GetErrorMessage(0), true, false);
					this.OnLoadingEnded.Invoke();
				}
				string text = parsedAndOrderedVersions[0];
				AssetReference propReference = new AssetReference
				{
					AssetID = this.view.AssetId,
					AssetVersion = text,
					AssetType = "prop"
				};
				if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary.PropReferences.Any((AssetReference reference) => reference.AssetID == propReference.AssetID))
				{
					await MonoBehaviourSingleton<GameEditor>.Instance.SetPropVersionInGameLibrary(propReference.AssetID, propReference.AssetVersion);
				}
				else
				{
					await MonoBehaviourSingleton<GameEditor>.Instance.AddPropToGameLibrary(propReference);
				}
				this.OnLoadingEnded.Invoke();
				if (this.inspectorTool.IsActive)
				{
					this.view.Review();
				}
			}
		}

		// Token: 0x04000983 RID: 2435
		public static Action OnCancelEyeDrop;

		// Token: 0x04000984 RID: 2436
		[SerializeField]
		private UIInspectorToolPanelView view;

		// Token: 0x04000985 RID: 2437
		[SerializeField]
		private UIInputField label;

		// Token: 0x04000986 RID: 2438
		[SerializeField]
		private UIButton cancelEyeDropButton;

		// Token: 0x04000987 RID: 2439
		[SerializeField]
		private UIButton reAddToGameLibraryButton;

		// Token: 0x04000988 RID: 2440
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000989 RID: 2441
		private InspectorTool inspectorTool;
	}
}
