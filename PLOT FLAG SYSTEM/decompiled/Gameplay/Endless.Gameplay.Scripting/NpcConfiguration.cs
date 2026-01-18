using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.Scripting;

public class NpcConfiguration
{
	public CharacterVisualsReference NpcVisuals = new CharacterVisualsReference
	{
		Id = "f6787b45-dfe3-4075-bb17-920d1d66b4e0"
	};

	public NpcClassCustomizationData NpcClass = NpcClassCustomizationDataUtility.GetDefaultClassCustomizationData(Endless.Gameplay.LuaEnums.NpcClass.Grunt);

	public int Health = 10;

	public int CombatMode = 2;

	public int DamageMode;

	public int PhysicsMode;

	public int MovementMode = 1;

	public int IdleBehavior = 1;

	public int Team = 1;

	public int PathfindingRange = 2;

	public int Group;

	public int SpawnAnimation;

	public NpcConfiguration()
	{
	}

	public NpcConfiguration(NpcEntity npcEntity)
	{
		NpcVisuals = npcEntity.CharacterVisuals;
		NpcClass = npcEntity.NpcClass;
		Health = npcEntity.WorldObject.GetUserComponent<HealthComponent>().MaxHealth;
		CombatMode = (int)npcEntity.BaseCombatMode;
		DamageMode = (int)npcEntity.BaseDamageMode;
		PhysicsMode = (int)npcEntity.BasePhysicsMode;
		MovementMode = (int)npcEntity.BaseMovementMode;
		IdleBehavior = (int)npcEntity.IdleBehavior;
		Team = (int)npcEntity.Team;
		PathfindingRange = (int)npcEntity.PathfindingRange;
		Group = (int)npcEntity.Group;
		SpawnAnimation = (int)npcEntity.SpawnAnimation;
	}

	public NpcConfiguration(NetworkableNpcConfig networkableNpcConfig)
	{
		NpcVisuals = networkableNpcConfig.NpcVisuals;
		NpcClass = NpcClassCustomizationDataUtility.DeserializeNpcClassCustomizationData(networkableNpcConfig.ClassEnum, networkableNpcConfig.serializedClassCustomizationData);
		Health = networkableNpcConfig.Health;
		CombatMode = networkableNpcConfig.CombatMode;
		DamageMode = networkableNpcConfig.DamageMode;
		PhysicsMode = networkableNpcConfig.PhysicsMode;
		MovementMode = networkableNpcConfig.MovementMode;
		IdleBehavior = networkableNpcConfig.IdleBehavior;
		Team = networkableNpcConfig.Team;
		PathfindingRange = networkableNpcConfig.PathfindingRange;
		Group = networkableNpcConfig.Group;
		SpawnAnimation = networkableNpcConfig.SpawnAnimation;
	}
}
