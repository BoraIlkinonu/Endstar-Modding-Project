using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200027B RID: 635
	public class UIActiveLevelRoleVisibilityHandler : UIGameObject
	{
		// Token: 0x06000A78 RID: 2680 RVA: 0x00030F58 File Offset: 0x0002F158
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayAndHideHandler.SetToHideEnd(true);
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.OnCreatorStarted));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(this.OnCreatorEnded));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnLeavingSession.AddListener(new UnityAction(this.OnLeavingSession));
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x00030FDC File Offset: 0x0002F1DC
		private async void OnCreatorStarted()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCreatorStarted", Array.Empty<object>());
			}
			this.activeLevelAssetId = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
			this.Subscribe();
			GetAllRolesResult getAllRolesResult = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(this.activeLevelAssetId, MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame.AssetID, new Action<IReadOnlyList<UserRole>>(this.OnLevelUserRolesChanged), false);
			if (getAllRolesResult.WasChanged)
			{
				this.OnLevelUserRolesChanged(getAllRolesResult.Roles);
			}
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x00031013 File Offset: 0x0002F213
		private void OnCreatorEnded()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCreatorEnded", Array.Empty<object>());
			}
			this.Unsubscribe();
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x00031034 File Offset: 0x0002F234
		private void OnLeavingSession()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLeavingSession", Array.Empty<object>());
			}
			try
			{
				this.Unsubscribe();
				this.displayAndHideHandler.Hide();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x00031084 File Offset: 0x0002F284
		private void Subscribe()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Subscribe", Array.Empty<object>());
			}
			if (this.subscribed)
			{
				this.Unsubscribe();
			}
			MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.activeLevelAssetId, new Action<IReadOnlyList<UserRole>>(this.OnLevelUserRolesChanged));
			this.subscribed = true;
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x000310DA File Offset: 0x0002F2DA
		private void Unsubscribe()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Unsubscribe", Array.Empty<object>());
			}
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.activeLevelAssetId, new Action<IReadOnlyList<UserRole>>(this.OnLevelUserRolesChanged));
			this.subscribed = false;
		}

		// Token: 0x06000A7E RID: 2686 RVA: 0x00031118 File Offset: 0x0002F318
		private void OnLevelUserRolesChanged(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLevelUserRolesChanged", new object[] { userRoles.Count });
			}
			int activeUserId = EndlessServices.Instance.CloudService.ActiveUserId;
			Roles roleForUserId = userRoles.GetRoleForUserId(activeUserId);
			this.View(roleForUserId);
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x0003116C File Offset: 0x0002F36C
		private void View(Roles activeLevelLocalClientRole)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { activeLevelLocalClientRole });
			}
			bool flag;
			switch (this.evaluateBy)
			{
			case UIActiveLevelRoleVisibilityHandler.EvaluateBys.Equals:
				flag = activeLevelLocalClientRole == this.role;
				break;
			case UIActiveLevelRoleVisibilityHandler.EvaluateBys.IsGreaterThan:
				flag = activeLevelLocalClientRole.IsGreaterThan(this.role);
				break;
			case UIActiveLevelRoleVisibilityHandler.EvaluateBys.IsGreaterThanOrEqualTo:
				flag = activeLevelLocalClientRole.IsGreaterThanOrEqualTo(this.role);
				break;
			case UIActiveLevelRoleVisibilityHandler.EvaluateBys.IsLessThan:
				flag = activeLevelLocalClientRole.IsLessThan(this.role);
				break;
			case UIActiveLevelRoleVisibilityHandler.EvaluateBys.IsLessThanOrEqualTo:
				flag = activeLevelLocalClientRole.IsLessThanOrEqualTo(this.role);
				break;
			default:
				DebugUtility.LogNoEnumSupportError<UIActiveLevelRoleVisibilityHandler.EvaluateBys>(this, "View", this.evaluateBy, new object[] { activeLevelLocalClientRole });
				flag = false;
				break;
			}
			if (flag)
			{
				this.displayAndHideHandler.Display();
				return;
			}
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x040008B8 RID: 2232
		[SerializeField]
		private Roles role = Roles.Viewer;

		// Token: 0x040008B9 RID: 2233
		[SerializeField]
		private UIActiveLevelRoleVisibilityHandler.EvaluateBys evaluateBy;

		// Token: 0x040008BA RID: 2234
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x040008BB RID: 2235
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040008BC RID: 2236
		private SerializableGuid activeLevelAssetId;

		// Token: 0x040008BD RID: 2237
		private bool subscribed;

		// Token: 0x0200027C RID: 636
		private enum EvaluateBys
		{
			// Token: 0x040008BF RID: 2239
			Equals,
			// Token: 0x040008C0 RID: 2240
			IsGreaterThan,
			// Token: 0x040008C1 RID: 2241
			IsGreaterThanOrEqualTo,
			// Token: 0x040008C2 RID: 2242
			IsLessThan,
			// Token: 0x040008C3 RID: 2243
			IsLessThanOrEqualTo
		}
	}
}
