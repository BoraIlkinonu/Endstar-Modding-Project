using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003F6 RID: 1014
	public class UIBlockedUserView : UIBaseSocialView<BlockedUser>
	{
		// Token: 0x06001953 RID: 6483 RVA: 0x00074CF2 File Offset: 0x00072EF2
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.unblockButton.onClick.AddListener(new UnityAction(this.InvokeUnblock));
		}

		// Token: 0x14000037 RID: 55
		// (add) Token: 0x06001954 RID: 6484 RVA: 0x00074D28 File Offset: 0x00072F28
		// (remove) Token: 0x06001955 RID: 6485 RVA: 0x00074D60 File Offset: 0x00072F60
		public event Action Unblock;

		// Token: 0x06001956 RID: 6486 RVA: 0x00074D98 File Offset: 0x00072F98
		public override void View(BlockedUser model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			User user = UISocialUtility.ExtractNonActiveUser(model);
			this.userView.View(user);
		}

		// Token: 0x06001957 RID: 6487 RVA: 0x00074DD5 File Offset: 0x00072FD5
		private void InvokeUnblock()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeUnblock", Array.Empty<object>());
			}
			Action unblock = this.Unblock;
			if (unblock == null)
			{
				return;
			}
			unblock();
		}

		// Token: 0x04001439 RID: 5177
		[Header("UIBlockedUserView")]
		[SerializeField]
		private UIUserView userView;

		// Token: 0x0400143A RID: 5178
		[SerializeField]
		private UIButton unblockButton;
	}
}
