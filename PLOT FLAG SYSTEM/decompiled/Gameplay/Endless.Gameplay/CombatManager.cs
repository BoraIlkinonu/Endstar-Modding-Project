using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Endless.Gameplay;

public class CombatManager : EndlessBehaviourSingleton<CombatManager>, IStartSubscriber, IGameEndSubscriber
{
	private readonly Dictionary<HittableComponent, List<AttackRequest>> attackRequestsByHittable = new Dictionary<HittableComponent, List<AttackRequest>>();

	private readonly HashSet<NpcEntity> meleeNpcs = new HashSet<NpcEntity>();

	private readonly HashSet<NpcEntity> rangedNpcs = new HashSet<NpcEntity>();

	private readonly HashSet<NpcEntity> zombieNpcs = new HashSet<NpcEntity>();

	private readonly Dictionary<HittableComponent, List<NpcEntity>> potentialAttackersMap = new Dictionary<HittableComponent, List<NpcEntity>>();

	private readonly Dictionary<NpcEntity, HashSet<List<NpcEntity>>> npcPotentialAttackersMembershipMap = new Dictionary<NpcEntity, HashSet<List<NpcEntity>>>();

	[Tooltip("Minimum number of seconds between attacks being scheduled per target")]
	[SerializeField]
	private float minAttackDelay;

	private uint MinAttackDelayFrames => (uint)Mathf.RoundToInt(minAttackDelay / NetClock.FixedDeltaTime);

	public event Action OnUpdatingCombat;

	public void EndlessStart()
	{
		UnifiedStateUpdater.OnUpdateCombat += HandleUpdateCombat;
		attackRequestsByHittable.Clear();
	}

	public void EndlessGameEnd()
	{
		UnifiedStateUpdater.OnUpdateCombat -= HandleUpdateCombat;
	}

	private void HandleUpdateCombat(uint frame)
	{
		this.OnUpdatingCombat?.Invoke();
		using (new ProfilerMarker("Update Combat").Auto())
		{
			UpdateZombieCombatants();
			potentialAttackersMap.Clear();
			npcPotentialAttackersMembershipMap.Clear();
			AddNpcsToCombatMaps(rangedNpcs, npcPotentialAttackersMembershipMap, potentialAttackersMap);
			HittableComponent key;
			List<NpcEntity> value;
			foreach (KeyValuePair<HittableComponent, List<NpcEntity>> item in potentialAttackersMap)
			{
				item.Deconstruct(out key, out value);
				HittableComponent target = key;
				List<NpcEntity> list = value;
				int count = list.Count;
				if (count <= 1)
				{
					if (count == 0)
					{
						continue;
					}
				}
				else
				{
					list.Sort((NpcEntity entity1, NpcEntity entity2) => math.distancesq(target.NavPosition, entity1.Position).CompareTo(math.distancesq(target.NavPosition, entity2.Position)));
				}
				AssignCombatState(list[0], NpcEnum.CombatState.Attacking, target, npcPotentialAttackersMembershipMap);
			}
			AddNpcsToCombatMaps(meleeNpcs, npcPotentialAttackersMembershipMap, potentialAttackersMap);
			foreach (KeyValuePair<HittableComponent, List<NpcEntity>> item2 in potentialAttackersMap)
			{
				item2.Deconstruct(out key, out value);
				HittableComponent target2 = key;
				List<NpcEntity> list2 = value;
				if (list2.Count > 1)
				{
					list2.Sort((NpcEntity entity1, NpcEntity entity2) => math.distancesq(target2.NavPosition, entity1.Position).CompareTo(math.distancesq(target2.NavPosition, entity2.Position)));
				}
			}
			bool flag = false;
			HashSet<HittableComponent> hashSet = new HashSet<HittableComponent>();
			while (!flag && potentialAttackersMap.Count > 0)
			{
				flag = true;
				hashSet.Clear();
				foreach (KeyValuePair<HittableComponent, List<NpcEntity>> item3 in potentialAttackersMap)
				{
					item3.Deconstruct(out key, out value);
					HittableComponent hittableComponent = key;
					List<NpcEntity> list3 = value;
					if (list3.Count == 0)
					{
						hashSet.Add(hittableComponent);
					}
					else if (!(hittableComponent.CurrentAttackersWeight >= hittableComponent.CombatCapacity))
					{
						flag = false;
						AssignCombatState(list3[0], NpcEnum.CombatState.Attacking, hittableComponent, npcPotentialAttackersMembershipMap);
					}
				}
				foreach (HittableComponent item4 in hashSet)
				{
					potentialAttackersMap.Remove(item4);
				}
			}
			while (potentialAttackersMap.Count > 0)
			{
				hashSet.Clear();
				foreach (KeyValuePair<HittableComponent, List<NpcEntity>> item5 in potentialAttackersMap)
				{
					item5.Deconstruct(out key, out value);
					HittableComponent hittableComponent2 = key;
					List<NpcEntity> list4 = value;
					if (list4.Count == 0)
					{
						hashSet.Add(hittableComponent2);
					}
					else
					{
						AssignCombatState(list4[0], NpcEnum.CombatState.Engaged, hittableComponent2, npcPotentialAttackersMembershipMap);
					}
				}
				foreach (HittableComponent item6 in hashSet)
				{
					potentialAttackersMap.Remove(item6);
				}
			}
			foreach (KeyValuePair<HittableComponent, List<AttackRequest>> item7 in attackRequestsByHittable)
			{
				item7.Deconstruct(out key, out var value2);
				HittableComponent hittableComponent3 = key;
				List<AttackRequest> list5 = value2;
				if (list5.Count != 0 && (bool)hittableComponent3 && frame >= hittableComponent3.LastAttackedFrame + MinAttackDelayFrames)
				{
					hittableComponent3.LastAttackedFrame = frame;
					list5[0].Response(list5[0].Requester);
					list5.RemoveAt(0);
				}
			}
		}
	}

	private static void AssignCombatState(NpcEntity attacker, NpcEnum.CombatState combatState, HittableComponent target, Dictionary<NpcEntity, HashSet<List<NpcEntity>>> npcPotentialAttackersMembershipMap)
	{
		if (attacker.Target != target)
		{
			attacker.Components.TargeterComponent.ForceOverrideTarget(target);
		}
		attacker.CombatState = combatState;
		target.CurrentAttackersWeight += attacker.Components.TargeterComponent.CombatWeight;
		foreach (List<NpcEntity> item in npcPotentialAttackersMembershipMap[attacker])
		{
			item?.Remove(attacker);
		}
		npcPotentialAttackersMembershipMap.Remove(attacker);
	}

	private static void AddNpcsToCombatMaps(HashSet<NpcEntity> entities, Dictionary<NpcEntity, HashSet<List<NpcEntity>>> npcPotentialAttackersMembershipMap, Dictionary<HittableComponent, List<NpcEntity>> potentialAttackersMap)
	{
		foreach (NpcEntity entity in entities)
		{
			if (!entity.Target || entity.CombatMode == CombatMode.Passive || (entity.CombatMode == CombatMode.Defensive && IsNotHostileToTarget(entity)))
			{
				entity.CombatState = NpcEnum.CombatState.None;
				continue;
			}
			npcPotentialAttackersMembershipMap.Add(entity, new HashSet<List<NpcEntity>>());
			foreach (HittableComponent currentTarget in entity.Components.TargeterComponent.CurrentTargets)
			{
				if (entity.CombatMode != CombatMode.Defensive || !IsNotHostileToTarget(entity, currentTarget))
				{
					if (potentialAttackersMap.TryGetValue(currentTarget, out var value))
					{
						value.Add(entity);
						npcPotentialAttackersMembershipMap[entity].Add(value);
						continue;
					}
					List<NpcEntity> list = new List<NpcEntity> { entity };
					potentialAttackersMap.Add(currentTarget, list);
					npcPotentialAttackersMembershipMap[entity].Add(list);
				}
			}
		}
	}

	private void UpdateZombieCombatants()
	{
		foreach (NpcEntity zombieNpc in zombieNpcs)
		{
			if (!zombieNpc.Target || (zombieNpc.CombatMode == CombatMode.Defensive && IsNotHostileToTarget(zombieNpc)))
			{
				zombieNpc.CombatState = NpcEnum.CombatState.None;
				continue;
			}
			zombieNpc.CombatState = NpcEnum.CombatState.Attacking;
			zombieNpc.Target.CurrentAttackersWeight += zombieNpc.Components.TargeterComponent.CombatWeight;
		}
	}

	private static bool IsNotHostileToTarget(NpcEntity entity)
	{
		HostilityComponent hostilityComponent = entity.Components.HittableComponent.HostilityComponent;
		if (!hostilityComponent.IsPermanentlyHostile(entity.Target))
		{
			return !hostilityComponent.RecentAttackers.ContainsKey(entity.Target);
		}
		return false;
	}

	private static bool IsNotHostileToTarget(NpcEntity entity, HittableComponent target)
	{
		HostilityComponent hostilityComponent = entity.Components.HittableComponent.HostilityComponent;
		if (!hostilityComponent.IsPermanentlyHostile(target))
		{
			return !hostilityComponent.RecentAttackers.ContainsKey(target);
		}
		return false;
	}

	public void RegisterCombatNpc(NpcEntity entity)
	{
		switch (entity.NpcClass.NpcClass)
		{
		case NpcClass.Grunt:
			meleeNpcs.Add(entity);
			break;
		case NpcClass.Rifleman:
			rangedNpcs.Add(entity);
			break;
		case NpcClass.Zombie:
			zombieNpcs.Add(entity);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case NpcClass.Blank:
			break;
		}
	}

	public void UnregisterCombatNpc(NpcEntity entity)
	{
		switch (entity.NpcClass.NpcClass)
		{
		case NpcClass.Grunt:
			meleeNpcs.Remove(entity);
			break;
		case NpcClass.Rifleman:
			rangedNpcs.Remove(entity);
			break;
		case NpcClass.Zombie:
			zombieNpcs.Remove(entity);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case NpcClass.Blank:
			break;
		}
	}

	public void SubmitAttackRequest(AttackRequest request)
	{
		if (attackRequestsByHittable.TryGetValue(request.Target, out var value))
		{
			value.Add(request);
			return;
		}
		attackRequestsByHittable.Add(request.Target, new List<AttackRequest> { request });
	}

	public void WithdrawAttackRequest(AttackRequest request)
	{
		if ((object)request.Target != null && attackRequestsByHittable.TryGetValue(request.Target, out var value))
		{
			value.Remove(request);
		}
	}
}
