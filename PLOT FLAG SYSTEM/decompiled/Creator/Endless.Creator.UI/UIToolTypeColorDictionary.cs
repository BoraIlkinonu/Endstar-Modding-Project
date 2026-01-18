using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Tool Type Color Dictionary", fileName = "Tool Type Color Dictionary")]
public class UIToolTypeColorDictionary : BaseEnumKeyScriptableObjectDictionary<ToolType, Color>
{
}
