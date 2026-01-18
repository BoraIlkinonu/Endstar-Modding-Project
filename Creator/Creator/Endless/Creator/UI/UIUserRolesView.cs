using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002B7 RID: 695
	public class UIUserRolesView : UIGameObject
	{
		// Token: 0x06000BC1 RID: 3009 RVA: 0x00037698 File Offset: 0x00035898
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.model.OnLocalClientRoleSet.AddListener(new UnityAction<Roles>(this.HandleAddUserRoleButtonVisibility));
			this.model.OnLoadingStarted.AddListener(new UnityAction(this.OnLoadingStarted));
			this.model.OnLoadingEnded.AddListener(new UnityAction(this.OnLoadingEnded));
			this.HandleAddUserRoleButtonVisibility(this.model.LocalClientRole);
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x00037724 File Offset: 0x00035924
		public void SetBlockActivationOfAddUserRoleButton(bool blockActivationOfAddUserRoleButton)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetBlockActivationOfAddUserRoleButton", new object[] { blockActivationOfAddUserRoleButton });
			}
			this.blockActivationOfAddUserRoleButton = blockActivationOfAddUserRoleButton;
			if (this.addUserRoleButton.gameObject.activeSelf && blockActivationOfAddUserRoleButton)
			{
				this.addUserRoleButton.gameObject.SetActive(false);
			}
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x00037780 File Offset: 0x00035980
		public void HandleAddUserRoleButtonVisibility(Roles localClientRole)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleAddUserRoleButtonVisibility", new object[] { localClientRole });
			}
			bool flag = localClientRole.IsGreaterThanOrEqualTo(Roles.Editor) && !this.blockActivationOfAddUserRoleButton && this.model.AssetContext != AssetContexts.GameInspectorPlay;
			this.addUserRoleButton.gameObject.SetActive(flag);
		}

		// Token: 0x06000BC4 RID: 3012 RVA: 0x000377E7 File Offset: 0x000359E7
		private void OnLoadingStarted()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLoadingStarted", Array.Empty<object>());
			}
			this.addUserRoleButton.interactable = false;
		}

		// Token: 0x06000BC5 RID: 3013 RVA: 0x0003780D File Offset: 0x00035A0D
		private void OnLoadingEnded()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLoadingEnded", Array.Empty<object>());
			}
			this.addUserRoleButton.interactable = true;
		}

		// Token: 0x040009F3 RID: 2547
		[SerializeField]
		private UIUserRolesModel model;

		// Token: 0x040009F4 RID: 2548
		[SerializeField]
		private bool blockActivationOfAddUserRoleButton;

		// Token: 0x040009F5 RID: 2549
		[SerializeField]
		private UIButton addUserRoleButton;

		// Token: 0x040009F6 RID: 2550
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
