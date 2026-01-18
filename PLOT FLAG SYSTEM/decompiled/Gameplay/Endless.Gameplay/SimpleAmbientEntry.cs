using UnityEngine;

namespace Endless.Gameplay;

public class SimpleAmbientEntry : AmbientEntry
{
	[SerializeField]
	private Material skyboxMaterial;

	[SerializeField]
	private Color ambientLight;

	[SerializeField]
	private Light sunSource;

	public override void Activate()
	{
		base.Activate();
		RenderSettings.sun = sunSource;
		RenderSettings.ambientLight = ambientLight;
		RenderSettings.skybox = skyboxMaterial;
		base.gameObject.SetActive(value: true);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		base.gameObject.SetActive(value: false);
	}
}
