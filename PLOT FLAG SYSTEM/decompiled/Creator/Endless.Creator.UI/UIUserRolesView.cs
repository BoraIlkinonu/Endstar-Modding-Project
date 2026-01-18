using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIUserRolesView : UIGameObject
{
	[SerializeField]
	private UIUserRolesModel model;

	[SerializeField]
	private bool blockActivationOfAddUserRoleButton;

	[SerializeField]
	private UIButton addUserRoleButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		model.OnLocalClientRoleSet.AddListener(HandleAddUserRoleButtonVisibility);
		model.OnLoadingStarted.AddListener(OnLoadingStarted);
		model.OnLoadingEnded.AddListener(OnLoadingEnded);
		HandleAddUserRoleButtonVisibility(model.LocalClientRole);
	}

	public void SetBlockActivationOfAddUserRoleButton(bool blockActivationOfAddUserRoleButton)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetBlockActivationOfAddUserRoleButton", blockActivationOfAddUserRoleButton);
		}
		this.blockActivationOfAddUserRoleButton = blockActivationOfAddUserRoleButton;
		if (addUserRoleButton.gameObject.activeSelf && blockActivationOfAddUserRoleButton)
		{
			addUserRoleButton.gameObject.SetActive(value: false);
		}
	}

	public void HandleAddUserRoleButtonVisibility(Roles localClientRole)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleAddUserRoleButtonVisibility", localClientRole);
		}
		bool active = localClientRole.IsGreaterThanOrEqualTo(Roles.Editor) && !blockActivationOfAddUserRoleButton && model.AssetContext != AssetContexts.GameInspectorPlay;
		addUserRoleButton.gameObject.SetActive(active);
	}

	private void OnLoadingStarted()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLoadingStarted");
		}
		addUserRoleButton.interactable = false;
	}

	private void OnLoadingEnded()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnLoadingEnded");
		}
		addUserRoleButton.interactable = true;
	}
}
