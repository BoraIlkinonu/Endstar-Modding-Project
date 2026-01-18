using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class HostilityComponent : EndlessBehaviour, IScriptInjector, IStartSubscriber, IGameEndSubscriber, IHostilityComponent, IUpdateComponent
{
	private const float HOSTILITY_LOSS_RATE = 25f;

	private const float HOSTILITY_DAMAGE_ADDEND = 40f;

	[SerializeField]
	private HittableComponent hittableComponent;

	[SerializeField]
	private float attackerMemorySeconds;

	[SerializeField]
	private int hitsToPermanentlyHostile;

	private object luaInterface;

	private float AttackerRemovalValue => 0f - HostilityLossRate * attackerMemorySeconds;

	public Dictionary<HittableComponent, float> RecentAttackers { get; } = new Dictionary<HittableComponent, float>();

	public Dictionary<HittableComponent, int> PastAttackers { get; } = new Dictionary<HittableComponent, int>();

	private static float DeltaTime => NetClock.FixedDeltaTime;

	internal float HostilityLossRate { get; set; } = 25f;

	internal float HostilityDamageAddend { get; set; } = 40f;

	public object LuaObject => luaInterface ?? new Hostility(this);

	public Type LuaObjectType => typeof(Hostility);

	private void HandleOnDamaged(HittableComponent _, HealthModificationArgs healthModificationArgs)
	{
		Context source = healthModificationArgs.Source;
		if ((object)source?.WorldObject != null && source.WorldObject.TryGetUserComponent<HittableComponent>(out var component))
		{
			if (!RecentAttackers.TryAdd(component, HostilityDamageAddend))
			{
				RecentAttackers[component] += HostilityDamageAddend;
			}
			if (!PastAttackers.TryAdd(component, 1))
			{
				PastAttackers[component]++;
			}
		}
	}

	public bool IsPermanentlyHostile(HittableComponent target)
	{
		if (PastAttackers.ContainsKey(target))
		{
			return PastAttackers[target] >= hitsToPermanentlyHostile;
		}
		return false;
	}

	public void EndlessStart()
	{
		if (base.IsServer)
		{
			UnifiedStateUpdater.RegisterUpdateComponent(this);
			if ((bool)hittableComponent)
			{
				hittableComponent.OnDamaged += HandleOnDamaged;
			}
		}
	}

	public void EndlessGameEnd()
	{
		if (base.IsServer)
		{
			UnifiedStateUpdater.UnregisterUpdateComponent(this);
		}
	}

	public void UpdateHostility()
	{
		HittableComponent[] array = RecentAttackers.Keys.ToArray();
		List<HittableComponent> list = new List<HittableComponent>();
		HittableComponent[] array2 = array;
		foreach (HittableComponent hittableComponent in array2)
		{
			RecentAttackers[hittableComponent] -= 25f * DeltaTime;
			if (RecentAttackers[hittableComponent] < AttackerRemovalValue)
			{
				list.Add(hittableComponent);
			}
			foreach (HittableComponent item in list)
			{
				RecentAttackers.Remove(item);
			}
		}
	}
}
