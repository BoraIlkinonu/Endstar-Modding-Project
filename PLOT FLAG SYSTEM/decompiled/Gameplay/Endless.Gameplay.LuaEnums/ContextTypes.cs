using System;

namespace Endless.Gameplay.LuaEnums;

[Flags]
public enum ContextTypes
{
	None = 0,
	Player = 1,
	NPC = 2,
	PhysicsObject = 4
}
