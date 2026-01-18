using Endless.Creator.Test.LuaParsing;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Token Group Type Color Dictionary", fileName = "Token Group Type Color Dictionary")]
public class UITokenGroupTypeColorDictionary : BaseEnumKeyScriptableObjectDictionary<TokenGroupTypes, Color>
{
	[field: SerializeField]
	public Color LineHighlightColor { get; private set; } = Color.white;

	[field: SerializeField]
	public Color LineErrorColor { get; private set; } = Color.red;
}
