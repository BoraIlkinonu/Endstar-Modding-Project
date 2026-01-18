using System.Collections.Generic;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class HorizontalPlaneCellCollectionMode : PlaneCellCollectionMode
{
	public override string DisplayName => "Horizontal";

	public override List<Vector3Int> CollectPaintedPositions(Ray ray, int maximumPositions)
	{
		Plane plane = new Plane(Vector3.up, base.PlanePosition);
		return CollectPoints(plane, ray, maximumPositions);
	}

	protected override Vector3 GetPlanePosition()
	{
		return base.StartingPoint - Vector3.up * 0.49f;
	}
}
