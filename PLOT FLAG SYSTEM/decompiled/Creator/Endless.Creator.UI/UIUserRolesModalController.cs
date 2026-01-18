using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIUserRolesModalController : UIGameObject
{
	public static Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>> ConfirmationAction;

	[SerializeField]
	private UIUserRolesModalModel model;

	[SerializeField]
	private UIRolesRadio assetRoleRadio;

	[SerializeField]
	private UIRolesRadio scriptRoleRadio;

	[SerializeField]
	private UIRolesRadio visualRoleRadio;

	[SerializeField]
	private UIRolesRadio prefabRoleRadio;

	[SerializeField]
	private UIButton confirmButton;

	[SerializeField]
	private UIButton cancelButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		confirmButton.onClick.AddListener(Confirm);
		cancelButton.onClick.AddListener(MonoBehaviourSingleton<UIUserRoleWizard>.Instance.Cancel);
		assetRoleRadio.OnValueChanged.AddListener(model.AssetRole.SetValueFromUser);
		scriptRoleRadio.OnValueChanged.AddListener(model.ScriptRole.SetValueFromUser);
		visualRoleRadio.OnValueChanged.AddListener(model.VisualRole.SetValueFromUser);
		prefabRoleRadio.OnValueChanged.AddListener(model.PrefabRole.SetValueFromUser);
	}

	private void OnDestroy()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDestroy");
		}
		ConfirmationAction = null;
	}

	private void Confirm()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Confirm");
		}
		ConfirmationAction?.Invoke(model.EveryRoleValueIsNone, new UIUserRolesModalModel.RoleValue(model.AssetRole), new UIUserRolesModalModel.RoleValue(model.ScriptRole), model.ScriptAssetId, new UIUserRolesModalModel.RoleValue(model.PrefabRole), model.PrefabAssetId, model.Roles);
	}
}
