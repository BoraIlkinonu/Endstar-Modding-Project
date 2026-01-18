using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class CombatPositions
{
	private readonly HittableComponent targetable;

	private Vector3 generatedPosition;

	public bool hasGeneratedPositions;

	private readonly List<Vector3> nearPositions = new List<Vector3>();

	private readonly List<Vector3> aroundPositions = new List<Vector3>();

	public CombatPositions(HittableComponent targetableComponent)
	{
		targetable = targetableComponent;
		generatedPosition = new Vector3(-5000f, -5000f, -5000f);
	}

	public bool TryGetClosestNearPosition(Vector3 originPosition, out Vector3 closestPosition)
	{
		CheckCombatPositions();
		return TryGetClosestPosition(nearPositions, originPosition, out closestPosition);
	}

	public bool TryGetClosestAroundPosition(Vector3 originPosition, out Vector3 closestPosition)
	{
		CheckCombatPositions();
		return TryGetClosestPosition(aroundPositions, originPosition, out closestPosition);
	}

	public List<Vector3> GetNearPositions()
	{
		CheckCombatPositions();
		return new List<Vector3>(nearPositions);
	}

	public List<Vector3> GetAroundPositions()
	{
		CheckCombatPositions();
		return new List<Vector3>(aroundPositions);
	}

	private void CheckCombatPositions()
	{
		if (!hasGeneratedPositions || Vector3.Distance(generatedPosition, targetable.Position) < 0.3f)
		{
			BuildPositions();
		}
	}

	private void BuildPositions()
	{
		generatedPosition = targetable.Position;
		nearPositions.Clear();
		aroundPositions.Clear();
		List<Vector3> list = GeneratePotentialPositions(2f, 6);
		List<Vector3> list2 = GeneratePotentialPositions(4f, 12);
		foreach (Vector3 item in list)
		{
			if (SamplePositions(item, out var sampledPosition))
			{
				nearPositions.Add(sampledPosition);
			}
		}
		foreach (Vector3 item2 in list2)
		{
			if (SamplePositions(item2, out var sampledPosition2))
			{
				aroundPositions.Add(sampledPosition2);
			}
		}
	}

	private List<Vector3> GeneratePotentialPositions(float radius, int numPositions)
	{
		List<Vector3> list = new List<Vector3>();
		float num = 0f;
		float num2 = 360f / (float)numPositions;
		for (int i = 0; i < numPositions; i++)
		{
			float f = num * (MathF.PI / 180f);
			Vector3 item = generatedPosition + new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f)) * radius;
			list.Add(item);
			num += num2;
		}
		return list;
	}

	private static bool SamplePositions(Vector3 candidatePosition, out Vector3 sampledPosition)
	{
		if (NavMesh.SamplePosition(candidatePosition, out var hit, 0.5f, -1))
		{
			sampledPosition = hit.position;
			return true;
		}
		if (NavMesh.SamplePosition(candidatePosition + Vector3.up, out hit, 0.5f, -1))
		{
			sampledPosition = hit.position;
			return true;
		}
		if (NavMesh.SamplePosition(candidatePosition + Vector3.down, out hit, 0.5f, -1))
		{
			sampledPosition = hit.position;
			return true;
		}
		sampledPosition = Vector3.zero;
		return false;
	}

	private static bool TryGetClosestPosition(List<Vector3> positions, Vector3 originPosition, out Vector3 closestPosition)
	{
		closestPosition = Vector3.zero;
		if (positions.Count == 0)
		{
			return false;
		}
		float num = float.PositiveInfinity;
		foreach (Vector3 position in positions)
		{
			float num2 = Vector3.Distance(originPosition, position);
			if (!(num2 >= num))
			{
				closestPosition = position;
				num = num2;
			}
		}
		return true;
	}
}
