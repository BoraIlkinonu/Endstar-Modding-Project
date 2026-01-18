using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay
{
	// Token: 0x0200022F RID: 559
	[Serializable]
	public class BlankNpcCustomizationData : NpcClassCustomizationData
	{
		// Token: 0x17000222 RID: 546
		// (get) Token: 0x06000B93 RID: 2963 RVA: 0x0001965C File Offset: 0x0001785C
		[JsonIgnore]
		public override NpcClass NpcClass
		{
			get
			{
				return NpcClass.Blank;
			}
		}
	}
}
