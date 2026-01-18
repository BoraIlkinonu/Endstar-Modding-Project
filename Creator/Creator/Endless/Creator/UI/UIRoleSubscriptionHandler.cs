using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000279 RID: 633
	public class UIRoleSubscriptionHandler : UIGameObject
	{
		// Token: 0x06000A74 RID: 2676 RVA: 0x00030DB0 File Offset: 0x0002EFB0
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!MatchmakingClientController.Instance.ActiveGameId.IsEmpty)
			{
				UIRoleSubscriptionHandler.Sources sources = this.source;
				if (sources != UIRoleSubscriptionHandler.Sources.ActiveGame)
				{
					if (sources != UIRoleSubscriptionHandler.Sources.ActiveLevel)
					{
						DebugUtility.LogNoEnumSupportError<UIRoleSubscriptionHandler.Sources>(this, this.source);
						return;
					}
					this.subscribedAssetId = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
					this.ancestorId = MatchmakingClientController.Instance.ActiveGameId;
				}
				else
				{
					this.subscribedAssetId = MatchmakingClientController.Instance.ActiveGameId;
					this.ancestorId = SerializableGuid.Empty;
				}
				MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.subscribedAssetId, this.ancestorId, new Action<IReadOnlyList<UserRole>>(this.OnLevelUserRolesChanged));
				return;
			}
			this.subscribedAssetId = SerializableGuid.Empty;
			this.ancestorId = SerializableGuid.Empty;
			this.OnLevelUserRolesChanged(Array.Empty<UserRole>());
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x00030E8C File Offset: 0x0002F08C
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.subscribedAssetId.IsEmpty)
			{
				return;
			}
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.subscribedAssetId, new Action<IReadOnlyList<UserRole>>(this.OnLevelUserRolesChanged));
			this.subscribedAssetId = SerializableGuid.Empty;
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x00030EE8 File Offset: 0x0002F0E8
		private void OnLevelUserRolesChanged(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLevelUserRolesChanged", new object[] { userRoles.Count });
			}
			Roles roles;
			if (EndlessServices.Instance == null)
			{
				roles = Roles.None;
			}
			else
			{
				int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
				roles = userRoles.GetRoleForUserId(activeUserId);
			}
			this.roleSubscribable.Interface.OnLocalClientRoleChanged(roles);
		}

		// Token: 0x040008B0 RID: 2224
		[SerializeField]
		private InterfaceReference<IUIRoleSubscribable> roleSubscribable;

		// Token: 0x040008B1 RID: 2225
		[SerializeField]
		private UIRoleSubscriptionHandler.Sources source;

		// Token: 0x040008B2 RID: 2226
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040008B3 RID: 2227
		private SerializableGuid subscribedAssetId;

		// Token: 0x040008B4 RID: 2228
		private SerializableGuid ancestorId;

		// Token: 0x0200027A RID: 634
		private enum Sources
		{
			// Token: 0x040008B6 RID: 2230
			ActiveGame,
			// Token: 0x040008B7 RID: 2231
			ActiveLevel
		}
	}
}
