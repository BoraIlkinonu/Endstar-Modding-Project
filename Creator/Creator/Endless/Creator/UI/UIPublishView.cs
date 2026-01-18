using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200026F RID: 623
	public class UIPublishView : UIGameObject
	{
		// Token: 0x06000A56 RID: 2646 RVA: 0x000306A4 File Offset: 0x0002E8A4
		public void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.model.OnSynchronizeStart += this.OnSynchronizeStart;
			this.model.OnVersionsSet += this.SetDropdownValues;
			this.model.OnBetaVersionSet += this.DisplayBetaVersion;
			this.model.OnPublicVersionSet += this.DisplayPublicVersion;
			this.model.OnClientGameRoleSet += this.OnClientGameRoleSet;
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x0003073C File Offset: 0x0002E93C
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.model.OnSynchronizeStart -= this.OnSynchronizeStart;
			this.model.OnVersionsSet -= this.SetDropdownValues;
			this.model.OnBetaVersionSet -= this.DisplayBetaVersion;
			this.model.OnPublicVersionSet -= this.DisplayPublicVersion;
			this.model.OnClientGameRoleSet -= this.OnClientGameRoleSet;
		}

		// Token: 0x06000A58 RID: 2648 RVA: 0x000307D4 File Offset: 0x0002E9D4
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			this.SetInteractable(false);
			this.model.Synchronize();
		}

		// Token: 0x06000A59 RID: 2649 RVA: 0x00030800 File Offset: 0x0002EA00
		private void OnSynchronizeStart()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			this.betaDropdown.SetValueText(string.Empty);
			this.publicDropdown.SetValueText(string.Empty);
		}

		// Token: 0x06000A5A RID: 2650 RVA: 0x0003083C File Offset: 0x0002EA3C
		private void SetDropdownValues(List<string> versions)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDropdownValues", new object[] { versions.Count });
			}
			this.betaValue[0] = versions[0];
			this.publicValue[0] = versions[0];
			this.betaDropdown.SetOptionsAndValue(versions, this.betaValue[0], false);
			this.publicDropdown.SetOptionsAndValue(versions, this.publicValue[0], false);
		}

		// Token: 0x06000A5B RID: 2651 RVA: 0x000308B8 File Offset: 0x0002EAB8
		private void DisplayBetaVersion(string version)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayBetaVersion", new object[] { version });
			}
			this.betaDropdown.SetValue(version, false);
		}

		// Token: 0x06000A5C RID: 2652 RVA: 0x000308E4 File Offset: 0x0002EAE4
		private void DisplayPublicVersion(string version)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayPublicVersion", new object[] { version });
			}
			this.publicDropdown.SetValue(version, true);
		}

		// Token: 0x06000A5D RID: 2653 RVA: 0x00030910 File Offset: 0x0002EB10
		private void OnClientGameRoleSet(Roles clientGameRole)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnClientGameRoleSet", new object[] { clientGameRole });
			}
			bool flag = clientGameRole.IsGreaterThanOrEqualTo(Roles.Publisher);
			this.SetInteractable(flag);
		}

		// Token: 0x06000A5E RID: 2654 RVA: 0x0003094F File Offset: 0x0002EB4F
		private void SetInteractable(bool interactable)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.betaDropdown.SetIsInteractable(interactable);
			this.publicDropdown.SetIsInteractable(interactable);
		}

		// Token: 0x0400089A RID: 2202
		[SerializeField]
		private UIPublishModel model;

		// Token: 0x0400089B RID: 2203
		[SerializeField]
		private UIDropdownVersion betaDropdown;

		// Token: 0x0400089C RID: 2204
		[SerializeField]
		private UIDropdownVersion publicDropdown;

		// Token: 0x0400089D RID: 2205
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400089E RID: 2206
		private readonly string[] betaValue = new string[1];

		// Token: 0x0400089F RID: 2207
		private readonly string[] publicValue = new string[1];
	}
}
