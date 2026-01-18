using Endless.Gameplay.PlayerInventory;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class GameplayPlayerReferenceManager : PlayerReferenceManager
{
	[SerializeField]
	private PlayerEquipmentManager playerEquipmentManager;

	[SerializeField]
	private Inventory inventory;

	[SerializeField]
	private HealthComponent healthComponent;

	[SerializeField]
	private PlayerDownedComponent playerDownedComponent;

	[SerializeField]
	private WorldCollidable worldCollidable;

	[SerializeField]
	private GameObject losProbe;

	[SerializeField]
	private DamageReaction damageReaction;

	[SerializeField]
	private PlayerLuaComponent luaComponent;

	public override PlayerEquipmentManager PlayerEquipmentManager => playerEquipmentManager;

	public override Inventory Inventory => inventory;

	public override HealthComponent HealthComponent => healthComponent;

	public override PlayerDownedComponent PlayerDownedComponent => playerDownedComponent;

	public override WorldCollidable WorldCollidable => worldCollidable;

	public override Transform LosProbe => losProbe.transform;

	public override DamageReaction DamageReaction => damageReaction;

	public Context PlayerContext => LuaComponent.Context;

	public PlayerLuaComponent LuaComponent => luaComponent;

	protected override void OnValidate()
	{
		base.OnValidate();
		if (playerDownedComponent == null)
		{
			playerDownedComponent = GetComponent<PlayerDownedComponent>();
		}
		if (healthComponent == null)
		{
			healthComponent = GetComponent<HealthComponent>();
		}
		if (playerEquipmentManager == null)
		{
			playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
		}
		if (inventory == null)
		{
			inventory = GetComponent<Inventory>();
		}
		if (worldCollidable == null)
		{
			worldCollidable = GetComponent<WorldCollidable>();
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "GameplayPlayerReferenceManager";
	}
}
