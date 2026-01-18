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

namespace Endless.Gameplay.UI
{
	// Token: 0x02000389 RID: 905
	public class UIInteractionPromptAnchor : UIBaseAnchor
	{
		// Token: 0x06001711 RID: 5905 RVA: 0x0006BBCF File Offset: 0x00069DCF
		public static UIInteractionPromptAnchor CreateInstance(UIInteractionPromptAnchor prefab, Transform target, RectTransform container, PlayerReferenceManager playerReferenceManager, UIInteractionPromptVariable interactionPrompt, Vector3? offset = null)
		{
			UIInteractionPromptAnchor uiinteractionPromptAnchor = UIBaseAnchor.CreateAndInitialize<UIInteractionPromptAnchor>(prefab, target, container, offset);
			uiinteractionPromptAnchor.SetPlayerReferenceManager(playerReferenceManager);
			uiinteractionPromptAnchor.SetInteractionPrompt(interactionPrompt);
			return uiinteractionPromptAnchor;
		}

		// Token: 0x06001712 RID: 5906 RVA: 0x0006BBEC File Offset: 0x00069DEC
		public void SetPlayerReferenceManager(PlayerReferenceManager playerReferenceManager)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetPlayerReferenceManager", new object[] { playerReferenceManager });
			}
			this.playerReferenceManager = playerReferenceManager;
			PlayerInteractor.OnInteractionFailed = (Action<InteractableBase>)Delegate.Combine(PlayerInteractor.OnInteractionFailed, new Action<InteractableBase>(this.OnInteractionFailed));
			UIAnchorManager instance = MonoBehaviourSingleton<UIAnchorManager>.Instance;
			instance.OnAnchorRegistered = (Action<IUIAnchor>)Delegate.Combine(instance.OnAnchorRegistered, new Action<IUIAnchor>(this.OnAnchorRegistered));
			UIAnchorManager instance2 = MonoBehaviourSingleton<UIAnchorManager>.Instance;
			instance2.OnAnchorUnregistered = (Action<IUIAnchor>)Delegate.Combine(instance2.OnAnchorUnregistered, new Action<IUIAnchor>(this.OnAnchorUnregistered));
			Action onInitializeAction = UIInteractionPromptAnchor.OnInitializeAction;
			if (onInitializeAction == null)
			{
				return;
			}
			onInitializeAction();
		}

		// Token: 0x06001713 RID: 5907 RVA: 0x0006BC98 File Offset: 0x00069E98
		public void SetInteractionPrompt(UIInteractionPromptVariable interactionPrompt)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractionPrompt", new object[] { interactionPrompt });
			}
			this.DisplayInteractionPrompt(interactionPrompt.Value);
		}

		// Token: 0x06001714 RID: 5908 RVA: 0x0006BCC4 File Offset: 0x00069EC4
		public override void UpdatePosition()
		{
			base.UpdatePosition();
			if (!this.playerReferenceManager)
			{
				return;
			}
			PlayerInteractor playerInteractor = this.playerReferenceManager.PlayerInteractor;
			if (playerInteractor.CurrentInteractableTarget)
			{
				InteractableBase currentInteractable = playerInteractor.CurrentInteractable;
				float num = 0f;
				if (currentInteractable && currentInteractable.IsHeldInteraction)
				{
					num = playerInteractor.InteractionTime / currentInteractable.InteractionDuration;
				}
				if (Mathf.Approximately(this.inputImageFill.fillAmount, 0f) && num > 0f)
				{
					this.interactAttemptedTweens.Tween();
				}
				this.inputImageFill.fillAmount = num;
				return;
			}
			this.OnNewInteractableFocused(null);
		}

		// Token: 0x06001715 RID: 5909 RVA: 0x0006BD68 File Offset: 0x00069F68
		public override void Close()
		{
			if (this.displayAndHideHandler.IsTweeningHide)
			{
				return;
			}
			PlayerInteractor.OnInteractionFailed = (Action<InteractableBase>)Delegate.Remove(PlayerInteractor.OnInteractionFailed, new Action<InteractableBase>(this.OnInteractionFailed));
			UIAnchorManager instance = MonoBehaviourSingleton<UIAnchorManager>.Instance;
			instance.OnAnchorRegistered = (Action<IUIAnchor>)Delegate.Remove(instance.OnAnchorRegistered, new Action<IUIAnchor>(this.OnAnchorRegistered));
			UIAnchorManager instance2 = MonoBehaviourSingleton<UIAnchorManager>.Instance;
			instance2.OnAnchorUnregistered = (Action<IUIAnchor>)Delegate.Remove(instance2.OnAnchorUnregistered, new Action<IUIAnchor>(this.OnAnchorUnregistered));
			Action onCloseAction = UIInteractionPromptAnchor.OnCloseAction;
			if (onCloseAction != null)
			{
				onCloseAction();
			}
			base.Close();
		}

		// Token: 0x06001716 RID: 5910 RVA: 0x0006BE05 File Offset: 0x0006A005
		public void SetInteractionResultSprite(Sprite interactionResultSprite)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractionResultSprite", new object[] { interactionResultSprite.name });
			}
			this.interactionResultImage.sprite = interactionResultSprite;
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x0006BE38 File Offset: 0x0006A038
		private void DisplayInteractionPrompt(UIInteractionPrompt interactionPrompt)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayInteractionPrompt", new object[] { interactionPrompt.InteractionResultText });
			}
			this.interactionResultImage.sprite = interactionPrompt.InteractionResultSprite.GetActiveDeviceTypeValue();
			this.interactionResultImage.color = interactionPrompt.InteractionResultColor;
			this.interactionResultImage.gameObject.SetActive(interactionPrompt.InteractionResultSprite != null);
			this.supplementalInteractionResultImage.sprite = interactionPrompt.supplementalInteractionResultSprite;
			this.supplementalInteractionResultImage.gameObject.SetActive(interactionPrompt.supplementalInteractionResultSprite != null);
			this.interactionResultText.text = interactionPrompt.InteractionResultText;
		}

		// Token: 0x06001718 RID: 5912 RVA: 0x0006BEE8 File Offset: 0x0006A0E8
		private void OnNewInteractableFocused(InteractableCollider collider)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnNewInteractableFocused", new object[] { collider.DebugSafeName(true) });
			}
			if (collider == null)
			{
				this.Close();
				return;
			}
			UIInteractionPromptVariable interactionPrompt = collider.InteractableBase.InteractionPrompt;
			this.DisplayInteractionPrompt(interactionPrompt.Value);
			this.promptReplacedTweens.Tween();
		}

		// Token: 0x06001719 RID: 5913 RVA: 0x0006BF4B File Offset: 0x0006A14B
		private void OnInteractionFailed(InteractableBase target)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInteractionFailed", new object[] { target.gameObject.name });
			}
			this.interactFailedTweens.Tween();
		}

		// Token: 0x0600171A RID: 5914 RVA: 0x0006BF80 File Offset: 0x0006A180
		private void OnAnchorRegistered(IUIAnchor anchor)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAnchorRegistered", new object[] { anchor });
			}
			if (anchor is UIDialogueBubbleAnchor)
			{
				if (this.verboseLogging)
				{
					DebugUtility.LogMethod(this, "OnAnchorRegistered", new object[] { anchor });
				}
				this.displayAndHideHandler.Hide();
			}
		}

		// Token: 0x0600171B RID: 5915 RVA: 0x0006BFDC File Offset: 0x0006A1DC
		private void OnAnchorUnregistered(IUIAnchor anchor)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAnchorUnregistered", new object[] { anchor });
			}
			if (anchor is UIDialogueBubbleAnchor)
			{
				if (this.verboseLogging)
				{
					DebugUtility.LogMethod(this, "OnAnchorUnregistered", new object[] { anchor });
				}
				this.displayAndHideHandler.Display();
			}
		}

		// Token: 0x04001280 RID: 4736
		public static Action OnInitializeAction;

		// Token: 0x04001281 RID: 4737
		public static Action OnCloseAction;

		// Token: 0x04001282 RID: 4738
		[Header("UIInteractionPromptAnchor")]
		[SerializeField]
		private Image inputImageFill;

		// Token: 0x04001283 RID: 4739
		[SerializeField]
		private Image interactionResultImage;

		// Token: 0x04001284 RID: 4740
		[SerializeField]
		private TextMeshProUGUI interactionResultText;

		// Token: 0x04001285 RID: 4741
		[SerializeField]
		private Image supplementalInteractionResultImage;

		// Token: 0x04001286 RID: 4742
		[SerializeField]
		private TweenCollection promptReplacedTweens;

		// Token: 0x04001287 RID: 4743
		[SerializeField]
		private TweenCollection interactAttemptedTweens;

		// Token: 0x04001288 RID: 4744
		[SerializeField]
		private TweenCollection interactFailedTweens;

		// Token: 0x04001289 RID: 4745
		private PlayerReferenceManager playerReferenceManager;
	}
}
