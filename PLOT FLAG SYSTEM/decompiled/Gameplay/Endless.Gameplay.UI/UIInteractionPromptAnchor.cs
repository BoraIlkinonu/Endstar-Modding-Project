using System;
using Endless.Gameplay.SoVariables;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.UI.Anchors;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIInteractionPromptAnchor : UIBaseAnchor
{
	public static Action OnInitializeAction;

	public static Action OnCloseAction;

	[Header("UIInteractionPromptAnchor")]
	[SerializeField]
	private Image inputImageFill;

	[SerializeField]
	private Image interactionResultImage;

	[SerializeField]
	private TextMeshProUGUI interactionResultText;

	[SerializeField]
	private Image supplementalInteractionResultImage;

	[SerializeField]
	private TweenCollection promptReplacedTweens;

	[SerializeField]
	private TweenCollection interactAttemptedTweens;

	[SerializeField]
	private TweenCollection interactFailedTweens;

	private PlayerReferenceManager playerReferenceManager;

	public static UIInteractionPromptAnchor CreateInstance(UIInteractionPromptAnchor prefab, Transform target, RectTransform container, PlayerReferenceManager playerReferenceManager, UIInteractionPromptVariable interactionPrompt, Vector3? offset = null)
	{
		UIInteractionPromptAnchor uIInteractionPromptAnchor = UIBaseAnchor.CreateAndInitialize(prefab, target, container, offset);
		uIInteractionPromptAnchor.SetPlayerReferenceManager(playerReferenceManager);
		uIInteractionPromptAnchor.SetInteractionPrompt(interactionPrompt);
		return uIInteractionPromptAnchor;
	}

	public void SetPlayerReferenceManager(PlayerReferenceManager playerReferenceManager)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetPlayerReferenceManager", playerReferenceManager);
		}
		this.playerReferenceManager = playerReferenceManager;
		PlayerInteractor.OnInteractionFailed = (Action<InteractableBase>)Delegate.Combine(PlayerInteractor.OnInteractionFailed, new Action<InteractableBase>(OnInteractionFailed));
		UIAnchorManager instance = MonoBehaviourSingleton<UIAnchorManager>.Instance;
		instance.OnAnchorRegistered = (Action<IUIAnchor>)Delegate.Combine(instance.OnAnchorRegistered, new Action<IUIAnchor>(OnAnchorRegistered));
		UIAnchorManager instance2 = MonoBehaviourSingleton<UIAnchorManager>.Instance;
		instance2.OnAnchorUnregistered = (Action<IUIAnchor>)Delegate.Combine(instance2.OnAnchorUnregistered, new Action<IUIAnchor>(OnAnchorUnregistered));
		OnInitializeAction?.Invoke();
	}

	public void SetInteractionPrompt(UIInteractionPromptVariable interactionPrompt)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractionPrompt", interactionPrompt);
		}
		DisplayInteractionPrompt(interactionPrompt.Value);
	}

	public override void UpdatePosition()
	{
		base.UpdatePosition();
		if (!playerReferenceManager)
		{
			return;
		}
		PlayerInteractor playerInteractor = playerReferenceManager.PlayerInteractor;
		if ((bool)playerInteractor.CurrentInteractableTarget)
		{
			InteractableBase currentInteractable = playerInteractor.CurrentInteractable;
			float num = 0f;
			if ((bool)currentInteractable && currentInteractable.IsHeldInteraction)
			{
				num = playerInteractor.InteractionTime / currentInteractable.InteractionDuration;
			}
			if (Mathf.Approximately(inputImageFill.fillAmount, 0f) && num > 0f)
			{
				interactAttemptedTweens.Tween();
			}
			inputImageFill.fillAmount = num;
		}
		else
		{
			OnNewInteractableFocused(null);
		}
	}

	public override void Close()
	{
		if (!displayAndHideHandler.IsTweeningHide)
		{
			PlayerInteractor.OnInteractionFailed = (Action<InteractableBase>)Delegate.Remove(PlayerInteractor.OnInteractionFailed, new Action<InteractableBase>(OnInteractionFailed));
			UIAnchorManager instance = MonoBehaviourSingleton<UIAnchorManager>.Instance;
			instance.OnAnchorRegistered = (Action<IUIAnchor>)Delegate.Remove(instance.OnAnchorRegistered, new Action<IUIAnchor>(OnAnchorRegistered));
			UIAnchorManager instance2 = MonoBehaviourSingleton<UIAnchorManager>.Instance;
			instance2.OnAnchorUnregistered = (Action<IUIAnchor>)Delegate.Remove(instance2.OnAnchorUnregistered, new Action<IUIAnchor>(OnAnchorUnregistered));
			OnCloseAction?.Invoke();
			base.Close();
		}
	}

	public void SetInteractionResultSprite(Sprite interactionResultSprite)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractionResultSprite", interactionResultSprite.name);
		}
		interactionResultImage.sprite = interactionResultSprite;
	}

	private void DisplayInteractionPrompt(UIInteractionPrompt interactionPrompt)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayInteractionPrompt", interactionPrompt.InteractionResultText);
		}
		interactionResultImage.sprite = interactionPrompt.InteractionResultSprite.GetActiveDeviceTypeValue();
		interactionResultImage.color = interactionPrompt.InteractionResultColor;
		interactionResultImage.gameObject.SetActive(interactionPrompt.InteractionResultSprite != null);
		supplementalInteractionResultImage.sprite = interactionPrompt.supplementalInteractionResultSprite;
		supplementalInteractionResultImage.gameObject.SetActive(interactionPrompt.supplementalInteractionResultSprite != null);
		interactionResultText.text = interactionPrompt.InteractionResultText;
	}

	private void OnNewInteractableFocused(InteractableCollider collider)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnNewInteractableFocused", collider.DebugSafeName());
		}
		if (collider == null)
		{
			Close();
			return;
		}
		UIInteractionPromptVariable interactionPrompt = collider.InteractableBase.InteractionPrompt;
		DisplayInteractionPrompt(interactionPrompt.Value);
		promptReplacedTweens.Tween();
	}

	private void OnInteractionFailed(InteractableBase target)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnInteractionFailed", target.gameObject.name);
		}
		interactFailedTweens.Tween();
	}

	private void OnAnchorRegistered(IUIAnchor anchor)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAnchorRegistered", anchor);
		}
		if (anchor is UIDialogueBubbleAnchor)
		{
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAnchorRegistered", anchor);
			}
			displayAndHideHandler.Hide();
		}
	}

	private void OnAnchorUnregistered(IUIAnchor anchor)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAnchorUnregistered", anchor);
		}
		if (anchor is UIDialogueBubbleAnchor)
		{
			if (verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAnchorUnregistered", anchor);
			}
			displayAndHideHandler.Display();
		}
	}
}
