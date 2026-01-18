using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

[CreateAssetMenu(menuName = "ScriptableObject/UI/Core/Dictionaries/Game Asset Type Style Dictionary", fileName = "Game Asset Type Style Dictionary")]
public class UIGameAssetTypeStyleDictionary : BaseEnumKeyScriptableObjectDictionary<UIGameAssetTypes, UIGameAssetTypeStyle>
{
}
