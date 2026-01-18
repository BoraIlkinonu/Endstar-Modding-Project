using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B3 RID: 1203
	public abstract class AttributeModifierNode : InstructionNode, INpcAttributeModifier
	{
		// Token: 0x06001DDC RID: 7644
		public abstract InstructionNode GetNode();

		// Token: 0x170005C4 RID: 1476
		// (get) Token: 0x06001DDD RID: 7645 RVA: 0x00083145 File Offset: 0x00081345
		public CombatMode CombatMode
		{
			get
			{
				return this.combatMode;
			}
		}

		// Token: 0x170005C5 RID: 1477
		// (get) Token: 0x06001DDE RID: 7646 RVA: 0x0008314D File Offset: 0x0008134D
		public DamageMode DamageMode
		{
			get
			{
				return this.damageMode;
			}
		}

		// Token: 0x170005C6 RID: 1478
		// (get) Token: 0x06001DDF RID: 7647 RVA: 0x00083155 File Offset: 0x00081355
		public PhysicsMode PhysicsMode
		{
			get
			{
				return this.physicsMode;
			}
		}

		// Token: 0x170005C7 RID: 1479
		// (get) Token: 0x06001DE0 RID: 7648 RVA: 0x0008315D File Offset: 0x0008135D
		public NpcEnum.FallMode FallMode
		{
			get
			{
				return this.fallMode;
			}
		}

		// Token: 0x170005C8 RID: 1480
		// (get) Token: 0x06001DE1 RID: 7649 RVA: 0x00083165 File Offset: 0x00081365
		public MovementMode MovementMode
		{
			get
			{
				return this.movementMode;
			}
		}

		// Token: 0x170005C9 RID: 1481
		// (get) Token: 0x06001DE2 RID: 7650
		public abstract NpcEnum.AttributeRank AttributeRank { get; }

		// Token: 0x0400174D RID: 5965
		[SerializeField]
		protected CombatMode combatMode;

		// Token: 0x0400174E RID: 5966
		[SerializeField]
		protected DamageMode damageMode;

		// Token: 0x0400174F RID: 5967
		[SerializeField]
		protected PhysicsMode physicsMode;

		// Token: 0x04001750 RID: 5968
		[SerializeField]
		protected NpcEnum.FallMode fallMode;

		// Token: 0x04001751 RID: 5969
		[SerializeField]
		protected MovementMode movementMode;
	}
}
