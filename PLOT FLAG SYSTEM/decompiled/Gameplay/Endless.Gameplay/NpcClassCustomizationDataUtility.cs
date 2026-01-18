using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay;

public static class NpcClassCustomizationDataUtility
{
	public static NpcClassCustomizationData DeserializeNpcClassCustomizationData(NpcClass npcClass, string jsonData)
	{
		return npcClass switch
		{
			NpcClass.Blank => JsonConvert.DeserializeObject<BlankNpcCustomizationData>(jsonData), 
			NpcClass.Grunt => JsonConvert.DeserializeObject<GruntNpcCustomizationData>(jsonData), 
			NpcClass.Rifleman => JsonConvert.DeserializeObject<RiflemanNpcCustomizationData>(jsonData), 
			NpcClass.Zombie => JsonConvert.DeserializeObject<ZombieNpcCustomizationData>(jsonData), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static NpcClassCustomizationData GetDefaultClassCustomizationData(NpcClass npcClass)
	{
		return npcClass switch
		{
			NpcClass.Blank => new BlankNpcCustomizationData(), 
			NpcClass.Grunt => new GruntNpcCustomizationData(), 
			NpcClass.Rifleman => new RiflemanNpcCustomizationData(), 
			NpcClass.Zombie => new ZombieNpcCustomizationData(), 
			_ => throw new ArgumentOutOfRangeException("npcClass", npcClass, null), 
		};
	}
}
