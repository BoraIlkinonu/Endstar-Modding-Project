using System;
using System.Collections;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x0200002F RID: 47
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class UITextTypewriterEffect : UIGameObject
	{
		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000140 RID: 320 RVA: 0x000085CB File Offset: 0x000067CB
		private TextMeshProUGUI TextComponent
		{
			get
			{
				if (!this.textComponent)
				{
					base.TryGetComponent<TextMeshProUGUI>(out this.textComponent);
				}
				return this.textComponent;
			}
		}

		// Token: 0x06000141 RID: 321 RVA: 0x000085ED File Offset: 0x000067ED
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			if (this.playOnEnable)
			{
				this.playOnEnable = false;
				this.Play();
			}
		}

		// Token: 0x06000142 RID: 322 RVA: 0x0000861C File Offset: 0x0000681C
		public void Play()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Play", Array.Empty<object>());
			}
			if (!base.gameObject.activeInHierarchy)
			{
				this.playOnEnable = true;
				return;
			}
			switch (this.textTypewriterEffect)
			{
			case TextTypewriterEffects.Characters:
				this.TextComponent.maxVisibleCharacters = 0;
				break;
			case TextTypewriterEffects.Words:
				this.TextComponent.maxVisibleWords = 0;
				break;
			case TextTypewriterEffects.Lines:
				this.TextComponent.maxVisibleLines = 0;
				break;
			default:
				DebugUtility.LogWarning(this, "Play", string.Format("{0} has no support for a {1} value of {2}", "UITextTypewriterEffect", "textTypewriterEffect", this.textTypewriterEffect), Array.Empty<object>());
				break;
			}
			if (this.playCoroutine != null)
			{
				base.StopCoroutine(this.playCoroutine);
			}
			this.playCoroutine = base.StartCoroutine(this.PlayCoroutine());
		}

		// Token: 0x06000143 RID: 323 RVA: 0x000086F0 File Offset: 0x000068F0
		private IEnumerator PlayCoroutine()
		{
			bool isComplete = false;
			while (!isComplete)
			{
				yield return new WaitForSeconds(this.incrementWaitTime);
				switch (this.textTypewriterEffect)
				{
				case TextTypewriterEffects.Characters:
				{
					TextMeshProUGUI textMeshProUGUI = this.TextComponent;
					int num = textMeshProUGUI.maxVisibleCharacters;
					textMeshProUGUI.maxVisibleCharacters = num + 1;
					isComplete = this.TextComponent.maxVisibleCharacters == this.TextComponent.textInfo.characterCount;
					break;
				}
				case TextTypewriterEffects.Words:
				{
					TextMeshProUGUI textMeshProUGUI2 = this.TextComponent;
					int num = textMeshProUGUI2.maxVisibleWords;
					textMeshProUGUI2.maxVisibleWords = num + 1;
					isComplete = this.TextComponent.maxVisibleWords == this.TextComponent.textInfo.wordCount;
					break;
				}
				case TextTypewriterEffects.Lines:
				{
					TextMeshProUGUI textMeshProUGUI3 = this.TextComponent;
					int num = textMeshProUGUI3.maxVisibleLines;
					textMeshProUGUI3.maxVisibleLines = num + 1;
					isComplete = this.TextComponent.maxVisibleWords == this.TextComponent.textInfo.lineCount;
					break;
				}
				default:
					DebugUtility.LogWarning(this, "PlayCoroutine", string.Format("{0} has no support for a {1} value of {2}", "UITextTypewriterEffect", "textTypewriterEffect", this.textTypewriterEffect), Array.Empty<object>());
					break;
				}
			}
			yield break;
		}

		// Token: 0x040000A4 RID: 164
		[SerializeField]
		private TextTypewriterEffects textTypewriterEffect;

		// Token: 0x040000A5 RID: 165
		[SerializeField]
		[Tooltip("In Seconds")]
		private float incrementWaitTime = 0.1f;

		// Token: 0x040000A6 RID: 166
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000A7 RID: 167
		private bool playOnEnable;

		// Token: 0x040000A8 RID: 168
		private Coroutine playCoroutine;

		// Token: 0x040000A9 RID: 169
		private TextMeshProUGUI textComponent;
	}
}
