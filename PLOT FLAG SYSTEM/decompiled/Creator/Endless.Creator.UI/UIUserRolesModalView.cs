using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIUserRolesModalView : UIEscapableModalView
{
	[SerializeField]
	private UIUserRolesModalModel model;

	[SerializeField]
	private float propModalHeight = 900f;

	[SerializeField]
	private float nonPropModalHeight = 500f;

	[SerializeField]
	private LayoutElement modalContainerLayoutElement;

	[SerializeField]
	private TextMeshProUGUI userNameText;

	[SerializeField]
	private TextMeshProUGUI assetNameText;

	[SerializeField]
	private TextMeshProUGUI assetRoleRadioNameText;

	[SerializeField]
	private UIRolesRadio assetRoleRadio;

	[SerializeField]
	private UIRolesRadio scriptRoleRadio;

	[SerializeField]
	private UIRolesRadio visualRoleRadio;

	[SerializeField]
	private UIRolesRadio prefabRoleRadio;

	[SerializeField]
	private UIRolesRadio[] everyRoleRadio = Array.Empty<UIRolesRadio>();

	[SerializeField]
	private GameObject[] setActiveIfAssetTypeIsProp = Array.Empty<GameObject>();

	[SerializeField]
	private UIRolesDescriptionsDictionary rolesDescriptionsDictionary;

	[SerializeField]
	private TextMeshProUGUI roleText;

	[SerializeField]
	private TextMeshProUGUI roleDescriptionText;

	[SerializeField]
	private UIContentSizeFitter roleContentSizeFitter;

	[SerializeField]
	private string defaultRoleDescriptionText = "Select a pip to learn about the role.";

	private AssetContexts context;

	protected override void Start()
	{
		base.Start();
		model.AssetRole.OnValueChangedFromServer.AddListener(ViewAssetRole);
		model.ScriptRole.OnValueChangedFromServer.AddListener(ViewScriptRole);
		model.VisualRole.OnValueChangedFromServer.AddListener(ViewVisualRole);
		model.PrefabRole.OnValueChangedFromServer.AddListener(ViewPrefabRole);
		model.AssetRole.OnCleared.AddListener(HideAssetRole);
		model.ScriptRole.OnCleared.AddListener(HideScriptRole);
		model.VisualRole.OnCleared.AddListener(HideVisualRole);
		model.PrefabRole.OnCleared.AddListener(HidePrefabRole);
		UIRolesRadio[] array = everyRoleRadio;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnValueChanged.AddListener(ViewRoleDescription);
		}
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		User user = (User)modalData[0];
		Roles localClientRole = (Roles)modalData[1];
		SerializableGuid assetId = (SerializableGuid)modalData[2];
		UIUserRoleWizard.AssetTypes assetTypes = (UIUserRoleWizard.AssetTypes)modalData[3];
		Roles roles = (Roles)modalData[4];
		string text = (string)modalData[5];
		context = (AssetContexts)modalData[6];
		Roles roles2 = (Roles)modalData[7];
		model.Initialize(user, localClientRole, assetId, assetTypes, roles, context, roles2);
		bool flag = assetTypes == UIUserRoleWizard.AssetTypes.Prop;
		modalContainerLayoutElement.minHeight = (flag ? propModalHeight : nonPropModalHeight);
		userNameText.text = user.UserName;
		assetNameText.text = text;
		assetRoleRadioNameText.gameObject.SetActive(flag);
		roleText.text = "Role";
		roleDescriptionText.text = defaultRoleDescriptionText;
		roleContentSizeFitter.RequestLayout();
		GameObject[] array = setActiveIfAssetTypeIsProp;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(flag);
		}
		if (context == AssetContexts.NewGame)
		{
			assetRoleRadio.EnableControls();
			assetRoleRadio.SetOriginalValue(roles2);
			assetRoleRadio.SetValue(roles, triggerOnValueChanged: true);
			assetRoleRadio.gameObject.SetActive(value: true);
		}
		else
		{
			ViewAssetRole();
		}
	}

	public override void Close()
	{
		base.Close();
		model.Clear();
	}

	private void ViewRoleDescription(Roles role)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewRoleDescription", role);
		}
		roleText.text = role.ToString();
		roleDescriptionText.text = rolesDescriptionsDictionary[role].Game;
		roleContentSizeFitter.RequestLayout();
	}

	private void ViewAssetRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "ViewAssetRole", string.Format("{0}: {1}, {2}: {3}", "AssetRole", model.AssetRole, "AssetRoleOwnerCount", model.AssetRoleOwnerCount));
		}
		HandleRoleRadio(assetRoleRadio, model.AssetRole, model.AssetRoleOwnerCount);
	}

	private void ViewScriptRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "ViewScriptRole", string.Format("{0}: {1}, {2}: {3}", "ScriptRole", model.ScriptRole, "ScriptRoleOwnerCount", model.ScriptRoleOwnerCount));
		}
		HandleRoleRadio(scriptRoleRadio, model.ScriptRole, model.ScriptRoleOwnerCount);
	}

	private void ViewVisualRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "ViewVisualRole", string.Format("{0}: {1}, {2}: {3}", "VisualRole", model.VisualRole, "VisualRole", model.VisualRoleOwnerCount));
		}
		HandleRoleRadio(visualRoleRadio, model.VisualRole, model.VisualRoleOwnerCount);
	}

	private void ViewPrefabRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "ViewPrefabRole", string.Format("{0}: {1}, {2}: {3}", "PrefabRole", model.PrefabRole, "PrefabRoleOwnerCount", model.PrefabRoleOwnerCount));
		}
		HandleRoleRadio(prefabRoleRadio, model.PrefabRole, model.PrefabRoleOwnerCount);
	}

	private void HandleRoleRadio(UIRolesRadio rolesRadio, UIUserRolesModalModel.RoleValue roleValue, int ownerCount)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleRoleRadio", rolesRadio.gameObject.name, roleValue, ownerCount);
		}
		rolesRadio.SetValue(roleValue.Value, triggerOnValueChanged: false);
		rolesRadio.SetOriginalValue(roleValue.OriginalValue);
		if (roleValue.OriginalValue.IsGreaterThanOrEqualTo(Roles.Owner))
		{
			if (model.TargetUser.Id == EndlessServices.Instance.CloudService.ActiveUserId && ownerCount > 1)
			{
				rolesRadio.EnableControls();
			}
			else
			{
				rolesRadio.DisableControls();
			}
		}
		else if (roleValue.LocalClientValue.IsGreaterThanOrEqualTo(Roles.Owner))
		{
			rolesRadio.EnableControls();
		}
		else
		{
			rolesRadio.DisableControls(roleValue.LocalClientValue.Level());
		}
		rolesRadio.gameObject.SetActive(value: true);
	}

	private void HideAssetRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideAssetRole");
		}
		if (context != AssetContexts.NewGame)
		{
			assetRoleRadio.gameObject.SetActive(value: false);
		}
	}

	private void HideScriptRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideScriptRole");
		}
		scriptRoleRadio.gameObject.SetActive(value: false);
	}

	private void HideVisualRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HideVisualRole");
		}
		visualRoleRadio.gameObject.SetActive(value: false);
	}

	private void HidePrefabRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "HidePrefabRole");
		}
		prefabRoleRadio.gameObject.SetActive(value: false);
	}
}
