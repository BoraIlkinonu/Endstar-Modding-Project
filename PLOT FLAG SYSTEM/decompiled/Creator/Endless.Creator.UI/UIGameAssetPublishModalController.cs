using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAssetPublishModalController : UIGameObject
{
	private static readonly string unpublishedState = UIPublishStates.Unpublished.ToEndlessCloudServicesCompatibleString();

	[SerializeField]
	private UIGameAssetPublishModalView modalView;

	[SerializeField]
	private UIGameAssetPublishModalView modalSourceView;

	[SerializeField]
	private UIDropdownVersion betaVersionDropdown;

	[SerializeField]
	private UIDropdownVersion publicVersionDropdown;

	[TextArea]
	[SerializeField]
	private string betaConfirmationExplanation = $"Publishing will make your asset available to all players in the {UIPublishStates.Beta} channel.";

	[TextArea]
	[SerializeField]
	private string publicConfirmationExplanation = $"Publishing will make your asset available to all players in the {UIPublishStates.Public} channel.";

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIGameAssetPublishModalModel Model => modalView.Model;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		betaVersionDropdown.OnValueChanged.AddListener(OnBetaVersionChanged);
		publicVersionDropdown.OnValueChanged.AddListener(OnPublicVersionChanged);
	}

	private void OnBetaVersionChanged()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBetaVersionChanged");
		}
		HandleVersionChange(UIPublishStates.Beta, Model.VersionBeta, betaVersionDropdown, betaConfirmationExplanation);
	}

	private void OnPublicVersionChanged()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPublicVersionChanged");
		}
		HandleVersionChange(UIPublishStates.Public, Model.VersionPublic, publicVersionDropdown, publicConfirmationExplanation);
	}

	private void HandleVersionChange(UIPublishStates targetState, string currentVersion, UIDropdownVersion dropdown, string confirmationExplanation)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleVersionChange", targetState, currentVersion, dropdown, confirmationExplanation);
		}
		string value = dropdown.Value;
		if (!(value == currentVersion))
		{
			if (value == unpublishedState)
			{
				Model.Unpublish(targetState);
				return;
			}
			string confirmationText = "Set v" + dropdown.UserFacingVersion + " to " + targetState.ToEndlessCloudServicesCompatibleString() + "?\n" + confirmationExplanation;
			ShowConfirmation(confirmationText, value, targetState);
		}
	}

	private void ShowConfirmation(string confirmationText, string newVersion, UIPublishStates state)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ShowConfirmation", confirmationText, newVersion, state);
		}
		string assetId = Model.GameAsset.AssetID;
		MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(confirmationText, delegate
		{
			Model.ChangePublishState(assetId, newVersion, state);
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(modalSourceView, UIModalManagerStackActions.ClearStack, Model);
		}, MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack);
	}
}
