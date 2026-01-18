using UnityEngine;

namespace Endless.Creator.LevelEditing;

public class CellReferenceIndicator : MonoBehaviour
{
	[SerializeField]
	private GameObject arrow;

	public void SetRotationArrowEnabled(bool arrowEnabled)
	{
		arrow.SetActive(arrowEnabled);
	}
}
