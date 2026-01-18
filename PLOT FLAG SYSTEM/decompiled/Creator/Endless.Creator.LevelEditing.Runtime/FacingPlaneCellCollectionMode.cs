using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class FacingPlaneCellCollectionMode : PlaneCellCollectionMode
{
	public override string DisplayName => "Vertical Facing";

	public override List<Vector3Int> CollectPaintedPositions(Ray ray, int maximumPositions)
	{
		Vector3 rhs = -Vector3.ProjectOnPlane(MonoBehaviourSingleton<CameraController>.Instance.GameplayCamera.transform.forward, Vector3.up).normalized;
		Vector3[] array = new Vector3[4]
		{
			Vector3.forward,
			Vector3.back,
			Vector3.right,
			Vector3.left
		};
		float num = -1f;
		Vector3 inNormal = Vector3.zero;
		for (int i = 0; i < array.Length; i++)
		{
			float num2 = Vector3.Dot(array[i], rhs);
			if (num2 > num)
			{
				num = num2;
				inNormal = array[i];
			}
		}
		Plane plane = new Plane(inNormal, base.PlanePosition);
		return CollectPoints(plane, ray, maximumPositions);
	}

	protected override Vector3 GetPlanePosition()
	{
		return base.StartingPoint - Vector3.up * 0.49f;
	}
}
