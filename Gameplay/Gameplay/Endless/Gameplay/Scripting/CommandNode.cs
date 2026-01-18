using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B5 RID: 1205
	public class CommandNode : GoapNode
	{
		// Token: 0x170005CE RID: 1486
		// (get) Token: 0x06001DF0 RID: 7664 RVA: 0x000832F4 File Offset: 0x000814F4
		public override object LuaObject
		{
			get
			{
				object obj;
				if ((obj = this.luaObject) == null)
				{
					obj = (this.luaObject = new Command(this));
				}
				return obj;
			}
		}

		// Token: 0x170005CF RID: 1487
		// (get) Token: 0x06001DF1 RID: 7665 RVA: 0x0008331A File Offset: 0x0008151A
		public override string InstructionName
		{
			get
			{
				return "Generated Command";
			}
		}

		// Token: 0x170005D0 RID: 1488
		// (get) Token: 0x06001DF2 RID: 7666 RVA: 0x00083321 File Offset: 0x00081521
		public override Type LuaObjectType
		{
			get
			{
				return typeof(Command);
			}
		}

		// Token: 0x06001DF3 RID: 7667 RVA: 0x000831B1 File Offset: 0x000813B1
		public override InstructionNode GetNode()
		{
			return this;
		}

		// Token: 0x170005D1 RID: 1489
		// (get) Token: 0x06001DF4 RID: 7668 RVA: 0x0003FE71 File Offset: 0x0003E071
		public override NpcEnum.AttributeRank AttributeRank
		{
			get
			{
				return NpcEnum.AttributeRank.Command;
			}
		}

		// Token: 0x06001DF5 RID: 7669 RVA: 0x00083330 File Offset: 0x00081530
		public override void GiveInstruction(Context context)
		{
			NpcEntity npcEntity = this.NpcReference.GetNpcEntity(context);
			if (npcEntity)
			{
				this.GiveInstruction(npcEntity);
				object[] array;
				this.scriptComponent.TryExecuteFunction("ConfigureNpc", out array, new object[] { npcEntity.Context });
			}
		}

		// Token: 0x06001DF6 RID: 7670 RVA: 0x0008337B File Offset: 0x0008157B
		private void GiveInstruction(NpcEntity npcEntity)
		{
			npcEntity.Components.GoapController.CurrentCommandNode = this;
		}

		// Token: 0x06001DF7 RID: 7671 RVA: 0x00083390 File Offset: 0x00081590
		public override void RescindInstruction(Context context)
		{
			NpcEntity npcEntity = this.NpcReference.GetNpcEntity(context);
			if (npcEntity)
			{
				this.RescindInstruction(npcEntity);
			}
		}

		// Token: 0x06001DF8 RID: 7672 RVA: 0x000833B9 File Offset: 0x000815B9
		private void RescindInstruction(NpcEntity npcEntity)
		{
			if (npcEntity.Components.GoapController.CurrentCommandNode == this)
			{
				npcEntity.Components.GoapController.CurrentCommandNode = null;
			}
		}

		// Token: 0x06001DF9 RID: 7673 RVA: 0x000833E0 File Offset: 0x000815E0
		protected override void AddGoal(NpcEntity entity, int goalNumber)
		{
			GenericWorldState genericWorldState = MonoBehaviourSingleton<WorldStateDictionary>.Instance[base.GetDesiredWorldState(goalNumber)];
			ScriptingGoal scriptingGoal = new ScriptingGoal(goalNumber, new Func<float, float>(CommandNode.ClampFunc), "Command" + goalNumber.ToString(), genericWorldState, entity, this.scriptComponent);
			this.AddedGoalMap[entity].Add(scriptingGoal);
			entity.Components.GoapController.AddGoal(scriptingGoal);
			ScriptingGoal scriptingGoal2 = scriptingGoal;
			scriptingGoal2.OnGoalTerminated = (Action<Goal, Goal.TerminationCode>)Delegate.Combine(scriptingGoal2.OnGoalTerminated, new Action<Goal, Goal.TerminationCode>(this.HandleOnGoalTerminated));
		}

		// Token: 0x06001DFA RID: 7674 RVA: 0x00083472 File Offset: 0x00081672
		protected void HandleOnGoalTerminated(Goal goal, Goal.TerminationCode terminationCode)
		{
			switch (terminationCode)
			{
			case Goal.TerminationCode.Completed:
				this.HandleOnGoalComplete(goal);
				return;
			case Goal.TerminationCode.Interrupted:
				return;
			case Goal.TerminationCode.Failed:
				this.HandleOnGoalFailed(goal);
				return;
			default:
				throw new ArgumentOutOfRangeException("terminationCode", terminationCode, null);
			}
		}

		// Token: 0x06001DFB RID: 7675 RVA: 0x000834AA File Offset: 0x000816AA
		protected override void RemoveGoal(NpcEntity entity, Goal goal)
		{
			entity.Components.GoapController.RemoveGoal(goal);
			goal.OnGoalTerminated = (Action<Goal, Goal.TerminationCode>)Delegate.Remove(goal.OnGoalTerminated, new Action<Goal, Goal.TerminationCode>(this.HandleOnGoalTerminated));
		}

		// Token: 0x06001DFC RID: 7676 RVA: 0x000834E0 File Offset: 0x000816E0
		protected virtual void HandleOnGoalComplete(Goal goal)
		{
			HashSet<Goal> hashSet;
			if (!this.AddedGoalMap.TryGetValue(goal.NpcEntity, out hashSet))
			{
				return;
			}
			this.RemoveGoal(goal.NpcEntity, goal);
			hashSet.Remove(goal);
			if (this.CompletionMode == CommandNode.GoalCompletionMode.All && hashSet.Count > 0)
			{
				return;
			}
			this.RescindInstruction(goal.NpcEntity.Context);
			EndlessEvent onCommandSucceeded = this.OnCommandSucceeded;
			if (onCommandSucceeded != null)
			{
				onCommandSucceeded.Invoke(goal.NpcEntity.Context);
			}
			EndlessEvent onCommandFinished = this.OnCommandFinished;
			if (onCommandFinished == null)
			{
				return;
			}
			onCommandFinished.Invoke(goal.NpcEntity.Context);
		}

		// Token: 0x06001DFD RID: 7677 RVA: 0x00083574 File Offset: 0x00081774
		protected virtual void HandleOnGoalFailed(Goal goal)
		{
			this.RescindInstruction(goal.NpcEntity.Context);
			EndlessEvent onCommandFailed = this.OnCommandFailed;
			if (onCommandFailed != null)
			{
				onCommandFailed.Invoke(goal.NpcEntity.Context);
			}
			EndlessEvent onCommandFinished = this.OnCommandFinished;
			if (onCommandFinished == null)
			{
				return;
			}
			onCommandFinished.Invoke(goal.NpcEntity.Context);
		}

		// Token: 0x06001DFE RID: 7678 RVA: 0x000835C9 File Offset: 0x000817C9
		protected static float ClampFunc(float priority)
		{
			if (priority != 0f)
			{
				return Mathf.Clamp(priority, 40f, 69f);
			}
			return 0f;
		}

		// Token: 0x04001752 RID: 5970
		[HideInInspector]
		public EndlessEvent OnCommandSucceeded = new EndlessEvent();

		// Token: 0x04001753 RID: 5971
		[HideInInspector]
		public EndlessEvent OnCommandFailed = new EndlessEvent();

		// Token: 0x04001754 RID: 5972
		[HideInInspector]
		public EndlessEvent OnCommandFinished = new EndlessEvent();

		// Token: 0x04001755 RID: 5973
		public CommandNode.GoalCompletionMode CompletionMode;

		// Token: 0x020004B6 RID: 1206
		public enum GoalCompletionMode
		{
			// Token: 0x04001757 RID: 5975
			All,
			// Token: 0x04001758 RID: 5976
			Any
		}
	}
}
