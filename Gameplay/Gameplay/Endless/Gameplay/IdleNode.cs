using System;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001A6 RID: 422
	public class IdleNode : BehaviorNode
	{
		// Token: 0x0600097C RID: 2428 RVA: 0x0002BD26 File Offset: 0x00029F26
		public override void GiveInstruction(Context context)
		{
			base.GiveInstruction(context);
			base.Entity.NpcBlackboard.Set<bool>(NpcBlackboard.Key.CanFidget, this.canFidget);
		}

		// Token: 0x040007B6 RID: 1974
		[SerializeField]
		private bool canFidget;
	}
}
