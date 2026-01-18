using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

[CreateAssetMenu(menuName = "ScriptableObject/UI/Core/Dictionaries/Game Asset Type Filter Dictionary", fileName = "Game Asset Type Filter Dictionary")]
public class UIGameAssetTypeFilterDictionary : BaseEnumKeyScriptableObjectDictionary<UIGameAssetTypes, string[]>
{
}
