using System;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B4 RID: 1204
	public class BehaviorNode : GoapNode
	{
		// Token: 0x170005CA RID: 1482
		// (get) Token: 0x06001DE4 RID: 7652 RVA: 0x00083178 File Offset: 0x00081378
		public override object LuaObject
		{
			get
			{
				object obj;
				if ((obj = this.luaObject) == null)
				{
					obj = (this.luaObject = new Behavior(this));
				}
				return obj;
			}
		}

		// Token: 0x170005CB RID: 1483
		// (get) Token: 0x06001DE5 RID: 7653 RVA: 0x0008319E File Offset: 0x0008139E
		public override string InstructionName
		{
			get
			{
				return "Generated Behavior";
			}
		}

		// Token: 0x170005CC RID: 1484
		// (get) Token: 0x06001DE6 RID: 7654 RVA: 0x000831A5 File Offset: 0x000813A5
		public override Type LuaObjectType
		{
			get
			{
				return typeof(Behavior);
			}
		}

		// Token: 0x06001DE7 RID: 7655 RVA: 0x000831B1 File Offset: 0x000813B1
		public override InstructionNode GetNode()
		{
			return this;
		}

		// Token: 0x170005CD RID: 1485
		// (get) Token: 0x06001DE8 RID: 7656 RVA: 0x00017586 File Offset: 0x00015786
		public override NpcEnum.AttributeRank AttributeRank
		{
			get
			{
				return NpcEnum.AttributeRank.Behavior;
			}
		}

		// Token: 0x06001DE9 RID: 7657 RVA: 0x000831B4 File Offset: 0x000813B4
		public override void GiveInstruction(Context context)
		{
			NpcEntity npcEntity = this.NpcReference.GetNpcEntity(context);
			if (npcEntity)
			{
				this.GiveInstruction(npcEntity);
			}
		}

		// Token: 0x06001DEA RID: 7658 RVA: 0x000831E0 File Offset: 0x000813E0
		protected void GiveInstruction(NpcEntity npcEntity)
		{
			npcEntity.Components.GoapController.CurrentBehaviorNode = this;
			object[] array;
			this.scriptComponent.TryExecuteFunction("ConfigureNpc", out array, new object[] { npcEntity.Context });
		}

		// Token: 0x06001DEB RID: 7659 RVA: 0x00083220 File Offset: 0x00081420
		protected override void AddGoal(NpcEntity entity, int goalNumber)
		{
			GenericWorldState genericWorldState = MonoBehaviourSingleton<WorldStateDictionary>.Instance[base.GetDesiredWorldState(goalNumber)];
			ScriptingGoal scriptingGoal = new ScriptingGoal(goalNumber, new Func<float, float>(BehaviorNode.ClampFunc), goalNumber.ToString(), genericWorldState, entity, this.scriptComponent);
			this.AddedGoalMap[entity].Add(scriptingGoal);
			entity.Components.GoapController.AddGoal(scriptingGoal);
		}

		// Token: 0x06001DEC RID: 7660 RVA: 0x00083288 File Offset: 0x00081488
		public override void RescindInstruction(Context context)
		{
			NpcEntity npcEntity = this.NpcReference.GetNpcEntity(context);
			if (npcEntity)
			{
				this.RescindInstruction(npcEntity);
			}
		}

		// Token: 0x06001DED RID: 7661 RVA: 0x000832B1 File Offset: 0x000814B1
		protected void RescindInstruction(NpcEntity npcEntity)
		{
			if (npcEntity.Components.GoapController.CurrentBehaviorNode == this)
			{
				npcEntity.Components.GoapController.CurrentBehaviorNode = null;
			}
		}

		// Token: 0x06001DEE RID: 7662 RVA: 0x000832D7 File Offset: 0x000814D7
		private static float ClampFunc(float priority)
		{
			return Mathf.Clamp(priority, 0f, 20f);
		}
	}
}
