using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay
{
	// Token: 0x02000327 RID: 807
	public interface IGroupInstructionNode : IInstructionNode
	{
		// Token: 0x060012C7 RID: 4807
		void GiveGroupInstruction(Context instigator, NpcGroup group);

		// Token: 0x060012C8 RID: 4808
		void RescindGroupInstruction(Context instigator, NpcGroup group);
	}
}
