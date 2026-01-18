using Endless.Gameplay.LuaEnums;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.Scripting;

public class NetworkableNpcConfig : INetworkSerializable
{
	public CharacterVisualsReference NpcVisuals = new CharacterVisualsReference
	{
		Id = "f6787b45-dfe3-4075-bb17-920d1d66b4e0"
	};

	public NpcClass ClassEnum;

	public string serializedClassCustomizationData;

	public int Health;

	public int CombatMode;

	public int DamageMode;

	public int PhysicsMode;

	public int MovementMode;

	public int IdleBehavior;

	public int Team;

	public int PathfindingRange;

	public int Group;

	public int SpawnAnimation;

	public NetworkableNpcConfig()
	{
		NpcConfiguration npcConfiguration = new NpcConfiguration();
		serializedClassCustomizationData = JsonConvert.SerializeObject(npcConfiguration.NpcClass);
		Health = npcConfiguration.Health;
		CombatMode = npcConfiguration.CombatMode;
		DamageMode = npcConfiguration.DamageMode;
		PhysicsMode = npcConfiguration.PhysicsMode;
		MovementMode = npcConfiguration.MovementMode;
		IdleBehavior = npcConfiguration.IdleBehavior;
		Team = npcConfiguration.Team;
		PathfindingRange = npcConfiguration.PathfindingRange;
		Group = npcConfiguration.Group;
		SpawnAnimation = npcConfiguration.SpawnAnimation;
	}

	public NetworkableNpcConfig(NpcConfiguration npcConfiguration)
	{
		serializedClassCustomizationData = JsonConvert.SerializeObject(npcConfiguration.NpcClass);
		ClassEnum = npcConfiguration.NpcClass.NpcClass;
		NpcVisuals = npcConfiguration.NpcVisuals;
		Health = npcConfiguration.Health;
		CombatMode = npcConfiguration.CombatMode;
		DamageMode = npcConfiguration.DamageMode;
		PhysicsMode = npcConfiguration.PhysicsMode;
		MovementMode = npcConfiguration.MovementMode;
		IdleBehavior = npcConfiguration.IdleBehavior;
		Team = npcConfiguration.Team;
		PathfindingRange = npcConfiguration.PathfindingRange;
		Group = npcConfiguration.Group;
		SpawnAnimation = npcConfiguration.SpawnAnimation;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		if (serializer.IsWriter)
		{
			serializer.GetFastBufferWriter().WriteValueSafe(NpcVisuals.AssetId.ToString());
		}
		else
		{
			serializer.GetFastBufferReader().ReadValueSafe(out var s, oneByteChars: false);
			NpcVisuals = new CharacterVisualsReference
			{
				Id = s
			};
		}
		serializer.SerializeValue(ref ClassEnum, default(FastBufferWriter.ForEnums));
		serializer.SerializeValue(ref serializedClassCustomizationData);
		serializer.SerializeValue(ref Health, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref CombatMode, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref DamageMode, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref PhysicsMode, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref MovementMode, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref IdleBehavior, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref Team, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref PathfindingRange, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref Group, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref SpawnAnimation, default(FastBufferWriter.ForPrimitives));
	}
}
