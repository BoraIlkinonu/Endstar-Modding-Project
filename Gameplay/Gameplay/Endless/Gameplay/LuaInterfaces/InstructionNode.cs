using System;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000455 RID: 1109
	public class InstructionNode
	{
		// Token: 0x17000587 RID: 1415
		// (get) Token: 0x06001BB5 RID: 7093 RVA: 0x0007C931 File Offset: 0x0007AB31
		protected InstructionNode ManagedNode { get; }

		// Token: 0x06001BB6 RID: 7094 RVA: 0x0007C939 File Offset: 0x0007AB39
		internal InstructionNode(InstructionNode node)
		{
			this.ManagedNode = node;
		}

		// Token: 0x06001BB7 RID: 7095 RVA: 0x0007C948 File Offset: 0x0007AB48
		public Context GetContext()
		{
			return this.ManagedNode.Context;
		}

		// Token: 0x06001BB8 RID: 7096 RVA: 0x0007C955 File Offset: 0x0007AB55
		public void SetContextBehavior(bool useContext)
		{
			this.ManagedNode.NpcReference.useContext = useContext;
		}

		// Token: 0x06001BB9 RID: 7097 RVA: 0x0007C968 File Offset: 0x0007AB68
		public Context GetNpcReference()
		{
			return this.ManagedNode.NpcReference.GetNpc();
		}

		// Token: 0x06001BBA RID: 7098 RVA: 0x0007C669 File Offset: 0x0007A869
		public float GetTime()
		{
			return Time.time;
		}

		// Token: 0x06001BBB RID: 7099 RVA: 0x0007C97A File Offset: 0x0007AB7A
		public virtual void GiveInstruction(Context _, Context context)
		{
			this.ManagedNode.GiveInstruction(context);
		}

		// Token: 0x06001BBC RID: 7100 RVA: 0x0007C988 File Offset: 0x0007AB88
		public virtual void RescindInstruction(Context _, Context context)
		{
			this.ManagedNode.RescindInstruction(context);
		}
	}
}
