using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000133 RID: 307
	public class FalloffChecker
	{
		// Token: 0x06000722 RID: 1826 RVA: 0x00021ED6 File Offset: 0x000200D6
		public FalloffChecker(Transform transform, HittableComponent hittable, NpcEntity entity, IndividualStateUpdater stateUpdater)
		{
			this.transform = transform;
			this.entity = entity;
			stateUpdater.OnUpdateState += this.HandleOnUpdateState;
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x00021F00 File Offset: 0x00020100
		private void HandleOnUpdateState(uint obj)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActiveStage || this.transform.position.y >= MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight)
			{
				return;
			}
			this.entity.Components.HittableComponent.ModifyHealth(new HealthModificationArgs(-10000, this.entity.Context.LevelContext, DamageType.Normal, HealthChangeType.Unavoidable));
			this.entity.Despawn();
		}

		// Token: 0x040005B7 RID: 1463
		private readonly Transform transform;

		// Token: 0x040005B8 RID: 1464
		private readonly NpcEntity entity;
	}
}
