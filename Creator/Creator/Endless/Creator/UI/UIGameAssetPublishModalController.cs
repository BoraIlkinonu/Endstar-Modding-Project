using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001A8 RID: 424
	public class UIGameAssetPublishModalController : UIGameObject
	{
		// Token: 0x170000A5 RID: 165
		// (get) Token: 0x06000636 RID: 1590 RVA: 0x0002013B File Offset: 0x0001E33B
		private UIGameAssetPublishModalModel Model
		{
			get
			{
				return this.modalView.Model;
			}
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x00020148 File Offset: 0x0001E348
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.betaVersionDropdown.OnValueChanged.AddListener(new UnityAction(this.OnBetaVersionChanged));
			this.publicVersionDropdown.OnValueChanged.AddListener(new UnityAction(this.OnPublicVersionChanged));
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x000201A5 File Offset: 0x0001E3A5
		private void OnBetaVersionChanged()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBetaVersionChanged", Array.Empty<object>());
			}
			this.HandleVersionChange(UIPublishStates.Beta, this.Model.VersionBeta, this.betaVersionDropdown, this.betaConfirmationExplanation);
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x000201DD File Offset: 0x0001E3DD
		private void OnPublicVersionChanged()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPublicVersionChanged", Array.Empty<object>());
			}
			this.HandleVersionChange(UIPublishStates.Public, this.Model.VersionPublic, this.publicVersionDropdown, this.publicConfirmationExplanation);
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x00020218 File Offset: 0x0001E418
		private void HandleVersionChange(UIPublishStates targetState, string currentVersion, UIDropdownVersion dropdown, string confirmationExplanation)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleVersionChange", new object[] { targetState, currentVersion, dropdown, confirmationExplanation });
			}
			string value = dropdown.Value;
			if (value == currentVersion)
			{
				return;
			}
			if (value == UIGameAssetPublishModalController.unpublishedState)
			{
				this.Model.Unpublish(targetState);
				return;
			}
			string text = string.Concat(new string[]
			{
				"Set v",
				dropdown.UserFacingVersion,
				" to ",
				targetState.ToEndlessCloudServicesCompatibleString(),
				"?\n",
				confirmationExplanation
			});
			this.ShowConfirmation(text, value, targetState);
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x000202C4 File Offset: 0x0001E4C4
		private void ShowConfirmation(string confirmationText, string newVersion, UIPublishStates state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ShowConfirmation", new object[] { confirmationText, newVersion, state });
			}
			string assetId = this.Model.GameAsset.AssetID;
			MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(confirmationText, delegate
			{
				this.Model.ChangePublishState(assetId, newVersion, state);
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.modalSourceView, UIModalManagerStackActions.ClearStack, new object[] { this.Model });
			}, new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack), UIModalManagerStackActions.MaintainStack);
		}

		// Token: 0x04000581 RID: 1409
		private static readonly string unpublishedState = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();

		// Token: 0x04000582 RID: 1410
		[SerializeField]
		private UIGameAssetPublishModalView modalView;

		// Token: 0x04000583 RID: 1411
		[SerializeField]
		private UIGameAssetPublishModalView modalSourceView;

		// Token: 0x04000584 RID: 1412
		[SerializeField]
		private UIDropdownVersion betaVersionDropdown;

		// Token: 0x04000585 RID: 1413
		[SerializeField]
		private UIDropdownVersion publicVersionDropdown;

		// Token: 0x04000586 RID: 1414
		[TextArea]
		[SerializeField]
		private string betaConfirmationExplanation = string.Format("Publishing will make your asset available to all players in the {0} channel.", UIPublishStates.Beta);

		// Token: 0x04000587 RID: 1415
		[TextArea]
		[SerializeField]
		private string publicConfirmationExplanation = string.Format("Publishing will make your asset available to all players in the {0} channel.", UIPublishStates.Public);

		// Token: 0x04000588 RID: 1416
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
