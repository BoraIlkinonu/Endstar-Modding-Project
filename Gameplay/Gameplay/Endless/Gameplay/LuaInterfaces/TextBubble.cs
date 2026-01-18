using System;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using NLua;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000468 RID: 1128
	public class TextBubble
	{
		// Token: 0x1700058C RID: 1420
		// (get) Token: 0x06001C4F RID: 7247 RVA: 0x0007D82C File Offset: 0x0007BA2C
		public float ShakeDuration
		{
			get
			{
				return this.textBubble.ShakeDuration;
			}
		}

		// Token: 0x06001C50 RID: 7248 RVA: 0x0007D839 File Offset: 0x0007BA39
		internal TextBubble(TextBubble dialogue)
		{
			this.textBubble = dialogue;
		}

		// Token: 0x06001C51 RID: 7249 RVA: 0x0007D848 File Offset: 0x0007BA48
		public void Display(Context instigator, long index)
		{
			this.textBubble.Display((int)index, true, true);
		}

		// Token: 0x06001C52 RID: 7250 RVA: 0x0007D859 File Offset: 0x0007BA59
		public void Display(Context instigator, long index, bool showProgress, bool showInteract)
		{
			this.textBubble.Display((int)index, showProgress, showInteract);
		}

		// Token: 0x06001C53 RID: 7251 RVA: 0x0007D86B File Offset: 0x0007BA6B
		public void DisplayWithDuration(Context instigator, int index, float duration)
		{
			this.textBubble.DisplayWithDuration(index, duration, true, true);
		}

		// Token: 0x06001C54 RID: 7252 RVA: 0x0007D87C File Offset: 0x0007BA7C
		public void DisplayWithDuration(Context instigator, int index, float duration, bool showProgress, bool showInteract)
		{
			this.textBubble.DisplayWithDuration(index, duration, showProgress, showInteract);
		}

		// Token: 0x06001C55 RID: 7253 RVA: 0x0007D88F File Offset: 0x0007BA8F
		public void DisplayForTarget(Context instigator, Context target, long index)
		{
			if (target == null || !target.IsPlayer())
			{
				throw new ArgumentException("Invalid target passed. Must be a player!", "target");
			}
			this.textBubble.DisplayForTarget(target, (int)index, true, true);
		}

		// Token: 0x06001C56 RID: 7254 RVA: 0x0007D8BC File Offset: 0x0007BABC
		public void DisplayForTarget(Context instigator, Context target, long index, bool showProgress, bool showInteract)
		{
			if (target == null || !target.IsPlayer())
			{
				throw new ArgumentException("Invalid target passed. Must be a player!", "target");
			}
			this.textBubble.DisplayForTarget(target, (int)index, showProgress, showInteract);
		}

		// Token: 0x06001C57 RID: 7255 RVA: 0x0007D8EB File Offset: 0x0007BAEB
		public void DisplayForTargetWithDuration(Context instigator, Context target, long index, float duration)
		{
			if (target == null || !target.IsPlayer())
			{
				throw new ArgumentException("Invalid target passed. Must be a player!", "target");
			}
			this.textBubble.DisplayForTargetWithDuration(target, (int)index, duration, true, true);
		}

		// Token: 0x06001C58 RID: 7256 RVA: 0x0007D91A File Offset: 0x0007BB1A
		public void DisplayForTargetWithDuration(Context instigator, Context target, long index, float duration, bool showProgress, bool showInteract)
		{
			if (target == null || !target.IsPlayer())
			{
				throw new ArgumentException("Invalid target passed. Must be a player!", "target");
			}
			this.textBubble.DisplayForTargetWithDuration(target, (int)index, duration, showProgress, showInteract);
		}

		// Token: 0x06001C59 RID: 7257 RVA: 0x0007D94B File Offset: 0x0007BB4B
		public void Close(Context instigator)
		{
			this.textBubble.Close();
		}

		// Token: 0x06001C5A RID: 7258 RVA: 0x0007D958 File Offset: 0x0007BB58
		public void CloseForTarget(Context instigator, Context target)
		{
			if (target == null || !target.IsPlayer())
			{
				throw new ArgumentException("Invalid target passed. Must be a player!", "target");
			}
			this.textBubble.CloseForTarget(target);
		}

		// Token: 0x06001C5B RID: 7259 RVA: 0x0007D981 File Offset: 0x0007BB81
		public string[] GetTexts()
		{
			return LocalizedStringCollection.GetStrings(this.textBubble.runtimeText, LocalizedString.ActiveLanguage);
		}

		// Token: 0x06001C5C RID: 7260 RVA: 0x0007D99D File Offset: 0x0007BB9D
		public void SetLocalizedTextViaTable(Context instigator, LuaTable newText)
		{
			this.textBubble.UpdateText(newText);
		}

		// Token: 0x06001C5D RID: 7261 RVA: 0x0007D9AB File Offset: 0x0007BBAB
		public int GetTextLength()
		{
			return this.textBubble.GetTextLength();
		}

		// Token: 0x06001C5E RID: 7262 RVA: 0x0007D9B8 File Offset: 0x0007BBB8
		public void SetLocalizedText(Context instigator, LocalizedString[] text)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == null)
				{
					text[i] = new LocalizedString();
				}
			}
			this.textBubble.UpdateText(text);
		}

		// Token: 0x06001C5F RID: 7263 RVA: 0x0007D9F1 File Offset: 0x0007BBF1
		public void SetLocalizedText(Context instigator, LocalizedString text)
		{
			this.textBubble.UpdateText(new LocalizedString[] { text });
		}

		// Token: 0x06001C60 RID: 7264 RVA: 0x0007DA0D File Offset: 0x0007BC0D
		public void SetDisplayName(Context instigator, LocalizedString text)
		{
			this.textBubble.DisplayName = text;
		}

		// Token: 0x06001C61 RID: 7265 RVA: 0x0007DA1B File Offset: 0x0007BC1B
		public void SetFontSize(Context instigator, float fontSize)
		{
			this.textBubble.SetFontSize(fontSize);
		}

		// Token: 0x06001C62 RID: 7266 RVA: 0x0007DA29 File Offset: 0x0007BC29
		public void ResetFontSize(Context instigator)
		{
			this.textBubble.ResetFontSize();
		}

		// Token: 0x06001C63 RID: 7267 RVA: 0x0007DA36 File Offset: 0x0007BC36
		public void ShakeForTarget(Context instigator, Context target)
		{
			if (target == null || !target.IsPlayer())
			{
				throw new ArgumentException("Invalid target passed. Must be a player!", "target");
			}
			this.textBubble.ShakeForTarget(target);
		}

		// Token: 0x06001C64 RID: 7268 RVA: 0x0007DA5F File Offset: 0x0007BC5F
		public void ShakeForAll(Context instigator)
		{
			this.textBubble.ShakeForAll();
		}

		// Token: 0x06001C65 RID: 7269 RVA: 0x0007DA6C File Offset: 0x0007BC6C
		public void ShowAlert(Context instigator, string displayText)
		{
			this.textBubble.ShowAlert(displayText);
		}

		// Token: 0x06001C66 RID: 7270 RVA: 0x0007DA7A File Offset: 0x0007BC7A
		public void ShowAlertForTarget(Context instigator, Context target, string displayText)
		{
			if (target == null || !target.IsPlayer())
			{
				throw new ArgumentException("Invalid target passed. Must be a player!", "target");
			}
			this.textBubble.ShowAlertForTarget(target, displayText);
		}

		// Token: 0x040015CA RID: 5578
		private readonly TextBubble textBubble;
	}
}
