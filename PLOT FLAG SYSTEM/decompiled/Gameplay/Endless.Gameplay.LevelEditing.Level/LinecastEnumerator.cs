using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

public class LinecastEnumerator : IEnumerator<Vector3Int>, IEnumerator, IDisposable
{
	private readonly Vector3 endPosition;

	private readonly int steps;

	private readonly Ray ray;

	private int currentStepCount;

	public Vector3Int Current { get; private set; }

	public Vector3 PositionAtStep { get; private set; }

	object IEnumerator.Current => Current;

	public LinecastEnumerator(Ray ray, float length, float scalar)
	{
		this.ray = ray;
		endPosition = ray.origin + ray.direction * length;
		steps = Mathf.CeilToInt(length) + 1;
		steps = (int)((float)steps * scalar);
		Current = Stage.WorldSpacePointToGridCoordinate(ray.origin);
		currentStepCount = 0;
	}

	public LinecastEnumerator(int steps)
	{
		this.steps = steps;
	}

	public void Dispose()
	{
	}

	public bool MoveNext()
	{
		do
		{
			currentStepCount++;
			PositionAtStep = Vector3.Lerp(ray.origin, endPosition, (float)currentStepCount / (float)steps);
			Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(PositionAtStep);
			if (vector3Int != Current)
			{
				Current = vector3Int;
				return true;
			}
		}
		while (currentStepCount < steps);
		return false;
	}

	public void Reset()
	{
		currentStepCount = 0;
	}
}
