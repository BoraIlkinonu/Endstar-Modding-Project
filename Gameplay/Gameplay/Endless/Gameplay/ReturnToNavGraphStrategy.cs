using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000185 RID: 389
	public class ReturnToNavGraphStrategy : IActionStrategy
	{
		// Token: 0x060008DD RID: 2269 RVA: 0x0002995E File Offset: 0x00027B5E
		public ReturnToNavGraphStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x060008DE RID: 2270 RVA: 0x00029978 File Offset: 0x00027B78
		public Func<float> GetCost { get; }

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x060008DF RID: 2271 RVA: 0x00029980 File Offset: 0x00027B80
		// (set) Token: 0x060008E0 RID: 2272 RVA: 0x00029988 File Offset: 0x00027B88
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008E1 RID: 2273 RVA: 0x00029994 File Offset: 0x00027B94
		public void Start()
		{
			this.Status = GoapAction.Status.InProgress;
			List<Vector3> list = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(this.entity.FootPosition, 1f);
			if (list.Count == 0)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.offMeshPosition = this.entity.FootPosition;
			this.returnPosition = list[0];
			this.startTime = Time.time;
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x000299FC File Offset: 0x00027BFC
		public void Update(float deltaTime)
		{
			float num = (Time.time - this.startTime) / this.lerpTime;
			this.entity.Position = Vector3.Lerp(this.offMeshPosition, this.returnPosition, num) + Vector3.up * 0.5f;
			if (num > 1f)
			{
				this.Status = GoapAction.Status.Complete;
			}
		}

		// Token: 0x0400074A RID: 1866
		private readonly NpcEntity entity;

		// Token: 0x0400074B RID: 1867
		private Vector3 offMeshPosition;

		// Token: 0x0400074C RID: 1868
		private Vector3 returnPosition;

		// Token: 0x0400074D RID: 1869
		private readonly float lerpTime = 1f;

		// Token: 0x0400074E RID: 1870
		private float startTime;
	}
}
