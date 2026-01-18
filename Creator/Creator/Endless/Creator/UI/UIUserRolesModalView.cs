using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001E0 RID: 480
	public class UIUserRolesModalView : UIEscapableModalView
	{
		// Token: 0x06000769 RID: 1897 RVA: 0x00024CD4 File Offset: 0x00022ED4
		protected override void Start()
		{
			base.Start();
			this.model.AssetRole.OnValueChangedFromServer.AddListener(new UnityAction(this.ViewAssetRole));
			this.model.ScriptRole.OnValueChangedFromServer.AddListener(new UnityAction(this.ViewScriptRole));
			this.model.VisualRole.OnValueChangedFromServer.AddListener(new UnityAction(this.ViewVisualRole));
			this.model.PrefabRole.OnValueChangedFromServer.AddListener(new UnityAction(this.ViewPrefabRole));
			this.model.AssetRole.OnCleared.AddListener(new UnityAction(this.HideAssetRole));
			this.model.ScriptRole.OnCleared.AddListener(new UnityAction(this.HideScriptRole));
			this.model.VisualRole.OnCleared.AddListener(new UnityAction(this.HideVisualRole));
			this.model.PrefabRole.OnCleared.AddListener(new UnityAction(this.HidePrefabRole));
			UIRolesRadio[] array = this.everyRoleRadio;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnValueChanged.AddListener(new UnityAction<Roles>(this.ViewRoleDescription));
			}
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x00024E20 File Offset: 0x00023020
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			User user = (User)modalData[0];
			Roles roles = (Roles)modalData[1];
			SerializableGuid serializableGuid = (SerializableGuid)modalData[2];
			UIUserRoleWizard.AssetTypes assetTypes = (UIUserRoleWizard.AssetTypes)modalData[3];
			Roles roles2 = (Roles)modalData[4];
			string text = (string)modalData[5];
			this.context = (AssetContexts)modalData[6];
			Roles roles3 = (Roles)modalData[7];
			this.model.Initialize(user, roles, serializableGuid, assetTypes, roles2, this.context, roles3);
			bool flag = assetTypes == UIUserRoleWizard.AssetTypes.Prop;
			this.modalContainerLayoutElement.minHeight = (flag ? this.propModalHeight : this.nonPropModalHeight);
			this.userNameText.text = user.UserName;
			this.assetNameText.text = text;
			this.assetRoleRadioNameText.gameObject.SetActive(flag);
			this.roleText.text = "Role";
			this.roleDescriptionText.text = this.defaultRoleDescriptionText;
			this.roleContentSizeFitter.RequestLayout();
			GameObject[] array = this.setActiveIfAssetTypeIsProp;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(flag);
			}
			if (this.context == AssetContexts.NewGame)
			{
				this.assetRoleRadio.EnableControls();
				this.assetRoleRadio.SetOriginalValue(roles3);
				this.assetRoleRadio.SetValue(roles2, true);
				this.assetRoleRadio.gameObject.SetActive(true);
				return;
			}
			this.ViewAssetRole();
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x00024F8A File Offset: 0x0002318A
		public override void Close()
		{
			base.Close();
			this.model.Clear();
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00024FA0 File Offset: 0x000231A0
		private void ViewRoleDescription(Roles role)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewRoleDescription", new object[] { role });
			}
			this.roleText.text = role.ToString();
			this.roleDescriptionText.text = this.rolesDescriptionsDictionary[role].Game;
			this.roleContentSizeFitter.RequestLayout();
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x00025010 File Offset: 0x00023210
		private void ViewAssetRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "ViewAssetRole", string.Format("{0}: {1}, {2}: {3}", new object[]
				{
					"AssetRole",
					this.model.AssetRole,
					"AssetRoleOwnerCount",
					this.model.AssetRoleOwnerCount
				}), Array.Empty<object>());
			}
			this.HandleRoleRadio(this.assetRoleRadio, this.model.AssetRole, this.model.AssetRoleOwnerCount);
		}

		// Token: 0x0600076E RID: 1902 RVA: 0x00025098 File Offset: 0x00023298
		private void ViewScriptRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "ViewScriptRole", string.Format("{0}: {1}, {2}: {3}", new object[]
				{
					"ScriptRole",
					this.model.ScriptRole,
					"ScriptRoleOwnerCount",
					this.model.ScriptRoleOwnerCount
				}), Array.Empty<object>());
			}
			this.HandleRoleRadio(this.scriptRoleRadio, this.model.ScriptRole, this.model.ScriptRoleOwnerCount);
		}

		// Token: 0x0600076F RID: 1903 RVA: 0x00025120 File Offset: 0x00023320
		private void ViewVisualRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "ViewVisualRole", string.Format("{0}: {1}, {2}: {3}", new object[]
				{
					"VisualRole",
					this.model.VisualRole,
					"VisualRole",
					this.model.VisualRoleOwnerCount
				}), Array.Empty<object>());
			}
			this.HandleRoleRadio(this.visualRoleRadio, this.model.VisualRole, this.model.VisualRoleOwnerCount);
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x000251A8 File Offset: 0x000233A8
		private void ViewPrefabRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "ViewPrefabRole", string.Format("{0}: {1}, {2}: {3}", new object[]
				{
					"PrefabRole",
					this.model.PrefabRole,
					"PrefabRoleOwnerCount",
					this.model.PrefabRoleOwnerCount
				}), Array.Empty<object>());
			}
			this.HandleRoleRadio(this.prefabRoleRadio, this.model.PrefabRole, this.model.PrefabRoleOwnerCount);
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x00025230 File Offset: 0x00023430
		private void HandleRoleRadio(UIRolesRadio rolesRadio, UIUserRolesModalModel.RoleValue roleValue, int ownerCount)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleRoleRadio", new object[]
				{
					rolesRadio.gameObject.name,
					roleValue,
					ownerCount
				});
			}
			rolesRadio.SetValue(roleValue.Value, false);
			rolesRadio.SetOriginalValue(roleValue.OriginalValue);
			if (roleValue.OriginalValue.IsGreaterThanOrEqualTo(Roles.Owner))
			{
				if (this.model.TargetUser.Id == EndlessServices.Instance.CloudService.ActiveUserId && ownerCount > 1)
				{
					rolesRadio.EnableControls();
				}
				else
				{
					rolesRadio.DisableControls();
				}
			}
			else if (roleValue.LocalClientValue.IsGreaterThanOrEqualTo(Roles.Owner))
			{
				rolesRadio.EnableControls();
			}
			else
			{
				rolesRadio.DisableControls(roleValue.LocalClientValue.Level());
			}
			rolesRadio.gameObject.SetActive(true);
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x00025302 File Offset: 0x00023502
		private void HideAssetRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideAssetRole", Array.Empty<object>());
			}
			if (this.context == AssetContexts.NewGame)
			{
				return;
			}
			this.assetRoleRadio.gameObject.SetActive(false);
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x00025337 File Offset: 0x00023537
		private void HideScriptRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideScriptRole", Array.Empty<object>());
			}
			this.scriptRoleRadio.gameObject.SetActive(false);
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x00025362 File Offset: 0x00023562
		private void HideVisualRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideVisualRole", Array.Empty<object>());
			}
			this.visualRoleRadio.gameObject.SetActive(false);
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x0002538D File Offset: 0x0002358D
		private void HidePrefabRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HidePrefabRole", Array.Empty<object>());
			}
			this.prefabRoleRadio.gameObject.SetActive(false);
		}

		// Token: 0x040006AA RID: 1706
		[SerializeField]
		private UIUserRolesModalModel model;

		// Token: 0x040006AB RID: 1707
		[SerializeField]
		private float propModalHeight = 900f;

		// Token: 0x040006AC RID: 1708
		[SerializeField]
		private float nonPropModalHeight = 500f;

		// Token: 0x040006AD RID: 1709
		[SerializeField]
		private LayoutElement modalContainerLayoutElement;

		// Token: 0x040006AE RID: 1710
		[SerializeField]
		private TextMeshProUGUI userNameText;

		// Token: 0x040006AF RID: 1711
		[SerializeField]
		private TextMeshProUGUI assetNameText;

		// Token: 0x040006B0 RID: 1712
		[SerializeField]
		private TextMeshProUGUI assetRoleRadioNameText;

		// Token: 0x040006B1 RID: 1713
		[SerializeField]
		private UIRolesRadio assetRoleRadio;

		// Token: 0x040006B2 RID: 1714
		[SerializeField]
		private UIRolesRadio scriptRoleRadio;

		// Token: 0x040006B3 RID: 1715
		[SerializeField]
		private UIRolesRadio visualRoleRadio;

		// Token: 0x040006B4 RID: 1716
		[SerializeField]
		private UIRolesRadio prefabRoleRadio;

		// Token: 0x040006B5 RID: 1717
		[SerializeField]
		private UIRolesRadio[] everyRoleRadio = Array.Empty<UIRolesRadio>();

		// Token: 0x040006B6 RID: 1718
		[SerializeField]
		private GameObject[] setActiveIfAssetTypeIsProp = Array.Empty<GameObject>();

		// Token: 0x040006B7 RID: 1719
		[SerializeField]
		private UIRolesDescriptionsDictionary rolesDescriptionsDictionary;

		// Token: 0x040006B8 RID: 1720
		[SerializeField]
		private TextMeshProUGUI roleText;

		// Token: 0x040006B9 RID: 1721
		[SerializeField]
		private TextMeshProUGUI roleDescriptionText;

		// Token: 0x040006BA RID: 1722
		[SerializeField]
		private UIContentSizeFitter roleContentSizeFitter;

		// Token: 0x040006BB RID: 1723
		[SerializeField]
		private string defaultRoleDescriptionText = "Select a pip to learn about the role.";

		// Token: 0x040006BC RID: 1724
		private AssetContexts context;
	}
}
