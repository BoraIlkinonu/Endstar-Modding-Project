using UnityEngine;

namespace Endless.Gameplay;

public class CombatPositionDebugger : MonoBehaviour, NetClock.ISimulateFrameLateSubscriber
{
	[SerializeField]
	private CombatPositionGenerator combatPositionGenerator;

	public void Start()
	{
		NetClock.Register(this);
	}

	public void OnDestroy()
	{
		NetClock.Unregister(this);
	}

	public void SimulateFrameLate(uint frame)
	{
		if ((object)combatPositionGenerator == null || combatPositionGenerator.CombatPositions == null)
		{
			return;
		}
		foreach (Vector3 nearPosition in combatPositionGenerator.CombatPositions.GetNearPositions())
		{
			Debug.DrawLine(nearPosition, nearPosition + Vector3.up, Color.cyan, NetClock.FixedDeltaTime);
		}
		foreach (Vector3 aroundPosition in combatPositionGenerator.CombatPositions.GetAroundPositions())
		{
			Debug.DrawLine(aroundPosition, aroundPosition + Vector3.up, Color.magenta, NetClock.FixedDeltaTime);
		}
	}
}
