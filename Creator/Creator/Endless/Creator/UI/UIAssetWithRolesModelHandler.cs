using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Assets;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.Debugging;
using Runtime.Shared.Matchmaking;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200009E RID: 158
	public abstract class UIAssetWithRolesModelHandler<T> : UIAssetModelHandler<T> where T : Asset
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x06000280 RID: 640 RVA: 0x00011546 File Offset: 0x0000F746
		// (set) Token: 0x06000281 RID: 641 RVA: 0x0001154E File Offset: 0x0000F74E
		public Roles LocalClientRole { get; private set; } = Roles.None;

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000282 RID: 642 RVA: 0x00011557 File Offset: 0x0000F757
		// (set) Token: 0x06000283 RID: 643 RVA: 0x0001155F File Offset: 0x0000F75F
		public IEnumerable<UserRole> UserRoles { get; private set; }

		// Token: 0x06000284 RID: 644 RVA: 0x00011568 File Offset: 0x0000F768
		public override void Clear()
		{
			base.Clear();
			if (base.Model != null)
			{
				MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(base.Model.AssetID, new Action<IReadOnlyList<UserRole>>(this.AssetRightsUpdated));
			}
			this.SetLocalClientRole(Roles.None);
			this.SetUserRoles(Array.Empty<UserRole>());
		}

		// Token: 0x06000285 RID: 645 RVA: 0x000115C8 File Offset: 0x0000F7C8
		public void GetAllUsersWithRolesForAsset()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("GetAllUsersWithRolesForAsset", this);
			}
			base.OnLoadingStarted.Invoke();
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(base.Model.AssetID, new Action<IReadOnlyList<UserRole>>(this.AssetRightsUpdated));
		}

		// Token: 0x06000286 RID: 646 RVA: 0x00011620 File Offset: 0x0000F820
		private void AssetRightsUpdated(IReadOnlyList<UserRole> userRoles)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "AssetRightsUpdated", "userRoles", userRoles.Count), this);
			}
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			UserRole userRole = userRoles.FirstOrDefault((UserRole role) => role.UserId == activeUserId);
			base.OnLoadingEnded.Invoke();
			if (userRole != null)
			{
				this.LocalClientRole = userRole.Role;
			}
			else
			{
				this.LocalClientRole = Roles.None;
			}
			this.OnLocalClientRoleSet.Invoke(this.LocalClientRole);
		}

		// Token: 0x06000287 RID: 647 RVA: 0x000116BC File Offset: 0x0000F8BC
		private void SetLocalClientRole(Roles newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetLocalClientRole", "newValue", newValue), this);
			}
			this.LocalClientRole = newValue;
			this.OnLocalClientRoleSet.Invoke(this.LocalClientRole);
		}

		// Token: 0x06000288 RID: 648 RVA: 0x00011709 File Offset: 0x0000F909
		private void SetUserRoles(IEnumerable<UserRole> newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetUserRoles", "newValue", newValue), this);
			}
			this.UserRoles = newValue;
			this.OnUserRolesSet.Invoke(this.UserRoles);
		}

		// Token: 0x040002C1 RID: 705
		public UnityEvent<Roles> OnLocalClientRoleSet = new UnityEvent<Roles>();

		// Token: 0x040002C2 RID: 706
		public UnityEvent<IEnumerable<UserRole>> OnUserRolesSet = new UnityEvent<IEnumerable<UserRole>>();
	}
}
