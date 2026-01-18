using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay;

public class DynamicAttributes
{
	private readonly NpcEntity npcEntity;

	private readonly List<IAttributeSourceController> attributeModifierControllers;

	private CombatMode combatMode;

	private DamageMode damageMode;

	private PhysicsMode physicsMode;

	private NpcEnum.FallMode fallMode;

	private MovementMode movementMode;

	public CombatMode CombatMode
	{
		get
		{
			return combatMode;
		}
		private set
		{
			if (combatMode != value)
			{
				combatMode = value;
				this.OnCombatModeChanged?.Invoke();
			}
		}
	}

	public DamageMode DamageMode
	{
		get
		{
			return damageMode;
		}
		private set
		{
			if (damageMode != value)
			{
				damageMode = value;
				this.OnDamageModeChanged?.Invoke();
			}
		}
	}

	public PhysicsMode PhysicsMode
	{
		get
		{
			return physicsMode;
		}
		private set
		{
			if (physicsMode != value)
			{
				physicsMode = value;
				this.OnPhysicsModeChanged?.Invoke();
			}
		}
	}

	public NpcEnum.FallMode FallMode
	{
		get
		{
			return fallMode;
		}
		private set
		{
			if (fallMode != value)
			{
				fallMode = value;
				this.OnFallModeChanged?.Invoke();
			}
		}
	}

	public MovementMode MovementMode
	{
		get
		{
			return movementMode;
		}
		private set
		{
			if (movementMode != value)
			{
				movementMode = value;
				this.OnMovementModeChanged?.Invoke();
			}
		}
	}

	public event Action OnCombatModeChanged;

	public event Action OnDamageModeChanged;

	public event Action OnPhysicsModeChanged;

	public event Action OnFallModeChanged;

	public event Action OnMovementModeChanged;

	public DynamicAttributes(NpcEntity entity, IEnumerable<IAttributeSourceController> attributeModifierControllers)
	{
		npcEntity = entity;
		this.attributeModifierControllers = new List<IAttributeSourceController>(attributeModifierControllers);
		Initialize();
	}

	private void Initialize()
	{
		npcEntity.OnBaseAttributeChanged += EvaluateNpcEntity;
		foreach (IAttributeSourceController attributeModifierController in attributeModifierControllers)
		{
			attributeModifierController.OnAttributeSourceChanged += EvaluateNpcEntity;
		}
		EvaluateNpcEntity();
	}

	private void EvaluateNpcEntity()
	{
		(CombatMode, NpcEnum.AttributeRank) tuple = (npcEntity.BaseCombatMode, NpcEnum.AttributeRank.Base);
		(DamageMode, NpcEnum.AttributeRank) tuple2 = (npcEntity.BaseDamageMode, NpcEnum.AttributeRank.Base);
		(PhysicsMode, NpcEnum.AttributeRank) tuple3 = (npcEntity.BasePhysicsMode, NpcEnum.AttributeRank.Base);
		(NpcEnum.FallMode, NpcEnum.AttributeRank) tuple4 = (npcEntity.BaseFallMode, NpcEnum.AttributeRank.Base);
		(MovementMode, NpcEnum.AttributeRank) tuple5 = (npcEntity.BaseMovementMode, NpcEnum.AttributeRank.Base);
		foreach (IAttributeSourceController attributeModifierController in attributeModifierControllers)
		{
			foreach (INpcAttributeModifier attributeModifier in attributeModifierController.GetAttributeModifiers())
			{
				if (attributeModifier != null)
				{
					if (attributeModifier.CombatMode != CombatMode.UseDefault && tuple.Item2 < attributeModifier.AttributeRank)
					{
						tuple = (attributeModifier.CombatMode, attributeModifier.AttributeRank);
					}
					if (attributeModifier.DamageMode != DamageMode.UseDefault && tuple2.Item2 < attributeModifier.AttributeRank)
					{
						tuple2 = (attributeModifier.DamageMode, attributeModifier.AttributeRank);
					}
					if (attributeModifier.PhysicsMode != PhysicsMode.UseDefault && tuple3.Item2 < attributeModifier.AttributeRank)
					{
						tuple3 = (attributeModifier.PhysicsMode, attributeModifier.AttributeRank);
					}
					if (attributeModifier.FallMode != NpcEnum.FallMode.UseDefault && tuple4.Item2 < attributeModifier.AttributeRank)
					{
						tuple4 = (attributeModifier.FallMode, attributeModifier.AttributeRank);
					}
					if (attributeModifier.MovementMode != MovementMode.UseDefault && tuple5.Item2 < attributeModifier.AttributeRank)
					{
						tuple5 = (attributeModifier.MovementMode, attributeModifier.AttributeRank);
					}
				}
			}
		}
		(CombatMode, _) = tuple;
		(DamageMode, _) = tuple2;
		(PhysicsMode, _) = tuple3;
		(FallMode, _) = tuple4;
		(MovementMode, _) = tuple5;
	}
}
