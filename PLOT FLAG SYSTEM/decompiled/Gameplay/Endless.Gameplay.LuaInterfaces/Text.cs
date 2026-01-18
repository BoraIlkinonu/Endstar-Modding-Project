using Endless.Gameplay.Scripting;
using Endless.Shared;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces;

public class Text
{
	private readonly TextComponent textComponent;

	internal Text(TextComponent textComponent)
	{
		this.textComponent = textComponent;
	}

	public void DisplayText(Context instigator, bool display)
	{
		textComponent.DisplayText = display;
	}

	public LocalizedString GetLocalizedText(Context instigator)
	{
		return textComponent.RuntimeText;
	}

	public string GetRawText(Context instigator)
	{
		return textComponent.RuntimeText.GetLocalizedString();
	}

	public void SetColor(Context instigator, UnityEngine.Color color)
	{
		textComponent.Color = color;
	}

	public void SetAlpha(Context instigator, float alpha)
	{
		textComponent.Alpha = alpha;
	}

	public void SetCharacterSpacing(Context instigator, float spacing)
	{
		textComponent.CharacterSpacing = spacing;
	}

	public void SetLineSpacing(Context instigator, float spacing)
	{
		textComponent.LineSpacing = spacing;
	}

	public void SetFontSize(Context instigator, float fontSize)
	{
		textComponent.FontSize = fontSize;
	}

	public void SetAutoSizingEnabled(Context instigator, bool useAutoSizing)
	{
		textComponent.EnableAutoSizing = useAutoSizing;
	}

	public void SetAutoSizingMinimum(Context instigator, float minimum)
	{
		textComponent.MinFontSize = minimum;
	}

	public void SetAutoSizingMaximum(Context instigator, float maximum)
	{
		textComponent.MaxFontSize = maximum;
	}

	internal void SetAlignment(Context instigator, TextAlignmentOptions textAlignmentOptions)
	{
		textComponent.TextAlignmentOptions = textAlignmentOptions;
	}

	public void SetLocalizedText(Context instigator, LocalizedString text)
	{
		textComponent.RuntimeText = text;
	}

	public void SetRawText(Context instigator, string text)
	{
		textComponent.RuntimeText = new LocalizedString();
		textComponent.RuntimeText.SetStringValue(text, LocalizedString.ActiveLanguage);
	}
}
