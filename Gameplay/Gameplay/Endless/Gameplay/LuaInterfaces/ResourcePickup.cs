using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000461 RID: 1121
	public class ResourcePickup
	{
		// Token: 0x06001C09 RID: 7177 RVA: 0x0007D2F8 File Offset: 0x0007B4F8
		public bool GetAllowPickupWhileDowned()
		{
			return this.instantPickup.AllowPickupWhileDowned;
		}

		// Token: 0x06001C0A RID: 7178 RVA: 0x0007D305 File Offset: 0x0007B505
		public void SetAllowPickupWhileDowned(Context instigator, bool allow)
		{
			this.instantPickup.AllowPickupWhileDowned = allow;
		}

		// Token: 0x06001C0B RID: 7179 RVA: 0x0007D313 File Offset: 0x0007B513
		internal ResourcePickup(ResourcePickup instantPickup)
		{
			this.instantPickup = instantPickup;
		}

		// Token: 0x040015C3 RID: 5571
		private ResourcePickup instantPickup;
	}
}
