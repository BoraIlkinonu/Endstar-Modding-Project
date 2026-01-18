using Endless.Shared;
using Endless.Shared.Audio;
using UnityEngine;

namespace Endless.Gameplay;

public class FootstepController : MonoBehaviour
{
	[SerializeField]
	private float runningFootstepTriggerThreshold = 0.3f;

	[SerializeField]
	private float walkingFootstepTriggerThreshold = 0.15f;

	[SerializeField]
	private PoolableAudioSource footstepAudioSourcePrefab;

	[SerializeField]
	private AudioGroup footstepGroup;

	[SerializeField]
	private float runningVolume = 0.5f;

	[SerializeField]
	private float walkingVolume = 0.25f;

	private float footstepAccumulation;

	public void UpdateFootsteps(float horizontalVelocityMagnitude, bool walking)
	{
		footstepAccumulation += horizontalVelocityMagnitude * Time.deltaTime;
		float num = (walking ? walkingFootstepTriggerThreshold : runningFootstepTriggerThreshold);
		if (footstepAccumulation > num)
		{
			footstepAccumulation %= num;
			PoolableAudioSource poolableAudioSource = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(footstepAudioSourcePrefab, base.transform.position);
			poolableAudioSource.transform.SetParent(null);
			poolableAudioSource.AudioSource.volume = (walking ? walkingVolume : runningVolume);
			footstepGroup.PlayWithManagedPool(this, poolableAudioSource);
		}
	}
}
