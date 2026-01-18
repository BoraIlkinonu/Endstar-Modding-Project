using UnityEngine;

namespace Endless.Gameplay;

public struct TargeterDatum
{
	public Vector3 Position;

	public Vector3 LookVector;

	public float ProximityDistance;

	public float MaxDistance;

	public float VerticalViewAngle;

	public float HorizontalViewAngle;

	public bool UseXray;
}
