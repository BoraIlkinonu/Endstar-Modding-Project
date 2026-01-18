using System.Text;
using Endless.Gameplay;

namespace Endless.Creator.UI;

public struct UIWireColorDropdownValue
{
	public readonly UIWireColorDictionaryEntryValue WireColorDictionaryEntryValue;

	public readonly WireColor WireColor;

	public string WireColorName;

	public UIWireColorDropdownValue(UIWireColorDictionaryEntryValue wireColorDictionaryEntryValue, WireColor wireColor)
	{
		WireColorDictionaryEntryValue = wireColorDictionaryEntryValue;
		WireColor = wireColor;
		WireColorName = AddSpaceBeforeCapitals(wireColor.ToString(), preserveAcronyms: false);
	}

	private static string AddSpaceBeforeCapitals(string input, bool preserveAcronyms)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			if (ShouldInsertSpace(input, i, preserveAcronyms))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(input[i]);
		}
		return stringBuilder.ToString();
	}

	private static bool ShouldInsertSpace(string input, int index, bool preserveAcronyms)
	{
		if (index <= 0 || !char.IsUpper(input[index]))
		{
			return false;
		}
		if (preserveAcronyms && index < input.Length - 1 && char.IsUpper(input[index + 1]))
		{
			return false;
		}
		return true;
	}
}
