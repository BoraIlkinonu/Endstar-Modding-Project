using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Gameplay.LevelEditing;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoDetailView : UIGameObject, IUIViewable<PropLibrary.RuntimePropInfo>, IClearable, IUILoadingSpinnerViewCompatible
{
	public enum Modes
	{
		Write,
		Read,
		Restricted
	}

	private const string EDIT_SCRIPT_LABEL = "Edit Script";

	private const string VIEW_SCRIPT_LABEL = "View Script";

	[Header("UIRuntimePropInfoDetailView")]
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private UIButton editScriptButton;

	[SerializeField]
	private TextMeshProUGUI editScriptButtonText;

	[SerializeField]
	private GameObject cantEditScriptTooltip;

	[SerializeField]
	private TextMeshProUGUI versionText;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private bool isInjected;

	private bool propIsOpenSource;

	private bool scriptIsOpenSource;

	private bool canEditProp;

	private bool canEditScript;

	private SerializableGuid propId;

	private SerializableGuid scriptId;

	private bool isSubscribedToRightsManager;

	public Modes Mode { get; private set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public PropLibrary.RuntimePropInfo Model { get; private set; }

	private int LocalClientUserId => EndlessServices.Instance.CloudService.ActiveUserId;

	public async void View(PropLibrary.RuntimePropInfo model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		Clear();
		Model = model;
		nameText.text = model.PropData.Name;
		iconImage.sprite = model.Icon;
		GameLibrary gameLibrary = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary;
		isInjected = !gameLibrary.PropReferences.Any((AssetReference propReference) => propReference.AssetID == model.PropData.AssetID);
		propIsOpenSource = model.EndlessProp.Prop.OpenSource;
		scriptIsOpenSource = model.EndlessProp.ScriptComponent.Script.OpenSource;
		descriptionText.text = model.PropData.Description;
		if (verboseLogging)
		{
			DebugUtility.Log(JsonUtility.ToJson(model), this);
		}
		versionText.text = model.PropData.AssetVersion;
		if (isInjected || !model.PropData.HasScript)
		{
			return;
		}
		propId = model.PropData.AssetID;
		scriptId = model.PropData.ScriptAsset.AssetID;
		if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.IsInjectedProp(model.PropData.AssetID))
		{
			isSubscribedToRightsManager = true;
			canEditProp = false;
			canEditScript = false;
			HandleEditScriptButtonInteractabilityAndText();
			return;
		}
		OnLoadingStarted.Invoke();
		GetAllRolesResult[] array = await Task.WhenAll(new List<Task<GetAllRolesResult>>
		{
			MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(propId, OnPropRolesUpdated),
			MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(scriptId, OnScriptRolesUpdated)
		});
		if (array[0].WasChanged)
		{
			OnPropRolesUpdated(array[0].Roles);
		}
		if (array[1].WasChanged)
		{
			OnScriptRolesUpdated(array[1].Roles);
		}
		OnLoadingEnded.Invoke();
		MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(propId, OnPropRolesUpdated);
		MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(scriptId, OnScriptRolesUpdated);
		isSubscribedToRightsManager = true;
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		iconImage.sprite = null;
		canEditProp = false;
		canEditScript = false;
		editScriptButtonText.text = "Edit Script";
		editScriptButton.interactable = false;
		propId = SerializableGuid.Empty;
		scriptId = SerializableGuid.Empty;
		if (isSubscribedToRightsManager)
		{
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(propId, OnPropRolesUpdated);
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(scriptId, OnScriptRolesUpdated);
			isSubscribedToRightsManager = false;
		}
	}

	private void OnPropRolesUpdated(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPropRolesUpdated", userRoles);
		}
		Roles roleForUserId = userRoles.GetRoleForUserId(LocalClientUserId);
		canEditProp = roleForUserId.IsGreaterThanOrEqualTo(Roles.Editor);
		HandleEditScriptButtonInteractabilityAndText();
	}

	private void OnScriptRolesUpdated(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnScriptRolesUpdated", userRoles);
		}
		Roles roleForUserId = userRoles.GetRoleForUserId(LocalClientUserId);
		canEditScript = roleForUserId.IsGreaterThanOrEqualTo(Roles.Editor);
		HandleEditScriptButtonInteractabilityAndText();
	}

	private void HandleEditScriptButtonInteractabilityAndText()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleEditScriptButtonInteractabilityAndText");
		}
		bool flag = canEditProp && canEditScript;
		if (flag)
		{
			Mode = Modes.Write;
		}
		else
		{
			Mode = (scriptIsOpenSource ? Modes.Read : Modes.Restricted);
		}
		editScriptButtonText.text = (flag ? "Edit Script" : "View Script");
		UIButton uIButton = editScriptButton;
		Modes mode = Mode;
		uIButton.interactable = mode == Modes.Write || mode == Modes.Read;
		cantEditScriptTooltip.SetActive(!flag);
	}
}
