using UnityEngine;

namespace Endless.Gameplay;

public class AutoplayAutodestroyEffect : MonoBehaviour
{
	private ParticleSystem targetParticleSystem;

	private void Start()
	{
		targetParticleSystem = GetComponent<ParticleSystem>();
		if ((bool)targetParticleSystem && !targetParticleSystem.isPlaying)
		{
			targetParticleSystem.Play();
		}
	}

	private void Update()
	{
		if ((bool)targetParticleSystem && !targetParticleSystem.IsAlive())
		{
			Object.Destroy(base.gameObject);
		}
	}
}
