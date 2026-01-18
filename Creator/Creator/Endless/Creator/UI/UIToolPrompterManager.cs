using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002AF RID: 687
	public class UIToolPrompterManager : UIMonoBehaviourSingleton<UIToolPrompterManager>
	{
		// Token: 0x06000B91 RID: 2961 RVA: 0x00036600 File Offset: 0x00034800
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayAndHideHandler.OnHideComplete.AddListener(new UnityAction(this.ClearToolPromptText));
			this.initialDisplayTweenApplyTextOnComplete.OnTweenComplete.AddListener(new UnityAction(this.ApplyNextText));
			this.displayNextTextTweenSwapTextOnComplete.OnTweenComplete.AddListener(new UnityAction(this.ApplyNextText));
			this.cancelButton.onClick.AddListener(new UnityAction(this.Cancel));
			this.displayAndHideHandler.SetToHideEnd(true);
			this.ClearToolPromptText();
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(this.Hide));
			this.cancelButtonDisplayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x06000B92 RID: 2962 RVA: 0x000366D0 File Offset: 0x000348D0
		public void Display(string text, bool showCancelButton = false)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[] { text, showCancelButton });
			}
			if (this.displayAndHideHandler.IsTweeningHide)
			{
				return;
			}
			this.nextText = text;
			if (this.toolPromptText.IsNullOrEmptyOrWhiteSpace)
			{
				this.displayAndHideHandler.Display();
				base.gameObject.SetActive(true);
			}
			else
			{
				this.displayNextTextTweens.Tween();
			}
			this.horizontalLayoutGroup.Spacing = (float)(showCancelButton ? 10 : 0);
			this.horizontalLayoutGroup.Padding.right = (showCancelButton ? 0 : this.horizontalLayoutGroup.Padding.left);
			if (showCancelButton)
			{
				this.cancelButtonDisplayAndHideHandler.Display();
				return;
			}
			if (this.cancelButtonDisplayAndHideHandler.IsDisplaying)
			{
				this.cancelButtonDisplayAndHideHandler.Hide();
			}
		}

		// Token: 0x06000B93 RID: 2963 RVA: 0x000367AC File Offset: 0x000349AC
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			if (this.displayNextTextTweens.IsAnyTweening())
			{
				this.displayNextTextTweens.Cancel();
			}
			if (this.initialDisplayTweenApplyTextOnComplete.IsTweening)
			{
				this.initialDisplayTweenApplyTextOnComplete.Cancel();
			}
			if (this.displayNextTextTweenSwapTextOnComplete.IsTweening)
			{
				this.displayNextTextTweenSwapTextOnComplete.Cancel();
			}
			if (this.cancelButtonDisplayAndHideHandler.IsDisplaying)
			{
				this.cancelButtonDisplayAndHideHandler.Hide();
			}
			this.displayAndHideHandler.Hide();
		}

		// Token: 0x06000B94 RID: 2964 RVA: 0x0003683C File Offset: 0x00034A3C
		private void ApplyNextText()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNextText", Array.Empty<object>());
			}
			this.toolPromptText.Value = this.nextText;
		}

		// Token: 0x06000B95 RID: 2965 RVA: 0x00036867 File Offset: 0x00034A67
		private void ClearToolPromptText()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearToolPromptText", Array.Empty<object>());
			}
			this.toolPromptText.Clear();
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x0003688C File Offset: 0x00034A8C
		private void Cancel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Cancel", Array.Empty<object>());
			}
			Action onCancel = UIToolPrompterManager.OnCancel;
			if (onCancel == null)
			{
				return;
			}
			onCancel();
		}

		// Token: 0x040009BE RID: 2494
		public static Action OnCancel;

		// Token: 0x040009BF RID: 2495
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x040009C0 RID: 2496
		[SerializeField]
		private UIHorizontalLayoutGroup horizontalLayoutGroup;

		// Token: 0x040009C1 RID: 2497
		[SerializeField]
		private UIText toolPromptText;

		// Token: 0x040009C2 RID: 2498
		[Tooltip("When this tween complete, it will apply the text. Best to do when invisible.")]
		[SerializeField]
		private BaseTween initialDisplayTweenApplyTextOnComplete;

		// Token: 0x040009C3 RID: 2499
		[SerializeField]
		private TweenCollection displayNextTextTweens;

		// Token: 0x040009C4 RID: 2500
		[Tooltip("When this tween complete, it will apply the next text change. Best to do when invisible.")]
		[SerializeField]
		private BaseTween displayNextTextTweenSwapTextOnComplete;

		// Token: 0x040009C5 RID: 2501
		[SerializeField]
		private UIButton cancelButton;

		// Token: 0x040009C6 RID: 2502
		[SerializeField]
		private UIDisplayAndHideHandler cancelButtonDisplayAndHideHandler;

		// Token: 0x040009C7 RID: 2503
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040009C8 RID: 2504
		private string nextText;
	}
}
