using System;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using NLua;

namespace Endless.Gameplay.LuaInterfaces;

public class TextBubble
{
	private readonly Endless.Gameplay.TextBubble textBubble;

	public float ShakeDuration => textBubble.ShakeDuration;

	internal TextBubble(Endless.Gameplay.TextBubble dialogue)
	{
		textBubble = dialogue;
	}

	public void Display(Context instigator, long index)
	{
		textBubble.Display((int)index);
	}

	public void Display(Context instigator, long index, bool showProgress, bool showInteract)
	{
		textBubble.Display((int)index, showProgress, showInteract);
	}

	public void DisplayWithDuration(Context instigator, int index, float duration)
	{
		textBubble.DisplayWithDuration(index, duration);
	}

	public void DisplayWithDuration(Context instigator, int index, float duration, bool showProgress, bool showInteract)
	{
		textBubble.DisplayWithDuration(index, duration, showProgress, showInteract);
	}

	public void DisplayForTarget(Context instigator, Context target, long index)
	{
		if (target == null || !target.IsPlayer())
		{
			throw new ArgumentException("Invalid target passed. Must be a player!", "target");
		}
		textBubble.DisplayForTarget(target, (int)index);
	}

	public void DisplayForTarget(Context instigator, Context target, long index, bool showProgress, bool showInteract)
	{
		if (target == null || !target.IsPlayer())
		{
			throw new ArgumentException("Invalid target passed. Must be a player!", "target");
		}
		textBubble.DisplayForTarget(target, (int)index, showProgress, showInteract);
	}

	public void DisplayForTargetWithDuration(Context instigator, Context target, long index, float duration)
	{
		if (target == null || !target.IsPlayer())
		{
			throw new ArgumentException("Invalid target passed. Must be a player!", "target");
		}
		textBubble.DisplayForTargetWithDuration(target, (int)index, duration);
	}

	public void DisplayForTargetWithDuration(Context instigator, Context target, long index, float duration, bool showProgress, bool showInteract)
	{
		if (target == null || !target.IsPlayer())
		{
			throw new ArgumentException("Invalid target passed. Must be a player!", "target");
		}
		textBubble.DisplayForTargetWithDuration(target, (int)index, duration, showProgress, showInteract);
	}

	public void Close(Context instigator)
	{
		textBubble.Close();
	}

	public void CloseForTarget(Context instigator, Context target)
	{
		if (target == null || !target.IsPlayer())
		{
			throw new ArgumentException("Invalid target passed. Must be a player!", "target");
		}
		textBubble.CloseForTarget(target);
	}

	public string[] GetTexts()
	{
		return LocalizedStringCollection.GetStrings(textBubble.runtimeText, LocalizedString.ActiveLanguage);
	}

	public void SetLocalizedTextViaTable(Context instigator, LuaTable newText)
	{
		textBubble.UpdateText(newText);
	}

	public int GetTextLength()
	{
		return textBubble.GetTextLength();
	}

	public void SetLocalizedText(Context instigator, LocalizedString[] text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == null)
			{
				text[i] = new LocalizedString();
			}
		}
		textBubble.UpdateText(text);
	}

	public void SetLocalizedText(Context instigator, LocalizedString text)
	{
		textBubble.UpdateText(new LocalizedString[1] { text });
	}

	public void SetDisplayName(Context instigator, LocalizedString text)
	{
		textBubble.DisplayName = text;
	}

	public void SetFontSize(Context instigator, float fontSize)
	{
		textBubble.SetFontSize(fontSize);
	}

	public void ResetFontSize(Context instigator)
	{
		textBubble.ResetFontSize();
	}

	public void ShakeForTarget(Context instigator, Context target)
	{
		if (target == null || !target.IsPlayer())
		{
			throw new ArgumentException("Invalid target passed. Must be a player!", "target");
		}
		textBubble.ShakeForTarget(target);
	}

	public void ShakeForAll(Context instigator)
	{
		textBubble.ShakeForAll();
	}

	public void ShowAlert(Context instigator, string displayText)
	{
		textBubble.ShowAlert(displayText);
	}

	public void ShowAlertForTarget(Context instigator, Context target, string displayText)
	{
		if (target == null || !target.IsPlayer())
		{
			throw new ArgumentException("Invalid target passed. Must be a player!", "target");
		}
		textBubble.ShowAlertForTarget(target, displayText);
	}
}
