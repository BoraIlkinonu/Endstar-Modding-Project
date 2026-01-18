using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000454 RID: 1108
	public class InstantPickup
	{
		// Token: 0x06001BAE RID: 7086 RVA: 0x0007C8D2 File Offset: 0x0007AAD2
		public bool GetAllowPickupWhileDowned()
		{
			return this.instantPickup.AllowPickupWhileDowned;
		}

		// Token: 0x06001BAF RID: 7087 RVA: 0x0007C8DF File Offset: 0x0007AADF
		public void SetAllowPickupWhileDowned(Context instigator, bool allow)
		{
			this.instantPickup.AllowPickupWhileDowned = allow;
		}

		// Token: 0x06001BB0 RID: 7088 RVA: 0x0007C8ED File Offset: 0x0007AAED
		public PickupFilter GetPickupFilter()
		{
			return this.instantPickup.CurrentPickupFilter;
		}

		// Token: 0x06001BB1 RID: 7089 RVA: 0x0007C8FA File Offset: 0x0007AAFA
		public void SetPickupFilter(Context instigator, int newFilter)
		{
			this.instantPickup.CurrentPickupFilter = (PickupFilter)newFilter;
		}

		// Token: 0x06001BB2 RID: 7090 RVA: 0x0007C908 File Offset: 0x0007AB08
		public void Respawn(Context instigator)
		{
			this.instantPickup.Respawn();
		}

		// Token: 0x06001BB3 RID: 7091 RVA: 0x0007C915 File Offset: 0x0007AB15
		public void ForceCollect(Context instigator)
		{
			this.instantPickup.ForceCollect();
		}

		// Token: 0x06001BB4 RID: 7092 RVA: 0x0007C922 File Offset: 0x0007AB22
		internal InstantPickup(InstantPickup instantPickup)
		{
			this.instantPickup = instantPickup;
		}

		// Token: 0x040015B7 RID: 5559
		private InstantPickup instantPickup;
	}
}
