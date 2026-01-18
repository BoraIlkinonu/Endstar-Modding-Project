using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces;

public class GroupInstruction
{
	protected IGroupInstructionNode GroupNode { get; }

	internal GroupInstruction(IGroupInstructionNode node)
	{
		GroupNode = node;
	}

	public Context GetContext()
	{
		return GroupNode.GetContext();
	}

	public float GetTime()
	{
		return Time.time;
	}

	public void GiveGroupInstruction(Context instigator, int group)
	{
		GroupNode.GiveGroupInstruction(instigator, (NpcGroup)group);
	}

	public void RescindGroupInstruction(Context instigator, int group)
	{
		GroupNode.RescindGroupInstruction(instigator, (NpcGroup)group);
	}
}
