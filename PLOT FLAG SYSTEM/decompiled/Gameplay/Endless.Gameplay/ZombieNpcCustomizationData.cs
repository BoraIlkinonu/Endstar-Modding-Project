using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay;

[Serializable]
public class ZombieNpcCustomizationData : NpcClassCustomizationData
{
	[JsonProperty]
	public bool ZombifyTarget;

	[JsonIgnore]
	public override NpcClass NpcClass => NpcClass.Zombie;
}
