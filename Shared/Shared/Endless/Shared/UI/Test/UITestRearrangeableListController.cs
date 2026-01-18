using System;
using UnityEngine;

namespace Endless.Shared.UI.Test
{
	// Token: 0x0200029C RID: 668
	public class UITestRearrangeableListController : UIBaseRearrangeableListController<int>
	{
		// Token: 0x06001097 RID: 4247 RVA: 0x00046C8B File Offset: 0x00044E8B
		protected override void Synchronize()
		{
			base.Synchronize();
			this.testListModelHandler.Synchronize();
		}

		// Token: 0x04000A7A RID: 2682
		[Header("UITestLocalFilterableListController")]
		[SerializeField]
		private UITestListModelHandler testListModelHandler;
	}
}
