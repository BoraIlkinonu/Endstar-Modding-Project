using Endless.Gameplay.Scripting;

namespace Endless.Gameplay;

public interface IInteractionBehavior : INpcAttributeModifier
{
	void RescindInstruction(Context npcEntity);

	bool AttemptInteractServerLogic(Context interactor, Context npc, int colliderIndex);

	void OnInteracted(Context interactor, Context npc, int colliderIndex);

	void InteractionStopped(Context interactor, Context npc);
}
