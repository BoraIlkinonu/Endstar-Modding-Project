using System;
using System.Collections.Generic;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000435 RID: 1077
	public abstract class FsmState
	{
		// Token: 0x1700056F RID: 1391
		// (get) Token: 0x06001AF5 RID: 6901 RVA: 0x0007AE1E File Offset: 0x0007901E
		protected NpcEntity Entity { get; }

		// Token: 0x17000570 RID: 1392
		// (get) Token: 0x06001AF6 RID: 6902 RVA: 0x0007AE26 File Offset: 0x00079026
		protected Components Components
		{
			get
			{
				return this.Entity.Components;
			}
		}

		// Token: 0x17000571 RID: 1393
		// (get) Token: 0x06001AF7 RID: 6903 RVA: 0x0007AE33 File Offset: 0x00079033
		// (set) Token: 0x06001AF8 RID: 6904 RVA: 0x0007AE3B File Offset: 0x0007903B
		private List<Transition> Transitions { get; set; } = new List<Transition>();

		// Token: 0x17000572 RID: 1394
		// (get) Token: 0x06001AF9 RID: 6905 RVA: 0x0007AE44 File Offset: 0x00079044
		public string StateName
		{
			get
			{
				return this.State.ToString();
			}
		}

		// Token: 0x17000573 RID: 1395
		// (get) Token: 0x06001AFA RID: 6906
		public abstract NpcEnum.FsmState State { get; }

		// Token: 0x06001AFB RID: 6907 RVA: 0x0007AE65 File Offset: 0x00079065
		protected FsmState(NpcEntity entity)
		{
			this.Entity = entity;
		}

		// Token: 0x06001AFC RID: 6908 RVA: 0x0007AE7F File Offset: 0x0007907F
		public void Initialize(List<Transition> transitions)
		{
			this.Transitions = transitions;
		}

		// Token: 0x06001AFD RID: 6909 RVA: 0x0007AE88 File Offset: 0x00079088
		public void Initialize(Transition transition)
		{
			this.Initialize(new List<Transition> { transition });
		}

		// Token: 0x06001AFE RID: 6910 RVA: 0x0007AE9C File Offset: 0x0007909C
		public virtual void Enter()
		{
			this.Entity.CurrentState = this;
			this.Entity.Components.IndividualStateUpdater.OnProcessTransitions += this.HandleOnProcessTransitions;
			this.Entity.Components.Proxy.OnUpdate += this.Update;
			this.Entity.Components.IndividualStateUpdater.OnWriteState += this.WriteState;
		}

		// Token: 0x06001AFF RID: 6911 RVA: 0x0007AF1C File Offset: 0x0007911C
		protected virtual void Exit()
		{
			this.Entity.Components.IndividualStateUpdater.OnProcessTransitions -= this.HandleOnProcessTransitions;
			this.Entity.Components.Proxy.OnUpdate -= this.Update;
			this.Entity.Components.IndividualStateUpdater.OnWriteState -= this.WriteState;
		}

		// Token: 0x06001B00 RID: 6912 RVA: 0x0007AF90 File Offset: 0x00079190
		private void HandleOnProcessTransitions()
		{
			int i = 0;
			while (i < this.Transitions.Count)
			{
				Transition transition = this.Transitions[i];
				if (transition.AreConditionsMet(this.Entity))
				{
					this.Exit();
					transition.TargetState.Enter();
					Action<NpcEntity> transitionAction = transition.TransitionAction;
					if (transitionAction == null)
					{
						return;
					}
					transitionAction(this.Entity);
					return;
				}
				else
				{
					i++;
				}
			}
		}

		// Token: 0x06001B01 RID: 6913 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void Update()
		{
		}

		// Token: 0x06001B02 RID: 6914 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void WriteState(ref NpcState npcState)
		{
		}
	}
}
