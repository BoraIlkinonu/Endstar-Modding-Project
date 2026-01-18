using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000259 RID: 601
	public class WorldStateDictionary : MonoBehaviourSingleton<WorldStateDictionary>
	{
		// Token: 0x1700023B RID: 571
		public GenericWorldState this[WorldState key]
		{
			get
			{
				return this.worldStateMap[key];
			}
		}

		// Token: 0x06000C58 RID: 3160 RVA: 0x00042C57 File Offset: 0x00040E57
		protected override void Awake()
		{
			base.Awake();
			this.InitDictionary();
		}

		// Token: 0x06000C59 RID: 3161 RVA: 0x00042C68 File Offset: 0x00040E68
		private void InitDictionary()
		{
			this.worldStateMap.Add(WorldState.AttackTarget, new GenericWorldState(WorldState.AttackTarget, (NpcEntity _) => false));
			this.worldStateMap.Add(WorldState.Nothing, new GenericWorldState(WorldState.Nothing, (NpcEntity _) => false));
			this.worldStateMap.Add(WorldState.CanBackstep, new GenericWorldState(WorldState.CanBackstep, new Func<NpcEntity, bool>(WorldStateDictionary.CanBackstep)));
			this.worldStateMap.Add(WorldState.CanReachHealthPickup, new GenericWorldState(WorldState.CanReachHealthPickup, new Func<NpcEntity, bool>(WorldStateDictionary.CanReachHealthPickup)));
			this.worldStateMap.Add(WorldState.EngageTarget, new GenericWorldState(WorldState.EngageTarget, (NpcEntity _) => false));
			this.worldStateMap.Add(WorldState.GotAway, new GenericWorldState(WorldState.GotAway, (NpcEntity _) => false));
			this.worldStateMap.Add(WorldState.HasAttackPermission, new GenericWorldState(WorldState.HasAttackPermission, new Func<NpcEntity, bool>(WorldStateDictionary.HasAttackPermission)));
			this.worldStateMap.Add(WorldState.HasFollowTarget, new GenericWorldState(WorldState.HasFollowTarget, new Func<NpcEntity, bool>(WorldStateDictionary.HasFollowTarget)));
			this.worldStateMap.Add(WorldState.HasLos, new GenericWorldState(WorldState.HasLos, new Func<NpcEntity, bool>(WorldStateDictionary.HasLos)));
			this.worldStateMap.Add(WorldState.HasAttackTarget, new GenericWorldState(WorldState.HasAttackTarget, new Func<NpcEntity, bool>(WorldStateDictionary.HasAttackTarget)));
			this.worldStateMap.Add(WorldState.Idle, new GenericWorldState(WorldState.Idle, (NpcEntity _) => false));
			this.worldStateMap.Add(WorldState.IsAtDestination, new GenericWorldState(WorldState.IsAtDestination, new Func<NpcEntity, bool>(WorldStateDictionary.IsAtDestination)));
			this.worldStateMap.Add(WorldState.IsAttacker, new GenericWorldState(WorldState.IsAttacker, new Func<NpcEntity, bool>(WorldStateDictionary.IsAttacker)));
			this.worldStateMap.Add(WorldState.IsEngaged, new GenericWorldState(WorldState.IsEngaged, new Func<NpcEntity, bool>(WorldStateDictionary.IsEngaged)));
			this.worldStateMap.Add(WorldState.IsRangedAttacker, new GenericWorldState(WorldState.IsRangedAttacker, new Func<NpcEntity, bool>(WorldStateDictionary.IsRangedAttacker)));
			this.worldStateMap.Add(WorldState.IsFarEnoughAwayForMelee, new GenericWorldState(WorldState.IsFarEnoughAwayForMelee, new Func<NpcEntity, bool>(WorldStateDictionary.IsFarEnoughAwayForMelee)));
			this.worldStateMap.Add(WorldState.IsFarEnoughAwayForRanged, new GenericWorldState(WorldState.IsFarEnoughAwayForRanged, new Func<NpcEntity, bool>(WorldStateDictionary.IsFarEnoughAwayForRanged)));
			this.worldStateMap.Add(WorldState.IsInEngageRange, new GenericWorldState(WorldState.IsInEngageRange, new Func<NpcEntity, bool>(WorldStateDictionary.IsInEngageRange)));
			this.worldStateMap.Add(WorldState.IsInMeleeRange, new GenericWorldState(WorldState.IsInMeleeRange, new Func<NpcEntity, bool>(WorldStateDictionary.IsInMeleeRange)));
			this.worldStateMap.Add(WorldState.IsInRangedAttackRange, new GenericWorldState(WorldState.IsInRangedAttackRange, new Func<NpcEntity, bool>(WorldStateDictionary.IsInRangedAttackRange)));
			this.worldStateMap.Add(WorldState.IsNearFollowTarget, new GenericWorldState(WorldState.IsNearFollowTarget, new Func<NpcEntity, bool>(WorldStateDictionary.IsNearFollowTarget)));
			this.worldStateMap.Add(WorldState.IsOutsideCloseRange, new GenericWorldState(WorldState.IsOutsideCloseRange, new Func<NpcEntity, bool>(WorldStateDictionary.IsOutsideCloseRange)));
			this.worldStateMap.Add(WorldState.IsOutsideNearRange, new GenericWorldState(WorldState.IsOutsideNearRange, new Func<NpcEntity, bool>(WorldStateDictionary.IsOutsideNearRange)));
			this.worldStateMap.Add(WorldState.RecoverHealth, new GenericWorldState(WorldState.RecoverHealth, (NpcEntity _) => false));
			this.worldStateMap.Add(WorldState.Rove, new GenericWorldState(WorldState.Rove, (NpcEntity _) => false));
			this.worldStateMap.Add(WorldState.Wander, new GenericWorldState(WorldState.Wander, (NpcEntity _) => false));
			this.worldStateMap.Add(WorldState.CanFidget, new GenericWorldState(WorldState.CanFidget, new Func<NpcEntity, bool>(WorldStateDictionary.CanFidget)));
			this.worldStateMap.Add(WorldState.IsOnNavigableCell, new GenericWorldState(WorldState.IsOnNavigableCell, new Func<NpcEntity, bool>(WorldStateDictionary.IsOnNavigableCell)));
			this.worldStateMap.Add(WorldState.LostTarget, new GenericWorldState(WorldState.LostTarget, new Func<NpcEntity, bool>(WorldStateDictionary.LostTarget)));
			this.worldStateMap.Add(WorldState.IsNotPassive, new GenericWorldState(WorldState.IsNotPassive, new Func<NpcEntity, bool>(WorldStateDictionary.IsNotPassive)));
		}

		// Token: 0x06000C5A RID: 3162 RVA: 0x000430BB File Offset: 0x000412BB
		private static bool IsOnNavigableCell(NpcEntity entity)
		{
			return MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(entity.FootPosition);
		}

		// Token: 0x06000C5B RID: 3163 RVA: 0x000430D0 File Offset: 0x000412D0
		private static bool CanBackstep(NpcEntity entity)
		{
			Vector3 position = entity.Components.TargeterComponent.Target.Position;
			NavMeshHit navMeshHit;
			return NavMesh.SamplePosition(position + (position - position).normalized * entity.OptimalAttackDistance, out navMeshHit, 0.5f, -1);
		}

		// Token: 0x06000C5C RID: 3164 RVA: 0x0001965C File Offset: 0x0001785C
		private static bool CanReachHealthPickup(NpcEntity entity)
		{
			return false;
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x0004311E File Offset: 0x0004131E
		private static bool HasAttackPermission(NpcEntity entity)
		{
			return entity.HasAttackToken;
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x00043126 File Offset: 0x00041326
		private static bool HasFollowTarget(NpcEntity entity)
		{
			return entity.FollowTarget;
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x00043134 File Offset: 0x00041334
		private static bool HasLos(NpcEntity entity)
		{
			if (!entity.IsRangedAttacker || !entity.Components.TargeterComponent.LosProbe)
			{
				return false;
			}
			HittableComponent hittableComponent;
			if (!WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent))
			{
				return false;
			}
			Vector3 position = entity.Components.TargeterComponent.LosProbe.position;
			if (Physics.OverlapSphereNonAlloc(position, 0.1f, WorldStateDictionary.overlappedColliders, WorldStateDictionary.BlockingLayers) > 0)
			{
				return false;
			}
			foreach (TargetDatum targetDatum in hittableComponent.GetTargetableColliderData())
			{
				Vector3 vector = targetDatum.Position - position;
				int num = Physics.RaycastNonAlloc(new Ray(position, vector), WorldStateDictionary.hits, vector.magnitude, WorldStateDictionary.LosMask);
				for (int i = 0; i < num; i++)
				{
					if (((1 << WorldStateDictionary.hits[i].collider.gameObject.layer) & WorldStateDictionary.BlockingLayers.value) != 0)
					{
						return false;
					}
					if (WorldStateDictionary.hits[i].collider.GetInstanceID() == targetDatum.ColliderId)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000C60 RID: 3168 RVA: 0x0004328C File Offset: 0x0004148C
		private static bool HasAttackTarget(NpcEntity entity)
		{
			HittableComponent hittableComponent;
			return WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent);
		}

		// Token: 0x06000C61 RID: 3169 RVA: 0x000432A4 File Offset: 0x000414A4
		private static bool IsAtDestination(NpcEntity entity)
		{
			Vector3 vector;
			return entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.CommandDestination, out vector) && Vector3.Distance(entity.FootPosition, vector) < entity.DestinationTolerance;
		}

		// Token: 0x06000C62 RID: 3170 RVA: 0x000432D8 File Offset: 0x000414D8
		private static bool IsAttacker(NpcEntity entity)
		{
			return entity.CombatState == NpcEnum.CombatState.Attacking;
		}

		// Token: 0x06000C63 RID: 3171 RVA: 0x000432E3 File Offset: 0x000414E3
		private static bool IsEngaged(NpcEntity entity)
		{
			return entity.CombatState == NpcEnum.CombatState.Engaged;
		}

		// Token: 0x06000C64 RID: 3172 RVA: 0x000432EE File Offset: 0x000414EE
		private static bool IsRangedAttacker(NpcEntity entity)
		{
			return entity.IsRangedAttacker;
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x000432F6 File Offset: 0x000414F6
		private static bool IsNotPassive(NpcEntity entity)
		{
			return entity.Components.DynamicAttributes.CombatMode > CombatMode.Passive;
		}

		// Token: 0x06000C66 RID: 3174 RVA: 0x0004330C File Offset: 0x0004150C
		private static bool IsFarEnoughAwayForMelee(NpcEntity entity)
		{
			HittableComponent hittableComponent;
			return WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent) && Vector3.Distance(entity.transform.position, hittableComponent.Position) > entity.CloseDistance - 0.2f;
		}

		// Token: 0x06000C67 RID: 3175 RVA: 0x0004334C File Offset: 0x0004154C
		private static bool IsFarEnoughAwayForRanged(NpcEntity entity)
		{
			HittableComponent hittableComponent;
			return WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent) && Vector3.Distance(entity.transform.position, hittableComponent.Position) > entity.CloseDistance - 0.2f;
		}

		// Token: 0x06000C68 RID: 3176 RVA: 0x0004338C File Offset: 0x0004158C
		private static bool IsInEngageRange(NpcEntity entity)
		{
			HittableComponent hittableComponent;
			return WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent) && Vector3.Distance(entity.transform.position, hittableComponent.Position) < entity.AroundDistance + 0.2f;
		}

		// Token: 0x06000C69 RID: 3177 RVA: 0x000433C9 File Offset: 0x000415C9
		private static bool TryGetAttackTarget(NpcEntity entity, out HittableComponent attackTarget)
		{
			attackTarget = entity.Components.TargeterComponent.Target;
			return attackTarget && attackTarget.transform;
		}

		// Token: 0x06000C6A RID: 3178 RVA: 0x000433F4 File Offset: 0x000415F4
		private static bool IsInMeleeRange(NpcEntity entity)
		{
			HittableComponent hittableComponent;
			if (!WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent))
			{
				return false;
			}
			Vector3 position = hittableComponent.Position;
			return Vector3.Distance(entity.transform.position, position) < entity.NearDistance + 0.2f;
		}

		// Token: 0x06000C6B RID: 3179 RVA: 0x00043434 File Offset: 0x00041634
		private static bool IsInRangedAttackRange(NpcEntity entity)
		{
			HittableComponent hittableComponent;
			if (!WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent))
			{
				return false;
			}
			Vector3 position = hittableComponent.Position;
			return Vector3.Distance(entity.transform.position, position) < entity.MaxRangedAttackDistance;
		}

		// Token: 0x06000C6C RID: 3180 RVA: 0x00043470 File Offset: 0x00041670
		private static bool IsNearFollowTarget(NpcEntity entity)
		{
			HittableComponent followTarget = entity.FollowTarget;
			return followTarget && Vector3.Distance(entity.transform.position, followTarget.Position) <= 1f;
		}

		// Token: 0x06000C6D RID: 3181 RVA: 0x000434B0 File Offset: 0x000416B0
		private static bool IsOutsideCloseRange(NpcEntity entity)
		{
			HittableComponent hittableComponent;
			return WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent) && Vector3.Distance(entity.transform.position, hittableComponent.Position) > entity.CloseDistance - 0.2f;
		}

		// Token: 0x06000C6E RID: 3182 RVA: 0x000434ED File Offset: 0x000416ED
		private static bool LostTarget(NpcEntity entity)
		{
			return !entity.Target && entity.LostTarget;
		}

		// Token: 0x06000C6F RID: 3183 RVA: 0x00043508 File Offset: 0x00041708
		private static bool IsOutsideNearRange(NpcEntity entity)
		{
			HittableComponent hittableComponent;
			return WorldStateDictionary.TryGetAttackTarget(entity, out hittableComponent) && Vector3.Distance(entity.transform.position, hittableComponent.Position) > entity.NearDistance - 0.2f;
		}

		// Token: 0x06000C70 RID: 3184 RVA: 0x00043548 File Offset: 0x00041748
		private static bool CanFidget(NpcEntity entity)
		{
			bool flag;
			return entity.NpcBlackboard.TryGet<bool>(NpcBlackboard.Key.CanFidget, out flag) && flag;
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06000C71 RID: 3185 RVA: 0x00043565 File Offset: 0x00041765
		private static LayerMask LosMask
		{
			get
			{
				return MonoBehaviourSingleton<WorldStateDictionary>.Instance.losMask;
			}
		}

		// Token: 0x1700023D RID: 573
		// (get) Token: 0x06000C72 RID: 3186 RVA: 0x00043571 File Offset: 0x00041771
		private static LayerMask BlockingLayers
		{
			get
			{
				return MonoBehaviourSingleton<WorldStateDictionary>.Instance.blockingLayers;
			}
		}

		// Token: 0x04000B6C RID: 2924
		[SerializeField]
		private LayerMask losMask;

		// Token: 0x04000B6D RID: 2925
		[SerializeField]
		private LayerMask blockingLayers;

		// Token: 0x04000B6E RID: 2926
		private readonly Dictionary<WorldState, GenericWorldState> worldStateMap = new Dictionary<WorldState, GenericWorldState>();

		// Token: 0x04000B6F RID: 2927
		private static readonly RaycastHit[] hits = new RaycastHit[5];

		// Token: 0x04000B70 RID: 2928
		private static readonly Collider[] overlappedColliders = new Collider[5];
	}
}
