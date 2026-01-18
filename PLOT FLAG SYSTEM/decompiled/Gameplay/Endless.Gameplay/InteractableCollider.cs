using Endless.Gameplay.UI;
using Endless.Props;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class InteractableCollider : MonoBehaviour
{
	public UnityEvent OnSelectedLocally = new UnityEvent();

	public UnityEvent OnUnselectedLocally = new UnityEvent();

	[field: SerializeField]
	public InteractableBase InteractableBase { get; set; }

	[field: SerializeField]
	public ColliderInfo ColliderInfo { get; set; }

	public bool IsInteractable { get; set; } = true;

	public Vector3? OverrideAnchorPosition { get; set; }

	public bool UsePlayerForAnchor { get; set; }

	public UIInteractionPromptAnchor InteractionPromptAnchor { get; private set; }

	private void Start()
	{
		NetworkBehaviourSingleton<GameEndManager>.Instance.OnGameEndScreenTriggered += UnselectLocally;
	}

	private void OnDestroy()
	{
		NetworkBehaviourSingleton<GameEndManager>.Instance.OnGameEndScreenTriggered -= UnselectLocally;
	}

	public void UnselectLocally()
	{
		if ((bool)InteractionPromptAnchor)
		{
			InteractionPromptAnchor.Close();
			InteractionPromptAnchor = null;
		}
		OnUnselectedLocally.Invoke();
	}

	public void SelectLocally()
	{
		if (!InteractionPromptAnchor)
		{
			GameplayPlayerReferenceManager gameplayPlayerReferenceManager = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject() as GameplayPlayerReferenceManager;
			if (UsePlayerForAnchor)
			{
				InteractionPromptAnchor = MonoBehaviourSingleton<UIInteractionPromptAnchorManager>.Instance.CreateInstance(gameplayPlayerReferenceManager.ApperanceController.AppearanceAnimator.transform, gameplayPlayerReferenceManager, InteractableBase.InteractionPrompt, InteractableBase.InteractionPromptOffset * 2f);
			}
			else
			{
				Vector3 value = (OverrideAnchorPosition.HasValue ? (OverrideAnchorPosition.Value - base.transform.position + InteractableBase.InteractionPromptOffset) : InteractableBase.InteractionPromptOffset);
				InteractionPromptAnchor = MonoBehaviourSingleton<UIInteractionPromptAnchorManager>.Instance.CreateInstance(base.transform, gameplayPlayerReferenceManager, InteractableBase.InteractionPrompt, value);
			}
		}
		OnSelectedLocally.Invoke();
	}

	public void SetInteractionResultSprite(Sprite newInteractionResultSprite)
	{
		if ((bool)InteractionPromptAnchor)
		{
			InteractionPromptAnchor.SetInteractionResultSprite(newInteractionResultSprite);
		}
	}

	public bool CheckInteractionDistance(InteractorBase interactor)
	{
		float num = interactor.InteractOffset + interactor.InteractRadius * 2f + 2f;
		return ColliderInfo.GetDistanceFromPoint(interactor.transform.position) <= num;
	}

	internal void AttemptInteract(InteractorBase interactorBase)
	{
		InteractableBase.AttemptInteract(interactorBase, this);
	}
}
