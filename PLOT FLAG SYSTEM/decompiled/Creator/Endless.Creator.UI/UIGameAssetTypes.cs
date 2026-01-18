using System;

namespace Endless.Creator.UI;

[Flags]
public enum UIGameAssetTypes
{
	None = 0,
	Terrain = 1,
	Prop = 2,
	SFX = 4,
	Ambient = 8,
	Music = 0x10
}
