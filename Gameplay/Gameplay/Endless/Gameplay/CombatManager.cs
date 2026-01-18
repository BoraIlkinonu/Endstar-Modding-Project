using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000243 RID: 579
	public class CombatManager : EndlessBehaviourSingleton<CombatManager>, IStartSubscriber, IGameEndSubscriber
	{
		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06000BF3 RID: 3059 RVA: 0x00040FD0 File Offset: 0x0003F1D0
		private uint MinAttackDelayFrames
		{
			get
			{
				return (uint)Mathf.RoundToInt(this.minAttackDelay / NetClock.FixedDeltaTime);
			}
		}

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x06000BF4 RID: 3060 RVA: 0x00040FE4 File Offset: 0x0003F1E4
		// (remove) Token: 0x06000BF5 RID: 3061 RVA: 0x0004101C File Offset: 0x0003F21C
		public event Action OnUpdatingCombat;

		// Token: 0x06000BF6 RID: 3062 RVA: 0x00041051 File Offset: 0x0003F251
		public void EndlessStart()
		{
			UnifiedStateUpdater.OnUpdateCombat += this.HandleUpdateCombat;
			this.attackRequestsByHittable.Clear();
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x0004106F File Offset: 0x0003F26F
		public void EndlessGameEnd()
		{
			UnifiedStateUpdater.OnUpdateCombat -= this.HandleUpdateCombat;
		}

		// Token: 0x06000BF8 RID: 3064 RVA: 0x00041084 File Offset: 0x0003F284
		private void HandleUpdateCombat(uint frame)
		{
			Action onUpdatingCombat = this.OnUpdatingCombat;
			if (onUpdatingCombat != null)
			{
				onUpdatingCombat();
			}
			ProfilerMarker profilerMarker = new ProfilerMarker("Update Combat");
			using (profilerMarker.Auto())
			{
				this.UpdateZombieCombatants();
				this.potentialAttackersMap.Clear();
				this.npcPotentialAttackersMembershipMap.Clear();
				CombatManager.AddNpcsToCombatMaps(this.rangedNpcs, this.npcPotentialAttackersMembershipMap, this.potentialAttackersMap);
				foreach (KeyValuePair<HittableComponent, List<NpcEntity>> keyValuePair in this.potentialAttackersMap)
				{
					HittableComponent hittableComponent;
					List<NpcEntity> list;
					keyValuePair.Deconstruct(out hittableComponent, out list);
					HittableComponent target2 = hittableComponent;
					List<NpcEntity> list2 = list;
					int count = list2.Count;
					if (count <= 1)
					{
						if (count == 0)
						{
							continue;
						}
					}
					else
					{
						list2.Sort((NpcEntity entity1, NpcEntity entity2) => math.distancesq(target2.NavPosition, entity1.Position).CompareTo(math.distancesq(target2.NavPosition, entity2.Position)));
					}
					CombatManager.AssignCombatState(list2[0], NpcEnum.CombatState.Attacking, target2, this.npcPotentialAttackersMembershipMap);
				}
				CombatManager.AddNpcsToCombatMaps(this.meleeNpcs, this.npcPotentialAttackersMembershipMap, this.potentialAttackersMap);
				foreach (KeyValuePair<HittableComponent, List<NpcEntity>> keyValuePair in this.potentialAttackersMap)
				{
					HittableComponent hittableComponent;
					List<NpcEntity> list;
					keyValuePair.Deconstruct(out hittableComponent, out list);
					HittableComponent target = hittableComponent;
					List<NpcEntity> list3 = list;
					if (list3.Count > 1)
					{
						list3.Sort((NpcEntity entity1, NpcEntity entity2) => math.distancesq(target.NavPosition, entity1.Position).CompareTo(math.distancesq(target.NavPosition, entity2.Position)));
					}
				}
				bool flag = false;
				HashSet<HittableComponent> hashSet = new HashSet<HittableComponent>();
				while (!flag)
				{
					if (this.potentialAttackersMap.Count <= 0)
					{
						break;
					}
					flag = true;
					hashSet.Clear();
					foreach (KeyValuePair<HittableComponent, List<NpcEntity>> keyValuePair in this.potentialAttackersMap)
					{
						HittableComponent hittableComponent;
						List<NpcEntity> list;
						keyValuePair.Deconstruct(out hittableComponent, out list);
						HittableComponent hittableComponent2 = hittableComponent;
						List<NpcEntity> list4 = list;
						if (list4.Count == 0)
						{
							hashSet.Add(hittableComponent2);
						}
						else if (hittableComponent2.CurrentAttackersWeight < hittableComponent2.CombatCapacity)
						{
							flag = false;
							CombatManager.AssignCombatState(list4[0], NpcEnum.CombatState.Attacking, hittableComponent2, this.npcPotentialAttackersMembershipMap);
						}
					}
					foreach (HittableComponent hittableComponent3 in hashSet)
					{
						this.potentialAttackersMap.Remove(hittableComponent3);
					}
				}
				while (this.potentialAttackersMap.Count > 0)
				{
					hashSet.Clear();
					foreach (KeyValuePair<HittableComponent, List<NpcEntity>> keyValuePair in this.potentialAttackersMap)
					{
						HittableComponent hittableComponent;
						List<NpcEntity> list;
						keyValuePair.Deconstruct(out hittableComponent, out list);
						HittableComponent hittableComponent4 = hittableComponent;
						List<NpcEntity> list5 = list;
						if (list5.Count == 0)
						{
							hashSet.Add(hittableComponent4);
						}
						else
						{
							CombatManager.AssignCombatState(list5[0], NpcEnum.CombatState.Engaged, hittableComponent4, this.npcPotentialAttackersMembershipMap);
						}
					}
					foreach (HittableComponent hittableComponent5 in hashSet)
					{
						this.potentialAttackersMap.Remove(hittableComponent5);
					}
				}
				foreach (KeyValuePair<HittableComponent, List<AttackRequest>> keyValuePair2 in this.attackRequestsByHittable)
				{
					HittableComponent hittableComponent;
					List<AttackRequest> list6;
					keyValuePair2.Deconstruct(out hittableComponent, out list6);
					HittableComponent hittableComponent6 = hittableComponent;
					List<AttackRequest> list7 = list6;
					if (list7.Count != 0 && hittableComponent6 && frame >= hittableComponent6.LastAttackedFrame + this.MinAttackDelayFrames)
					{
						hittableComponent6.LastAttackedFrame = frame;
						list7[0].Response(list7[0].Requester);
						list7.RemoveAt(0);
					}
				}
			}
		}

		// Token: 0x06000BF9 RID: 3065 RVA: 0x00041514 File Offset: 0x0003F714
		private static void AssignCombatState(NpcEntity attacker, NpcEnum.CombatState combatState, HittableComponent target, Dictionary<NpcEntity, HashSet<List<NpcEntity>>> npcPotentialAttackersMembershipMap)
		{
			if (attacker.Target != target)
			{
				attacker.Components.TargeterComponent.ForceOverrideTarget(target);
			}
			attacker.CombatState = combatState;
			target.CurrentAttackersWeight += attacker.Components.TargeterComponent.CombatWeight;
			foreach (List<NpcEntity> list in npcPotentialAttackersMembershipMap[attacker])
			{
				if (list != null)
				{
					list.Remove(attacker);
				}
			}
			npcPotentialAttackersMembershipMap.Remove(attacker);
		}

		// Token: 0x06000BFA RID: 3066 RVA: 0x000415B8 File Offset: 0x0003F7B8
		private static void AddNpcsToCombatMaps(HashSet<NpcEntity> entities, Dictionary<NpcEntity, HashSet<List<NpcEntity>>> npcPotentialAttackersMembershipMap, Dictionary<HittableComponent, List<NpcEntity>> potentialAttackersMap)
		{
			foreach (NpcEntity npcEntity in entities)
			{
				if (!npcEntity.Target || npcEntity.CombatMode == CombatMode.Passive || (npcEntity.CombatMode == CombatMode.Defensive && CombatManager.IsNotHostileToTarget(npcEntity)))
				{
					npcEntity.CombatState = NpcEnum.CombatState.None;
				}
				else
				{
					npcPotentialAttackersMembershipMap.Add(npcEntity, new HashSet<List<NpcEntity>>());
					foreach (HittableComponent hittableComponent in npcEntity.Components.TargeterComponent.CurrentTargets)
					{
						if (npcEntity.CombatMode != CombatMode.Defensive || !CombatManager.IsNotHostileToTarget(npcEntity, hittableComponent))
						{
							List<NpcEntity> list;
							if (potentialAttackersMap.TryGetValue(hittableComponent, out list))
							{
								list.Add(npcEntity);
								npcPotentialAttackersMembershipMap[npcEntity].Add(list);
							}
							else
							{
								List<NpcEntity> list2 = new List<NpcEntity> { npcEntity };
								potentialAttackersMap.Add(hittableComponent, list2);
								npcPotentialAttackersMembershipMap[npcEntity].Add(list2);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000BFB RID: 3067 RVA: 0x000416E0 File Offset: 0x0003F8E0
		private void UpdateZombieCombatants()
		{
			foreach (NpcEntity npcEntity in this.zombieNpcs)
			{
				if (!npcEntity.Target || (npcEntity.CombatMode == CombatMode.Defensive && CombatManager.IsNotHostileToTarget(npcEntity)))
				{
					npcEntity.CombatState = NpcEnum.CombatState.None;
				}
				else
				{
					npcEntity.CombatState = NpcEnum.CombatState.Attacking;
					npcEntity.Target.CurrentAttackersWeight += npcEntity.Components.TargeterComponent.CombatWeight;
				}
			}
		}

		// Token: 0x06000BFC RID: 3068 RVA: 0x0004177C File Offset: 0x0003F97C
		private static bool IsNotHostileToTarget(NpcEntity entity)
		{
			HostilityComponent hostilityComponent = entity.Components.HittableComponent.HostilityComponent;
			return !hostilityComponent.IsPermanentlyHostile(entity.Target) && !hostilityComponent.RecentAttackers.ContainsKey(entity.Target);
		}

		// Token: 0x06000BFD RID: 3069 RVA: 0x000417C0 File Offset: 0x0003F9C0
		private static bool IsNotHostileToTarget(NpcEntity entity, HittableComponent target)
		{
			HostilityComponent hostilityComponent = entity.Components.HittableComponent.HostilityComponent;
			return !hostilityComponent.IsPermanentlyHostile(target) && !hostilityComponent.RecentAttackers.ContainsKey(target);
		}

		// Token: 0x06000BFE RID: 3070 RVA: 0x000417F8 File Offset: 0x0003F9F8
		public void RegisterCombatNpc(NpcEntity entity)
		{
			switch (entity.NpcClass.NpcClass)
			{
			case NpcClass.Blank:
				return;
			case NpcClass.Grunt:
				this.meleeNpcs.Add(entity);
				return;
			case NpcClass.Rifleman:
				this.rangedNpcs.Add(entity);
				return;
			case NpcClass.Zombie:
				this.zombieNpcs.Add(entity);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06000BFF RID: 3071 RVA: 0x0004185C File Offset: 0x0003FA5C
		public void UnregisterCombatNpc(NpcEntity entity)
		{
			switch (entity.NpcClass.NpcClass)
			{
			case NpcClass.Blank:
				return;
			case NpcClass.Grunt:
				this.meleeNpcs.Remove(entity);
				return;
			case NpcClass.Rifleman:
				this.rangedNpcs.Remove(entity);
				return;
			case NpcClass.Zombie:
				this.zombieNpcs.Remove(entity);
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06000C00 RID: 3072 RVA: 0x000418C0 File Offset: 0x0003FAC0
		public void SubmitAttackRequest(AttackRequest request)
		{
			List<AttackRequest> list;
			if (this.attackRequestsByHittable.TryGetValue(request.Target, out list))
			{
				list.Add(request);
				return;
			}
			this.attackRequestsByHittable.Add(request.Target, new List<AttackRequest> { request });
		}

		// Token: 0x06000C01 RID: 3073 RVA: 0x00041908 File Offset: 0x0003FB08
		public void WithdrawAttackRequest(AttackRequest request)
		{
			List<AttackRequest> list;
			if (request.Target != null && this.attackRequestsByHittable.TryGetValue(request.Target, out list))
			{
				list.Remove(request);
			}
		}

		// Token: 0x04000B1A RID: 2842
		private readonly Dictionary<HittableComponent, List<AttackRequest>> attackRequestsByHittable = new Dictionary<HittableComponent, List<AttackRequest>>();

		// Token: 0x04000B1B RID: 2843
		private readonly HashSet<NpcEntity> meleeNpcs = new HashSet<NpcEntity>();

		// Token: 0x04000B1C RID: 2844
		private readonly HashSet<NpcEntity> rangedNpcs = new HashSet<NpcEntity>();

		// Token: 0x04000B1D RID: 2845
		private readonly HashSet<NpcEntity> zombieNpcs = new HashSet<NpcEntity>();

		// Token: 0x04000B1E RID: 2846
		private readonly Dictionary<HittableComponent, List<NpcEntity>> potentialAttackersMap = new Dictionary<HittableComponent, List<NpcEntity>>();

		// Token: 0x04000B1F RID: 2847
		private readonly Dictionary<NpcEntity, HashSet<List<NpcEntity>>> npcPotentialAttackersMembershipMap = new Dictionary<NpcEntity, HashSet<List<NpcEntity>>>();

		// Token: 0x04000B20 RID: 2848
		[Tooltip("Minimum number of seconds between attacks being scheduled per target")]
		[SerializeField]
		private float minAttackDelay;
	}
}
