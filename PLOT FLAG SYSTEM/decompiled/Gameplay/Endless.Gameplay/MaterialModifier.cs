using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public class MaterialModifier : EndlessBehaviour
{
	[SerializeField]
	private float hurtFlashDuration = 0.1f;

	[SerializeField]
	protected List<Renderer> renderers = new List<Renderer>();

	protected MaterialPropertyBlock propBlock;

	private static readonly int hurtFlash = Shader.PropertyToID("HURT_FLASH");

	private static readonly int emissiveCracksAmount = Shader.PropertyToID("_Emissive_Cracks_Amount");

	private void Awake()
	{
		propBlock = new MaterialPropertyBlock();
		if (renderers.Count == 0)
		{
			renderers.AddRange(GetComponentsInChildren<Renderer>());
		}
	}

	private void OnValidate()
	{
		if (renderers.Count == 0)
		{
			renderers.AddRange(GetComponentsInChildren<Renderer>());
		}
	}

	public void SetZombieCracks(float value)
	{
		foreach (Renderer renderer in renderers)
		{
			renderer.material.SetFloat(emissiveCracksAmount, value);
		}
	}

	private IEnumerator HurtFlash()
	{
		for (float timeElapsed = 0f; timeElapsed < hurtFlashDuration; timeElapsed += Time.deltaTime)
		{
			float value = Mathf.Lerp(0f, 1f, timeElapsed / 0.1f);
			foreach (Renderer renderer in renderers)
			{
				renderer.material.SetFloat(hurtFlash, value);
			}
			yield return null;
		}
		foreach (Renderer renderer2 in renderers)
		{
			renderer2.material.SetFloat(hurtFlash, 0f);
		}
	}

	public void StartHurtFlash()
	{
		StartCoroutine(HurtFlash());
	}
}
