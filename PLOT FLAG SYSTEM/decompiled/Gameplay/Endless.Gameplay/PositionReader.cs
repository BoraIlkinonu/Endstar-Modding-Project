using UnityEngine;

namespace Endless.Gameplay;

public class PositionReader
{
	private readonly Transform transform;

	public PositionReader(Transform transform, IndividualStateUpdater individualStateUpdater)
	{
		this.transform = transform;
		individualStateUpdater.OnStateInterpolated += HandleOnStateInterpolated;
	}

	private void HandleOnStateInterpolated(NpcState state)
	{
		if (!float.IsNaN(state.Position.x))
		{
			transform.position = state.Position;
		}
	}
}
