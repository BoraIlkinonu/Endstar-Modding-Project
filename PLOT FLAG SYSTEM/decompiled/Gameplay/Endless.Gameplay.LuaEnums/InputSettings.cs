using System;

namespace Endless.Gameplay.LuaEnums;

[Flags]
public enum InputSettings
{
	None = 0,
	Walk = 1,
	Run = 2,
	Jump = 4,
	Equipment = 8,
	Interaction = 0x10
}
