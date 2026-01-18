using Endless.Gameplay.SoVariables;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIInteractionPromptAnchorManager : MonoBehaviourSingleton<UIInteractionPromptAnchorManager>
{
	[SerializeField]
	private UIInteractionPromptAnchor interactionPromptAnchorSource;

	[SerializeField]
	private RectTransform container;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public UIInteractionPromptAnchor CreateInstance(Transform target, PlayerReferenceManager playerReferenceManager, UIInteractionPromptVariable interactionPrompt, Vector3? offset = null)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "CreateInstance", target, container, playerReferenceManager, interactionPrompt, offset);
		}
		return UIInteractionPromptAnchor.CreateInstance(interactionPromptAnchorSource, target, container, playerReferenceManager, interactionPrompt, offset);
	}
}
