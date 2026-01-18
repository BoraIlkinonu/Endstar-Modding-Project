using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;

namespace Endless.Gameplay
{
	// Token: 0x02000178 RID: 376
	public class GoapAction
	{
		// Token: 0x0600087D RID: 2173 RVA: 0x0002820A File Offset: 0x0002640A
		private GoapAction(GoapAction.ActionKind action)
		{
			this.Action = action;
		}

		// Token: 0x17000199 RID: 409
		// (get) Token: 0x0600087E RID: 2174 RVA: 0x0002822F File Offset: 0x0002642F
		public GoapAction.ActionKind Action { get; }

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x0600087F RID: 2175 RVA: 0x00028238 File Offset: 0x00026438
		public string Name
		{
			get
			{
				return this.Action.ToString();
			}
		}

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000880 RID: 2176 RVA: 0x00028259 File Offset: 0x00026459
		public HashSet<GenericWorldState> Prerequisites { get; } = new HashSet<GenericWorldState>();

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06000881 RID: 2177 RVA: 0x00028261 File Offset: 0x00026461
		public HashSet<GenericWorldState> Effects { get; } = new HashSet<GenericWorldState>();

		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06000882 RID: 2178 RVA: 0x00028269 File Offset: 0x00026469
		public GoapAction.Status ActionStatus
		{
			get
			{
				return this.strategy.Status;
			}
		}

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06000883 RID: 2179 RVA: 0x00028276 File Offset: 0x00026476
		public float Cost
		{
			get
			{
				Func<float> getCost = this.strategy.GetCost;
				if (getCost == null)
				{
					return this.staticCost;
				}
				return getCost();
			}
		}

		// Token: 0x06000884 RID: 2180 RVA: 0x00028294 File Offset: 0x00026494
		public bool ArePrerequisitesMet(NpcEntity entity)
		{
			using (HashSet<GenericWorldState>.Enumerator enumerator = this.Prerequisites.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.Evaluate(entity))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000885 RID: 2181 RVA: 0x000282F0 File Offset: 0x000264F0
		public void Start()
		{
			this.strategy.Start();
		}

		// Token: 0x06000886 RID: 2182 RVA: 0x000282FD File Offset: 0x000264FD
		public void Stop()
		{
			this.strategy.Stop();
		}

		// Token: 0x06000887 RID: 2183 RVA: 0x0002830A File Offset: 0x0002650A
		public void Tick(uint frame)
		{
			this.strategy.Tick(frame);
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x00028318 File Offset: 0x00026518
		public void Update(float deltaTime)
		{
			this.strategy.Update(deltaTime);
		}

		// Token: 0x06000889 RID: 2185 RVA: 0x00028328 File Offset: 0x00026528
		public bool SatisfiesState(GenericWorldState worldState)
		{
			using (HashSet<GenericWorldState>.Enumerator enumerator = this.Effects.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Equals(worldState))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x04000704 RID: 1796
		private IActionStrategy strategy;

		// Token: 0x04000705 RID: 1797
		private float staticCost;

		// Token: 0x02000179 RID: 377
		public enum ActionKind
		{
			// Token: 0x0400070A RID: 1802
			Idle,
			// Token: 0x0400070B RID: 1803
			Fidget,
			// Token: 0x0400070C RID: 1804
			MoveToCommandDestination,
			// Token: 0x0400070D RID: 1805
			MoveToBehaviorDestination,
			// Token: 0x0400070E RID: 1806
			ReturnToNavGraph,
			// Token: 0x0400070F RID: 1807
			CombatIdle,
			// Token: 0x04000710 RID: 1808
			CombatFidget,
			// Token: 0x04000711 RID: 1809
			ApproachTarget,
			// Token: 0x04000712 RID: 1810
			Strafe,
			// Token: 0x04000713 RID: 1811
			Taunt,
			// Token: 0x04000714 RID: 1812
			WaitToAttack,
			// Token: 0x04000715 RID: 1813
			Backstep,
			// Token: 0x04000716 RID: 1814
			MeleeAttack,
			// Token: 0x04000717 RID: 1815
			MoveNearTarget,
			// Token: 0x04000718 RID: 1816
			FallBack,
			// Token: 0x04000719 RID: 1817
			GetInLos,
			// Token: 0x0400071A RID: 1818
			RangedAttack,
			// Token: 0x0400071B RID: 1819
			ZombieMeleeAttack,
			// Token: 0x0400071C RID: 1820
			FindTarget
		}

		// Token: 0x0200017A RID: 378
		public class Builder
		{
			// Token: 0x0600088A RID: 2186 RVA: 0x00028384 File Offset: 0x00026584
			public Builder(GoapAction.ActionKind actionKind)
			{
				this.action = new GoapAction(actionKind)
				{
					staticCost = 1f
				};
			}

			// Token: 0x0600088B RID: 2187 RVA: 0x000283A3 File Offset: 0x000265A3
			public GoapAction.Builder WithCost(float cost)
			{
				this.action.staticCost = cost;
				return this;
			}

			// Token: 0x0600088C RID: 2188 RVA: 0x000283B2 File Offset: 0x000265B2
			public GoapAction.Builder WithStrategy(IActionStrategy strategy)
			{
				this.action.strategy = strategy;
				return this;
			}

			// Token: 0x0600088D RID: 2189 RVA: 0x000283C1 File Offset: 0x000265C1
			public GoapAction.Builder AddPrerequisite(GenericWorldState prerequisite)
			{
				this.action.Prerequisites.Add(prerequisite);
				return this;
			}

			// Token: 0x0600088E RID: 2190 RVA: 0x000283D6 File Offset: 0x000265D6
			public GoapAction.Builder AddPrerequisite(WorldState worldState)
			{
				this.action.Prerequisites.Add(MonoBehaviourSingleton<WorldStateDictionary>.Instance[worldState]);
				return this;
			}

			// Token: 0x0600088F RID: 2191 RVA: 0x000283F5 File Offset: 0x000265F5
			public GoapAction.Builder AddEffect(GenericWorldState effect)
			{
				this.action.Effects.Add(effect);
				return this;
			}

			// Token: 0x06000890 RID: 2192 RVA: 0x0002840A File Offset: 0x0002660A
			public GoapAction.Builder AddEffect(WorldState worldState)
			{
				this.action.Effects.Add(MonoBehaviourSingleton<WorldStateDictionary>.Instance[worldState]);
				return this;
			}

			// Token: 0x06000891 RID: 2193 RVA: 0x00028429 File Offset: 0x00026629
			public GoapAction Build()
			{
				return this.action;
			}

			// Token: 0x0400071D RID: 1821
			private readonly GoapAction action;
		}

		// Token: 0x0200017B RID: 379
		public enum Status
		{
			// Token: 0x0400071F RID: 1823
			InProgress,
			// Token: 0x04000720 RID: 1824
			Complete,
			// Token: 0x04000721 RID: 1825
			Failed
		}
	}
}
