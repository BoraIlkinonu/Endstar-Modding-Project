using System;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay
{
	// Token: 0x02000136 RID: 310
	public interface IInteractionBehavior : INpcAttributeModifier
	{
		// Token: 0x0600072B RID: 1835
		void RescindInstruction(Context npcEntity);

		// Token: 0x0600072C RID: 1836
		bool AttemptInteractServerLogic(Context interactor, Context npc, int colliderIndex);

		// Token: 0x0600072D RID: 1837
		void OnInteracted(Context interactor, Context npc, int colliderIndex);

		// Token: 0x0600072E RID: 1838
		void InteractionStopped(Context interactor, Context npc);
	}
}
