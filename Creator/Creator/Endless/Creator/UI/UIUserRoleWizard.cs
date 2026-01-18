using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Data;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002B8 RID: 696
	public class UIUserRoleWizard : UIMonoBehaviourSingleton<UIUserRoleWizard>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000BC7 RID: 3015 RVA: 0x00037833 File Offset: 0x00035A33
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000BC8 RID: 3016 RVA: 0x0003783B File Offset: 0x00035A3B
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x06000BC9 RID: 3017 RVA: 0x00037844 File Offset: 0x00035A44
		public void InitializeAddUserFlow(UIUserRolesModel userRolesModel, List<User> usersToDisplay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeAddUserFlow", new object[]
				{
					userRolesModel,
					usersToDisplay.DebugSafeCount<User>()
				});
			}
			this.targetAssetRole = Roles.None;
			this.userIdsToOmitFromSelection = new HashSet<int>();
			this.usersToDisplay = usersToDisplay;
			this.Initialize(userRolesModel);
			this.flow = UIUserRoleWizard.Flows.Add;
			this.SetState(UIUserRoleWizard.States.UserSelection);
		}

		// Token: 0x06000BCA RID: 3018 RVA: 0x000378A8 File Offset: 0x00035AA8
		public void InitializeChangeUserFlow(UIUserRolesModel userRolesModel, UserRole userRoleForAsset, string userName)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "InitializeChangeUserFlow", new object[] { userRolesModel, userRoleForAsset, userName });
			}
			this.localClientRole = userRolesModel.LocalClientRole;
			this.userIdsToOmitFromSelection = new HashSet<int>(userRolesModel.UserIds);
			this.usersToDisplay = null;
			this.targetUser = new User(userRoleForAsset.UserId, null, userName);
			this.targetAssetRole = userRoleForAsset.Role;
			this.Initialize(userRolesModel);
			this.flow = UIUserRoleWizard.Flows.Change;
			this.SetState(UIUserRoleWizard.States.RoleSelection);
		}

		// Token: 0x06000BCB RID: 3019 RVA: 0x00037934 File Offset: 0x00035B34
		public void Cancel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Cancel", Array.Empty<object>());
			}
			switch (this.state)
			{
			case UIUserRoleWizard.States.Uninitialized:
			case UIUserRoleWizard.States.UserSelection:
				break;
			case UIUserRoleWizard.States.RoleSelection:
				UIUserRolesModalController.ConfirmationAction = (Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>)Delegate.Remove(UIUserRolesModalController.ConfirmationAction, new Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>(this.FinalizeRoleApplications));
				break;
			default:
				DebugUtility.LogNoEnumSupportError<UIUserRoleWizard.States>(this, "Cancel", this.state, Array.Empty<object>());
				break;
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			this.SetState(UIUserRoleWizard.States.Uninitialized);
		}

		// Token: 0x06000BCC RID: 3020 RVA: 0x000379C0 File Offset: 0x00035BC0
		private void Initialize(UIUserRolesModel userRolesModel)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { userRolesModel });
			}
			this.assetId = userRolesModel.AssetId;
			this.assetType = userRolesModel.AssetType;
			this.userRolesModel = userRolesModel;
			this.localClientRole = userRolesModel.LocalClientRole;
			this.userIdsToOmitFromSelection = new HashSet<int>(userRolesModel.UserIds);
			UIModalManager.OnModalClosedByUser = (Action<UIBaseModalView>)Delegate.Combine(UIModalManager.OnModalClosedByUser, new Action<UIBaseModalView>(this.OnModalClosedByUser));
		}

		// Token: 0x06000BCD RID: 3021 RVA: 0x00037A48 File Offset: 0x00035C48
		private void SetState(UIUserRoleWizard.States state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetState", new object[] { state });
			}
			if (this.state == state)
			{
				DebugUtility.LogError(this, "SetState", "UIUserRolesModel is already in that state!", new object[] { state });
				return;
			}
			this.state = state;
			switch (state)
			{
			case UIUserRoleWizard.States.Uninitialized:
				UIModalManager.OnModalClosedByUser = (Action<UIBaseModalView>)Delegate.Remove(UIModalManager.OnModalClosedByUser, new Action<UIBaseModalView>(this.OnModalClosedByUser));
				return;
			case UIUserRoleWizard.States.UserSelection:
			{
				if (this.assetType == UIUserRoleWizard.AssetTypes.Level)
				{
					this.DisplayLevelRoleUserSelectionWindowAsync();
					return;
				}
				User user = new User(EndlessServices.Instance.CloudService.ActiveUserId, null, EndlessServices.Instance.CloudService.ActiveUserName);
				List<User> list = new List<User> { user };
				if (!Application.isEditor && !Debug.isDebugBuild)
				{
					list.Add(this.endlessStudiosUserId.User);
				}
				list.AddRange(this.userRolesModel.Users);
				UIUserSearchWindowModel uiuserSearchWindowModel = new UIUserSearchWindowModel("Give Role", list, SelectionType.MustSelect1, new Action<List<object>>(this.SetTargetUserAndProceedStateToRoleSelection));
				if (this.userSelectionWindow)
				{
					this.ClearUserSearchWindowReference();
				}
				this.userSelectionWindow = UIUserSearchWindowView.Display(uiuserSearchWindowModel, null);
				this.userSelectionWindow.CloseUnityEvent.AddListener(new UnityAction(this.ClearUserSearchWindowReference));
				return;
			}
			case UIUserRoleWizard.States.RoleSelection:
				UIUserRolesModalController.ConfirmationAction = (Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>)Delegate.Combine(UIUserRolesModalController.ConfirmationAction, new Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>(this.FinalizeRoleApplications));
				MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.userRolesModalSource, UIModalManagerStackActions.ClearStack, new object[]
				{
					this.targetUser,
					this.localClientRole,
					this.assetId,
					this.assetType,
					this.targetAssetRole,
					this.userRolesModel.AssetName,
					this.userRolesModel.AssetContext,
					Roles.None
				});
				return;
			default:
				DebugUtility.LogNoEnumSupportError<UIUserRoleWizard.States>(this, "SetState", state, new object[] { state });
				return;
			}
		}

		// Token: 0x06000BCE RID: 3022 RVA: 0x00037C68 File Offset: 0x00035E68
		private async void DisplayLevelRoleUserSelectionWindowAsync()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayLevelRoleUserSelectionWindowAsync", Array.Empty<object>());
			}
			List<object> list = await this.GetUserWithGameRoleButNoLevelRoleAsync();
			UIIEnumerableWindowModel uiienumerableWindowModel = new UIIEnumerableWindowModel(-1, "Give Role", UIBaseIEnumerableView.ArrangementStyle.StraightVerticalVirtualized, new SelectionType?(SelectionType.MustSelect1), list, null, new List<object>(), null, new Action<List<object>>(this.SetTargetUserAndProceedStateToRoleSelection));
			if (this.userSelectionWindow)
			{
				this.ClearUserSearchWindowReference();
			}
			this.userSelectionWindow = UIIEnumerableWindowView.Display(uiienumerableWindowModel, null);
			this.userSelectionWindow.CloseUnityEvent.AddListener(new UnityAction(this.ClearUserSearchWindowReference));
		}

		// Token: 0x06000BCF RID: 3023 RVA: 0x00037CA0 File Offset: 0x00035EA0
		private async Task<List<object>> GetUserWithGameRoleButNoLevelRoleAsync()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetUserWithGameRoleButNoLevelRoleAsync", Array.Empty<object>());
			}
			GetAllRolesResult getAllRolesResult = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid, null, false);
			List<object> userWithGameRoleButNoLevelRole = new List<object>();
			foreach (UserRole userRole in getAllRolesResult.Roles)
			{
				if (userRole.InheritedFromParent)
				{
					string text = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(userRole.UserId);
					userWithGameRoleButNoLevelRole.Add(new User(userRole.UserId, null, text));
					userRole = null;
				}
			}
			IEnumerator<UserRole> enumerator = null;
			if (this.verboseLogging)
			{
				DebugUtility.DebugEnumerable<object>("userWithGameRoleButNoLevelRole", userWithGameRoleButNoLevelRole, this);
			}
			return userWithGameRoleButNoLevelRole;
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x00037CE4 File Offset: 0x00035EE4
		private void OnModalClosedByUser(UIBaseModalView modal)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnModalClosedByUser", new object[] { modal });
			}
			switch (this.state)
			{
			case UIUserRoleWizard.States.Uninitialized:
				break;
			case UIUserRoleWizard.States.UserSelection:
				this.SetState(this.state - 1);
				return;
			case UIUserRoleWizard.States.RoleSelection:
				UIUserRolesModalController.ConfirmationAction = (Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>)Delegate.Remove(UIUserRolesModalController.ConfirmationAction, new Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>(this.FinalizeRoleApplications));
				this.SetState((this.flow == UIUserRoleWizard.Flows.Add) ? (this.state - 1) : UIUserRoleWizard.States.Uninitialized);
				return;
			default:
				DebugUtility.LogNoEnumSupportError<UIUserRoleWizard.States>(this, "OnModalClosedByUser", this.state, new object[] { modal });
				break;
			}
		}

		// Token: 0x06000BD1 RID: 3025 RVA: 0x00037D90 File Offset: 0x00035F90
		private void SetTargetUserAndProceedStateToRoleSelection(List<object> target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetTargetUserAndProceedStateToRoleSelection", new object[] { target.Count });
			}
			if (target.Count == 0)
			{
				return;
			}
			User user = target[0] as User;
			this.targetUser = user;
			this.SetState(UIUserRoleWizard.States.RoleSelection);
		}

		// Token: 0x06000BD2 RID: 3026 RVA: 0x00037DE8 File Offset: 0x00035FE8
		private void FinalizeRoleApplications(bool everyRoleValueIsNone, UIUserRolesModalModel.RoleValue assetRole, UIUserRolesModalModel.RoleValue scriptRole, SerializableGuid scriptAssetId, UIUserRolesModalModel.RoleValue prefabRole, SerializableGuid prefabAssetId, IReadOnlyCollection<UIUserRolesModalModel.RoleValue> roles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "FinalizeRoleApplications", new object[] { everyRoleValueIsNone, assetRole, scriptRole, scriptAssetId, prefabRole, prefabAssetId, roles.Count });
			}
			UIUserRolesModalController.ConfirmationAction = (Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>)Delegate.Remove(UIUserRolesModalController.ConfirmationAction, new Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>>(this.FinalizeRoleApplications));
			this.everyRoleValueIsNone = everyRoleValueIsNone;
			this.assetRole = assetRole;
			this.scriptRole = scriptRole;
			this.scriptAssetId = scriptAssetId;
			this.prefabRole = prefabRole;
			this.prefabAssetId = prefabAssetId;
			int num = roles.Count((UIUserRolesModalModel.RoleValue role) => role.UserChanged && role.Value == Roles.Owner);
			if (num > 0)
			{
				string text = this.ownerRoleConfirmationBodySource;
				text = string.Format(text, num, (num > 1) ? "s" : string.Empty);
				MonoBehaviourSingleton<UIModalManager>.Instance.Confirm(text, new Action(this.OnConfirmationOfSetOwnerRoles), new Action(MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack), UIModalManagerStackActions.MaintainStack);
				return;
			}
			this.ApplyRoles();
		}

		// Token: 0x06000BD3 RID: 3027 RVA: 0x00037F12 File Offset: 0x00036112
		private void OnConfirmationOfSetOwnerRoles()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnConfirmationOfSetOwnerRoles", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			this.ApplyRoles();
		}

		// Token: 0x06000BD4 RID: 3028 RVA: 0x00037F3C File Offset: 0x0003613C
		private async void ApplyRoles()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyRoles", Array.Empty<object>());
			}
			this.OnLoadingStarted.Invoke();
			bool isIrrelevantAddition = this.flow == UIUserRoleWizard.Flows.Add && this.everyRoleValueIsNone;
			if (!isIrrelevantAddition && this.userRolesModel.AssetContext != AssetContexts.NewGame)
			{
				try
				{
					if (this.assetRole.UserChanged)
					{
						await this.ApplyRole(this.assetRole, this.assetId);
					}
				}
				catch (Exception ex)
				{
					DebugUtility.LogException(ex, this);
				}
				if (this.assetType == UIUserRoleWizard.AssetTypes.Prop)
				{
					try
					{
						if (this.scriptRole.UserChanged)
						{
							await this.ApplyRole(this.scriptRole, this.scriptAssetId);
						}
					}
					catch (Exception ex2)
					{
						DebugUtility.LogException(ex2, this);
					}
					try
					{
						if (this.prefabRole.UserChanged)
						{
							await this.ApplyRole(this.prefabRole, this.prefabAssetId);
						}
					}
					catch (Exception ex3)
					{
						DebugUtility.LogException(ex3, this);
					}
				}
			}
			if (this.userRolesModel.gameObject.activeInHierarchy && (!isIrrelevantAddition || this.userRolesModel.AssetContext == AssetContexts.NewGame))
			{
				UserRole userRole = new UserRole(this.targetUser.Id, this.assetRole.Value);
				UIUserRoleWizard.Flows flows = this.flow;
				if (flows != UIUserRoleWizard.Flows.Add)
				{
					if (flows != UIUserRoleWizard.Flows.Change)
					{
						DebugUtility.LogNoEnumSupportError<UIUserRoleWizard.Flows>(this, "ApplyRoles", this.flow, Array.Empty<object>());
					}
					else if (userRole.Role == Roles.None)
					{
						if (this.userRolesModel.AssetContext == AssetContexts.NewGame)
						{
							this.userRolesModel.RemoveUser(userRole);
						}
					}
					else
					{
						this.userRolesModel.UpdateRole(userRole);
					}
				}
				else if (this.userRolesModel.AssetContext == AssetContexts.NewGame && userRole.Role != Roles.None)
				{
					this.userRolesModel.AddToEnd(userRole);
				}
			}
			this.OnLoadingEnded.Invoke();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			this.SetState(UIUserRoleWizard.States.Uninitialized);
			this.OnComplete.Invoke();
		}

		// Token: 0x06000BD5 RID: 3029 RVA: 0x00037F74 File Offset: 0x00036174
		private Task ApplyRole(UIUserRolesModalModel.RoleValue roleValue, SerializableGuid targetAssetId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyRole", new object[] { roleValue, targetAssetId });
			}
			return MonoBehaviourSingleton<RightsManager>.Instance.SetRightForUserOnAsset(targetAssetId, this.targetUser.Id, roleValue.Value);
		}

		// Token: 0x06000BD6 RID: 3030 RVA: 0x00037FC4 File Offset: 0x000361C4
		private void ClearUserSearchWindowReference()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearUserSearchWindowReference", Array.Empty<object>());
			}
			if (this.state == UIUserRoleWizard.States.UserSelection)
			{
				this.SetState(UIUserRoleWizard.States.Uninitialized);
			}
			this.userSelectionWindow.CloseUnityEvent.RemoveListener(new UnityAction(this.ClearUserSearchWindowReference));
			this.userSelectionWindow = null;
		}

		// Token: 0x040009F7 RID: 2551
		public UnityEvent OnComplete = new UnityEvent();

		// Token: 0x040009F8 RID: 2552
		[SerializeField]
		private UIGameLibraryAssetDetailModalView gameLibraryAssetDetailModalSource;

		// Token: 0x040009F9 RID: 2553
		[SerializeField]
		private UIUserRolesModalView userRolesModalSource;

		// Token: 0x040009FA RID: 2554
		[SerializeField]
		private EndlessStudiosUserId endlessStudiosUserId;

		// Token: 0x040009FB RID: 2555
		[TextArea]
		[SerializeField]
		private string ownerRoleConfirmationBodySource = "Are you <u>sure</u> you wish to give a role of <b>Owner</b> to <u>{0}</u> asset{1}?\nYou will not be able to change that!";

		// Token: 0x040009FC RID: 2556
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040009FD RID: 2557
		private SerializableGuid assetId;

		// Token: 0x040009FE RID: 2558
		private UIUserRolesModalModel.RoleValue assetRole;

		// Token: 0x040009FF RID: 2559
		private UIUserRoleWizard.AssetTypes assetType;

		// Token: 0x04000A00 RID: 2560
		private bool everyRoleValueIsNone;

		// Token: 0x04000A01 RID: 2561
		private UIUserRoleWizard.Flows flow;

		// Token: 0x04000A02 RID: 2562
		private Roles localClientRole;

		// Token: 0x04000A03 RID: 2563
		private SerializableGuid prefabAssetId;

		// Token: 0x04000A04 RID: 2564
		private UIUserRolesModalModel.RoleValue prefabRole;

		// Token: 0x04000A05 RID: 2565
		private SerializableGuid scriptAssetId;

		// Token: 0x04000A06 RID: 2566
		private UIUserRolesModalModel.RoleValue scriptRole;

		// Token: 0x04000A07 RID: 2567
		private UIUserRoleWizard.States state;

		// Token: 0x04000A08 RID: 2568
		private Roles targetAssetRole = Roles.None;

		// Token: 0x04000A09 RID: 2569
		private User targetUser;

		// Token: 0x04000A0A RID: 2570
		private HashSet<int> userIdsToOmitFromSelection;

		// Token: 0x04000A0B RID: 2571
		private UIUserRolesModel userRolesModel;

		// Token: 0x04000A0C RID: 2572
		private List<User> usersToDisplay = new List<User>();

		// Token: 0x04000A0D RID: 2573
		private UIBaseWindowView userSelectionWindow;

		// Token: 0x020002B9 RID: 697
		public enum AssetTypes
		{
			// Token: 0x04000A11 RID: 2577
			Game,
			// Token: 0x04000A12 RID: 2578
			Level,
			// Token: 0x04000A13 RID: 2579
			Tileset,
			// Token: 0x04000A14 RID: 2580
			Prop,
			// Token: 0x04000A15 RID: 2581
			Audio
		}

		// Token: 0x020002BA RID: 698
		private enum Flows
		{
			// Token: 0x04000A17 RID: 2583
			Add,
			// Token: 0x04000A18 RID: 2584
			Change
		}

		// Token: 0x020002BB RID: 699
		private enum States
		{
			// Token: 0x04000A1A RID: 2586
			Uninitialized,
			// Token: 0x04000A1B RID: 2587
			UserSelection,
			// Token: 0x04000A1C RID: 2588
			RoleSelection
		}
	}
}
