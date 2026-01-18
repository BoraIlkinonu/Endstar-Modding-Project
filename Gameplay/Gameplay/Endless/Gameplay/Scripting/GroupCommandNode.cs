using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B9 RID: 1209
	public class GroupCommandNode : CommandNode, IGroupInstructionNode, IInstructionNode
	{
		// Token: 0x170005D5 RID: 1493
		// (get) Token: 0x06001E0D RID: 7693 RVA: 0x0008383C File Offset: 0x00081A3C
		public override object LuaObject
		{
			get
			{
				object obj;
				if ((obj = this.luaObject) == null)
				{
					obj = (this.luaObject = new GroupCommand(this));
				}
				return obj;
			}
		}

		// Token: 0x170005D6 RID: 1494
		// (get) Token: 0x06001E0E RID: 7694 RVA: 0x00083862 File Offset: 0x00081A62
		public override string InstructionName
		{
			get
			{
				return "Generated Group Command";
			}
		}

		// Token: 0x170005D7 RID: 1495
		// (get) Token: 0x06001E0F RID: 7695 RVA: 0x00083869 File Offset: 0x00081A69
		public override Type LuaObjectType
		{
			get
			{
				return typeof(GroupCommand);
			}
		}

		// Token: 0x06001E10 RID: 7696 RVA: 0x000831B1 File Offset: 0x000813B1
		public override InstructionNode GetNode()
		{
			return this;
		}

		// Token: 0x170005D8 RID: 1496
		// (get) Token: 0x06001E11 RID: 7697 RVA: 0x0003FE71 File Offset: 0x0003E071
		public override NpcEnum.AttributeRank AttributeRank
		{
			get
			{
				return NpcEnum.AttributeRank.Command;
			}
		}

		// Token: 0x06001E12 RID: 7698 RVA: 0x00083878 File Offset: 0x00081A78
		public override void GiveInstruction(Context context)
		{
			if (context.IsNpc())
			{
				NpcEntity userComponent = context.WorldObject.GetUserComponent<NpcEntity>();
				userComponent.Components.GoapController.CurrentCommandNode = this;
				object[] array;
				this.scriptComponent.TryExecuteFunction("ConfigureNpc", out array, new object[] { userComponent.Context });
			}
		}

		// Token: 0x06001E13 RID: 7699 RVA: 0x000838CC File Offset: 0x00081ACC
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

		// Token: 0x06001E14 RID: 7700 RVA: 0x00083914 File Offset: 0x00081B14
		protected override void HandleOnGoalComplete(Goal goal)
		{
			NpcEntity npcEntity = goal.NpcEntity;
			HashSet<Goal> hashSet;
			if (!this.AddedGoalMap.TryGetValue(npcEntity, out hashSet))
			{
				return;
			}
			this.RemoveGoal(npcEntity, goal);
			hashSet.Remove(goal);
			if (this.CompletionMode == CommandNode.GoalCompletionMode.All && hashSet.Count > 0)
			{
				return;
			}
			this.RescindInstruction(goal.NpcEntity.Context);
			EndlessEvent onNpcCompletedCommand = this.OnNpcCompletedCommand;
			if (onNpcCompletedCommand != null)
			{
				onNpcCompletedCommand.Invoke(goal.NpcEntity.Context);
			}
			EndlessEvent onNpcFinishedCommand = this.OnNpcFinishedCommand;
			if (onNpcFinishedCommand != null)
			{
				onNpcFinishedCommand.Invoke(goal.NpcEntity.Context);
			}
			if (this.AddedGoalMap.Count == 0)
			{
				EndlessEvent onGroupNodeFinished = this.OnGroupNodeFinished;
				if (onGroupNodeFinished == null)
				{
					return;
				}
				onGroupNodeFinished.Invoke(base.Context);
			}
		}

		// Token: 0x06001E15 RID: 7701 RVA: 0x000839C8 File Offset: 0x00081BC8
		protected override void HandleOnGoalFailed(Goal goal)
		{
			this.RescindInstruction(goal.NpcEntity.Context);
			EndlessEvent onNpcFailedCommand = this.OnNpcFailedCommand;
			if (onNpcFailedCommand != null)
			{
				onNpcFailedCommand.Invoke(goal.NpcEntity.Context);
			}
			EndlessEvent onNpcFinishedCommand = this.OnNpcFinishedCommand;
			if (onNpcFinishedCommand == null)
			{
				return;
			}
			onNpcFinishedCommand.Invoke(goal.NpcEntity.Context);
		}

		// Token: 0x06001E16 RID: 7702 RVA: 0x00083A20 File Offset: 0x00081C20
		public void GiveGroupInstruction(Context _, NpcGroup group)
		{
			foreach (NpcEntity npcEntity in MonoBehaviourSingleton<NpcManager>.Instance.GetNpcsInGroup(group))
			{
				this.GiveInstruction(npcEntity.Context);
			}
		}

		// Token: 0x06001E17 RID: 7703 RVA: 0x00083A80 File Offset: 0x00081C80
		public void RescindGroupInstruction(Context _, NpcGroup group)
		{
			foreach (NpcEntity npcEntity in MonoBehaviourSingleton<NpcManager>.Instance.GetNpcsInGroup(group))
			{
				this.RescindInstruction(npcEntity.Context);
			}
		}

		// Token: 0x0400175A RID: 5978
		[HideInInspector]
		public EndlessEvent OnNpcCompletedCommand = new EndlessEvent();

		// Token: 0x0400175B RID: 5979
		[HideInInspector]
		public EndlessEvent OnNpcFailedCommand = new EndlessEvent();

		// Token: 0x0400175C RID: 5980
		[HideInInspector]
		public EndlessEvent OnNpcFinishedCommand = new EndlessEvent();

		// Token: 0x0400175D RID: 5981
		[HideInInspector]
		public EndlessEvent OnGroupNodeFinished = new EndlessEvent();
	}
}
