using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay
{
	// Token: 0x02000231 RID: 561
	[Serializable]
	public class RiflemanNpcCustomizationData : NpcClassCustomizationData
	{
		// Token: 0x17000224 RID: 548
		// (get) Token: 0x06000B97 RID: 2967 RVA: 0x0001BD04 File Offset: 0x00019F04
		[JsonIgnore]
		public override NpcClass NpcClass
		{
			get
			{
				return NpcClass.Rifleman;
			}
		}
	}
}
