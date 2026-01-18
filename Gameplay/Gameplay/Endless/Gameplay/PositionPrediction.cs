using System;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x0200014A RID: 330
	public class PositionPrediction : EndlessBehaviour, NetClock.ISimulateFrameLateSubscriber
	{
		// Token: 0x060007D6 RID: 2006 RVA: 0x00024D98 File Offset: 0x00022F98
		public bool TryGetPredictedNavigationPosition(float predictionTime, out Vector3 position)
		{
			position = default(Vector3);
			Vector3 predictedPosition = this.GetPredictedPosition(predictionTime);
			predictedPosition.y = this.buffer[0].y + 0.1f;
			RaycastHit raycastHit;
			NavMeshHit navMeshHit;
			if (Physics.Raycast(predictedPosition, Vector3.down, out raycastHit, 15f, LayerMask.GetMask(new string[] { "Default" })) && NavMesh.SamplePosition(raycastHit.point, out navMeshHit, 1f, NpcEntity.NavFilter))
			{
				position = navMeshHit.position;
				return true;
			}
			Vector3CircularBuffer vector3CircularBuffer = this.buffer;
			NavMeshHit navMeshHit2;
			if (Physics.Raycast(vector3CircularBuffer[vector3CircularBuffer.Count - 1], Vector3.down, out raycastHit, 15f, LayerMask.GetMask(new string[] { "Default" })) && NavMesh.SamplePosition(raycastHit.point, out navMeshHit2, 1f, NpcEntity.NavFilter))
			{
				position = navMeshHit2.position;
				return true;
			}
			return false;
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x00024E88 File Offset: 0x00023088
		public Vector3 GetPredictedPosition(float predictionTime)
		{
			Vector3 vector = this.buffer[0];
			Vector3CircularBuffer vector3CircularBuffer = this.buffer;
			Vector3 vector2 = vector3CircularBuffer[vector3CircularBuffer.Count - 1];
			float num = (float)this.buffer.Count * NetClock.FixedDeltaTime;
			Vector3 vector3 = (vector2 - vector) / num;
			return base.transform.position + vector3 * predictionTime;
		}

		// Token: 0x060007D8 RID: 2008 RVA: 0x00021ACF File Offset: 0x0001FCCF
		protected override void Start()
		{
			base.Start();
			NetClock.Register(this);
		}

		// Token: 0x060007D9 RID: 2009 RVA: 0x00021ADD File Offset: 0x0001FCDD
		protected override void OnDestroy()
		{
			base.OnDestroy();
			NetClock.Unregister(this);
		}

		// Token: 0x060007DA RID: 2010 RVA: 0x00024EEC File Offset: 0x000230EC
		public void SimulateFrameLate(uint frame)
		{
			this.buffer.Add(base.transform.position);
		}

		// Token: 0x0400063B RID: 1595
		private const int BUFFER_SIZE = 6;

		// Token: 0x0400063C RID: 1596
		private readonly Vector3CircularBuffer buffer = new Vector3CircularBuffer(6);
	}
}
