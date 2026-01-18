using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces;

public class Interactable
{
	private readonly Endless.Gameplay.Interactable interactable;

	internal Interactable(Endless.Gameplay.Interactable interactable)
	{
		this.interactable = interactable;
	}

	public int GetNumberOfInteractables()
	{
		return interactable.GetNumberOfInteractables();
	}

	public void SetInteractionAnimation(Context instigator, int interactionAnimation)
	{
		interactable.InteractionAnimation = (InteractionAnimation)interactionAnimation;
	}

	public void SetIsHeldInteraction(Context instigator, bool isHeldInteraction)
	{
		interactable.IsHeldInteraction = isHeldInteraction;
	}

	public void StopInteraction(Context instigator, Context targetInteractor)
	{
		interactable.scriptComponent.TryExecuteFunction("OnInteractionStopped", out var _, targetInteractor);
		InteractorBase component = targetInteractor.WorldObject.GetComponent<InteractorBase>();
		if (component != null)
		{
			component.InteractableStoppedInteraction();
		}
	}

	internal void SetInteractionDuration(Context instigator, float duration)
	{
		interactable.InteractionDuration = duration;
	}

	public void SetInteractableEnabled(Context instigator, int index, bool isEnabled)
	{
		interactable.SetInteractableEnabled(instigator, index, isEnabled);
	}

	public void SetAllInteractablesEnabled(Context instigator, bool isEnabled)
	{
		for (int i = 0; i < GetNumberOfInteractables(); i++)
		{
			interactable.SetInteractableEnabled(instigator, i, isEnabled);
		}
	}

	public void SetIconFromPropReference(Context instigator, PropLibraryReference propLibraryReference)
	{
		interactable.SetIconFromPropReference(propLibraryReference);
	}

	public void SetAnchorOverride(Context instigator, int index, UnityEngine.Vector3 overridePosition)
	{
		interactable.SetAnchorPosition(overridePosition, index);
	}

	public void ClearAnchorOverride(Context instigator, int index)
	{
		interactable.SetAnchorPosition(null, index);
	}

	public void SetUsePlayerAnchor(Context instigator, int index, bool usePlayerAnchor)
	{
		interactable.SetUsePlayerAnchor(usePlayerAnchor, index);
	}

	public void SetHidePromptDuringInteraction(Context instigator, bool value)
	{
		interactable.HidePromptDuringInteraction = value;
	}
}
