using UnityEngine;

namespace Endless.Gameplay;

public class RotationWriter
{
	private readonly Transform transform;

	public RotationWriter(Transform transform, IndividualStateUpdater individualStateUpdater)
	{
		this.transform = transform;
		individualStateUpdater.OnWriteState += HandleOnWriteState;
	}

	private void HandleOnWriteState(ref NpcState currentState)
	{
		currentState.Rotation = transform.rotation.eulerAngles.y;
	}
}
