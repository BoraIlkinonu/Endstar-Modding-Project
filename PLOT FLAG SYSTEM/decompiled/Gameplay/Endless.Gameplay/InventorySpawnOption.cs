using System;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay;

[Serializable]
public class InventorySpawnOption
{
	public SerializableGuid AssetId;

	public bool LockItem;

	public int Quantity;
}
