using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay
{
	// Token: 0x02000232 RID: 562
	[Serializable]
	public class ZombieNpcCustomizationData : NpcClassCustomizationData
	{
		// Token: 0x17000225 RID: 549
		// (get) Token: 0x06000B99 RID: 2969 RVA: 0x0003FE71 File Offset: 0x0003E071
		[JsonIgnore]
		public override NpcClass NpcClass
		{
			get
			{
				return NpcClass.Zombie;
			}
		}

		// Token: 0x04000AE7 RID: 2791
		[JsonProperty]
		public bool ZombifyTarget;
	}
}
