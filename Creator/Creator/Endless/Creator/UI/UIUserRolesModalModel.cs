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

namespace Endless.Creator.UI
{
	// Token: 0x020001DB RID: 475
	public class UIUserRolesModalModel : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000725 RID: 1829 RVA: 0x00023FE8 File Offset: 0x000221E8
		// (set) Token: 0x06000726 RID: 1830 RVA: 0x00023FF0 File Offset: 0x000221F0
		public User TargetUser { get; private set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x06000727 RID: 1831 RVA: 0x00023FF9 File Offset: 0x000221F9
		// (set) Token: 0x06000728 RID: 1832 RVA: 0x00024001 File Offset: 0x00022201
		public Roles LocalClientRole { get; private set; }

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x06000729 RID: 1833 RVA: 0x0002400A File Offset: 0x0002220A
		// (set) Token: 0x0600072A RID: 1834 RVA: 0x00024012 File Offset: 0x00022212
		public UIUserRoleWizard.AssetTypes AssetType { get; private set; }

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x0600072B RID: 1835 RVA: 0x0002401B File Offset: 0x0002221B
		// (set) Token: 0x0600072C RID: 1836 RVA: 0x00024023 File Offset: 0x00022223
		public Roles TargetAssetRole { get; private set; }

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x0600072D RID: 1837 RVA: 0x0002402C File Offset: 0x0002222C
		// (set) Token: 0x0600072E RID: 1838 RVA: 0x00024034 File Offset: 0x00022234
		public AssetContexts Context { get; private set; }

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x0600072F RID: 1839 RVA: 0x0002403D File Offset: 0x0002223D
		// (set) Token: 0x06000730 RID: 1840 RVA: 0x00024045 File Offset: 0x00022245
		public Roles OriginalRole { get; private set; }

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x06000731 RID: 1841 RVA: 0x0002404E File Offset: 0x0002224E
		// (set) Token: 0x06000732 RID: 1842 RVA: 0x00024056 File Offset: 0x00022256
		public SerializableGuid AssetId { get; private set; }

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000733 RID: 1843 RVA: 0x0002405F File Offset: 0x0002225F
		// (set) Token: 0x06000734 RID: 1844 RVA: 0x00024067 File Offset: 0x00022267
		public SerializableGuid ScriptAssetId { get; private set; }

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000735 RID: 1845 RVA: 0x00024070 File Offset: 0x00022270
		// (set) Token: 0x06000736 RID: 1846 RVA: 0x00024078 File Offset: 0x00022278
		public SerializableGuid PrefabAssetId { get; private set; }

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000737 RID: 1847 RVA: 0x00024081 File Offset: 0x00022281
		// (set) Token: 0x06000738 RID: 1848 RVA: 0x00024089 File Offset: 0x00022289
		public int AssetRoleOwnerCount { get; private set; }

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x06000739 RID: 1849 RVA: 0x00024092 File Offset: 0x00022292
		// (set) Token: 0x0600073A RID: 1850 RVA: 0x0002409A File Offset: 0x0002229A
		public int ScriptRoleOwnerCount { get; private set; }

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x0600073B RID: 1851 RVA: 0x000240A3 File Offset: 0x000222A3
		// (set) Token: 0x0600073C RID: 1852 RVA: 0x000240AB File Offset: 0x000222AB
		public int VisualRoleOwnerCount { get; private set; }

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x0600073D RID: 1853 RVA: 0x000240B4 File Offset: 0x000222B4
		// (set) Token: 0x0600073E RID: 1854 RVA: 0x000240BC File Offset: 0x000222BC
		public int PrefabRoleOwnerCount { get; private set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x0600073F RID: 1855 RVA: 0x000240C5 File Offset: 0x000222C5
		public IReadOnlyCollection<UIUserRolesModalModel.RoleValue> Roles
		{
			get
			{
				return this.roles;
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000740 RID: 1856 RVA: 0x000240CD File Offset: 0x000222CD
		public bool EveryRoleValueIsNone
		{
			get
			{
				return this.roles.All((UIUserRolesModalModel.RoleValue role) => role.Value == Endless.Shared.Roles.None);
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x06000741 RID: 1857 RVA: 0x000240F9 File Offset: 0x000222F9
		private int LocalClientUserId
		{
			get
			{
				return EndlessServices.Instance.CloudService.ActiveUserId;
			}
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x0002410C File Offset: 0x0002230C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.roles = new UIUserRolesModalModel.RoleValue[] { this.AssetRole, this.ScriptRole, this.VisualRole, this.PrefabRole };
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x00024164 File Offset: 0x00022364
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnLoadingStarted.AddListener(new UnityAction(this.OnUserRoleWizardLoadingStarted));
			MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnLoadingEnded.AddListener(new UnityAction(this.OnUserRoleWizardLoadingEnded));
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x000241C0 File Offset: 0x000223C0
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnLoadingStarted.RemoveListener(new UnityAction(this.OnUserRoleWizardLoadingStarted));
			MonoBehaviourSingleton<UIUserRoleWizard>.Instance.OnLoadingEnded.RemoveListener(new UnityAction(this.OnUserRoleWizardLoadingEnded));
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x06000745 RID: 1861 RVA: 0x0002421B File Offset: 0x0002241B
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000746 RID: 1862 RVA: 0x00024223 File Offset: 0x00022423
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000747 RID: 1863 RVA: 0x0002422C File Offset: 0x0002242C
		public async void Initialize(User targetUser, Roles localClientRole, SerializableGuid assetId, UIUserRoleWizard.AssetTypes assetType, Roles targetAssetRole, AssetContexts context, Roles originalRole)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { targetUser, localClientRole, assetId, assetType, targetAssetRole, context, originalRole });
			}
			this.initialized = true;
			this.TargetUser = targetUser;
			this.LocalClientRole = localClientRole;
			this.AssetId = assetId;
			this.AssetType = assetType;
			this.TargetAssetRole = targetAssetRole;
			this.Context = context;
			this.OriginalRole = originalRole;
			if (this.Context != AssetContexts.NewGame)
			{
				MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.AssetId, new Action<IReadOnlyList<UserRole>>(this.OnAssetRoleUpdatedFromServer));
			}
			if (this.AssetType == UIUserRoleWizard.AssetTypes.Prop)
			{
				this.OnLoadingStarted.Invoke();
				GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.GetAssetAsync(assetId, "", null, false, 10);
				this.OnLoadingEnded.Invoke();
				if (graphQlResult.HasErrors)
				{
					ErrorHandler.HandleError(ErrorCodes.UIUserRolesModalModel_GetPropAsset, graphQlResult.GetErrorMessage(0), true, false);
				}
				else
				{
					this.prop = JsonConvert.DeserializeObject<Prop>(graphQlResult.GetDataMember().ToString());
					try
					{
						this.ScriptAssetId = ((this.prop.ScriptAsset == null) ? SerializableGuid.Empty : this.prop.ScriptAsset.AssetID);
						this.PrefabAssetId = ((this.prop.PrefabAsset == null) ? SerializableGuid.Empty : this.prop.PrefabAsset.AssetID);
						if (this.ScriptAssetId.IsEmpty)
						{
							this.ScriptRole.Clear();
						}
						else
						{
							MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.ScriptAssetId, new Action<IReadOnlyList<UserRole>>(this.OnScriptRoleUpdatedFromServer));
						}
						if (this.PrefabAssetId.IsEmpty)
						{
							this.PrefabRole.Clear();
						}
						else
						{
							MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.PrefabAssetId, new Action<IReadOnlyList<UserRole>>(this.OnPrefabRoleUpdatedFromServer));
						}
					}
					catch (Exception ex)
					{
						ErrorHandler.HandleError(ErrorCodes.UIUserRolesModalModel_RetrievingUsersWithRolesForAsset, ex, true, false);
					}
				}
			}
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x000242A0 File Offset: 0x000224A0
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			if (this.initialized)
			{
				if (this.Context != AssetContexts.NewGame)
				{
					MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.AssetId, new Action<IReadOnlyList<UserRole>>(this.OnAssetRoleUpdatedFromServer));
				}
				if (this.AssetType == UIUserRoleWizard.AssetTypes.Prop && this.prop != null)
				{
					if (this.ScriptAssetId != SerializableGuid.Empty)
					{
						MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.ScriptAssetId, new Action<IReadOnlyList<UserRole>>(this.OnScriptRoleUpdatedFromServer));
					}
					if (this.PrefabAssetId != SerializableGuid.Empty)
					{
						MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.PrefabAssetId, new Action<IReadOnlyList<UserRole>>(this.OnPrefabRoleUpdatedFromServer));
					}
				}
			}
			this.TargetUser = null;
			this.AssetId = SerializableGuid.Empty;
			this.prop = null;
			this.AssetRole.Clear();
			this.ScriptRole.Clear();
			this.VisualRole.Clear();
			this.PrefabRole.Clear();
			this.initialized = false;
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x000243AE File Offset: 0x000225AE
		private void OnAssetRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAssetRoleUpdatedFromServer", new object[] { userRoles.Count });
			}
			this.AssetRoleOwnerCount = this.OnRoleUpdatedFromServer(userRoles, this.AssetRole);
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x000243EA File Offset: 0x000225EA
		private void OnScriptRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScriptRoleUpdatedFromServer", new object[] { userRoles.Count });
			}
			this.ScriptRoleOwnerCount = this.OnRoleUpdatedFromServer(userRoles, this.ScriptRole);
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x00024426 File Offset: 0x00022626
		private void OnVisualRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnVisualRoleUpdatedFromServer", new object[] { userRoles.Count });
			}
			this.VisualRoleOwnerCount = this.OnRoleUpdatedFromServer(userRoles, this.VisualRole);
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x00024462 File Offset: 0x00022662
		private void OnPrefabRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPrefabRoleUpdatedFromServer", new object[] { userRoles.Count });
			}
			this.PrefabRoleOwnerCount = this.OnRoleUpdatedFromServer(userRoles, this.PrefabRole);
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x000244A0 File Offset: 0x000226A0
		private int OnRoleUpdatedFromServer(IReadOnlyList<UserRole> userRoles, UIUserRolesModalModel.RoleValue roleValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnRoleUpdatedFromServer", new object[] { userRoles.Count, roleValue });
			}
			Roles roleForUserId = userRoles.GetRoleForUserId(this.TargetUser.Id);
			Roles roleForUserId2 = userRoles.GetRoleForUserId(this.LocalClientUserId);
			roleValue.SetValueFromServer(roleForUserId, roleForUserId2);
			return userRoles.Count((UserRole userRole) => userRole.Role == Endless.Shared.Roles.Owner);
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x00024524 File Offset: 0x00022724
		private void OnUserRoleWizardLoadingStarted()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserRoleWizardLoadingStarted", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x00024549 File Offset: 0x00022749
		private void OnUserRoleWizardLoadingEnded()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserRoleWizardLoadingEnded", Array.Empty<object>());
			}
			this.OnLoadingEnded.Invoke();
		}

		// Token: 0x04000675 RID: 1653
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000676 RID: 1654
		public readonly UIUserRolesModalModel.RoleValue AssetRole = new UIUserRolesModalModel.RoleValue();

		// Token: 0x04000677 RID: 1655
		public readonly UIUserRolesModalModel.RoleValue ScriptRole = new UIUserRolesModalModel.RoleValue();

		// Token: 0x04000678 RID: 1656
		public readonly UIUserRolesModalModel.RoleValue VisualRole = new UIUserRolesModalModel.RoleValue();

		// Token: 0x04000679 RID: 1657
		public readonly UIUserRolesModalModel.RoleValue PrefabRole = new UIUserRolesModalModel.RoleValue();

		// Token: 0x0400067A RID: 1658
		private bool initialized;

		// Token: 0x0400067B RID: 1659
		private UIUserRolesModalModel.RoleValue[] roles = Array.Empty<UIUserRolesModalModel.RoleValue>();

		// Token: 0x0400067C RID: 1660
		private Prop prop;

		// Token: 0x020001DC RID: 476
		public class RoleValue
		{
			// Token: 0x170000DD RID: 221
			// (get) Token: 0x06000751 RID: 1873 RVA: 0x000245D0 File Offset: 0x000227D0
			// (set) Token: 0x06000752 RID: 1874 RVA: 0x000245D8 File Offset: 0x000227D8
			public bool UserChanged { get; private set; }

			// Token: 0x170000DE RID: 222
			// (get) Token: 0x06000753 RID: 1875 RVA: 0x000245E1 File Offset: 0x000227E1
			// (set) Token: 0x06000754 RID: 1876 RVA: 0x000245E9 File Offset: 0x000227E9
			public Roles OriginalValue { get; private set; } = Endless.Shared.Roles.None;

			// Token: 0x170000DF RID: 223
			// (get) Token: 0x06000755 RID: 1877 RVA: 0x000245F2 File Offset: 0x000227F2
			// (set) Token: 0x06000756 RID: 1878 RVA: 0x000245FA File Offset: 0x000227FA
			public Roles Value { get; private set; } = Endless.Shared.Roles.None;

			// Token: 0x06000757 RID: 1879 RVA: 0x00024603 File Offset: 0x00022803
			public RoleValue()
			{
			}

			// Token: 0x06000758 RID: 1880 RVA: 0x00024644 File Offset: 0x00022844
			public RoleValue(UIUserRolesModalModel.RoleValue cloneFrom)
			{
				this.UserChanged = cloneFrom.UserChanged;
				this.OriginalValue = cloneFrom.OriginalValue;
				this.Value = cloneFrom.Value;
				this.LocalClientValue = cloneFrom.LocalClientValue;
			}

			// Token: 0x170000E0 RID: 224
			// (get) Token: 0x06000759 RID: 1881 RVA: 0x000246BD File Offset: 0x000228BD
			// (set) Token: 0x0600075A RID: 1882 RVA: 0x000246C5 File Offset: 0x000228C5
			public Roles LocalClientValue { get; private set; } = Endless.Shared.Roles.None;

			// Token: 0x0600075B RID: 1883 RVA: 0x000246D0 File Offset: 0x000228D0
			public override string ToString()
			{
				return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", new object[] { "UserChanged", this.UserChanged, "OriginalValue", this.OriginalValue, "Value", this.Value, "LocalClientValue", this.LocalClientValue });
			}

			// Token: 0x0600075C RID: 1884 RVA: 0x00024748 File Offset: 0x00022948
			public void SetValueFromServer(Roles value, Roles localClientValue)
			{
				this.OriginalValue = value;
				this.OnOriginalValueChanged.Invoke(this.OriginalValue);
				if (this.UserChanged)
				{
					return;
				}
				this.Value = value;
				this.LocalClientValue = localClientValue;
				this.UserChanged = false;
				this.OnValueChangedFromServer.Invoke();
			}

			// Token: 0x0600075D RID: 1885 RVA: 0x00024796 File Offset: 0x00022996
			public void SetValueFromUser(Roles value)
			{
				this.Value = value;
				this.UserChanged = true;
			}

			// Token: 0x0600075E RID: 1886 RVA: 0x000247A6 File Offset: 0x000229A6
			public void Clear()
			{
				this.OriginalValue = Endless.Shared.Roles.None;
				this.Value = Endless.Shared.Roles.None;
				this.UserChanged = false;
				this.OnCleared.Invoke();
			}

			// Token: 0x0400068F RID: 1679
			public readonly UnityEvent<Roles> OnOriginalValueChanged = new UnityEvent<Roles>();

			// Token: 0x04000690 RID: 1680
			public readonly UnityEvent OnValueChangedFromServer = new UnityEvent();

			// Token: 0x04000691 RID: 1681
			public readonly UnityEvent OnCleared = new UnityEvent();
		}
	}
}
