using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public struct PerceptionRequest
{
	public bool IsBoxcast;

	public Vector3 Position;

	public Vector3 LookVector;

	public float ProximityDistance;

	public float MaxDistance;

	public float VerticalValue;

	public float HorizontalValue;

	public bool UseXray;

	public List<PerceptionResult> PerceptionResults;

	public Action PerceptionUpdatedCallback;

	public TargeterDatum GetTargeterDatum()
	{
		return new TargeterDatum
		{
			Position = Position,
			LookVector = LookVector,
			ProximityDistance = ProximityDistance,
			MaxDistance = MaxDistance,
			VerticalViewAngle = VerticalValue,
			HorizontalViewAngle = HorizontalValue,
			UseXray = UseXray
		};
	}
}
