using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay
{
	// Token: 0x020001A1 RID: 417
	public class ScriptingGoal : Goal
	{
		// Token: 0x06000961 RID: 2401 RVA: 0x0002B980 File Offset: 0x00029B80
		public ScriptingGoal(int goalNumber, Func<float, float> clampFunc, string goalName, GenericWorldState desiredWorldState, NpcEntity npcEntity, EndlessScriptComponent scriptComponent)
			: base(goalName, desiredWorldState, npcEntity)
		{
			this.scriptComponent = scriptComponent;
			this.goalNumber = goalNumber;
			this.clampFunc = clampFunc;
		}

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06000962 RID: 2402 RVA: 0x0002B9A4 File Offset: 0x00029BA4
		public override float Priority
		{
			get
			{
				long num2;
				float num = (float)(this.scriptComponent.TryExecuteFunction<long>("Priority", out num2, new object[]
				{
					this.NpcEntity.Context,
					this.goalNumber
				}) ? num2 : 0L);
				return this.clampFunc(num);
			}
		}

		// Token: 0x06000963 RID: 2403 RVA: 0x0002B9FC File Offset: 0x00029BFC
		public override void Activate()
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("Activate", out array, new object[]
			{
				this.NpcEntity.Context,
				this.goalNumber
			});
		}

		// Token: 0x06000964 RID: 2404 RVA: 0x0002BA40 File Offset: 0x00029C40
		public override void GoalTerminated(Goal.TerminationCode code)
		{
			base.GoalTerminated(code);
			object[] array;
			this.scriptComponent.TryExecuteFunction("Deactivate", out array, new object[]
			{
				this.NpcEntity.Context,
				this.goalNumber
			});
			Action<Goal, Goal.TerminationCode> onGoalTerminated = this.OnGoalTerminated;
			if (onGoalTerminated == null)
			{
				return;
			}
			onGoalTerminated(this, code);
		}

		// Token: 0x040007A9 RID: 1961
		private readonly EndlessScriptComponent scriptComponent;

		// Token: 0x040007AA RID: 1962
		private readonly int goalNumber;

		// Token: 0x040007AB RID: 1963
		private readonly Func<float, float> clampFunc;
	}
}
