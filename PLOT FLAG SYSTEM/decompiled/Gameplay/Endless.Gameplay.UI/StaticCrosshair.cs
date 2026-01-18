using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class StaticCrosshair : CrosshairBase
{
	[SerializeField]
	private Image image;

	public override void OnShow()
	{
		image.enabled = true;
	}

	public override void OnHide()
	{
		image.enabled = false;
	}
}
