using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004BB RID: 1211
	public interface IGroupNode
	{
		// Token: 0x06001E1F RID: 7711 RVA: 0x00002DB0 File Offset: 0x00000FB0
		void GiveGroupInstruction(Context _, NpcGroup group)
		{
		}

		// Token: 0x06001E20 RID: 7712 RVA: 0x00002DB0 File Offset: 0x00000FB0
		void RescindGroupInstruction(Context _, NpcGroup group)
		{
		}
	}
}
