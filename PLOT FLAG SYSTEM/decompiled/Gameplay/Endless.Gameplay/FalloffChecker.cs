using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public class FalloffChecker
{
	private readonly Transform transform;

	private readonly NpcEntity entity;

	public FalloffChecker(Transform transform, HittableComponent hittable, NpcEntity entity, IndividualStateUpdater stateUpdater)
	{
		this.transform = transform;
		this.entity = entity;
		stateUpdater.OnUpdateState += HandleOnUpdateState;
	}

	private void HandleOnUpdateState(uint obj)
	{
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && !(transform.position.y >= MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.StageFallOffHeight))
		{
			entity.Components.HittableComponent.ModifyHealth(new HealthModificationArgs(-10000, entity.Context.LevelContext, DamageType.Normal, HealthChangeType.Unavoidable));
			entity.Despawn();
		}
	}
}
