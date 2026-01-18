using System;
using Endless.Creator.Test.LuaParsing;

namespace Endless.Creator.UI;

[Serializable]
public struct UIUserScriptAutocompleteListModelItem
{
	public TokenGroupTypes Type;

	public string Value;

	public UIUserScriptAutocompleteListModelItem(TokenGroupTypes type, string value)
	{
		Type = type;
		Value = value;
	}

	public override string ToString()
	{
		return string.Format("| {0}: {1}, {2}: {3} |", "Type", Type, "Value", Value);
	}
}
