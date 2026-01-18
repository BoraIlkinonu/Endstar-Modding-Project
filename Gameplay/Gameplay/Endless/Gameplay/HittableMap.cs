using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000352 RID: 850
	public class HittableMap : EndlessBehaviourSingleton<HittableMap>
	{
		// Token: 0x0600153A RID: 5434 RVA: 0x000657DC File Offset: 0x000639DC
		public void AddCollidersToMaps(HittableComponent hittableComponent)
		{
			foreach (Collider collider in hittableComponent.HittableColliders)
			{
				this.hittableMap.Add(collider, hittableComponent);
				this.colliderIdMap.Add(collider.GetInstanceID(), hittableComponent);
			}
		}

		// Token: 0x0600153B RID: 5435 RVA: 0x00065848 File Offset: 0x00063A48
		public void RemoveCollidersFromMaps(HittableComponent hittableComponent)
		{
			foreach (Collider collider in hittableComponent.HittableColliders)
			{
				this.hittableMap.Remove(collider);
				this.colliderIdMap.Remove(collider.GetInstanceID());
			}
		}

		// Token: 0x0600153C RID: 5436 RVA: 0x000658B4 File Offset: 0x00063AB4
		public HittableComponent GetHittableFromMap(Collider hitCollider)
		{
			return this.hittableMap.GetValueOrDefault(hitCollider);
		}

		// Token: 0x0600153D RID: 5437 RVA: 0x000658C2 File Offset: 0x00063AC2
		public HittableComponent GetHittableFromMap(int instanceId)
		{
			return this.colliderIdMap.GetValueOrDefault(instanceId);
		}

		// Token: 0x04001170 RID: 4464
		private readonly Dictionary<Collider, HittableComponent> hittableMap = new Dictionary<Collider, HittableComponent>();

		// Token: 0x04001171 RID: 4465
		private readonly Dictionary<int, HittableComponent> colliderIdMap = new Dictionary<int, HittableComponent>();
	}
}
