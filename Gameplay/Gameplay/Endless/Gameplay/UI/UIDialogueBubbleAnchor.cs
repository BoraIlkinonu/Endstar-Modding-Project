using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.UI.Anchors;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000397 RID: 919
	public class UIDialogueBubbleAnchor : UIBaseAnchor
	{
		// Token: 0x170004D3 RID: 1235
		// (get) Token: 0x06001763 RID: 5987 RVA: 0x0006D000 File Offset: 0x0006B200
		// (set) Token: 0x06001764 RID: 5988 RVA: 0x0006D008 File Offset: 0x0006B208
		public string NextText { get; private set; }

		// Token: 0x170004D4 RID: 1236
		// (get) Token: 0x06001765 RID: 5989 RVA: 0x0006D011 File Offset: 0x0006B211
		// (set) Token: 0x06001766 RID: 5990 RVA: 0x0006D019 File Offset: 0x0006B219
		public float NextFontSize { get; private set; }

		// Token: 0x06001767 RID: 5991 RVA: 0x0006D022 File Offset: 0x0006B222
		public static UIDialogueBubbleAnchor CreateInstance(UIDialogueBubbleAnchor prefab, Transform target, RectTransform container, string displayName, Vector3? offset = null)
		{
			UIDialogueBubbleAnchor uidialogueBubbleAnchor = UIBaseAnchor.CreateAndInitialize<UIDialogueBubbleAnchor>(prefab, target, container, offset);
			uidialogueBubbleAnchor.SetDisplayName(displayName);
			uidialogueBubbleAnchor.Initialize();
			return uidialogueBubbleAnchor;
		}

		// Token: 0x06001768 RID: 5992 RVA: 0x0006D03B File Offset: 0x0006B23B
		public void SetDisplayName(string displayName)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDisplayName", new object[] { displayName });
			}
			this.displayNameContainer.SetActive(!displayName.IsNullOrEmptyOrWhiteSpace());
			this.displayNameText.text = displayName;
		}

		// Token: 0x06001769 RID: 5993 RVA: 0x0006D07C File Offset: 0x0006B27C
		public void Initialize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			this.SetFontSize(this.defaultFontSize);
			this.onNextTextTweenToSwapTextOnComplete.OnTweenComplete.AddListener(new UnityAction(this.ApplyNextText));
			base.transform.SetAsLastSibling();
		}

		// Token: 0x0600176A RID: 5994 RVA: 0x0006D0D4 File Offset: 0x0006B2D4
		public void DisplayText(string textToDisplay, int currentIndex, int maximumIndex, bool skipOnNextTextTween, float fontSize, bool showIndex = true, bool showInteract = true)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayText", new object[] { textToDisplay, currentIndex, maximumIndex, skipOnNextTextTween, fontSize, showIndex, showInteract });
			}
			this.nextPrompt.SetActive(showInteract);
			if (this.onNextTextTweens.IsAnyTweening())
			{
				this.onNextTextTweens.Cancel();
			}
			if (showIndex)
			{
				this.positionText.text = string.Format("{0}/{1}", currentIndex + 1, maximumIndex);
			}
			this.positionPrompt.SetActive(showIndex);
			if (currentIndex >= maximumIndex)
			{
				this.Close();
			}
			else if (skipOnNextTextTween)
			{
				this.text.text = textToDisplay;
				this.SetFontSize(fontSize);
			}
			else
			{
				this.NextText = textToDisplay;
				this.NextFontSize = fontSize;
				this.onNextTextTweens.Tween();
			}
			UIContentSizeFitter[] array = this.contentSizeFitters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RequestLayout();
			}
		}

		// Token: 0x0600176B RID: 5995 RVA: 0x0006D1EE File Offset: 0x0006B3EE
		public void SetFontSize(float newFontSize)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetFontSize", new object[] { newFontSize });
			}
			this.text.fontSize = newFontSize;
		}

		// Token: 0x0600176C RID: 5996 RVA: 0x0006D21E File Offset: 0x0006B41E
		public void Shake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Shake", Array.Empty<object>());
			}
			this.StopShaking();
			this.anchoredPositionShaker.ShakeDuration = 0.5f;
			this.anchoredPositionShaker.Shake();
		}

		// Token: 0x0600176D RID: 5997 RVA: 0x0006D25C File Offset: 0x0006B45C
		private void ApplyNextText()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplyNextText", Array.Empty<object>());
			}
			this.text.text = this.NextText;
			this.SetFontSize(this.NextFontSize);
			UIContentSizeFitter[] array = this.contentSizeFitters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RequestLayout();
			}
			this.StopShaking();
		}

		// Token: 0x0600176E RID: 5998 RVA: 0x0006D2C1 File Offset: 0x0006B4C1
		private void StopShaking()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "StopShaking", Array.Empty<object>());
			}
			if (!this.anchoredPositionShaker.IsShaking)
			{
				return;
			}
			this.anchoredPositionShaker.Stop();
		}

		// Token: 0x0600176F RID: 5999 RVA: 0x0006D2F4 File Offset: 0x0006B4F4
		public void ShowAlert(string displayText)
		{
			UIDialogueFloatingText uidialogueFloatingText = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIDialogueFloatingText>(this.poolingAlertText, default(Vector3), default(Quaternion), null);
			uidialogueFloatingText.transform.SetParent(MonoBehaviourSingleton<UIAnchorManager>.Instance.transform);
			uidialogueFloatingText.transform.SetPositionAndRotation(base.transform.position + new Vector3(0f, 10f, 0f), Quaternion.identity);
			uidialogueFloatingText.SetDisplayText(displayText);
			uidialogueFloatingText.SetupLocalPositionTween();
			uidialogueFloatingText.Tween();
		}

		// Token: 0x06001770 RID: 6000 RVA: 0x0006D37F File Offset: 0x0006B57F
		public override void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			if (this.displayAndHideHandler.IsTweeningHide)
			{
				return;
			}
			this.StopShaking();
			base.Close();
		}

		// Token: 0x040012CD RID: 4813
		public const float SHAKE_DURATION = 0.5f;

		// Token: 0x040012CE RID: 4814
		[Header("UIDialogueBubbleAnchor")]
		[SerializeField]
		private float defaultFontSize = 22f;

		// Token: 0x040012CF RID: 4815
		[SerializeField]
		private UIContentSizeFitter[] contentSizeFitters = Array.Empty<UIContentSizeFitter>();

		// Token: 0x040012D0 RID: 4816
		[SerializeField]
		private GameObject displayNameContainer;

		// Token: 0x040012D1 RID: 4817
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x040012D2 RID: 4818
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x040012D3 RID: 4819
		[SerializeField]
		private TextMeshProUGUI positionText;

		// Token: 0x040012D4 RID: 4820
		[SerializeField]
		private GameObject nextPrompt;

		// Token: 0x040012D5 RID: 4821
		[SerializeField]
		private UIAnchoredPositionShaker anchoredPositionShaker;

		// Token: 0x040012D6 RID: 4822
		[SerializeField]
		private GameObject positionPrompt;

		// Token: 0x040012D7 RID: 4823
		[SerializeField]
		private TweenCollection onNextTextTweens;

		// Token: 0x040012D8 RID: 4824
		[SerializeField]
		private UIDialogueFloatingText poolingAlertText;

		// Token: 0x040012D9 RID: 4825
		[Tooltip("When this tween complete, it will apply the next text change. Best to do when invisible.")]
		[SerializeField]
		private BaseTween onNextTextTweenToSwapTextOnComplete;
	}
}
