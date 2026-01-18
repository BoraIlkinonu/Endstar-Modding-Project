using UnityEngine;

namespace Endless.Gameplay;

public class DynamicObjectResetter : EndlessBehaviour, IStartSubscriber, IGameEndSubscriber
{
	private Vector3 startPosition;

	private Quaternion startRotation;

	public void EndlessStart()
	{
		startPosition = base.transform.position;
		startRotation = base.transform.rotation;
	}

	public void EndlessGameEnd()
	{
		base.transform.position = startPosition;
		base.transform.rotation = startRotation;
	}
}
