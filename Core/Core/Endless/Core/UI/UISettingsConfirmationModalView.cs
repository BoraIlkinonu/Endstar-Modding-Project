using System;
using Endless.Core.UI.Settings;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Core.UI
{
	// Token: 0x0200006C RID: 108
	public class UISettingsConfirmationModalView : UIBaseModalView
	{
		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060001F6 RID: 502 RVA: 0x0000B498 File Offset: 0x00009698
		// (set) Token: 0x060001F7 RID: 503 RVA: 0x0000B4A0 File Offset: 0x000096A0
		public UISettingsVideoController SettingsVideoController { get; private set; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060001F8 RID: 504 RVA: 0x0000B4A9 File Offset: 0x000096A9
		// (set) Token: 0x060001F9 RID: 505 RVA: 0x0000B4B1 File Offset: 0x000096B1
		public Action InvokeOnAction { get; private set; }

		// Token: 0x060001FA RID: 506 RVA: 0x0000B4BA File Offset: 0x000096BA
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.SettingsVideoController = (UISettingsVideoController)modalData[0];
			this.InvokeOnAction = (Action)modalData[1];
		}

		// Token: 0x060001FB RID: 507 RVA: 0x0000B4DF File Offset: 0x000096DF
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
		}
	}
}
