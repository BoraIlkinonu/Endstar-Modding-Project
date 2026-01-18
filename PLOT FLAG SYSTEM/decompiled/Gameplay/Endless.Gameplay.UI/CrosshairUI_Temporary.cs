using UnityEngine;

namespace Endless.Gameplay.UI;

public class CrosshairUI_Temporary : MonoBehaviour
{
	private void Start()
	{
		base.gameObject.SetActive(value: false);
	}
}
