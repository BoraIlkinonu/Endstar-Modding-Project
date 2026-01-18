using System;
using System.Collections.Generic;

namespace Endless.Gameplay
{
	// Token: 0x02000251 RID: 593
	public readonly struct AttackData
	{
		// Token: 0x06000C50 RID: 3152 RVA: 0x00042BA2 File Offset: 0x00040DA2
		public AttackData(uint startFrame, uint endFrame, MeleeAttackData meleeAttackData)
		{
			this.StartFrame = startFrame;
			this.EndFrame = endFrame;
			this.MeleeAttackData = meleeAttackData;
			this.meleeHits = new HashSet<HittableComponent>();
		}

		// Token: 0x04000B4F RID: 2895
		public readonly uint StartFrame;

		// Token: 0x04000B50 RID: 2896
		public readonly uint EndFrame;

		// Token: 0x04000B51 RID: 2897
		public readonly MeleeAttackData MeleeAttackData;

		// Token: 0x04000B52 RID: 2898
		public readonly HashSet<HittableComponent> meleeHits;
	}
}
