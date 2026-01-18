using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200045B RID: 1115
	public class Navigation
	{
		// Token: 0x06001BDB RID: 7131 RVA: 0x0007CBF9 File Offset: 0x0007ADF9
		internal Navigation(DynamicNavigationComponent dynamicNavigationComponent)
		{
			this.navigationComponent = dynamicNavigationComponent;
		}

		// Token: 0x06001BDC RID: 7132 RVA: 0x0007CC08 File Offset: 0x0007AE08
		public void SetBlockingBehavior(Context instigator, bool isBlocking)
		{
			this.navigationComponent.SetBlockingBehavior(instigator, isBlocking);
		}

		// Token: 0x040015BD RID: 5565
		private readonly DynamicNavigationComponent navigationComponent;
	}
}
