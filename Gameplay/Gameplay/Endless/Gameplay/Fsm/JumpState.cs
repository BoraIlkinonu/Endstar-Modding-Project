using System;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000438 RID: 1080
	public class JumpState : FsmState
	{
		// Token: 0x06001B0D RID: 6925 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public JumpState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x17000576 RID: 1398
		// (get) Token: 0x06001B0E RID: 6926 RVA: 0x0007B18B File Offset: 0x0007938B
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Jump;
			}
		}
	}
}
