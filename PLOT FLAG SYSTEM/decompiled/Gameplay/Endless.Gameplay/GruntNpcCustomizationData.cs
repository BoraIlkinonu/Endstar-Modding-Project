using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay;

[Serializable]
public class GruntNpcCustomizationData : NpcClassCustomizationData
{
	[JsonIgnore]
	public override NpcClass NpcClass => NpcClass.Grunt;
}
