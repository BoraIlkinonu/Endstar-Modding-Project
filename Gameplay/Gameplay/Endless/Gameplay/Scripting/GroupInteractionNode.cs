using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004BA RID: 1210
	public class GroupInteractionNode : InteractionNode, IGroupInstructionNode, IInstructionNode
	{
		// Token: 0x170005D9 RID: 1497
		// (get) Token: 0x06001E19 RID: 7705 RVA: 0x00083B14 File Offset: 0x00081D14
		public override object LuaObject
		{
			get
			{
				return this.luaObject = new GroupInteraction(this);
			}
		}

		// Token: 0x170005DA RID: 1498
		// (get) Token: 0x06001E1A RID: 7706 RVA: 0x00083B30 File Offset: 0x00081D30
		public override Type LuaObjectType
		{
			get
			{
				return typeof(GroupInteraction);
			}
		}

		// Token: 0x170005DB RID: 1499
		// (get) Token: 0x06001E1B RID: 7707 RVA: 0x00083B3C File Offset: 0x00081D3C
		public override string InstructionName
		{
			get
			{
				return "Generated Group Interaction";
			}
		}

		// Token: 0x06001E1C RID: 7708 RVA: 0x00083B44 File Offset: 0x00081D44
		public void GiveGroupInstruction(Context _, NpcGroup group)
		{
			foreach (NpcEntity npcEntity in MonoBehaviourSingleton<NpcManager>.Instance.GetNpcsInGroup(group))
			{
				base.GiveInstruction(npcEntity);
			}
		}

		// Token: 0x06001E1D RID: 7709 RVA: 0x00083B9C File Offset: 0x00081D9C
		public void RescindGroupInstruction(Context _, NpcGroup group)
		{
			foreach (NpcEntity npcEntity in MonoBehaviourSingleton<NpcManager>.Instance.GetNpcsInGroup(group))
			{
				base.RescindInstruction(npcEntity);
			}
		}
	}
}
