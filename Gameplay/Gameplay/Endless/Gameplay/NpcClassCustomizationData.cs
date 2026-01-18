using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;

namespace Endless.Gameplay
{
	// Token: 0x0200022E RID: 558
	[Serializable]
	public abstract class NpcClassCustomizationData
	{
		// Token: 0x17000220 RID: 544
		// (get) Token: 0x06000B90 RID: 2960 RVA: 0x0003FE40 File Offset: 0x0003E040
		[JsonIgnore]
		public string ClassName
		{
			get
			{
				return this.NpcClass.ToString();
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06000B91 RID: 2961 RVA: 0x0003FE61 File Offset: 0x0003E061
		[JsonProperty]
		public virtual NpcClass NpcClass { get; }
	}
}
