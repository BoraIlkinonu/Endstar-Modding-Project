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
	// Token: 0x02000158 RID: 344
	public class UIRoleInteractabilityHandler : UIGameObject
	{
		// Token: 0x06000532 RID: 1330 RVA: 0x0001C4EC File Offset: 0x0001A6EC
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!this.interfaceTarget)
			{
				base.enabled = false;
				return;
			}
			if (!MatchmakingClientController.Instance.ActiveGameId.IsEmpty)
			{
				UIRoleInteractabilityHandler.Sources sources = this.source;
				if (sources != UIRoleInteractabilityHandler.Sources.ActiveGame)
				{
					if (sources != UIRoleInteractabilityHandler.Sources.ActiveLevel)
					{
						DebugUtility.LogNoEnumSupportError<UIRoleInteractabilityHandler.Sources>(this, this.source);
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
				MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.subscribedAssetId, this.ancestorId, new Action<IReadOnlyList<UserRole>>(this.OnUserRolesChanged));
				return;
			}
			this.subscribedAssetId = SerializableGuid.Empty;
			this.ancestorId = SerializableGuid.Empty;
			this.OnUserRolesChanged(Array.Empty<UserRole>());
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x0001C5DC File Offset: 0x0001A7DC
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.interfaceTarget == null || this.subscribedAssetId.IsEmpty)
			{
				return;
			}
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.subscribedAssetId, new Action<IReadOnlyList<UserRole>>(this.OnUserRolesChanged));
			this.subscribedAssetId = SerializableGuid.Empty;
		}

		// Token: 0x06000534 RID: 1332 RVA: 0x0001C640 File Offset: 0x0001A840
		private void OnUserRolesChanged(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserRolesChanged", new object[] { userRoles.Count });
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
			bool flag = roles.IsGreaterThanOrEqualTo(Roles.Editor);
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}", "localUserCanInteract", flag), this);
			}
			this.interfaceTarget.Interface.SetLocalUserCanInteract(flag);
		}

		// Token: 0x040004B0 RID: 1200
		[SerializeField]
		private InterfaceReference<IRoleInteractable> interfaceTarget;

		// Token: 0x040004B1 RID: 1201
		[SerializeField]
		private UIRoleInteractabilityHandler.Sources source;

		// Token: 0x040004B2 RID: 1202
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040004B3 RID: 1203
		private SerializableGuid subscribedAssetId;

		// Token: 0x040004B4 RID: 1204
		private SerializableGuid ancestorId;

		// Token: 0x02000159 RID: 345
		private enum Sources
		{
			// Token: 0x040004B6 RID: 1206
			ActiveGame,
			// Token: 0x040004B7 RID: 1207
			ActiveLevel
		}
	}
}
