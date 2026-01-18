using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000351 RID: 849
	public class HittableComponent : EndlessBehaviour, IAwakeSubscriber, IGameEndSubscriber, IComponentBase, IScriptInjector
	{
		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x0600150F RID: 5391 RVA: 0x000650D4 File Offset: 0x000632D4
		// (set) Token: 0x06001510 RID: 5392 RVA: 0x000650DC File Offset: 0x000632DC
		public float CombatCapacity { get; private set; }

		// Token: 0x17000459 RID: 1113
		// (get) Token: 0x06001511 RID: 5393 RVA: 0x000650E5 File Offset: 0x000632E5
		// (set) Token: 0x06001512 RID: 5394 RVA: 0x000650ED File Offset: 0x000632ED
		public PositionPrediction PositionPrediction { get; private set; }

		// Token: 0x1700045A RID: 1114
		// (get) Token: 0x06001513 RID: 5395 RVA: 0x000650F6 File Offset: 0x000632F6
		// (set) Token: 0x06001514 RID: 5396 RVA: 0x000650FE File Offset: 0x000632FE
		public CombatPositionGenerator CombatPositionGenerator { get; private set; }

		// Token: 0x1700045B RID: 1115
		// (get) Token: 0x06001515 RID: 5397 RVA: 0x00065107 File Offset: 0x00063307
		// (set) Token: 0x06001516 RID: 5398 RVA: 0x0006510F File Offset: 0x0006330F
		public HostilityComponent HostilityComponent { get; private set; }

		// Token: 0x1700045C RID: 1116
		// (get) Token: 0x06001517 RID: 5399 RVA: 0x00065118 File Offset: 0x00063318
		// (set) Token: 0x06001518 RID: 5400 RVA: 0x00065120 File Offset: 0x00063320
		public List<Collider> HittableColliders { get; private set; }

		// Token: 0x14000028 RID: 40
		// (add) Token: 0x06001519 RID: 5401 RVA: 0x0006512C File Offset: 0x0006332C
		// (remove) Token: 0x0600151A RID: 5402 RVA: 0x00065164 File Offset: 0x00063364
		public event Action<HittableComponent, HealthModificationArgs> OnDamaged;

		// Token: 0x1700045D RID: 1117
		// (get) Token: 0x0600151B RID: 5403 RVA: 0x00065199 File Offset: 0x00063399
		// (set) Token: 0x0600151C RID: 5404 RVA: 0x000651A1 File Offset: 0x000633A1
		public uint LastAttackedFrame { get; set; }

		// Token: 0x1700045E RID: 1118
		// (get) Token: 0x0600151D RID: 5405 RVA: 0x000651AA File Offset: 0x000633AA
		public HashSet<TargeterComponent> Targeters { get; } = new HashSet<TargeterComponent>();

		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x0600151E RID: 5406 RVA: 0x00017DAC File Offset: 0x00015FAC
		public global::UnityEngine.Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x0600151F RID: 5407 RVA: 0x000651B2 File Offset: 0x000633B2
		public bool HasHealth
		{
			get
			{
				return !this.healthComponent || this.healthComponent.CurrentHealth > 0;
			}
		}

		// Token: 0x17000461 RID: 1121
		// (get) Token: 0x06001520 RID: 5408 RVA: 0x000651D1 File Offset: 0x000633D1
		public bool IsFullHealth
		{
			get
			{
				return !this.healthComponent || (this.healthComponent && this.healthComponent.CurrentHealth == this.healthComponent.MaxHealth);
			}
		}

		// Token: 0x17000462 RID: 1122
		// (get) Token: 0x06001521 RID: 5409 RVA: 0x00065209 File Offset: 0x00063409
		public Team Team
		{
			get
			{
				if (!this.teamComponent)
				{
					return Team.Team1;
				}
				return this.teamComponent.Team;
			}
		}

		// Token: 0x17000463 RID: 1123
		// (get) Token: 0x06001522 RID: 5410 RVA: 0x00065225 File Offset: 0x00063425
		// (set) Token: 0x06001523 RID: 5411 RVA: 0x00065230 File Offset: 0x00063430
		public bool IsTargetable
		{
			get
			{
				return this.isTargetable;
			}
			private set
			{
				if (value)
				{
					MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Add(this);
					MonoBehaviourSingleton<TargetingManager>.Instance.AddTargetable(this);
				}
				else
				{
					MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Remove(this);
					MonoBehaviourSingleton<TargetingManager>.Instance.RemoveTargetable(this);
				}
				this.isTargetable = value;
			}
		}

		// Token: 0x17000464 RID: 1124
		// (get) Token: 0x06001524 RID: 5412 RVA: 0x00065281 File Offset: 0x00063481
		// (set) Token: 0x06001525 RID: 5413 RVA: 0x00065289 File Offset: 0x00063489
		public bool IsDamageable { get; set; } = true;

		// Token: 0x17000465 RID: 1125
		// (get) Token: 0x06001526 RID: 5414 RVA: 0x00065294 File Offset: 0x00063494
		public global::UnityEngine.Vector3 NavPosition
		{
			get
			{
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(base.transform.position + global::UnityEngine.Vector3.down * 0.5f, out navMeshHit, 0.25f, -1))
				{
					return navMeshHit.position;
				}
				return this.WorldObject.transform.position;
			}
		}

		// Token: 0x06001527 RID: 5415 RVA: 0x000652E8 File Offset: 0x000634E8
		internal HealthChangeResult ModifyHealth(HealthModificationArgs modificationArgs)
		{
			if (modificationArgs.Delta < 0)
			{
				if (!this.IsDamageable && modificationArgs.HealthChangeType != HealthChangeType.Unavoidable)
				{
					return HealthChangeResult.NoChange;
				}
				int num = modificationArgs.Delta;
				if (this.shieldComponent)
				{
					num = this.shieldComponent.DamageShields(modificationArgs);
				}
				modificationArgs.Delta = num;
				if (num < 0)
				{
					Action<HittableComponent, HealthModificationArgs> onDamaged = this.OnDamaged;
					if (onDamaged != null)
					{
						onDamaged(this, modificationArgs);
					}
				}
			}
			this.RaiseHealthModifiedEvent(modificationArgs.Source, modificationArgs.Delta);
			if (!this.healthComponent)
			{
				return HealthChangeResult.NoChange;
			}
			return this.healthComponent.ModifyHealth(modificationArgs);
		}

		// Token: 0x06001528 RID: 5416 RVA: 0x0006537E File Offset: 0x0006357E
		public void SetIsTargetable(bool value)
		{
			if (value != this.IsTargetable)
			{
				this.IsTargetable = value;
			}
		}

		// Token: 0x06001529 RID: 5417 RVA: 0x00065390 File Offset: 0x00063590
		public List<TargetDatum> GetTargetableColliderData()
		{
			this.targetData.Clear();
			foreach (Collider collider in this.HittableColliders)
			{
				this.targetData.Add(new TargetDatum
				{
					Position = collider.bounds.center,
					ColliderId = collider.GetInstanceID()
				});
			}
			return this.targetData;
		}

		// Token: 0x0600152A RID: 5418 RVA: 0x00065424 File Offset: 0x00063624
		public float ModifyOwnTargetScore(Context context, float currentScore)
		{
			object[] array;
			if (this.scriptComponent && this.scriptComponent.TryExecuteFunction("ModifyOwnTargetScore", out array, new object[] { currentScore, context }))
			{
				object obj = array[0];
				if (obj is double)
				{
					double num = (double)obj;
					return (float)num;
				}
			}
			return currentScore;
		}

		// Token: 0x0600152B RID: 5419 RVA: 0x0006547C File Offset: 0x0006367C
		public List<Collider> GetTargetableColliders()
		{
			return this.HittableColliders;
		}

		// Token: 0x0600152C RID: 5420 RVA: 0x00065484 File Offset: 0x00063684
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (MonoBehaviourSingleton<PerceptionManager>.Instance)
			{
				MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Remove(this);
			}
			if (MonoBehaviourSingleton<TargetingManager>.Instance)
			{
				MonoBehaviourSingleton<TargetingManager>.Instance.RemoveTargetable(this);
			}
			if (MonoBehaviourSingleton<HittableMap>.Instance)
			{
				MonoBehaviourSingleton<HittableMap>.Instance.RemoveCollidersFromMaps(this);
			}
		}

		// Token: 0x0600152D RID: 5421 RVA: 0x000654E4 File Offset: 0x000636E4
		public void EndlessAwake()
		{
			MonoBehaviourSingleton<CombatManager>.Instance.OnUpdatingCombat += this.HandleOnUpdatingCombat;
			if (this.IsTargetable)
			{
				MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Add(this);
				MonoBehaviourSingleton<TargetingManager>.Instance.AddTargetable(this);
			}
			this.WorldObject.TryGetUserComponent<HealthComponent>(out this.healthComponent);
			this.WorldObject.TryGetUserComponent<ShieldComponent>(out this.shieldComponent);
			this.WorldObject.TryGetUserComponent<TeamComponent>(out this.teamComponent);
			MonoBehaviourSingleton<HittableMap>.Instance.AddCollidersToMaps(this);
			foreach (Collider collider in this.HittableColliders)
			{
				PositionPrediction positionPrediction = collider.gameObject.AddComponent<PositionPrediction>();
				this.PositionPredictions.Add(collider, positionPrediction);
			}
		}

		// Token: 0x0600152E RID: 5422 RVA: 0x000655C4 File Offset: 0x000637C4
		private void HandleOnUpdatingCombat()
		{
			this.CurrentAttackersWeight = 0f;
		}

		// Token: 0x0600152F RID: 5423 RVA: 0x000655D4 File Offset: 0x000637D4
		public void EndlessGameEnd()
		{
			if (this.IsTargetable)
			{
				MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Remove(this);
				MonoBehaviourSingleton<TargetingManager>.Instance.RemoveTargetable(this);
				this.Targeters.Clear();
			}
			foreach (KeyValuePair<Collider, PositionPrediction> keyValuePair in this.PositionPredictions)
			{
				global::UnityEngine.Object.Destroy(keyValuePair.Value);
			}
			this.PositionPredictions.Clear();
			MonoBehaviourSingleton<HittableMap>.Instance.RemoveCollidersFromMaps(this);
			MonoBehaviourSingleton<CombatManager>.Instance.OnUpdatingCombat -= this.HandleOnUpdatingCombat;
		}

		// Token: 0x17000466 RID: 1126
		// (get) Token: 0x06001530 RID: 5424 RVA: 0x00065688 File Offset: 0x00063888
		// (set) Token: 0x06001531 RID: 5425 RVA: 0x00065690 File Offset: 0x00063890
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000467 RID: 1127
		// (get) Token: 0x06001532 RID: 5426 RVA: 0x00065699 File Offset: 0x00063899
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(HittableReferences);
			}
		}

		// Token: 0x06001533 RID: 5427 RVA: 0x000656A8 File Offset: 0x000638A8
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			HittableReferences hittableReferences = (HittableReferences)referenceBase;
			List<Collider> list = new List<Collider>();
			foreach (ColliderInfo colliderInfo in hittableReferences.HittableColliders)
			{
				list.AddRange(colliderInfo.CachedColliders);
			}
			this.HittableColliders = list;
			this.propDamageReaction.Initialize(this, hittableReferences);
		}

		// Token: 0x06001534 RID: 5428 RVA: 0x0006571C File Offset: 0x0006391C
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x06001535 RID: 5429 RVA: 0x00065728 File Offset: 0x00063928
		public object LuaObject
		{
			get
			{
				Hittable hittable;
				if ((hittable = this.luaHittableInterface) == null)
				{
					hittable = (this.luaHittableInterface = new Hittable(this));
				}
				return hittable;
			}
		}

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x06001536 RID: 5430 RVA: 0x0006574E File Offset: 0x0006394E
		public Type LuaObjectType
		{
			get
			{
				return typeof(Hittable);
			}
		}

		// Token: 0x06001537 RID: 5431 RVA: 0x0006575A File Offset: 0x0006395A
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001538 RID: 5432 RVA: 0x00065764 File Offset: 0x00063964
		private void RaiseHealthModifiedEvent(Context other, int delta)
		{
			if (this.scriptComponent)
			{
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnHealthModified", out array, new object[] { other, delta });
			}
		}

		// Token: 0x04001160 RID: 4448
		public ThreatLevel ThreatLevel;

		// Token: 0x04001161 RID: 4449
		[SerializeField]
		private PropDamageReaction propDamageReaction;

		// Token: 0x04001162 RID: 4450
		[SerializeField]
		internal HealthComponent healthComponent;

		// Token: 0x04001163 RID: 4451
		[SerializeField]
		private ShieldComponent shieldComponent;

		// Token: 0x04001164 RID: 4452
		[SerializeField]
		private TeamComponent teamComponent;

		// Token: 0x04001165 RID: 4453
		private bool isTargetable = true;

		// Token: 0x04001166 RID: 4454
		private readonly List<TargetDatum> targetData = new List<TargetDatum>();

		// Token: 0x04001169 RID: 4457
		public float CurrentAttackersWeight;

		// Token: 0x0400116B RID: 4459
		public readonly Dictionary<Collider, PositionPrediction> PositionPredictions = new Dictionary<Collider, PositionPrediction>();

		// Token: 0x0400116E RID: 4462
		[SerializeField]
		private EndlessScriptComponent scriptComponent;

		// Token: 0x0400116F RID: 4463
		private Hittable luaHittableInterface;
	}
}
