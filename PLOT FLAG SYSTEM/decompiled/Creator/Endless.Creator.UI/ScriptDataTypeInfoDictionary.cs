using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Script Data Type Info Dictionary", fileName = "Script Data Type Info Dictionary")]
public class ScriptDataTypeInfoDictionary : BaseEnumKeyScriptableObjectDictionary<ScriptDataTypes, DisplayNameAndDescription>
{
}
