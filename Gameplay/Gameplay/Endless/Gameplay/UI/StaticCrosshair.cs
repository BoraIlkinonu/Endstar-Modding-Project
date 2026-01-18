using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000409 RID: 1033
	public class StaticCrosshair : CrosshairBase
	{
		// Token: 0x060019D3 RID: 6611 RVA: 0x00076BA5 File Offset: 0x00074DA5
		public override void OnShow()
		{
			this.image.enabled = true;
		}

		// Token: 0x060019D4 RID: 6612 RVA: 0x00076BB3 File Offset: 0x00074DB3
		public override void OnHide()
		{
			this.image.enabled = false;
		}

		// Token: 0x04001485 RID: 5253
		[SerializeField]
		private Image image;
	}
}
