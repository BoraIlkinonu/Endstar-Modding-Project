using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000057 RID: 87
	public class UIListModelAddButtonHandler : UIGameObject, IValidatable
	{
		// Token: 0x0600019C RID: 412 RVA: 0x0000A1B0 File Offset: 0x000083B0
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (!this.iListModelTarget)
			{
				DebugUtility.LogError(this, "OnEnable", "iListModelTarget is null!", Array.Empty<object>());
				base.enabled = false;
				return;
			}
			if (!this.iListModelTarget.TryGetComponent<IListModel>(out this.iListModel))
			{
				DebugUtility.LogError("iListModelTarget does not implement an IListModel interface!", this);
				base.enabled = false;
				return;
			}
			if (!MatchmakingClientController.Instance.ActiveGameId.IsEmpty)
			{
				UIListModelAddButtonHandler.Sources sources = this.source;
				if (sources != UIListModelAddButtonHandler.Sources.ActiveGame)
				{
					if (sources != UIListModelAddButtonHandler.Sources.ActiveLevel)
					{
						DebugUtility.LogNoEnumSupportError<UIListModelAddButtonHandler.Sources>(this, this.source);
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

		// Token: 0x0600019D RID: 413 RVA: 0x0000A2DC File Offset: 0x000084DC
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.iListModelTarget == null || this.iListModel == null || !this.iListModel.DisplayAddButton || this.subscribedAssetId.IsEmpty)
			{
				return;
			}
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.subscribedAssetId, new Action<IReadOnlyList<UserRole>>(this.OnUserRolesChanged));
			this.subscribedAssetId = SerializableGuid.Empty;
		}

		// Token: 0x0600019E RID: 414 RVA: 0x0000A35C File Offset: 0x0000855C
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (DebugUtility.DebugIsNull("iListModelTarget", this.iListModelTarget, this))
			{
				return;
			}
			if (!this.iListModelTarget.TryGetComponent<IListModel>(out this.iListModel))
			{
				DebugUtility.LogError("iListModelTarget must implement an IListModel interface!", this);
			}
		}

		// Token: 0x0600019F RID: 415 RVA: 0x0000A3B4 File Offset: 0x000085B4
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
				DebugUtility.Log(string.Format("{0}: {1}", "canInteract", flag), this);
			}
			this.iListModel.SetAddButtonIsInteractable(flag);
		}

		// Token: 0x04000125 RID: 293
		[SerializeField]
		private GameObject iListModelTarget;

		// Token: 0x04000126 RID: 294
		[SerializeField]
		private UIListModelAddButtonHandler.Sources source;

		// Token: 0x04000127 RID: 295
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000128 RID: 296
		private SerializableGuid subscribedAssetId;

		// Token: 0x04000129 RID: 297
		private SerializableGuid ancestorId;

		// Token: 0x0400012A RID: 298
		private IListModel iListModel;

		// Token: 0x02000058 RID: 88
		private enum Sources
		{
			// Token: 0x0400012C RID: 300
			ActiveGame,
			// Token: 0x0400012D RID: 301
			ActiveLevel
		}
	}
}
