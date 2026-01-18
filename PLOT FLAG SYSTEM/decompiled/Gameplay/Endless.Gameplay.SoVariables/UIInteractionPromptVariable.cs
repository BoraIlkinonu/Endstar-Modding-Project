using Endless.Gameplay.UI;
using Endless.Shared.SoVariables;
using UnityEngine;

namespace Endless.Gameplay.SoVariables;

[CreateAssetMenu(menuName = "SoVariable/Create UIInteractionPromptVariable", fileName = "Interaction Prompt Variable", order = int.MaxValue)]
public class UIInteractionPromptVariable : SoVariable<UIInteractionPrompt>
{
}
