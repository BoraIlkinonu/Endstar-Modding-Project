using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000374 RID: 884
	public class HorizontalPlaneCellCollectionMode : PlaneCellCollectionMode
	{
		// Token: 0x060010DF RID: 4319 RVA: 0x000518EC File Offset: 0x0004FAEC
		public override List<Vector3Int> CollectPaintedPositions(Ray ray, int maximumPositions)
		{
			Plane plane = new Plane(Vector3.up, base.PlanePosition);
			return base.CollectPoints(plane, ray, maximumPositions);
		}

		// Token: 0x17000283 RID: 643
		// (get) Token: 0x060010E0 RID: 4320 RVA: 0x00051914 File Offset: 0x0004FB14
		public override string DisplayName
		{
			get
			{
				return "Horizontal";
			}
		}

		// Token: 0x060010E1 RID: 4321 RVA: 0x0005191B File Offset: 0x0004FB1B
		protected override Vector3 GetPlanePosition()
		{
			return base.StartingPoint - Vector3.up * 0.49f;
		}
	}
}
