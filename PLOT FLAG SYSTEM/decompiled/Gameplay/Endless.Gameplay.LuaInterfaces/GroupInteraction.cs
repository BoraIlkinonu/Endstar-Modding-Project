using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

public class GroupInteraction : GroupInstruction
{
	private InteractionNode GroupNodeAsInteraction => (InteractionNode)base.GroupNode;

	internal GroupInteraction(IGroupInstructionNode node)
		: base(node)
	{
	}

	public void SetIsHeldInteraction(Context instigator, bool isHeldInteraction)
	{
		GroupNodeAsInteraction.IsHeldInteraction = isHeldInteraction;
	}

	public void SetInteractionAnimation(Context instigator, InteractionAnimation interactionAnimation)
	{
		GroupNodeAsInteraction.InteractionAnimation = interactionAnimation;
	}

	public void SetInteractionDuration(Context instigator, float duration)
	{
		GroupNodeAsInteraction.InteractionDuration = duration;
	}

	public void StopInteraction(Context interactor, Context npcContext)
	{
		GroupNodeAsInteraction.scriptComponent.TryExecuteFunction("OnInteractionStopped", out var _, interactor, npcContext);
		npcContext.WorldObject.GetUserComponent<NpcEntity>().Components.Parameters.InteractionFinishedTrigger = true;
		interactor.WorldObject.GetComponent<InteractorBase>().InteractableStoppedInteraction();
	}

	public void InteractionCompleted(Context interactor)
	{
		GroupNodeAsInteraction.InteractionComplete(interactor);
	}

	public void InteractionCanceled(Context interactor)
	{
		GroupNodeAsInteraction.InteractionCanceled(interactor);
	}
}
