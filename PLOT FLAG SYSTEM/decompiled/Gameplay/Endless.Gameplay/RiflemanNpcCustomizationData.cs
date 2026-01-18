using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay;

[Serializable]
public class RiflemanNpcCustomizationData : NpcClassCustomizationData
{
	[JsonIgnore]
	public override NpcClass NpcClass => NpcClass.Rifleman;
}
