using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x0200049A RID: 1178
	public class NpcConfiguration
	{
		// Token: 0x06001CEE RID: 7406 RVA: 0x0007EA74 File Offset: 0x0007CC74
		public NpcConfiguration()
		{
		}

		// Token: 0x06001CEF RID: 7407 RVA: 0x0007EADC File Offset: 0x0007CCDC
		public NpcConfiguration(NpcEntity npcEntity)
		{
			this.NpcVisuals = npcEntity.CharacterVisuals;
			this.NpcClass = npcEntity.NpcClass;
			this.Health = npcEntity.WorldObject.GetUserComponent<HealthComponent>().MaxHealth;
			this.CombatMode = (int)npcEntity.BaseCombatMode;
			this.DamageMode = (int)npcEntity.BaseDamageMode;
			this.PhysicsMode = (int)npcEntity.BasePhysicsMode;
			this.MovementMode = (int)npcEntity.BaseMovementMode;
			this.IdleBehavior = (int)npcEntity.IdleBehavior;
			this.Team = (int)npcEntity.Team;
			this.PathfindingRange = (int)npcEntity.PathfindingRange;
			this.Group = (int)npcEntity.Group;
			this.SpawnAnimation = (int)npcEntity.SpawnAnimation;
		}

		// Token: 0x06001CF0 RID: 7408 RVA: 0x0007EBDC File Offset: 0x0007CDDC
		public NpcConfiguration(NetworkableNpcConfig networkableNpcConfig)
		{
			this.NpcVisuals = networkableNpcConfig.NpcVisuals;
			this.NpcClass = NpcClassCustomizationDataUtility.DeserializeNpcClassCustomizationData(networkableNpcConfig.ClassEnum, networkableNpcConfig.serializedClassCustomizationData);
			this.Health = networkableNpcConfig.Health;
			this.CombatMode = networkableNpcConfig.CombatMode;
			this.DamageMode = networkableNpcConfig.DamageMode;
			this.PhysicsMode = networkableNpcConfig.PhysicsMode;
			this.MovementMode = networkableNpcConfig.MovementMode;
			this.IdleBehavior = networkableNpcConfig.IdleBehavior;
			this.Team = networkableNpcConfig.Team;
			this.PathfindingRange = networkableNpcConfig.PathfindingRange;
			this.Group = networkableNpcConfig.Group;
			this.SpawnAnimation = networkableNpcConfig.SpawnAnimation;
		}

		// Token: 0x040016C3 RID: 5827
		public CharacterVisualsReference NpcVisuals = new CharacterVisualsReference
		{
			Id = "f6787b45-dfe3-4075-bb17-920d1d66b4e0"
		};

		// Token: 0x040016C4 RID: 5828
		public NpcClassCustomizationData NpcClass = NpcClassCustomizationDataUtility.GetDefaultClassCustomizationData(Endless.Gameplay.LuaEnums.NpcClass.Grunt);

		// Token: 0x040016C5 RID: 5829
		public int Health = 10;

		// Token: 0x040016C6 RID: 5830
		public int CombatMode = 2;

		// Token: 0x040016C7 RID: 5831
		public int DamageMode;

		// Token: 0x040016C8 RID: 5832
		public int PhysicsMode;

		// Token: 0x040016C9 RID: 5833
		public int MovementMode = 1;

		// Token: 0x040016CA RID: 5834
		public int IdleBehavior = 1;

		// Token: 0x040016CB RID: 5835
		public int Team = 1;

		// Token: 0x040016CC RID: 5836
		public int PathfindingRange = 2;

		// Token: 0x040016CD RID: 5837
		public int Group;

		// Token: 0x040016CE RID: 5838
		public int SpawnAnimation;
	}
}
