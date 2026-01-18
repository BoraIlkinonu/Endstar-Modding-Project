using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class CombatPositionGenerator : MonoBehaviour
{
	[SerializeField]
	private HittableComponent targetable;

	public CombatPositions CombatPositions { get; private set; }

	private void Awake()
	{
		CombatPositions = new CombatPositions(targetable);
	}

	public bool TryGetClosestMeleePosition(Vector3 originPosition, float attackDistance, out Vector3 meleePosition)
	{
		return TryGetClosestMeleePosition(originPosition, targetable.Position, attackDistance, out meleePosition);
	}

	public static bool TryGetClosestMeleePosition(Vector3 originPosition, Vector3 targetPosition, float attackDistance, out Vector3 meleePosition)
	{
		meleePosition = Vector3.zero;
		if (!NavMesh.SamplePosition(targetPosition + (originPosition - targetPosition).normalized * attackDistance, out var hit, 1f, -1))
		{
			return false;
		}
		meleePosition = hit.position;
		return true;
	}

	public bool TryGetClosestNearPosition(Vector3 originPosition, out Vector3 nearPosition)
	{
		return CombatPositions.TryGetClosestNearPosition(originPosition, out nearPosition);
	}

	public bool TryGetClosestAroundPosition(Vector3 originPosition, out Vector3 aroundPosition)
	{
		return CombatPositions.TryGetClosestAroundPosition(originPosition, out aroundPosition);
	}
}
