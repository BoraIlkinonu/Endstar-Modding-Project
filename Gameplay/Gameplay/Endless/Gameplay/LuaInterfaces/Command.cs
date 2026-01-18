using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000446 RID: 1094
	internal class Command : InstructionNode
	{
		// Token: 0x06001B71 RID: 7025 RVA: 0x0007C29B File Offset: 0x0007A49B
		internal Command(InstructionNode node)
			: base(node)
		{
		}

		// Token: 0x06001B72 RID: 7026 RVA: 0x0007C4D9 File Offset: 0x0007A6D9
		public void SetGoalCompletionMode(Context instigator, CommandNode.GoalCompletionMode completionMode)
		{
			((CommandNode)base.ManagedNode).CompletionMode = completionMode;
		}
	}
}
