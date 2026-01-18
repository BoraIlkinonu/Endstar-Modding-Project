using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Data;
using Endless.Gameplay.RightsManagement;
using Endless.GraphQl;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIUserRolesModalModel : UIGameObject, IUILoadingSpinnerViewCompatible
{
	public class RoleValue
	{
		public readonly UnityEvent<Roles> OnOriginalValueChanged = new UnityEvent<Roles>();

		public readonly UnityEvent OnValueChangedFromServer = new UnityEvent();

		public readonly UnityEvent OnCleared = new UnityEvent();

		public bool UserChanged { get; private set; }

		public Roles OriginalValue { get; private set; } = Endless.Shared.Roles.None;

		public Roles Value { get; private set; } = Endless.Shared.Roles.None;

		public Roles LocalClientValue { get; private set; } = Endless.Shared.Roles.None;

		public RoleValue()
		{
		}

		public RoleValue(RoleValue cloneFrom)
		{
			UserChanged = cloneFrom.UserChanged;
			OriginalValue = cloneFrom.OriginalValue;
			Value = cloneFrom.Value;
			LocalClientValue = cloneFrom.LocalClientValue;
		}

		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", "UserChanged", UserChanged, "OriginalValue", OriginalValue, "Value", Value, "LocalClientValue", LocalClientValue);
		}

		public void SetValueFromServer(Roles value, Roles localClientValue)
		{
			OriginalValue = value;
			OnOriginalValueChanged.Invoke(OriginalValue);
			if (!UserChanged)
			{
				Value = value;
				LocalClientValue = localClientValue;
				UserChanged = false;
				OnValueChangedFromServer.Invoke();
			}
		}

		public void SetValueFromUser(Roles value)
		{
			Value = value;
			UserChanged = true;
		}

		public void Clear()
		{
			OriginalValue = Endless.Shared.Roles.None;
			Value = Endless.Shared.Roles.None;
			UserChanged = false;
			OnCleared.Invoke();
		}
	}

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public readonly RoleValue AssetRole = new RoleValue();

	public readonly RoleValue ScriptRole = new RoleValue();

	public readonly RoleValue VisualRole = new RoleValue();

	public readonly RoleValue PrefabRole = new RoleValue();

	private bool initialized;

	private RoleValue[] roles = Array.Empty<RoleValue>();

	private Prop prop;

	public User TargetUser { get; private set; }

	public Roles LocalClientRole { get; private set; }

	public UIUserRoleWizard.AssetTypes AssetType { get; private set; }

	public Roles TargetAssetRole { get; private set; }

	public AssetContexts Context { get; private set; }

	public Roles OriginalRole { get; private set; }

	public SerializableGuid AssetId { get; private set; }

	public SerializableGuid ScriptAssetId { get; private set; }

	public SerializableGuid PrefabAssetId { get; private set; }

	public int AssetRoleOwnerCount { get; private set; }

	public int ScriptRoleOwnerCount { get; private set; }

	public int VisualRoleOwnerCount { get; private set; }

	public int PrefabRoleOwnerCount { get; private set; }

	public IReadOnlyCollection<RoleValue> Roles => (IReadOnlyCollection<RoleValue>)(object)roles;

	public bool EveryRoleValueIsNone => roles.All((RoleValue role) => role.Value == Endless.Shared.Roles.None);

	private int LocalClientUserId => EndlessServices.Instance.CloudService.ActiveUserId;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		roles = new RoleValue[4] { AssetRole, ScriptRole, VisualRole, PrefabRole };
	}

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnLoadingStarted.AddListener(OnUserRoleWizardLoadingStarted);
		MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnLoadingEnded.AddListener(OnUserRoleWizardLoadingEnded);
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnLoadingStarted.RemoveListener(OnUserRoleWizardLoadingStarted);
		MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnLoadingEnded.RemoveListener(OnUserRoleWizardLoadingEnded);
	}

	public async void Initialize(User targetUser, Roles localClientRole, SerializableGuid assetId, UIUserRoleWizard.AssetTypes assetType, Roles targetAssetRole, AssetContexts context, Roles originalRole)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", targetUser, localClientRole, assetId, assetType, targetAssetRole, context, originalRole);
		}
		initialized = true;
		TargetUser = targetUser;
		LocalClientRole = localClientRole;
		AssetId = assetId;
		AssetType = assetType;
		TargetAssetRole = targetAssetRole;
		Context = context;
		OriginalRole = originalRole;
		if (Context != AssetContexts.NewGame)
		{
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(AssetId, OnAssetRoleUpdatedFromServer);
		}
		if (AssetType != UIUserRoleWizard.AssetTypes.Prop)
		{
			return;
		}
		OnLoadingStarted.Invoke();
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId);
		OnLoadingEnded.Invoke();
		if (graphQlResult.HasErrors)
		{
			ErrorHandler.HandleError(ErrorCodes.UIUserRolesModalModel_GetPropAsset, graphQlResult.GetErrorMessage());
			return;
		}
		prop = JsonConvert.DeserializeObject<Prop>(graphQlResult.GetDataMember().ToString());
		try
		{
			ScriptAssetId = ((prop.ScriptAsset == null) ? SerializableGuid.Empty : ((SerializableGuid)prop.ScriptAsset.AssetID));
			PrefabAssetId = ((prop.PrefabAsset == null) ? SerializableGuid.Empty : ((SerializableGuid)prop.PrefabAsset.AssetID));
			if (ScriptAssetId.IsEmpty)
			{
				ScriptRole.Clear();
			}
			else
			{
				MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(ScriptAssetId, OnScriptRoleUpdatedFromServer);
			}
			if (PrefabAssetId.IsEmpty)
			{
				PrefabRole.Clear();
			}
			else
			{
				MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(PrefabAssetId, OnPrefabRoleUpdatedFromServer);
			}
		}
		catch (Exception exception)
		{
			ErrorHandler.HandleError(ErrorCodes.UIUserRolesModalModel_RetrievingUsersWithRolesForAsset, exception);
		}
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		if (initialized)
		{
			if (Context != AssetContexts.NewGame)
			{
				MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(AssetId, OnAssetRoleUpdatedFromServer);
			}
			if (AssetType == UIUserRoleWizard.AssetTypes.Prop && prop != null)
			{
				if (ScriptAssetId != SerializableGuid.Empty)
				{
					MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(ScriptAssetId, OnScriptRoleUpdatedFromServer);
				}
				if (PrefabAssetId != SerializableGuid.Empty)
				{
					MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(PrefabAssetId, OnPrefabRoleUpdatedFromServer);
				}
			}
		}
		TargetUser = null;
		AssetId = SerializableGuid.Empty;
		prop = null;
		AssetRole.Clear();
		ScriptRole.Clear();
		VisualRole.Clear();
		PrefabRole.Clear();
		initialized = false;
	}

	private void OnAssetRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAssetRoleUpdatedFromServer", userRoles.Count);
		}
		AssetRoleOwnerCount = OnRoleUpdatedFromServer(userRoles, AssetRole);
	}

	private void OnScriptRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnScriptRoleUpdatedFromServer", userRoles.Count);
		}
		ScriptRoleOwnerCount = OnRoleUpdatedFromServer(userRoles, ScriptRole);
	}

	private void OnVisualRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnVisualRoleUpdatedFromServer", userRoles.Count);
		}
		VisualRoleOwnerCount = OnRoleUpdatedFromServer(userRoles, VisualRole);
	}

	private void OnPrefabRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPrefabRoleUpdatedFromServer", userRoles.Count);
		}
		PrefabRoleOwnerCount = OnRoleUpdatedFromServer(userRoles, PrefabRole);
	}

	private int OnRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles, RoleValue roleValue)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnRoleUpdatedFromServer", userRoles.Count, roleValue);
		}
		Roles roleForUserId = userRoles.GetRoleForUserId(TargetUser.Id);
		Roles roleForUserId2 = userRoles.GetRoleForUserId(LocalClientUserId);
		roleValue.SetValueFromServer(roleForUserId, roleForUserId2);
		return userRoles.Count((UserRole userRole) => userRole.Role == Endless.Shared.Roles.Owner);
	}

	private void OnUserRoleWizardLoadingStarted()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnUserRoleWizardLoadingStarted");
		}
		OnLoadingStarted.Invoke();
	}

	private void OnUserRoleWizardLoadingEnded()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnUserRoleWizardLoadingEnded");
		}
		OnLoadingEnded.Invoke();
	}
}
