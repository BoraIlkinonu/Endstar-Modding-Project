using UnityEngine;

namespace Endless.Creator;

public class CreatorGun : MonoBehaviour
{
	[SerializeField]
	private Renderer colorableRenderer;

	[SerializeField]
	private string colorPropertyName = "Color";

	[SerializeField]
	private ParticleSystem idleParticleSystem;

	[SerializeField]
	private ParticleSystem muzzleFlashParticleSystem;

	[SerializeField]
	private float colorAmplificationFactor = 32f;

	private Color lastColor = Color.white;

	private void Start()
	{
		ApplyColor(lastColor);
	}

	public void SetColor(Color newColor)
	{
		if (lastColor != newColor)
		{
			ApplyColor(newColor);
		}
	}

	private void ApplyColor(Color newColor)
	{
		Color value = new Color(newColor.r * colorAmplificationFactor, newColor.g * colorAmplificationFactor, newColor.b * colorAmplificationFactor, newColor.a);
		lastColor = newColor;
		colorableRenderer.material.SetColor(colorPropertyName, value);
		ParticleSystem.MainModule main = idleParticleSystem.main;
		main.startColor = newColor;
	}

	public void StartFlash()
	{
		muzzleFlashParticleSystem.Play();
	}
}
