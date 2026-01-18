using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200006D RID: 109
	public class HostilityComponent : EndlessBehaviour, IScriptInjector, IStartSubscriber, IGameEndSubscriber, IHostilityComponent, IUpdateComponent
	{
		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060001B5 RID: 437 RVA: 0x0000A759 File Offset: 0x00008959
		private float AttackerRemovalValue
		{
			get
			{
				return 0f - this.HostilityLossRate * this.attackerMemorySeconds;
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060001B6 RID: 438 RVA: 0x0000A76E File Offset: 0x0000896E
		public Dictionary<HittableComponent, float> RecentAttackers { get; } = new Dictionary<HittableComponent, float>();

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060001B7 RID: 439 RVA: 0x0000A776 File Offset: 0x00008976
		public Dictionary<HittableComponent, int> PastAttackers { get; } = new Dictionary<HittableComponent, int>();

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060001B8 RID: 440 RVA: 0x0000A77E File Offset: 0x0000897E
		private static float DeltaTime
		{
			get
			{
				return NetClock.FixedDeltaTime;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060001B9 RID: 441 RVA: 0x0000A785 File Offset: 0x00008985
		// (set) Token: 0x060001BA RID: 442 RVA: 0x0000A78D File Offset: 0x0000898D
		internal float HostilityLossRate { get; set; } = 25f;

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060001BB RID: 443 RVA: 0x0000A796 File Offset: 0x00008996
		// (set) Token: 0x060001BC RID: 444 RVA: 0x0000A79E File Offset: 0x0000899E
		internal float HostilityDamageAddend { get; set; } = 40f;

		// Token: 0x060001BD RID: 445 RVA: 0x0000A7A8 File Offset: 0x000089A8
		private void HandleOnDamaged(HittableComponent _, HealthModificationArgs healthModificationArgs)
		{
			Context source = healthModificationArgs.Source;
			HittableComponent hittableComponent;
			if (((source != null) ? source.WorldObject : null) == null || !source.WorldObject.TryGetUserComponent<HittableComponent>(out hittableComponent))
			{
				return;
			}
			if (!this.RecentAttackers.TryAdd(hittableComponent, this.HostilityDamageAddend))
			{
				Dictionary<HittableComponent, float> recentAttackers = this.RecentAttackers;
				HittableComponent hittableComponent2 = hittableComponent;
				recentAttackers[hittableComponent2] += this.HostilityDamageAddend;
			}
			if (!this.PastAttackers.TryAdd(hittableComponent, 1))
			{
				Dictionary<HittableComponent, int> pastAttackers = this.PastAttackers;
				HittableComponent hittableComponent2 = hittableComponent;
				pastAttackers[hittableComponent2]++;
			}
		}

		// Token: 0x060001BE RID: 446 RVA: 0x0000A837 File Offset: 0x00008A37
		public bool IsPermanentlyHostile(HittableComponent target)
		{
			return this.PastAttackers.ContainsKey(target) && this.PastAttackers[target] >= this.hitsToPermanentlyHostile;
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060001BF RID: 447 RVA: 0x0000A860 File Offset: 0x00008A60
		public object LuaObject
		{
			get
			{
				return this.luaInterface ?? new Hostility(this);
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x0000A872 File Offset: 0x00008A72
		public Type LuaObjectType
		{
			get
			{
				return typeof(Hostility);
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x0000A87E File Offset: 0x00008A7E
		public void EndlessStart()
		{
			if (!base.IsServer)
			{
				return;
			}
			UnifiedStateUpdater.RegisterUpdateComponent(this);
			if (this.hittableComponent)
			{
				this.hittableComponent.OnDamaged += this.HandleOnDamaged;
			}
		}

		// Token: 0x060001C2 RID: 450 RVA: 0x0000A8B3 File Offset: 0x00008AB3
		public void EndlessGameEnd()
		{
			if (!base.IsServer)
			{
				return;
			}
			UnifiedStateUpdater.UnregisterUpdateComponent(this);
		}

		// Token: 0x060001C3 RID: 451 RVA: 0x0000A8C4 File Offset: 0x00008AC4
		public void UpdateHostility()
		{
			HittableComponent[] array = this.RecentAttackers.Keys.ToArray<HittableComponent>();
			List<HittableComponent> list = new List<HittableComponent>();
			foreach (HittableComponent hittableComponent in array)
			{
				Dictionary<HittableComponent, float> recentAttackers = this.RecentAttackers;
				HittableComponent hittableComponent2 = hittableComponent;
				recentAttackers[hittableComponent2] -= 25f * HostilityComponent.DeltaTime;
				if (this.RecentAttackers[hittableComponent] < this.AttackerRemovalValue)
				{
					list.Add(hittableComponent);
				}
				foreach (HittableComponent hittableComponent3 in list)
				{
					this.RecentAttackers.Remove(hittableComponent3);
				}
			}
		}

		// Token: 0x04000192 RID: 402
		private const float HOSTILITY_LOSS_RATE = 25f;

		// Token: 0x04000193 RID: 403
		private const float HOSTILITY_DAMAGE_ADDEND = 40f;

		// Token: 0x04000194 RID: 404
		[SerializeField]
		private HittableComponent hittableComponent;

		// Token: 0x04000195 RID: 405
		[SerializeField]
		private float attackerMemorySeconds;

		// Token: 0x04000196 RID: 406
		[SerializeField]
		private int hitsToPermanentlyHostile;

		// Token: 0x0400019B RID: 411
		private object luaInterface;
	}
}
