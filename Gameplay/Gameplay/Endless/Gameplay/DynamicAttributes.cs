using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay
{
	// Token: 0x02000159 RID: 345
	public class DynamicAttributes
	{
		// Token: 0x17000181 RID: 385
		// (get) Token: 0x0600080E RID: 2062 RVA: 0x00025FEA File Offset: 0x000241EA
		// (set) Token: 0x0600080F RID: 2063 RVA: 0x00025FF2 File Offset: 0x000241F2
		public CombatMode CombatMode
		{
			get
			{
				return this.combatMode;
			}
			private set
			{
				if (this.combatMode == value)
				{
					return;
				}
				this.combatMode = value;
				Action onCombatModeChanged = this.OnCombatModeChanged;
				if (onCombatModeChanged == null)
				{
					return;
				}
				onCombatModeChanged();
			}
		}

		// Token: 0x17000182 RID: 386
		// (get) Token: 0x06000810 RID: 2064 RVA: 0x00026015 File Offset: 0x00024215
		// (set) Token: 0x06000811 RID: 2065 RVA: 0x0002601D File Offset: 0x0002421D
		public DamageMode DamageMode
		{
			get
			{
				return this.damageMode;
			}
			private set
			{
				if (this.damageMode == value)
				{
					return;
				}
				this.damageMode = value;
				Action onDamageModeChanged = this.OnDamageModeChanged;
				if (onDamageModeChanged == null)
				{
					return;
				}
				onDamageModeChanged();
			}
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000812 RID: 2066 RVA: 0x00026040 File Offset: 0x00024240
		// (set) Token: 0x06000813 RID: 2067 RVA: 0x00026048 File Offset: 0x00024248
		public PhysicsMode PhysicsMode
		{
			get
			{
				return this.physicsMode;
			}
			private set
			{
				if (this.physicsMode == value)
				{
					return;
				}
				this.physicsMode = value;
				Action onPhysicsModeChanged = this.OnPhysicsModeChanged;
				if (onPhysicsModeChanged == null)
				{
					return;
				}
				onPhysicsModeChanged();
			}
		}

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000814 RID: 2068 RVA: 0x0002606B File Offset: 0x0002426B
		// (set) Token: 0x06000815 RID: 2069 RVA: 0x00026073 File Offset: 0x00024273
		public NpcEnum.FallMode FallMode
		{
			get
			{
				return this.fallMode;
			}
			private set
			{
				if (this.fallMode == value)
				{
					return;
				}
				this.fallMode = value;
				Action onFallModeChanged = this.OnFallModeChanged;
				if (onFallModeChanged == null)
				{
					return;
				}
				onFallModeChanged();
			}
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000816 RID: 2070 RVA: 0x00026096 File Offset: 0x00024296
		// (set) Token: 0x06000817 RID: 2071 RVA: 0x0002609E File Offset: 0x0002429E
		public MovementMode MovementMode
		{
			get
			{
				return this.movementMode;
			}
			private set
			{
				if (this.movementMode == value)
				{
					return;
				}
				this.movementMode = value;
				Action onMovementModeChanged = this.OnMovementModeChanged;
				if (onMovementModeChanged == null)
				{
					return;
				}
				onMovementModeChanged();
			}
		}

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x06000818 RID: 2072 RVA: 0x000260C4 File Offset: 0x000242C4
		// (remove) Token: 0x06000819 RID: 2073 RVA: 0x000260FC File Offset: 0x000242FC
		public event Action OnCombatModeChanged;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x0600081A RID: 2074 RVA: 0x00026134 File Offset: 0x00024334
		// (remove) Token: 0x0600081B RID: 2075 RVA: 0x0002616C File Offset: 0x0002436C
		public event Action OnDamageModeChanged;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x0600081C RID: 2076 RVA: 0x000261A4 File Offset: 0x000243A4
		// (remove) Token: 0x0600081D RID: 2077 RVA: 0x000261DC File Offset: 0x000243DC
		public event Action OnPhysicsModeChanged;

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x0600081E RID: 2078 RVA: 0x00026214 File Offset: 0x00024414
		// (remove) Token: 0x0600081F RID: 2079 RVA: 0x0002624C File Offset: 0x0002444C
		public event Action OnFallModeChanged;

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x06000820 RID: 2080 RVA: 0x00026284 File Offset: 0x00024484
		// (remove) Token: 0x06000821 RID: 2081 RVA: 0x000262BC File Offset: 0x000244BC
		public event Action OnMovementModeChanged;

		// Token: 0x06000822 RID: 2082 RVA: 0x000262F1 File Offset: 0x000244F1
		public DynamicAttributes(NpcEntity entity, IEnumerable<IAttributeSourceController> attributeModifierControllers)
		{
			this.npcEntity = entity;
			this.attributeModifierControllers = new List<IAttributeSourceController>(attributeModifierControllers);
			this.Initialize();
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x00026314 File Offset: 0x00024514
		private void Initialize()
		{
			this.npcEntity.OnBaseAttributeChanged += this.EvaluateNpcEntity;
			foreach (IAttributeSourceController attributeSourceController in this.attributeModifierControllers)
			{
				attributeSourceController.OnAttributeSourceChanged += this.EvaluateNpcEntity;
			}
			this.EvaluateNpcEntity();
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x00026390 File Offset: 0x00024590
		private void EvaluateNpcEntity()
		{
			ValueTuple<CombatMode, NpcEnum.AttributeRank> valueTuple = new ValueTuple<CombatMode, NpcEnum.AttributeRank>(this.npcEntity.BaseCombatMode, NpcEnum.AttributeRank.Base);
			ValueTuple<DamageMode, NpcEnum.AttributeRank> valueTuple2 = new ValueTuple<DamageMode, NpcEnum.AttributeRank>(this.npcEntity.BaseDamageMode, NpcEnum.AttributeRank.Base);
			ValueTuple<PhysicsMode, NpcEnum.AttributeRank> valueTuple3 = new ValueTuple<PhysicsMode, NpcEnum.AttributeRank>(this.npcEntity.BasePhysicsMode, NpcEnum.AttributeRank.Base);
			ValueTuple<NpcEnum.FallMode, NpcEnum.AttributeRank> valueTuple4 = new ValueTuple<NpcEnum.FallMode, NpcEnum.AttributeRank>(this.npcEntity.BaseFallMode, NpcEnum.AttributeRank.Base);
			ValueTuple<MovementMode, NpcEnum.AttributeRank> valueTuple5 = new ValueTuple<MovementMode, NpcEnum.AttributeRank>(this.npcEntity.BaseMovementMode, NpcEnum.AttributeRank.Base);
			foreach (IAttributeSourceController attributeSourceController in this.attributeModifierControllers)
			{
				foreach (INpcAttributeModifier npcAttributeModifier in attributeSourceController.GetAttributeModifiers())
				{
					if (npcAttributeModifier != null)
					{
						if (npcAttributeModifier.CombatMode != CombatMode.UseDefault && valueTuple.Item2 < npcAttributeModifier.AttributeRank)
						{
							valueTuple = new ValueTuple<CombatMode, NpcEnum.AttributeRank>(npcAttributeModifier.CombatMode, npcAttributeModifier.AttributeRank);
						}
						if (npcAttributeModifier.DamageMode != DamageMode.UseDefault && valueTuple2.Item2 < npcAttributeModifier.AttributeRank)
						{
							valueTuple2 = new ValueTuple<DamageMode, NpcEnum.AttributeRank>(npcAttributeModifier.DamageMode, npcAttributeModifier.AttributeRank);
						}
						if (npcAttributeModifier.PhysicsMode != PhysicsMode.UseDefault && valueTuple3.Item2 < npcAttributeModifier.AttributeRank)
						{
							valueTuple3 = new ValueTuple<PhysicsMode, NpcEnum.AttributeRank>(npcAttributeModifier.PhysicsMode, npcAttributeModifier.AttributeRank);
						}
						if (npcAttributeModifier.FallMode != NpcEnum.FallMode.UseDefault && valueTuple4.Item2 < npcAttributeModifier.AttributeRank)
						{
							valueTuple4 = new ValueTuple<NpcEnum.FallMode, NpcEnum.AttributeRank>(npcAttributeModifier.FallMode, npcAttributeModifier.AttributeRank);
						}
						if (npcAttributeModifier.MovementMode != MovementMode.UseDefault && valueTuple5.Item2 < npcAttributeModifier.AttributeRank)
						{
							valueTuple5 = new ValueTuple<MovementMode, NpcEnum.AttributeRank>(npcAttributeModifier.MovementMode, npcAttributeModifier.AttributeRank);
						}
					}
				}
			}
			this.CombatMode = valueTuple.Item1;
			this.DamageMode = valueTuple2.Item1;
			this.PhysicsMode = valueTuple3.Item1;
			this.FallMode = valueTuple4.Item1;
			this.MovementMode = valueTuple5.Item1;
		}

		// Token: 0x04000673 RID: 1651
		private readonly NpcEntity npcEntity;

		// Token: 0x04000674 RID: 1652
		private readonly List<IAttributeSourceController> attributeModifierControllers;

		// Token: 0x04000675 RID: 1653
		private CombatMode combatMode;

		// Token: 0x04000676 RID: 1654
		private DamageMode damageMode;

		// Token: 0x04000677 RID: 1655
		private PhysicsMode physicsMode;

		// Token: 0x04000678 RID: 1656
		private NpcEnum.FallMode fallMode;

		// Token: 0x04000679 RID: 1657
		private MovementMode movementMode;
	}
}
