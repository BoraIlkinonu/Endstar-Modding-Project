using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x0200035A RID: 858
	public class TargeterComponent : EndlessNetworkBehaviour, IAwakeSubscriber, IGameEndSubscriber, IComponentBase, IScriptInjector, IAwarenessComponent, IUpdateComponent
	{
		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x060015A2 RID: 5538 RVA: 0x00066C7C File Offset: 0x00064E7C
		// (set) Token: 0x060015A3 RID: 5539 RVA: 0x00066C84 File Offset: 0x00064E84
		public float MaxLookDistance { get; internal set; }

		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x060015A4 RID: 5540 RVA: 0x00066C8D File Offset: 0x00064E8D
		// (set) Token: 0x060015A5 RID: 5541 RVA: 0x00066C95 File Offset: 0x00064E95
		public float VerticalViewAngle { get; internal set; }

		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x060015A6 RID: 5542 RVA: 0x00066C9E File Offset: 0x00064E9E
		// (set) Token: 0x060015A7 RID: 5543 RVA: 0x00066CA6 File Offset: 0x00064EA6
		public float HorizontalViewAngle { get; internal set; }

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x060015A8 RID: 5544 RVA: 0x00066CAF File Offset: 0x00064EAF
		// (set) Token: 0x060015A9 RID: 5545 RVA: 0x00066CB7 File Offset: 0x00064EB7
		public float ProximitySenseDistance { get; internal set; }

		// Token: 0x14000029 RID: 41
		// (add) Token: 0x060015AA RID: 5546 RVA: 0x00066CC0 File Offset: 0x00064EC0
		// (remove) Token: 0x060015AB RID: 5547 RVA: 0x00066CF8 File Offset: 0x00064EF8
		public event Action<PerceptionDebuggingData> OnScoresUpdated;

		// Token: 0x1400002A RID: 42
		// (add) Token: 0x060015AC RID: 5548 RVA: 0x00066D30 File Offset: 0x00064F30
		// (remove) Token: 0x060015AD RID: 5549 RVA: 0x00066D68 File Offset: 0x00064F68
		public event Action<HittableComponent> OnTargetChanging;

		// Token: 0x1400002B RID: 43
		// (add) Token: 0x060015AE RID: 5550 RVA: 0x00066DA0 File Offset: 0x00064FA0
		// (remove) Token: 0x060015AF RID: 5551 RVA: 0x00066DD8 File Offset: 0x00064FD8
		public event Action<HittableComponent> OnTargetChanged;

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x060015B0 RID: 5552 RVA: 0x00066E0D File Offset: 0x0006500D
		public IReadOnlyCollection<HittableComponent> KnownHittables
		{
			get
			{
				return this.knownEntities.Keys;
			}
		}

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x060015B1 RID: 5553 RVA: 0x00066E1A File Offset: 0x0006501A
		private global::UnityEngine.Vector3 LookVector
		{
			get
			{
				return this.losProbe.transform.forward;
			}
		}

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x060015B2 RID: 5554 RVA: 0x00066E2C File Offset: 0x0006502C
		private global::UnityEngine.Vector3 LosPosition
		{
			get
			{
				return this.losProbe.transform.position;
			}
		}

		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x060015B3 RID: 5555 RVA: 0x00066E3E File Offset: 0x0006503E
		private Team Team
		{
			get
			{
				if (!this.teamComponent)
				{
					return Team.Friendly;
				}
				return this.teamComponent.Team;
			}
		}

		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x060015B4 RID: 5556 RVA: 0x00066E5A File Offset: 0x0006505A
		public global::UnityEngine.Vector3 Position
		{
			get
			{
				return this.WorldObject.transform.position;
			}
		}

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x060015B5 RID: 5557 RVA: 0x00066E6C File Offset: 0x0006506C
		// (set) Token: 0x060015B6 RID: 5558 RVA: 0x00066E74 File Offset: 0x00065074
		public Func<HittableComponent, float, float> TargetScoreModifier { private get; set; }

		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x060015B7 RID: 5559 RVA: 0x00066E7D File Offset: 0x0006507D
		public Transform LosProbe
		{
			get
			{
				return this.losProbe;
			}
		}

		// Token: 0x17000492 RID: 1170
		// (get) Token: 0x060015B8 RID: 5560 RVA: 0x00066E85 File Offset: 0x00065085
		// (set) Token: 0x060015B9 RID: 5561 RVA: 0x00066E90 File Offset: 0x00065090
		public HittableComponent Target
		{
			get
			{
				return this.target;
			}
			private set
			{
				if (value == this.target)
				{
					return;
				}
				if (this.target)
				{
					this.target.Targeters.Remove(this);
				}
				Action<HittableComponent> onTargetChanging = this.OnTargetChanging;
				if (onTargetChanging != null)
				{
					onTargetChanging(this.target);
				}
				object[] array;
				this.scriptComponent.TryExecuteFunction("OnTargetChanging", out array, new object[]
				{
					this.target ? this.target.WorldObject.Context : null,
					value ? value.WorldObject.Context : null
				});
				this.target = value;
				Action<HittableComponent> onTargetChanged = this.OnTargetChanged;
				if (onTargetChanged != null)
				{
					onTargetChanged(this.target);
				}
				if (this.target)
				{
					this.target.Targeters.Add(this);
				}
			}
		}

		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x060015BA RID: 5562 RVA: 0x00066F74 File Offset: 0x00065174
		// (set) Token: 0x060015BB RID: 5563 RVA: 0x00066F7C File Offset: 0x0006517C
		public HittableComponent LastTarget { get; private set; }

		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x060015BC RID: 5564 RVA: 0x00066F85 File Offset: 0x00065185
		// (set) Token: 0x060015BD RID: 5565 RVA: 0x00066F8D File Offset: 0x0006518D
		public float CombatWeight { get; set; } = 1f;

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x060015BE RID: 5566 RVA: 0x00066F96 File Offset: 0x00065196
		public IReadOnlyList<HittableComponent> CurrentTargets
		{
			get
			{
				return this.currentTargets;
			}
		}

		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x060015BF RID: 5567 RVA: 0x00066F9E File Offset: 0x0006519E
		// (set) Token: 0x060015C0 RID: 5568 RVA: 0x00066FA6 File Offset: 0x000651A6
		internal TargetSelectionMode TargetSelectionMode
		{
			get
			{
				return this.targetSelectionMode;
			}
			set
			{
				this.Target = null;
				this.targetSelectionMode = value;
			}
		}

		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x060015C1 RID: 5569 RVA: 0x00066FB6 File Offset: 0x000651B6
		// (set) Token: 0x060015C2 RID: 5570 RVA: 0x00066FBE File Offset: 0x000651BE
		internal TargetPrioritizationMode TargetPrioritizationMode
		{
			get
			{
				return this.targetPrioritizationMode;
			}
			set
			{
				this.Target = null;
				this.targetPrioritizationMode = value;
			}
		}

		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x060015C3 RID: 5571 RVA: 0x00066FCE File Offset: 0x000651CE
		// (set) Token: 0x060015C4 RID: 5572 RVA: 0x00066FD6 File Offset: 0x000651D6
		internal CurrentTargetHandlingMode CurrentTargetHandlingMode
		{
			get
			{
				return this.currentTargetHandlingMode;
			}
			set
			{
				this.Target = null;
				this.currentTargetHandlingMode = value;
			}
		}

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x060015C5 RID: 5573 RVA: 0x00066FE6 File Offset: 0x000651E6
		// (set) Token: 0x060015C6 RID: 5574 RVA: 0x00066FEE File Offset: 0x000651EE
		internal TargetHostilityMode TargetHostilityMode
		{
			get
			{
				return this.targetHostilityMode;
			}
			set
			{
				this.Target = null;
				this.targetHostilityMode = value;
			}
		}

		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x060015C7 RID: 5575 RVA: 0x00066FFE File Offset: 0x000651FE
		// (set) Token: 0x060015C8 RID: 5576 RVA: 0x00067006 File Offset: 0x00065206
		internal ZeroHealthTargetMode ZeroHealthTargetMode
		{
			get
			{
				return this.zeroHealthTargetMode;
			}
			set
			{
				this.Target = null;
				this.zeroHealthTargetMode = value;
			}
		}

		// Token: 0x060015C9 RID: 5577 RVA: 0x00067016 File Offset: 0x00065216
		[ContextMenu("Debug Targeting")]
		public void Debug()
		{
			MonoBehaviourSingleton<TargetingDebugger>.Instance.DebugTargeting(this);
		}

		// Token: 0x060015CA RID: 5578 RVA: 0x00067024 File Offset: 0x00065224
		private void OnPerceptionUpdated()
		{
			if (this.healthComponent != null && this.healthComponent.CurrentHealth <= 0)
			{
				return;
			}
			this.awaitingResponse = false;
			HashSet<HittableComponent> hashSet = new HashSet<HittableComponent>(this.knownEntities.Keys);
			foreach (PerceptionResult perceptionResult in this.perceptionResults)
			{
				if (!(perceptionResult.HittableComponent == null) && !(this.WorldObject == perceptionResult.HittableComponent.WorldObject))
				{
					float num = perceptionResult.Awareness * NetClock.FixedDeltaTime * 3f * (float)MonoBehaviourSingleton<TargeterManager>.Instance.TickOffsetRange;
					if (num > 0f)
					{
						hashSet.Remove(perceptionResult.HittableComponent);
					}
					if (!this.knownEntities.TryAdd(perceptionResult.HittableComponent, num))
					{
						float num2 = this.knownEntities[perceptionResult.HittableComponent];
						this.knownEntities[perceptionResult.HittableComponent] = Mathf.Min(num2 + num, 100f);
					}
				}
			}
			foreach (HittableComponent hittableComponent in hashSet)
			{
				float num3 = this.knownEntities[hittableComponent];
				this.knownEntities[hittableComponent] = Mathf.Max(num3 - NetClock.FixedDeltaTime * this.awarenessLossRate * (float)MonoBehaviourSingleton<TargeterManager>.Instance.TickOffsetRange, 0f);
			}
			this.mostRecentAwarenessScore = new Dictionary<HittableComponent, float>(this.knownEntities);
			this.UpdateTarget();
		}

		// Token: 0x060015CB RID: 5579 RVA: 0x000671D8 File Offset: 0x000653D8
		private bool IsValidTeamTarget(Team targetTeam)
		{
			bool flag;
			switch (this.targetSelectionMode)
			{
			case TargetSelectionMode.Allies:
				flag = this.Team == targetTeam;
				break;
			case TargetSelectionMode.Neutral:
				flag = targetTeam == Team.Neutral;
				break;
			case TargetSelectionMode.Enemies:
				flag = this.Team.IsHostileTo(targetTeam);
				break;
			case TargetSelectionMode.NonAllies:
				flag = this.Team != targetTeam;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return flag;
		}

		// Token: 0x060015CC RID: 5580 RVA: 0x0006723C File Offset: 0x0006543C
		private void UpdateTarget()
		{
			this.scorePairs.Clear();
			this.currentTargets.Clear();
			this.LastTarget = this.Target;
			float num;
			if (this.forcedTarget != null && this.knownEntities.TryGetValue(this.forcedTarget, out num) && num > 30f && this.IsValidTeamTarget(this.forcedTarget.Team) && (this.zeroHealthTargetMode == ZeroHealthTargetMode.Attend || this.forcedTarget.HasHealth))
			{
				this.Target = this.forcedTarget;
				this.currentTargets.Add(this.Target);
				return;
			}
			float num2;
			if (this.target && this.CurrentTargetHandlingMode == CurrentTargetHandlingMode.Preserve && this.knownEntities.TryGetValue(this.target, out num2) && num2 > 30f)
			{
				if (this.ZeroHealthTargetMode == ZeroHealthTargetMode.Attend)
				{
					return;
				}
				if (this.target.HasHealth)
				{
					return;
				}
			}
			foreach (KeyValuePair<HittableComponent, float> keyValuePair in this.knownEntities)
			{
				if (keyValuePair.Value >= 30f && keyValuePair.Key && keyValuePair.Key.transform && (!this.isNavigationDependent || MonoBehaviourSingleton<Pathfinding>.Instance.IsValidDestination(this.hittable.NavPosition, keyValuePair.Key.NavPosition, this.range, false)))
				{
					WorldObject worldObject = keyValuePair.Key.WorldObject;
					switch (this.TargetSelectionMode)
					{
					case TargetSelectionMode.Allies:
					{
						TeamComponent teamComponent;
						if (worldObject.TryGetUserComponent<TeamComponent>(out teamComponent) && this.Team == teamComponent.Team)
						{
							this.scorePairs.Add(new ValueTuple<float, HittableComponent>(0f, keyValuePair.Key));
						}
						break;
					}
					case TargetSelectionMode.Neutral:
					{
						TeamComponent teamComponent;
						if (worldObject.TryGetUserComponent<TeamComponent>(out teamComponent) && teamComponent.Team == Team.Neutral)
						{
							this.scorePairs.Add(new ValueTuple<float, HittableComponent>(0f, keyValuePair.Key));
						}
						break;
					}
					case TargetSelectionMode.Enemies:
						if (this.Team.IsHostileTo(keyValuePair.Key.Team))
						{
							this.scorePairs.Add(new ValueTuple<float, HittableComponent>(0f, keyValuePair.Key));
						}
						break;
					case TargetSelectionMode.NonAllies:
					{
						TeamComponent teamComponent;
						if (worldObject.TryGetUserComponent<TeamComponent>(out teamComponent) && this.Team != teamComponent.Team)
						{
							this.scorePairs.Add(new ValueTuple<float, HittableComponent>(0f, keyValuePair.Key));
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
			}
			if (this.TargetHostilityMode == TargetHostilityMode.Attend && this.hittable)
			{
				Dictionary<HittableComponent, float> recentAttackers = this.hittable.HostilityComponent.RecentAttackers;
				for (int i = 0; i < this.scorePairs.Count; i++)
				{
					ValueTuple<float, HittableComponent> valueTuple = this.scorePairs[i];
					float num3;
					if (recentAttackers.TryGetValue(valueTuple.Item2, out num3))
					{
						this.scorePairs[i] = new ValueTuple<float, HittableComponent>(Mathf.Max(num3, 0f), valueTuple.Item2);
					}
				}
			}
			switch (this.TargetPrioritizationMode)
			{
			case TargetPrioritizationMode.Default:
				this.AddClosenessScores();
				this.AddAngleScores();
				break;
			case TargetPrioritizationMode.Closest:
				this.AddClosenessScores();
				break;
			case TargetPrioritizationMode.SmallestAngle:
				this.AddAngleScores();
				break;
			case TargetPrioritizationMode.HighHealth:
				this.AddHighHealthScores();
				break;
			case TargetPrioritizationMode.LowHealth:
				this.AddLowHealthScores();
				break;
			case TargetPrioritizationMode.None:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (this.CurrentTargetHandlingMode == CurrentTargetHandlingMode.Prefer)
			{
				for (int j = 0; j < this.scorePairs.Count; j++)
				{
					ValueTuple<float, HittableComponent> valueTuple2 = this.scorePairs[j];
					if (!(valueTuple2.Item2 != this.target))
					{
						valueTuple2.Item1 += 25f;
						this.scorePairs[j] = valueTuple2;
					}
				}
			}
			if (this.ZeroHealthTargetMode == ZeroHealthTargetMode.Ignore)
			{
				for (int k = 0; k < this.scorePairs.Count; k++)
				{
					ValueTuple<float, HittableComponent> valueTuple3 = this.scorePairs[k];
					if (!valueTuple3.Item2.HasHealth)
					{
						valueTuple3.Item1 = 0f;
						this.scorePairs[k] = valueTuple3;
					}
				}
			}
			for (int l = 0; l < this.scorePairs.Count; l++)
			{
				ValueTuple<float, HittableComponent> valueTuple4 = this.scorePairs[l];
				float num4 = valueTuple4.Item1;
				float num5;
				switch (valueTuple4.Item2.ThreatLevel)
				{
				case ThreatLevel.Passive:
					num5 = num4 * 0.5f;
					break;
				case ThreatLevel.Low:
					num5 = num4 * 0.75f;
					break;
				case ThreatLevel.Medium:
					num5 = num4;
					break;
				case ThreatLevel.High:
					num5 = num4 * 1.5f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				num4 = num5;
				float num6 = valueTuple4.Item2.ModifyOwnTargetScore(this.WorldObject.Context, num4);
				if (this.TargetScoreModifier != null)
				{
					valueTuple4.Item1 = this.TargetScoreModifier(valueTuple4.Item2, num6);
				}
				this.scorePairs[l] = valueTuple4;
			}
			for (int m = 0; m < this.scorePairs.Count; m++)
			{
				ValueTuple<float, HittableComponent> valueTuple5 = this.scorePairs[m];
				object[] array;
				if (this.scriptComponent.TryExecuteFunction("ModifyTargetScore", out array, new object[]
				{
					valueTuple5.Item1,
					valueTuple5.Item2.WorldObject.Context
				}))
				{
					object obj = array[0];
					if (obj is double)
					{
						double num7 = (double)obj;
						valueTuple5.Item1 = (float)num7;
						this.scorePairs[m] = valueTuple5;
					}
				}
			}
			this.scorePairs.Sort(([TupleElementNames(new string[] { "score", "hittableComponent" })] ValueTuple<float, HittableComponent> a, [TupleElementNames(new string[] { "score", "hittableComponent" })] ValueTuple<float, HittableComponent> b) => -a.Item1.CompareTo(b.Item1));
			Action<PerceptionDebuggingData> onScoresUpdated = this.OnScoresUpdated;
			if (onScoresUpdated != null)
			{
				onScoresUpdated(new PerceptionDebuggingData(this.perceptionResults, this.mostRecentAwarenessScore, this.scorePairs));
			}
			if (this.scorePairs.Count > 0 && this.scorePairs[0].Item1 > 0f)
			{
				this.Target = this.scorePairs[0].Item2;
				float item = this.scorePairs[0].Item1;
				using (List<ValueTuple<float, HittableComponent>>.Enumerator enumerator2 = this.scorePairs.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						ValueTuple<float, HittableComponent> valueTuple6 = enumerator2.Current;
						if (valueTuple6.Item1 > item - this.secondaryTargetThreshold && this.currentTargets.Count <= 3)
						{
							this.currentTargets.Add(valueTuple6.Item2);
						}
					}
					goto IL_06D4;
				}
			}
			this.Target = null;
			IL_06D4:
			this.mostRecentAwarenessScore = null;
		}

		// Token: 0x060015CD RID: 5581 RVA: 0x00067958 File Offset: 0x00065B58
		private void AddClosenessScores()
		{
			for (int i = 0; i < this.scorePairs.Count; i++)
			{
				ValueTuple<float, HittableComponent> valueTuple = this.scorePairs[i];
				float num = global::UnityEngine.Vector3.Distance(this.WorldObject.transform.position, valueTuple.Item2.transform.position);
				float num2 = Mathf.Lerp(100f, 0f, math.square(num / this.MaxLookDistance));
				valueTuple.Item1 += num2;
				this.scorePairs[i] = valueTuple;
			}
		}

		// Token: 0x060015CE RID: 5582 RVA: 0x000679E4 File Offset: 0x00065BE4
		private void AddAngleScores()
		{
			for (int i = 0; i < this.scorePairs.Count; i++)
			{
				ValueTuple<float, HittableComponent> valueTuple = this.scorePairs[i];
				global::UnityEngine.Vector3 vector = valueTuple.Item2.transform.position - this.WorldObject.transform.position;
				float num = global::UnityEngine.Vector3.Angle(this.LookVector, vector);
				float num2 = Mathf.Lerp(100f, 0f, num / 180f);
				valueTuple.Item1 += num2;
				this.scorePairs[i] = valueTuple;
			}
		}

		// Token: 0x060015CF RID: 5583 RVA: 0x00067A7C File Offset: 0x00065C7C
		private void AddHighHealthScores()
		{
			for (int i = 0; i < this.scorePairs.Count; i++)
			{
				ValueTuple<float, HittableComponent> valueTuple = this.scorePairs[i];
				HealthComponent healthComponent;
				if (valueTuple.Item2.WorldObject.TryGetUserComponent<HealthComponent>(out healthComponent))
				{
					float num = Mathf.Lerp(0f, 100f, (float)healthComponent.CurrentHealth / (float)healthComponent.MaxHealth);
					valueTuple.Item1 += num;
					this.scorePairs[i] = valueTuple;
				}
			}
		}

		// Token: 0x060015D0 RID: 5584 RVA: 0x00067AF8 File Offset: 0x00065CF8
		private void AddLowHealthScores()
		{
			for (int i = 0; i < this.scorePairs.Count; i++)
			{
				ValueTuple<float, HittableComponent> valueTuple = this.scorePairs[i];
				HealthComponent healthComponent;
				if (valueTuple.Item2.WorldObject.TryGetUserComponent<HealthComponent>(out healthComponent))
				{
					float num = Mathf.Lerp(100f, 0f, (float)healthComponent.CurrentHealth / (float)healthComponent.MaxHealth);
					valueTuple.Item1 += num;
					this.scorePairs[i] = valueTuple;
				}
			}
		}

		// Token: 0x060015D1 RID: 5585 RVA: 0x00067B74 File Offset: 0x00065D74
		private void HandleOnDamaged(HittableComponent _, HealthModificationArgs healthModificationArgs)
		{
			Context source = healthModificationArgs.Source;
			HittableComponent hittableComponent;
			if (((source != null) ? source.WorldObject : null) == null || !source.WorldObject.TryGetUserComponent<HittableComponent>(out hittableComponent))
			{
				return;
			}
			if (!this.knownEntities.TryAdd(hittableComponent, 40f))
			{
				this.knownEntities[hittableComponent] = Mathf.Min(this.knownEntities[hittableComponent] + 40f, 100f);
			}
		}

		// Token: 0x060015D2 RID: 5586 RVA: 0x00067BE4 File Offset: 0x00065DE4
		public void AttemptSwitchTarget(Context targetContext)
		{
			if (targetContext == null)
			{
				return;
			}
			HittableComponent hittableComponent;
			if (targetContext.WorldObject.TryGetUserComponent<HittableComponent>(out hittableComponent))
			{
				this.forcedTarget = hittableComponent;
				if (this.removeForcedTargetRoutine != null)
				{
					base.StopCoroutine(this.removeForcedTargetRoutine);
				}
				this.removeForcedTargetRoutine = base.StartCoroutine(this.RemoveForcedTargetRoutine(1.5f));
			}
		}

		// Token: 0x060015D3 RID: 5587 RVA: 0x00067C36 File Offset: 0x00065E36
		private IEnumerator RemoveForcedTargetRoutine(float targetOverrideTime = 1.5f)
		{
			yield return new WaitForSeconds(targetOverrideTime);
			this.forcedTarget = null;
			this.removeForcedTargetRoutine = null;
			yield break;
		}

		// Token: 0x060015D4 RID: 5588 RVA: 0x00067C4C File Offset: 0x00065E4C
		internal void ForceOverrideTarget(HittableComponent newTarget)
		{
			if (!this.currentTargets.Contains(newTarget))
			{
				return;
			}
			this.Target = newTarget;
			this.forcedTarget = newTarget;
			if (this.removeForcedTargetRoutine != null)
			{
				base.StopCoroutine(this.removeForcedTargetRoutine);
			}
			this.removeForcedTargetRoutine = base.StartCoroutine(this.RemoveForcedTargetRoutine(1.5f));
		}

		// Token: 0x060015D5 RID: 5589 RVA: 0x00067CA1 File Offset: 0x00065EA1
		public override void OnDestroy()
		{
			base.OnDestroy();
			if (!base.IsServer)
			{
				return;
			}
			UnifiedStateUpdater.UnregisterUpdateComponent(this);
		}

		// Token: 0x060015D6 RID: 5590 RVA: 0x00067CB8 File Offset: 0x00065EB8
		public void EndlessAwake()
		{
			if (!base.IsServer)
			{
				return;
			}
			this.awarenessLossRate = 17.5f;
			TeamComponent teamComponent;
			this.WorldObject.TryGetUserComponent<TeamComponent>(out teamComponent);
			this.teamComponent = teamComponent;
			HittableComponent hittableComponent;
			if (this.WorldObject.TryGetUserComponent<HittableComponent>(out hittableComponent))
			{
				hittableComponent.OnDamaged += this.HandleOnDamaged;
				this.hittable = hittableComponent;
			}
			HealthComponent healthComponent;
			if (this.WorldObject.TryGetUserComponent<HealthComponent>(out healthComponent))
			{
				this.healthComponent = healthComponent;
				this.healthComponent.OnHealthZeroed_Internal.AddListener(new UnityAction(this.HandleOnHealthZeroed));
			}
			UnifiedStateUpdater.RegisterUpdateComponent(this);
			this.TargetSelectionMode = this.initialTargetSelectionMode;
			this.TargetPrioritizationMode = this.initialTargetPrioritizationMode;
			this.CurrentTargetHandlingMode = this.initialCurrentTargetHandlingMode;
			this.TargetHostilityMode = this.initialTargetHostilityMode;
			this.ZeroHealthTargetMode = this.initialZeroHealthTargetMode;
			MonoBehaviourSingleton<TargeterManager>.Instance.RegisterTargeter(this);
		}

		// Token: 0x060015D7 RID: 5591 RVA: 0x00067D95 File Offset: 0x00065F95
		private void HandleOnHealthZeroed()
		{
			UnifiedStateUpdater.UnregisterUpdateComponent(this);
			MonoBehaviourSingleton<TargeterManager>.Instance.UnregisterTargeter(this);
			this.Target = null;
		}

		// Token: 0x060015D8 RID: 5592 RVA: 0x00067DAF File Offset: 0x00065FAF
		public void EndlessGameEnd()
		{
			if (!base.IsServer)
			{
				return;
			}
			UnifiedStateUpdater.UnregisterUpdateComponent(this);
			MonoBehaviourSingleton<TargeterManager>.Instance.UnregisterTargeter(this);
		}

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x060015D9 RID: 5593 RVA: 0x00067DCB File Offset: 0x00065FCB
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(TargeterReferences);
			}
		}

		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x060015DA RID: 5594 RVA: 0x00067DD7 File Offset: 0x00065FD7
		// (set) Token: 0x060015DB RID: 5595 RVA: 0x00067DDF File Offset: 0x00065FDF
		public WorldObject WorldObject { get; private set; }

		// Token: 0x060015DC RID: 5596 RVA: 0x00067DE8 File Offset: 0x00065FE8
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			TargeterReferences targeterReferences = (TargeterReferences)referenceBase;
			this.losProbe = targeterReferences.LosProbe;
		}

		// Token: 0x060015DD RID: 5597 RVA: 0x00067E08 File Offset: 0x00066008
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x060015DE RID: 5598 RVA: 0x00067E14 File Offset: 0x00066014
		public object LuaObject
		{
			get
			{
				Targeter targeter;
				if ((targeter = this.luaInterface) == null)
				{
					targeter = (this.luaInterface = new Targeter(this));
				}
				return targeter;
			}
		}

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x060015DF RID: 5599 RVA: 0x00067E3A File Offset: 0x0006603A
		public Type LuaObjectType
		{
			get
			{
				return typeof(Targeter);
			}
		}

		// Token: 0x060015E0 RID: 5600 RVA: 0x00067E46 File Offset: 0x00066046
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x060015E1 RID: 5601 RVA: 0x00067E50 File Offset: 0x00066050
		public void UpdateAwareness()
		{
			if (this.awaitingResponse || !this.losProbe)
			{
				return;
			}
			if ((ulong)NetClock.CurrentFrame % (ulong)((long)MonoBehaviourSingleton<TargeterManager>.Instance.TickOffsetRange) != (ulong)((long)this.TickOffset))
			{
				return;
			}
			PerceptionRequest perceptionRequest = new PerceptionRequest
			{
				Position = this.LosPosition,
				LookVector = this.LookVector,
				MaxDistance = this.MaxLookDistance,
				VerticalValue = this.VerticalViewAngle,
				HorizontalValue = this.HorizontalViewAngle,
				ProximityDistance = this.ProximitySenseDistance,
				UseXray = this.useXRayLos,
				PerceptionResults = this.perceptionResults,
				PerceptionUpdatedCallback = new Action(this.OnPerceptionUpdated)
			};
			this.awaitingResponse = true;
			MonoBehaviourSingleton<PerceptionManager>.Instance.RequestPerception(perceptionRequest);
		}

		// Token: 0x060015E3 RID: 5603 RVA: 0x00067F7C File Offset: 0x0006617C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060015E4 RID: 5604 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060015E5 RID: 5605 RVA: 0x00067F92 File Offset: 0x00066192
		protected internal override string __getTypeName()
		{
			return "TargeterComponent";
		}

		// Token: 0x040011B0 RID: 4528
		private const float CURRENT_TARGET_SCORE_INCREASE = 25f;

		// Token: 0x040011B1 RID: 4529
		private const float MINIMUM_AWARENESS_THRESHOLD = 30f;

		// Token: 0x040011B2 RID: 4530
		private const float MAXIMUM_AWARENESS = 100f;

		// Token: 0x040011B3 RID: 4531
		private const float MAXIMUM_PARTIAL_SCORE = 100f;

		// Token: 0x040011B4 RID: 4532
		private const float AWARENESS_PER_HIT = 40f;

		// Token: 0x040011B5 RID: 4533
		private const float AWARENESS_GAIN_SCALAR = 3f;

		// Token: 0x040011B6 RID: 4534
		private const float DEFAULT_AWARENESS_LOSS_RATE = 17.5f;

		// Token: 0x040011BB RID: 4539
		[SerializeField]
		private TargetSelectionMode initialTargetSelectionMode;

		// Token: 0x040011BC RID: 4540
		[SerializeField]
		private TargetPrioritizationMode initialTargetPrioritizationMode;

		// Token: 0x040011BD RID: 4541
		[SerializeField]
		private CurrentTargetHandlingMode initialCurrentTargetHandlingMode;

		// Token: 0x040011BE RID: 4542
		[SerializeField]
		private TargetHostilityMode initialTargetHostilityMode;

		// Token: 0x040011BF RID: 4543
		[SerializeField]
		private ZeroHealthTargetMode initialZeroHealthTargetMode;

		// Token: 0x040011C0 RID: 4544
		[SerializeField]
		private float secondaryTargetThreshold;

		// Token: 0x040011C1 RID: 4545
		[SerializeField]
		[HideInInspector]
		private Transform losProbe;

		// Token: 0x040011C2 RID: 4546
		public int TickOffset;

		// Token: 0x040011C3 RID: 4547
		public bool isNavigationDependent;

		// Token: 0x040011C4 RID: 4548
		public PathfindingRange range = PathfindingRange.Global;

		// Token: 0x040011C5 RID: 4549
		public bool useXRayLos;

		// Token: 0x040011C6 RID: 4550
		public float awarenessLossRate;

		// Token: 0x040011C7 RID: 4551
		private HittableComponent forcedTarget;

		// Token: 0x040011C8 RID: 4552
		private HealthComponent healthComponent;

		// Token: 0x040011C9 RID: 4553
		private PerceptionDebuggingData perceptionDebuggingData;

		// Token: 0x040011CA RID: 4554
		private readonly List<PerceptionResult> perceptionResults = new List<PerceptionResult>();

		// Token: 0x040011CB RID: 4555
		private TeamComponent teamComponent;

		// Token: 0x040011CC RID: 4556
		[TupleElementNames(new string[] { "score", "hittableComponent" })]
		private readonly List<ValueTuple<float, HittableComponent>> scorePairs = new List<ValueTuple<float, HittableComponent>>();

		// Token: 0x040011CD RID: 4557
		private HittableComponent target;

		// Token: 0x040011CE RID: 4558
		private readonly List<HittableComponent> currentTargets = new List<HittableComponent>();

		// Token: 0x040011CF RID: 4559
		private HittableComponent hittable;

		// Token: 0x040011D0 RID: 4560
		private TargetSelectionMode targetSelectionMode;

		// Token: 0x040011D1 RID: 4561
		private TargetPrioritizationMode targetPrioritizationMode;

		// Token: 0x040011D2 RID: 4562
		private CurrentTargetHandlingMode currentTargetHandlingMode;

		// Token: 0x040011D3 RID: 4563
		private TargetHostilityMode targetHostilityMode;

		// Token: 0x040011D4 RID: 4564
		private ZeroHealthTargetMode zeroHealthTargetMode;

		// Token: 0x040011D5 RID: 4565
		private readonly Dictionary<HittableComponent, float> knownEntities = new Dictionary<HittableComponent, float>();

		// Token: 0x040011D6 RID: 4566
		private Dictionary<HittableComponent, float> mostRecentAwarenessScore;

		// Token: 0x040011D7 RID: 4567
		private bool awaitingResponse;

		// Token: 0x040011D8 RID: 4568
		private const float DEFAULT_FORCED_TARGET_TIME = 1.5f;

		// Token: 0x040011DF RID: 4575
		private Coroutine removeForcedTargetRoutine;

		// Token: 0x040011E1 RID: 4577
		[SerializeField]
		[HideInInspector]
		private EndlessScriptComponent scriptComponent;

		// Token: 0x040011E2 RID: 4578
		private Targeter luaInterface;
	}
}
