using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;

namespace Endless.Gameplay.Scripting;

public class GroupInteractionNode : InteractionNode, IGroupInstructionNode, IInstructionNode
{
	public override object LuaObject => luaObject = new GroupInteraction(this);

	public override Type LuaObjectType => typeof(GroupInteraction);

	public override string InstructionName => "Generated Group Interaction";

	public void GiveGroupInstruction(Context _, NpcGroup group)
	{
		foreach (NpcEntity item in MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.GetNpcsInGroup(group))
		{
			GiveInstruction(item);
		}
	}

	public void RescindGroupInstruction(Context _, NpcGroup group)
	{
		foreach (NpcEntity item in MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.GetNpcsInGroup(group))
		{
			RescindInstruction(item);
		}
	}
}
