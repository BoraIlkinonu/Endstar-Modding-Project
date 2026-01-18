using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200044E RID: 1102
	public class GroupCommand : GroupInstruction
	{
		// Token: 0x06001B8A RID: 7050 RVA: 0x0007C629 File Offset: 0x0007A829
		internal GroupCommand(IGroupInstructionNode node)
			: base(node)
		{
		}

		// Token: 0x06001B8B RID: 7051 RVA: 0x0007C632 File Offset: 0x0007A832
		public void SetGoalCompletionMode(Context instigator, CommandNode.GoalCompletionMode completionMode)
		{
			((CommandNode)base.GroupNode).CompletionMode = completionMode;
		}
	}
}
