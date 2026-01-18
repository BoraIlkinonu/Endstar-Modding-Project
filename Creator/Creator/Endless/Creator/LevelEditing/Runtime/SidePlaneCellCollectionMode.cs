using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Shared;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000376 RID: 886
	public class SidePlaneCellCollectionMode : PlaneCellCollectionMode
	{
		// Token: 0x060010E7 RID: 4327 RVA: 0x00051A14 File Offset: 0x0004FC14
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
			float num = 1f;
			Vector3 vector2 = Vector3.zero;
			for (int i = 0; i < array.Length; i++)
			{
				float num2 = Vector3.Dot(array[i], vector);
				if (num2 > 0f && num2 < num)
				{
					num = num2;
					vector2 = array[i];
				}
			}
			Plane plane = new Plane(vector2, base.PlanePosition);
			return base.CollectPoints(plane, ray, maximumPositions);
		}

		// Token: 0x17000285 RID: 645
		// (get) Token: 0x060010E8 RID: 4328 RVA: 0x00051AE5 File Offset: 0x0004FCE5
		public override string DisplayName
		{
			get
			{
				return "Vertical Long";
			}
		}

		// Token: 0x060010E9 RID: 4329 RVA: 0x0005191B File Offset: 0x0004FB1B
		protected override Vector3 GetPlanePosition()
		{
			return base.StartingPoint - Vector3.up * 0.49f;
		}
	}
}
