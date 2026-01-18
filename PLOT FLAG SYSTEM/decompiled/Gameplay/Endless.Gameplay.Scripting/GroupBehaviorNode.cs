using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;

namespace Endless.Gameplay.Scripting;

public class GroupBehaviorNode : BehaviorNode, IGroupInstructionNode, IInstructionNode
{
	public override object LuaObject => luaObject ?? (luaObject = new GroupBehavior(this));

	public override string InstructionName => "Generated Group Behavior";

	public override Type LuaObjectType => typeof(GroupBehavior);

	public void GiveGroupInstruction(Context instigator, NpcGroup group)
	{
		foreach (NpcEntity item in MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.GetNpcsInGroup(group))
		{
			GiveInstruction(item);
		}
	}

	public void RescindGroupInstruction(Context instigator, NpcGroup group)
	{
		foreach (NpcEntity item in MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.GetNpcsInGroup(group))
		{
			RescindInstruction(item);
		}
	}
}
