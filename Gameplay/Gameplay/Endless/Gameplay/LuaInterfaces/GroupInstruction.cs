using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200044F RID: 1103
	public class GroupInstruction
	{
		// Token: 0x17000585 RID: 1413
		// (get) Token: 0x06001B8C RID: 7052 RVA: 0x0007C645 File Offset: 0x0007A845
		protected IGroupInstructionNode GroupNode { get; }

		// Token: 0x06001B8D RID: 7053 RVA: 0x0007C64D File Offset: 0x0007A84D
		internal GroupInstruction(IGroupInstructionNode node)
		{
			this.GroupNode = node;
		}

		// Token: 0x06001B8E RID: 7054 RVA: 0x0007C65C File Offset: 0x0007A85C
		public Context GetContext()
		{
			return this.GroupNode.GetContext();
		}

		// Token: 0x06001B8F RID: 7055 RVA: 0x0007C669 File Offset: 0x0007A869
		public float GetTime()
		{
			return Time.time;
		}

		// Token: 0x06001B90 RID: 7056 RVA: 0x0007C670 File Offset: 0x0007A870
		public void GiveGroupInstruction(Context instigator, int group)
		{
			this.GroupNode.GiveGroupInstruction(instigator, (NpcGroup)group);
		}

		// Token: 0x06001B91 RID: 7057 RVA: 0x0007C67F File Offset: 0x0007A87F
		public void RescindGroupInstruction(Context instigator, int group)
		{
			this.GroupNode.RescindGroupInstruction(instigator, (NpcGroup)group);
		}
	}
}
