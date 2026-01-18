using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay
{
	// Token: 0x02000233 RID: 563
	public static class NpcClassCustomizationDataUtility
	{
		// Token: 0x06000B9B RID: 2971 RVA: 0x0003FE74 File Offset: 0x0003E074
		public static NpcClassCustomizationData DeserializeNpcClassCustomizationData(NpcClass npcClass, string jsonData)
		{
			NpcClassCustomizationData npcClassCustomizationData;
			switch (npcClass)
			{
			case NpcClass.Blank:
				npcClassCustomizationData = JsonConvert.DeserializeObject<BlankNpcCustomizationData>(jsonData);
				break;
			case NpcClass.Grunt:
				npcClassCustomizationData = JsonConvert.DeserializeObject<GruntNpcCustomizationData>(jsonData);
				break;
			case NpcClass.Rifleman:
				npcClassCustomizationData = JsonConvert.DeserializeObject<RiflemanNpcCustomizationData>(jsonData);
				break;
			case NpcClass.Zombie:
				npcClassCustomizationData = JsonConvert.DeserializeObject<ZombieNpcCustomizationData>(jsonData);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return npcClassCustomizationData;
		}

		// Token: 0x06000B9C RID: 2972 RVA: 0x0003FEC4 File Offset: 0x0003E0C4
		public static NpcClassCustomizationData GetDefaultClassCustomizationData(NpcClass npcClass)
		{
			NpcClassCustomizationData npcClassCustomizationData;
			switch (npcClass)
			{
			case NpcClass.Blank:
				npcClassCustomizationData = new BlankNpcCustomizationData();
				break;
			case NpcClass.Grunt:
				npcClassCustomizationData = new GruntNpcCustomizationData();
				break;
			case NpcClass.Rifleman:
				npcClassCustomizationData = new RiflemanNpcCustomizationData();
				break;
			case NpcClass.Zombie:
				npcClassCustomizationData = new ZombieNpcCustomizationData();
				break;
			default:
				throw new ArgumentOutOfRangeException("npcClass", npcClass, null);
			}
			return npcClassCustomizationData;
		}
	}
}
