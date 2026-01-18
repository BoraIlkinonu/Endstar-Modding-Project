using System;

namespace Endless.Gameplay;

[Flags]
public enum ReferenceFilter
{
	None = 0,
	NonStatic = 1,
	Npc = 2,
	PhysicsObject = 4,
	InventoryItem = 8,
	Key = 0x10,
	Resource = 0x20
}
