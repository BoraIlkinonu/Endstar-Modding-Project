using UnityEngine;

namespace Endless.Gameplay;

public class ThrownBombAppearanceController : NetworkRigidbodyAppearanceController
{
	[SerializeField]
	private ParticleSystem explosionObjectPrefab;

	private bool detonated;

	private uint detonateFrame;

	protected override void AfterUpdate()
	{
		base.AfterUpdate();
		if (!detonated && detonateFrame != 0 && stateRingBuffer.NextInterpolationState.NetFrame <= detonateFrame)
		{
			detonated = true;
			Object.Instantiate(explosionObjectPrefab, base.transform.position, Quaternion.identity).Play();
		}
	}

	public void SetDetonateFrame(uint frame)
	{
		detonateFrame = frame;
	}
}
