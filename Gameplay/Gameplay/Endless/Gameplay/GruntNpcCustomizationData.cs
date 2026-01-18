using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay
{
	// Token: 0x02000230 RID: 560
	[Serializable]
	public class GruntNpcCustomizationData : NpcClassCustomizationData
	{
		// Token: 0x17000223 RID: 547
		// (get) Token: 0x06000B95 RID: 2965 RVA: 0x00017586 File Offset: 0x00015786
		[JsonIgnore]
		public override NpcClass NpcClass
		{
			get
			{
				return NpcClass.Grunt;
			}
		}
	}
}
