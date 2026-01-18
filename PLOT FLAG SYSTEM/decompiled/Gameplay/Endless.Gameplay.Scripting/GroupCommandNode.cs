using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class GroupCommandNode : CommandNode, IGroupInstructionNode, IInstructionNode
{
	[HideInInspector]
	public EndlessEvent OnNpcCompletedCommand = new EndlessEvent();

	[HideInInspector]
	public EndlessEvent OnNpcFailedCommand = new EndlessEvent();

	[HideInInspector]
	public EndlessEvent OnNpcFinishedCommand = new EndlessEvent();

	[HideInInspector]
	public EndlessEvent OnGroupNodeFinished = new EndlessEvent();

	public override object LuaObject => luaObject ?? (luaObject = new GroupCommand(this));

	public override string InstructionName => "Generated Group Command";

	public override Type LuaObjectType => typeof(GroupCommand);

	public override NpcEnum.AttributeRank AttributeRank => NpcEnum.AttributeRank.Command;

	public override InstructionNode GetNode()
	{
		return this;
	}

	public override void GiveInstruction(Context context)
	{
		if (context.IsNpc())
		{
			NpcEntity userComponent = context.WorldObject.GetUserComponent<NpcEntity>();
			userComponent.Components.GoapController.CurrentCommandNode = this;
			scriptComponent.TryExecuteFunction("ConfigureNpc", out var _, userComponent.Context);
		}
	}

	public override void RescindInstruction(Context context)
	{
		if (context.IsNpc())
		{
			NpcEntity userComponent = context.WorldObject.GetUserComponent<NpcEntity>();
			if (userComponent.Components.GoapController.CurrentCommandNode == this)
			{
				userComponent.Components.GoapController.CurrentCommandNode = null;
			}
		}
	}

	protected override void HandleOnGoalComplete(Goal goal)
	{
		NpcEntity npcEntity = goal.NpcEntity;
		if (!AddedGoalMap.TryGetValue(npcEntity, out var value))
		{
			return;
		}
		RemoveGoal(npcEntity, goal);
		value.Remove(goal);
		if (CompletionMode != GoalCompletionMode.All || value.Count <= 0)
		{
			RescindInstruction(goal.NpcEntity.Context);
			OnNpcCompletedCommand?.Invoke(goal.NpcEntity.Context);
			OnNpcFinishedCommand?.Invoke(goal.NpcEntity.Context);
			if (AddedGoalMap.Count == 0)
			{
				OnGroupNodeFinished?.Invoke(base.Context);
			}
		}
	}

	protected override void HandleOnGoalFailed(Goal goal)
	{
		RescindInstruction(goal.NpcEntity.Context);
		OnNpcFailedCommand?.Invoke(goal.NpcEntity.Context);
		OnNpcFinishedCommand?.Invoke(goal.NpcEntity.Context);
	}

	public void GiveGroupInstruction(Context _, NpcGroup group)
	{
		foreach (NpcEntity item in MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.GetNpcsInGroup(group))
		{
			GiveInstruction(item.Context);
		}
	}

	public void RescindGroupInstruction(Context _, NpcGroup group)
	{
		foreach (NpcEntity item in MonoBehaviourSingleton<Endless.Gameplay.NpcManager>.Instance.GetNpcsInGroup(group))
		{
			RescindInstruction(item.Context);
		}
	}
}
