using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class GroupCommand : GroupInstruction
{
	internal GroupCommand(IGroupInstructionNode node)
		: base(node)
	{
	}

	public void SetGoalCompletionMode(Context instigator, CommandNode.GoalCompletionMode completionMode)
	{
		((CommandNode)base.GroupNode).CompletionMode = completionMode;
	}
}
