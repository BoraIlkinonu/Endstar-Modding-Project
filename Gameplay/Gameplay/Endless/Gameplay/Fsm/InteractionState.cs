using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000437 RID: 1079
	public class InteractionState : FsmState
	{
		// Token: 0x17000575 RID: 1397
		// (get) Token: 0x06001B09 RID: 6921 RVA: 0x0007B0C5 File Offset: 0x000792C5
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Interaction;
			}
		}

		// Token: 0x06001B0A RID: 6922 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public InteractionState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x06001B0B RID: 6923 RVA: 0x0007B0C9 File Offset: 0x000792C9
		public override void Enter()
		{
			base.Enter();
			List<InteractorBase> activeInteractors = base.Entity.Components.Interactable.ActiveInteractors;
			this.currentInteractor = activeInteractors[activeInteractors.Count - 1];
		}

		// Token: 0x06001B0C RID: 6924 RVA: 0x0007B0FC File Offset: 0x000792FC
		protected override void Update()
		{
			base.Update();
			Vector3 vector = this.currentInteractor.transform.position - base.Entity.Position;
			vector = new Vector3(vector.x, 0f, vector.z);
			Quaternion quaternion = Quaternion.LookRotation(vector);
			base.Entity.transform.rotation = Quaternion.RotateTowards(base.Entity.transform.rotation, quaternion, base.Entity.Settings.RotationSpeed * Time.deltaTime);
		}

		// Token: 0x0400158A RID: 5514
		private InteractorBase currentInteractor;
	}
}
