using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000375 RID: 885
	public class FacingPlaneCellCollectionMode : PlaneCellCollectionMode
	{
		// Token: 0x060010E3 RID: 4323 RVA: 0x00051944 File Offset: 0x0004FB44
		public override List<Vector3Int> CollectPaintedPositions(Ray ray, int maximumPositions)
		{
			Vector3 vector = -Vector3.ProjectOnPlane(MonoBehaviourSingleton<CameraController>.Instance.GameplayCamera.transform.forward, Vector3.up).normalized;
			Vector3[] array = new Vector3[]
			{
				Vector3.forward,
				Vector3.back,
				Vector3.right,
				Vector3.left
			};
			float num = -1f;
			Vector3 vector2 = Vector3.zero;
			for (int i = 0; i < array.Length; i++)
			{
				float num2 = Vector3.Dot(array[i], vector);
				if (num2 > num)
				{
					num = num2;
					vector2 = array[i];
				}
			}
			Plane plane = new Plane(vector2, base.PlanePosition);
			return base.CollectPoints(plane, ray, maximumPositions);
		}

		// Token: 0x17000284 RID: 644
		// (get) Token: 0x060010E4 RID: 4324 RVA: 0x00051A0C File Offset: 0x0004FC0C
		public override string DisplayName
		{
			get
			{
				return "Vertical Facing";
			}
		}

		// Token: 0x060010E5 RID: 4325 RVA: 0x0005191B File Offset: 0x0004FB1B
		protected override Vector3 GetPlanePosition()
		{
			return base.StartingPoint - Vector3.up * 0.49f;
		}
	}
}
