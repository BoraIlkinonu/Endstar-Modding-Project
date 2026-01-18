using System;
using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000EE RID: 238
	public class UICreatorPlayerListView : UIBaseListView<PlayerReferenceManager>
	{
		// Token: 0x060003F1 RID: 1009 RVA: 0x00019028 File Offset: 0x00017228
		protected override void Start()
		{
			base.Start();
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.displayAndHideHandler.Hide));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.displayAndHideHandler.Display));
		}

		// Token: 0x04000408 RID: 1032
		[Header("UICreatorPlayerListView")]
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;
	}
}
