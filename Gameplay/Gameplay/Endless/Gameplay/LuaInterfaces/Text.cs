using System;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000467 RID: 1127
	public class Text
	{
		// Token: 0x06001C40 RID: 7232 RVA: 0x0007D73C File Offset: 0x0007B93C
		internal Text(TextComponent textComponent)
		{
			this.textComponent = textComponent;
		}

		// Token: 0x06001C41 RID: 7233 RVA: 0x0007D74B File Offset: 0x0007B94B
		public void DisplayText(Context instigator, bool display)
		{
			this.textComponent.DisplayText = display;
		}

		// Token: 0x06001C42 RID: 7234 RVA: 0x0007D759 File Offset: 0x0007B959
		public LocalizedString GetLocalizedText(Context instigator)
		{
			return this.textComponent.RuntimeText;
		}

		// Token: 0x06001C43 RID: 7235 RVA: 0x0007D766 File Offset: 0x0007B966
		public string GetRawText(Context instigator)
		{
			return this.textComponent.RuntimeText.GetLocalizedString();
		}

		// Token: 0x06001C44 RID: 7236 RVA: 0x0007D778 File Offset: 0x0007B978
		public void SetColor(Context instigator, global::UnityEngine.Color color)
		{
			this.textComponent.Color = color;
		}

		// Token: 0x06001C45 RID: 7237 RVA: 0x0007D786 File Offset: 0x0007B986
		public void SetAlpha(Context instigator, float alpha)
		{
			this.textComponent.Alpha = alpha;
		}

		// Token: 0x06001C46 RID: 7238 RVA: 0x0007D794 File Offset: 0x0007B994
		public void SetCharacterSpacing(Context instigator, float spacing)
		{
			this.textComponent.CharacterSpacing = spacing;
		}

		// Token: 0x06001C47 RID: 7239 RVA: 0x0007D7A2 File Offset: 0x0007B9A2
		public void SetLineSpacing(Context instigator, float spacing)
		{
			this.textComponent.LineSpacing = spacing;
		}

		// Token: 0x06001C48 RID: 7240 RVA: 0x0007D7B0 File Offset: 0x0007B9B0
		public void SetFontSize(Context instigator, float fontSize)
		{
			this.textComponent.FontSize = fontSize;
		}

		// Token: 0x06001C49 RID: 7241 RVA: 0x0007D7BE File Offset: 0x0007B9BE
		public void SetAutoSizingEnabled(Context instigator, bool useAutoSizing)
		{
			this.textComponent.EnableAutoSizing = useAutoSizing;
		}

		// Token: 0x06001C4A RID: 7242 RVA: 0x0007D7CC File Offset: 0x0007B9CC
		public void SetAutoSizingMinimum(Context instigator, float minimum)
		{
			this.textComponent.MinFontSize = minimum;
		}

		// Token: 0x06001C4B RID: 7243 RVA: 0x0007D7DA File Offset: 0x0007B9DA
		public void SetAutoSizingMaximum(Context instigator, float maximum)
		{
			this.textComponent.MaxFontSize = maximum;
		}

		// Token: 0x06001C4C RID: 7244 RVA: 0x0007D7E8 File Offset: 0x0007B9E8
		internal void SetAlignment(Context instigator, TextAlignmentOptions textAlignmentOptions)
		{
			this.textComponent.TextAlignmentOptions = textAlignmentOptions;
		}

		// Token: 0x06001C4D RID: 7245 RVA: 0x0007D7F6 File Offset: 0x0007B9F6
		public void SetLocalizedText(Context instigator, LocalizedString text)
		{
			this.textComponent.RuntimeText = text;
		}

		// Token: 0x06001C4E RID: 7246 RVA: 0x0007D804 File Offset: 0x0007BA04
		public void SetRawText(Context instigator, string text)
		{
			this.textComponent.RuntimeText = new LocalizedString();
			this.textComponent.RuntimeText.SetStringValue(text, LocalizedString.ActiveLanguage);
		}

		// Token: 0x040015C9 RID: 5577
		private readonly TextComponent textComponent;
	}
}
