using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000220 RID: 544
	public class JumpTesting : MonoBehaviour
	{
		// Token: 0x06000B4B RID: 2891 RVA: 0x0003DBE4 File Offset: 0x0003BDE4
		[ContextMenu("Test and draw trajectory")]
		private void TestAndDrawTrajectory()
		{
			float3 @float = this.startPos;
			float3 float2 = this.endPos;
			if (!BurstPathfindingUtilities.CanReachJumpPosition(in @float, in float2, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
			{
				Debug.Log("Cannot reach");
				return;
			}
			Debug.Log("Can Reach");
			float3 float3 = this.startPos;
			float3 float4 = this.endPos;
			float num = BurstPathfindingUtilities.EstimateLaunchAngle(in float3, in float4);
			float3 = this.startPos;
			float4 = this.endPos;
			float3 float5;
			float num2;
			if (!BurstPathfindingUtilities.CalculateJumpVelocityWithAngle(in float3, in float4, num, NpcMovementValues.Gravity, out float5, out num2))
			{
				Debug.Log("Unable to find appropriate launch angle");
				return;
			}
			Debug.Log(string.Format("Found launch angle: {0}, flight time: {1}", float5, num2));
			List<Vector3> trajectoryPoints = JumpTesting.GetTrajectoryPoints(this.startPos, float5, num2, NpcMovementValues.Gravity, 5);
			if (trajectoryPoints.Count > 0)
			{
				for (int i = 0; i < trajectoryPoints.Count - 1; i++)
				{
					Debug.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1], Color.cyan, 5f);
				}
				return;
			}
			Debug.Log("Unable to fin");
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x0003DD24 File Offset: 0x0003BF24
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
							int num4 = -k;
							Vector3Int vector3Int = new Vector3Int(num2, num4, num3);
							float3 @float = zero.ToInt3();
							float3 float2 = vector3Int.ToInt3();
							if (BurstPathfindingUtilities.CanReachJumpPosition(in @float, in float2, NpcMovementValues.MaxVerticalVelocity, NpcMovementValues.MaxHorizontalVelocity, NpcMovementValues.Gravity))
							{
								num++;
							}
						}
					}
				}
			}
			Debug.Log(string.Format("Can reach {0}", num));
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x0003DDE4 File Offset: 0x0003BFE4
		private static List<Vector3> GetTrajectoryPoints(Vector3 startPosition, Vector3 initialVelocity, float timeOfFlight, float gravity, int numPoints)
		{
			List<Vector3> list = new List<Vector3>();
			float num = timeOfFlight / (float)numPoints;
			for (int i = 0; i <= numPoints; i++)
			{
				float num2 = (float)i * num;
				list.Add(BurstPathfindingUtilities.GetPointOnCurve(startPosition, initialVelocity, num2, gravity));
			}
			return list;
		}

		// Token: 0x04000AA8 RID: 2728
		[SerializeField]
		private Vector3 startPos;

		// Token: 0x04000AA9 RID: 2729
		[SerializeField]
		private Vector3 endPos;
	}
}
