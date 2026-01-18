using UnityEngine;

namespace Endless.Gameplay;

public class RotationReader
{
	private readonly Transform transform;

	private readonly NpcSettings settings;

	public RotationReader(Transform transform, NpcSettings settings, IndividualStateUpdater stateUpdater)
	{
		this.transform = transform;
		this.settings = settings;
		stateUpdater.OnStateInterpolated += HandleOnStateInterpolated;
	}

	private void HandleOnStateInterpolated(NpcState state)
	{
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, state.Rotation, 0f), settings.RotationSpeed * Time.deltaTime);
	}
}
