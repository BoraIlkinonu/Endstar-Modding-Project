using System;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay
{
	// Token: 0x020002F9 RID: 761
	[Serializable]
	public class InventorySpawnOption
	{
		// Token: 0x04000EDE RID: 3806
		public SerializableGuid AssetId;

		// Token: 0x04000EDF RID: 3807
		public bool LockItem;

		// Token: 0x04000EE0 RID: 3808
		public int Quantity;
	}
}
