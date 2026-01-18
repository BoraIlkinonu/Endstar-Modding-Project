using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

internal class Command : InstructionNode
{
	internal Command(Endless.Gameplay.Scripting.InstructionNode node)
		: base(node)
	{
	}

	public void SetGoalCompletionMode(Context instigator, CommandNode.GoalCompletionMode completionMode)
	{
		((CommandNode)base.ManagedNode).CompletionMode = completionMode;
	}
}
