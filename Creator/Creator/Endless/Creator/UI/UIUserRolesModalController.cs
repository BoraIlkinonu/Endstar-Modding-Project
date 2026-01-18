using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001DF RID: 479
	public class UIUserRolesModalController : UIGameObject
	{
		// Token: 0x06000765 RID: 1893 RVA: 0x00024B28 File Offset: 0x00022D28
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.confirmButton.onClick.AddListener(new UnityAction(this.Confirm));
			this.cancelButton.onClick.AddListener(new UnityAction(MonoBehaviourSingleton<UIUserRoleWizard>.Instance.Cancel));
			this.assetRoleRadio.OnValueChanged.AddListener(new UnityAction<Roles>(this.model.AssetRole.SetValueFromUser));
			this.scriptRoleRadio.OnValueChanged.AddListener(new UnityAction<Roles>(this.model.ScriptRole.SetValueFromUser));
			this.visualRoleRadio.OnValueChanged.AddListener(new UnityAction<Roles>(this.model.VisualRole.SetValueFromUser));
			this.prefabRoleRadio.OnValueChanged.AddListener(new UnityAction<Roles>(this.model.PrefabRole.SetValueFromUser));
		}

		// Token: 0x06000766 RID: 1894 RVA: 0x00024C21 File Offset: 0x00022E21
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			UIUserRolesModalController.ConfirmationAction = null;
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x00024C44 File Offset: 0x00022E44
		private void Confirm()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Confirm", Array.Empty<object>());
			}
			Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>> confirmationAction = UIUserRolesModalController.ConfirmationAction;
			if (confirmationAction == null)
			{
				return;
			}
			confirmationAction(this.model.EveryRoleValueIsNone, new UIUserRolesModalModel.RoleValue(this.model.AssetRole), new UIUserRolesModalModel.RoleValue(this.model.ScriptRole), this.model.ScriptAssetId, new UIUserRolesModalModel.RoleValue(this.model.PrefabRole), this.model.PrefabAssetId, this.model.Roles);
		}

		// Token: 0x040006A1 RID: 1697
		public static Action<bool, UIUserRolesModalModel.RoleValue, UIUserRolesModalModel.RoleValue, SerializableGuid, UIUserRolesModalModel.RoleValue, SerializableGuid, IReadOnlyCollection<UIUserRolesModalModel.RoleValue>> ConfirmationAction;

		// Token: 0x040006A2 RID: 1698
		[SerializeField]
		private UIUserRolesModalModel model;

		// Token: 0x040006A3 RID: 1699
		[SerializeField]
		private UIRolesRadio assetRoleRadio;

		// Token: 0x040006A4 RID: 1700
		[SerializeField]
		private UIRolesRadio scriptRoleRadio;

		// Token: 0x040006A5 RID: 1701
		[SerializeField]
		private UIRolesRadio visualRoleRadio;

		// Token: 0x040006A6 RID: 1702
		[SerializeField]
		private UIRolesRadio prefabRoleRadio;

		// Token: 0x040006A7 RID: 1703
		[SerializeField]
		private UIButton confirmButton;

		// Token: 0x040006A8 RID: 1704
		[SerializeField]
		private UIButton cancelButton;

		// Token: 0x040006A9 RID: 1705
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
