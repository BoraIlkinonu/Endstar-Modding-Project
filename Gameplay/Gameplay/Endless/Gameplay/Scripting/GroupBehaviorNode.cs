using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B8 RID: 1208
	public class GroupBehaviorNode : BehaviorNode, IGroupInstructionNode, IInstructionNode
	{
		// Token: 0x170005D2 RID: 1490
		// (get) Token: 0x06001E07 RID: 7687 RVA: 0x00083748 File Offset: 0x00081948
		public override object LuaObject
		{
			get
			{
				object obj;
				if ((obj = this.luaObject) == null)
				{
					obj = (this.luaObject = new GroupBehavior(this));
				}
				return obj;
			}
		}

		// Token: 0x170005D3 RID: 1491
		// (get) Token: 0x06001E08 RID: 7688 RVA: 0x0008376E File Offset: 0x0008196E
		public override string InstructionName
		{
			get
			{
				return "Generated Group Behavior";
			}
		}

		// Token: 0x170005D4 RID: 1492
		// (get) Token: 0x06001E09 RID: 7689 RVA: 0x00083775 File Offset: 0x00081975
		public override Type LuaObjectType
		{
			get
			{
				return typeof(GroupBehavior);
			}
		}

		// Token: 0x06001E0A RID: 7690 RVA: 0x00083784 File Offset: 0x00081984
		public void GiveGroupInstruction(Context instigator, NpcGroup group)
		{
			foreach (NpcEntity npcEntity in MonoBehaviourSingleton<NpcManager>.Instance.GetNpcsInGroup(group))
			{
				base.GiveInstruction(npcEntity);
			}
		}

		// Token: 0x06001E0B RID: 7691 RVA: 0x000837DC File Offset: 0x000819DC
		public void RescindGroupInstruction(Context instigator, NpcGroup group)
		{
			foreach (NpcEntity npcEntity in MonoBehaviourSingleton<NpcManager>.Instance.GetNpcsInGroup(group))
			{
				base.RescindInstruction(npcEntity);
			}
		}
	}
}
