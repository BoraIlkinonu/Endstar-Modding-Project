using System;
using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004B0 RID: 1200
	public class NetworkableNpcConfig : INetworkSerializable
	{
		// Token: 0x06001DBB RID: 7611 RVA: 0x000827E8 File Offset: 0x000809E8
		public NetworkableNpcConfig()
		{
			NpcConfiguration npcConfiguration = new NpcConfiguration();
			this.serializedClassCustomizationData = JsonConvert.SerializeObject(npcConfiguration.NpcClass);
			this.Health = npcConfiguration.Health;
			this.CombatMode = npcConfiguration.CombatMode;
			this.DamageMode = npcConfiguration.DamageMode;
			this.PhysicsMode = npcConfiguration.PhysicsMode;
			this.MovementMode = npcConfiguration.MovementMode;
			this.IdleBehavior = npcConfiguration.IdleBehavior;
			this.Team = npcConfiguration.Team;
			this.PathfindingRange = npcConfiguration.PathfindingRange;
			this.Group = npcConfiguration.Group;
			this.SpawnAnimation = npcConfiguration.SpawnAnimation;
		}

		// Token: 0x06001DBC RID: 7612 RVA: 0x000828A8 File Offset: 0x00080AA8
		public NetworkableNpcConfig(NpcConfiguration npcConfiguration)
		{
			this.serializedClassCustomizationData = JsonConvert.SerializeObject(npcConfiguration.NpcClass);
			this.ClassEnum = npcConfiguration.NpcClass.NpcClass;
			this.NpcVisuals = npcConfiguration.NpcVisuals;
			this.Health = npcConfiguration.Health;
			this.CombatMode = npcConfiguration.CombatMode;
			this.DamageMode = npcConfiguration.DamageMode;
			this.PhysicsMode = npcConfiguration.PhysicsMode;
			this.MovementMode = npcConfiguration.MovementMode;
			this.IdleBehavior = npcConfiguration.IdleBehavior;
			this.Team = npcConfiguration.Team;
			this.PathfindingRange = npcConfiguration.PathfindingRange;
			this.Group = npcConfiguration.Group;
			this.SpawnAnimation = npcConfiguration.SpawnAnimation;
		}

		// Token: 0x06001DBD RID: 7613 RVA: 0x0008297C File Offset: 0x00080B7C
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			if (serializer.IsWriter)
			{
				serializer.GetFastBufferWriter().WriteValueSafe(this.NpcVisuals.AssetId.ToString(), false);
			}
			else
			{
				string text;
				serializer.GetFastBufferReader().ReadValueSafe(out text, false);
				this.NpcVisuals = new CharacterVisualsReference
				{
					Id = text
				};
			}
			serializer.SerializeValue<NpcClass>(ref this.ClassEnum, default(FastBufferWriter.ForEnums));
			serializer.SerializeValue(ref this.serializedClassCustomizationData, false);
			serializer.SerializeValue<int>(ref this.Health, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.CombatMode, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.DamageMode, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.PhysicsMode, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.MovementMode, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.IdleBehavior, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.Team, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.PathfindingRange, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.Group, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.SpawnAnimation, default(FastBufferWriter.ForPrimitives));
		}

		// Token: 0x04001733 RID: 5939
		public CharacterVisualsReference NpcVisuals = new CharacterVisualsReference
		{
			Id = "f6787b45-dfe3-4075-bb17-920d1d66b4e0"
		};

		// Token: 0x04001734 RID: 5940
		public NpcClass ClassEnum;

		// Token: 0x04001735 RID: 5941
		public string serializedClassCustomizationData;

		// Token: 0x04001736 RID: 5942
		public int Health;

		// Token: 0x04001737 RID: 5943
		public int CombatMode;

		// Token: 0x04001738 RID: 5944
		public int DamageMode;

		// Token: 0x04001739 RID: 5945
		public int PhysicsMode;

		// Token: 0x0400173A RID: 5946
		public int MovementMode;

		// Token: 0x0400173B RID: 5947
		public int IdleBehavior;

		// Token: 0x0400173C RID: 5948
		public int Team;

		// Token: 0x0400173D RID: 5949
		public int PathfindingRange;

		// Token: 0x0400173E RID: 5950
		public int Group;

		// Token: 0x0400173F RID: 5951
		public int SpawnAnimation;
	}
}
