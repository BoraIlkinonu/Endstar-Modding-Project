using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class JumpTesting : MonoBehaviour
{
	[SerializeField]
	private Vector3 startPos;

	[SerializeField]
	private Vector3 endPos;

	[ContextMenu("Test and draw trajectory")]
	private void TestAndDrawTrajectory()
	{
		if (BurstPathfindingUtilities.CanReachJumpPosition((float3)startPos, (float3)endPos, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
		{
			Debug.Log("Can Reach");
			float launchAngleDegrees = BurstPathfindingUtilities.EstimateLaunchAngle((float3)startPos, (float3)endPos);
			if (BurstPathfindingUtilities.CalculateJumpVelocityWithAngle((float3)startPos, (float3)endPos, launchAngleDegrees, NpcMovementValues.Gravity, out var initialVelocity, out var timeOfFlight))
			{
				Debug.Log($"Found launch angle: {initialVelocity}, flight time: {timeOfFlight}");
				List<Vector3> trajectoryPoints = GetTrajectoryPoints(startPos, initialVelocity, timeOfFlight, NpcMovementValues.Gravity, 5);
				if (trajectoryPoints.Count > 0)
				{
					for (int i = 0; i < trajectoryPoints.Count - 1; i++)
					{
						Debug.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1], Color.cyan, 5f);
					}
				}
				else
				{
					Debug.Log("Unable to fin");
				}
			}
			else
			{
				Debug.Log("Unable to find appropriate launch angle");
			}
		}
		else
		{
			Debug.Log("Cannot reach");
		}
	}

	[ContextMenu("Test Droppable Octants")]
	public void TestDroppableOctants()
	{
		int num = 0;
		Vector3Int zero = Vector3Int.zero;
		for (int i = 0; i < 45; i++)
		{
			int num2 = -22 + i;
			for (int j = 0; j < 45; j++)
			{
				int num3 = -22 + j;
				for (int k = 0; k < 101; k++)
				{
					if (num2 != num3 || num3 != 0)
					{
						int y = -k;
						if (BurstPathfindingUtilities.CanReachJumpPosition(position2: (float3)new Vector3Int(num2, y, num3).ToInt3(), position1: (float3)zero.ToInt3(), maxInitialVerticalVelocity: NpcMovementValues.MaxVerticalVelocity, maxHorizontalVelocity: NpcMovementValues.MaxHorizontalVelocity, gravity: NpcMovementValues.Gravity))
						{
							num++;
						}
					}
				}
			}
		}
		Debug.Log($"Can reach {num}");
	}

	private static List<Vector3> GetTrajectoryPoints(Vector3 startPosition, Vector3 initialVelocity, float timeOfFlight, float gravity, int numPoints)
	{
		List<Vector3> list = new List<Vector3>();
		float num = timeOfFlight / (float)numPoints;
		for (int i = 0; i <= numPoints; i++)
		{
			float time = (float)i * num;
			list.Add(BurstPathfindingUtilities.GetPointOnCurve(startPosition, initialVelocity, time, gravity));
		}
		return list;
	}
}
