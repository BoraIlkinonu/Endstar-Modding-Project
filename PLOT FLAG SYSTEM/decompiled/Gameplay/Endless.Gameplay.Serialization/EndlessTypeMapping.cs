using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.Serialization;

public class EndlessTypeMapping : ScriptableObject
{
	private static EndlessTypeMapping instance;

	[Header("IMPORTANT. Do NOT remove from this list.")]
	[SerializeField]
	private List<string> typeNames = new List<string>();

	private Dictionary<string, int> typeNameLookup;

	private Dictionary<SemanticVersion, Dictionary<Type, EndlessTypeUpgrader>> temporaryConverters = new Dictionary<SemanticVersion, Dictionary<Type, EndlessTypeUpgrader>>
	{
		{
			new SemanticVersion(1, 0, 1),
			new Dictionary<Type, EndlessTypeUpgrader> { 
			{
				typeof(CharacterVisualsReference),
				new CharacterVisualsEndlessUpdater()
			} }
		},
		{
			new SemanticVersion(1, 0, 2),
			new Dictionary<Type, EndlessTypeUpgrader> { 
			{
				typeof(ResourcePickup.ResourceQuantity),
				new ResourcePickupAmountUpgrader()
			} }
		},
		{
			new SemanticVersion(1, 0, 3),
			new Dictionary<Type, EndlessTypeUpgrader> { 
			{
				typeof(NpcClass),
				new NpcClassUpgrader()
			} }
		},
		{
			new SemanticVersion(1, 0, 4),
			new Dictionary<Type, EndlessTypeUpgrader> { 
			{
				typeof(HealthLevel),
				new HealthLevelUpdater()
			} }
		},
		{
			new SemanticVersion(1, 0, 5),
			new Dictionary<Type, EndlessTypeUpgrader> { 
			{
				typeof(CellReference),
				new CellReferenceEndlessUpdater()
			} }
		},
		{
			new SemanticVersion(1, 0, 6),
			new Dictionary<Type, EndlessTypeUpgrader> { 
			{
				typeof(CellReference[]),
				new CellReferenceArrayEndlessUpdater()
			} }
		}
	};

	public readonly Type[] LuaInspectorTypes = new Type[43]
	{
		typeof(string),
		typeof(float),
		typeof(int),
		typeof(bool),
		typeof(LocalizedString),
		typeof(NpcInstanceReference),
		typeof(InventoryLibraryReference),
		typeof(LevelDestination),
		typeof(KeyLibraryReference),
		typeof(CombatMode),
		typeof(PhysicsMode),
		typeof(Team),
		typeof(IdleBehavior),
		typeof(PathfindingRange),
		typeof(NpcGroup),
		typeof(DamageMode),
		typeof(CellReference),
		typeof(CharacterVisualsReference),
		typeof(PropCombatMode),
		typeof(PropPhysicsMode),
		typeof(PropDamageMode),
		typeof(InstanceReference),
		typeof(PropMovementMode),
		typeof(MovementMode),
		typeof(NpcClassCustomizationData),
		typeof(NpcSpawnAnimation),
		typeof(AudioReference),
		typeof(TeleportType),
		typeof(PlayerReference),
		typeof(BasicStat),
		typeof(PerPlayerStat),
		typeof(ComparativeStat),
		typeof(Scope),
		typeof(CameraTransition),
		typeof(ResourceLibraryReference),
		typeof(Vector3),
		typeof(ContextTypes),
		typeof(InputSettings),
		typeof(ThreatLevel),
		typeof(Color),
		typeof(TradeInfo),
		typeof(TradeInfo.InventoryAndQuantityReference),
		typeof(ItemGrantBehavior)
	};

	public static EndlessTypeMapping Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load<EndlessTypeMapping>("EndlessTypeMapping");
			}
			return instance;
		}
	}

	public Dictionary<string, int> TypeNameLookup
	{
		get
		{
			if (typeNameLookup == null)
			{
				typeNameLookup = new Dictionary<string, int>();
				for (int i = 0; i < typeNames.Count; i++)
				{
					typeNameLookup.Add(typeNames[i], i);
				}
			}
			return typeNameLookup;
		}
	}

	public IReadOnlyList<SemanticVersion> VersionUpgrades => temporaryConverters.Keys.ToList();

	public int GetTypeId(string assemblyQualifiedTypeName)
	{
		return TypeNameLookup[assemblyQualifiedTypeName];
	}

	public string GetAssemblyQualifiedTypeName(int index)
	{
		return typeNames[index];
	}

	public bool HasTypeId(string assemblyQualifiedTypeName)
	{
		return TypeNameLookup.ContainsKey(assemblyQualifiedTypeName);
	}

	public Type GetTypeFromId(int index)
	{
		return Type.GetType(GetAssemblyQualifiedTypeName(index));
	}

	public void Upgrade(MemberChange memberChange, SemanticVersion version, bool isLua)
	{
		Type typeFromId = GetTypeFromId(memberChange.DataType);
		if (temporaryConverters.ContainsKey(version) && temporaryConverters[version].TryGetValue(typeFromId, out var value))
		{
			value.Upgrade(memberChange, isLua);
		}
	}
}
