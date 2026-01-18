using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

public class UICreatorReferenceManager : MonoBehaviourSingleton<UICreatorReferenceManager>
{
	[field: SerializeField]
	public RectTransform AnchorContainer { get; private set; }
}
