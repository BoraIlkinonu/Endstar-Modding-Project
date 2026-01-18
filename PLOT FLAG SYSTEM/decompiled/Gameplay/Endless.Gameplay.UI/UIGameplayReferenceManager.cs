using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIGameplayReferenceManager : MonoBehaviourSingleton<UIGameplayReferenceManager>
{
	[field: SerializeField]
	public RectTransform AnchorContainer { get; private set; }

	[field: SerializeField]
	public RectTransform GameplayWindowContainer { get; private set; }
}
