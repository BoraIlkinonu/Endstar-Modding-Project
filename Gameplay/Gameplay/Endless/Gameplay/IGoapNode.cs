using System;

namespace Endless.Gameplay
{
	// Token: 0x02000325 RID: 805
	public interface IGoapNode : IInstructionNode, INpcAttributeModifier
	{
		// Token: 0x060012C1 RID: 4801
		void RemoveNodeGoals(NpcEntity entity);

		// Token: 0x060012C2 RID: 4802
		void AddNodeGoals(NpcEntity entity);
	}
}
