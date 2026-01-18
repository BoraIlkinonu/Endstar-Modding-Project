using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class HitEffect : MonoBehaviour, IPoolableT
{
	[SerializeField]
	private ParticleSystem particles;

	private bool isActive;

	public ParticleSystem Particles => particles;

	public MonoBehaviour Prefab { get; set; }

	public void OnSpawn()
	{
		base.transform.SetParent(null, worldPositionStays: true);
		isActive = true;
	}

	public void OnDespawn()
	{
		particles.Stop();
	}

	public void PlayEffect()
	{
		particles.Play();
	}

	private void OnParticleSystemStopped()
	{
		if (isActive)
		{
			isActive = false;
			if (ProjectileManager.UsePooling)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
