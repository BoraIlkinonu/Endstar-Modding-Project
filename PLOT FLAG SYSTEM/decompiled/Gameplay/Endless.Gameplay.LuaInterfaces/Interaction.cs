using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class Interaction : InstructionNode
{
	private InteractionNode ManagedAsInteraction => (InteractionNode)base.ManagedNode;

	internal Interaction(Endless.Gameplay.Scripting.InstructionNode node)
		: base(node)
	{
	}

	public void SetIsHeldInteraction(Context instigator, bool isHeldInteraction)
	{
		ManagedAsInteraction.IsHeldInteraction = isHeldInteraction;
	}

	public void SetInteractionAnimation(Context instigator, InteractionAnimation interactionAnimation)
	{
		ManagedAsInteraction.InteractionAnimation = interactionAnimation;
	}

	public void SetInteractionDuration(Context instigator, float duration)
	{
		ManagedAsInteraction.InteractionDuration = duration;
	}

	public void StopInteraction(Context interactor, Context npcContext)
	{
		ManagedAsInteraction.scriptComponent.TryExecuteFunction("OnInteractionStopped", out var _, interactor, npcContext);
		npcContext.WorldObject.GetUserComponent<NpcEntity>().Components.Parameters.InteractionFinishedTrigger = true;
		interactor.WorldObject.GetComponent<InteractorBase>().InteractableStoppedInteraction();
	}

	public override void GiveInstruction(Context instigator, Context npc)
	{
		ManagedAsInteraction.GiveInstruction(npc);
	}

	public override void RescindInstruction(Context interactor, Context npcContext)
	{
		ManagedAsInteraction.RescindInstruction(npcContext);
	}

	public void InteractionCompleted(Context interactor)
	{
		ManagedAsInteraction.InteractionComplete(interactor);
	}

	public void InteractionCanceled(Context interactor)
	{
		ManagedAsInteraction.InteractionCanceled(interactor);
	}
}
