using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001F2 RID: 498
	public class UIHeartHalfSpaceView : UIPoolableGameObject
	{
		// Token: 0x060007CB RID: 1995 RVA: 0x0002660B File Offset: 0x0002480B
		public override void OnSpawn()
		{
			base.OnSpawn();
			this.displayAndHideHandler.SetToDisplayStart(false);
			this.displayAndHideHandler.Display();
		}

		// Token: 0x060007CC RID: 1996 RVA: 0x0002662A File Offset: 0x0002482A
		public void HideAndDespawnOnComplete()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HideAndDespawnOnComplete", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide(new Action(this.Despawn));
		}

		// Token: 0x060007CD RID: 1997 RVA: 0x0002665B File Offset: 0x0002485B
		private void Despawn()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Despawn", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIHeartHalfSpaceView>(this);
		}

		// Token: 0x040006EE RID: 1774
		[Header("UIHeartHalfSpaceView")]
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;
	}
}
