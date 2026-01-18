using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay;

[Serializable]
public class BlankNpcCustomizationData : NpcClassCustomizationData
{
	[JsonIgnore]
	public override NpcClass NpcClass => NpcClass.Blank;
}
