using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001A2 RID: 418
	public abstract class AttributeModifierNode : InstructionNode, INpcAttributeModifier
	{
		// Token: 0x06000965 RID: 2405
		public abstract InstructionNode GetNode();

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x06000966 RID: 2406 RVA: 0x0002BA9B File Offset: 0x00029C9B
		public CombatMode CombatMode
		{
			get
			{
				return this.combatMode;
			}
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x06000967 RID: 2407 RVA: 0x0002BAA3 File Offset: 0x00029CA3
		public DamageMode DamageMode
		{
			get
			{
				return this.damageMode;
			}
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x06000968 RID: 2408 RVA: 0x0002BAAB File Offset: 0x00029CAB
		public PhysicsMode PhysicsMode
		{
			get
			{
				return this.physicsMode;
			}
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06000969 RID: 2409 RVA: 0x0002BAB3 File Offset: 0x00029CB3
		public NpcEnum.FallMode FallMode
		{
			get
			{
				return this.fallMode;
			}
		}

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x0600096A RID: 2410 RVA: 0x0002BABB File Offset: 0x00029CBB
		public MovementMode MovementMode
		{
			get
			{
				return this.movementMode;
			}
		}

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x0600096B RID: 2411
		public abstract NpcEnum.AttributeRank AttributeRank { get; }

		// Token: 0x040007AC RID: 1964
		[SerializeField]
		protected CombatMode combatMode;

		// Token: 0x040007AD RID: 1965
		[SerializeField]
		protected DamageMode damageMode;

		// Token: 0x040007AE RID: 1966
		[SerializeField]
		protected PhysicsMode physicsMode;

		// Token: 0x040007AF RID: 1967
		[SerializeField]
		protected NpcEnum.FallMode fallMode;

		// Token: 0x040007B0 RID: 1968
		[SerializeField]
		protected MovementMode movementMode;
	}
}
