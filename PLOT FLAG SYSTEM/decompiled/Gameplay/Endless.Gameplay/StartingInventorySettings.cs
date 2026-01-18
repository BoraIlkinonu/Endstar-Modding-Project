using System;

namespace Endless.Gameplay;

[Serializable]
public class StartingInventorySettings
{
	public InventoryUsableDefinition InventoryDefintion;

	public bool LockItem;

	public int Count = 1;
}
