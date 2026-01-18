using System;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001A7 RID: 423
	public abstract class InstructionNode : EndlessBehaviour, IInstructionNode
	{
		// Token: 0x170001CC RID: 460
		// (get) Token: 0x0600097E RID: 2430 RVA: 0x0002BD50 File Offset: 0x00029F50
		private Component EntitySourceObject
		{
			get
			{
				Component component;
				if ((component = this.entityObjectSource) == null)
				{
					component = (this.entityObjectSource = base.GetComponentInParent<INpcSource>() as Component);
				}
				return component;
			}
		}

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x0600097F RID: 2431 RVA: 0x0002BD7B File Offset: 0x00029F7B
		protected NpcEntity Entity
		{
			get
			{
				if (!this.providedEntity)
				{
					return this.entitySource.Npc;
				}
				return this.providedEntity;
			}
		}

		// Token: 0x06000980 RID: 2432 RVA: 0x0002BD9C File Offset: 0x00029F9C
		private void Awake()
		{
			this.entitySource = this.EntitySourceObject.GetComponent<INpcSource>();
		}

		// Token: 0x06000981 RID: 2433 RVA: 0x0002BDAF File Offset: 0x00029FAF
		public Context GetContext()
		{
			return this.entitySource.Npc.Context;
		}

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x06000982 RID: 2434
		public abstract string InstructionName { get; }

		// Token: 0x06000983 RID: 2435 RVA: 0x0002BDC1 File Offset: 0x00029FC1
		public virtual void GiveInstruction(Context context)
		{
			if (!context.IsNpc())
			{
				return;
			}
			this.providedEntity = context.WorldObject.GetUserComponent<NpcEntity>();
		}

		// Token: 0x06000984 RID: 2436 RVA: 0x0002BDDD File Offset: 0x00029FDD
		public virtual void RescindInstruction(Context context)
		{
			this.providedEntity = null;
		}

		// Token: 0x040007B7 RID: 1975
		private Component entityObjectSource;

		// Token: 0x040007B8 RID: 1976
		private INpcSource entitySource;

		// Token: 0x040007B9 RID: 1977
		private NpcEntity providedEntity;
	}
}
