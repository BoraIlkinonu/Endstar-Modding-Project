using System;

namespace Endless.Gameplay
{
	// Token: 0x020000E2 RID: 226
	[Serializable]
	public class StartingInventorySettings
	{
		// Token: 0x040003F0 RID: 1008
		public InventoryUsableDefinition InventoryDefintion;

		// Token: 0x040003F1 RID: 1009
		public bool LockItem;

		// Token: 0x040003F2 RID: 1010
		public int Count = 1;
	}
}
