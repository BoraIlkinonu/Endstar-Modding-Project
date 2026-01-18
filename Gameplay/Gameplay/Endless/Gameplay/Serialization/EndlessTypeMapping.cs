using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Stats;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004BF RID: 1215
	public class EndlessTypeMapping : ScriptableObject
	{
		// Token: 0x170005E9 RID: 1513
		// (get) Token: 0x06001E48 RID: 7752 RVA: 0x000840A7 File Offset: 0x000822A7
		public static EndlessTypeMapping Instance
		{
			get
			{
				if (EndlessTypeMapping.instance == null)
				{
					EndlessTypeMapping.instance = Resources.Load<EndlessTypeMapping>("EndlessTypeMapping");
				}
				return EndlessTypeMapping.instance;
			}
		}

		// Token: 0x170005EA RID: 1514
		// (get) Token: 0x06001E49 RID: 7753 RVA: 0x000840CC File Offset: 0x000822CC
		public Dictionary<string, int> TypeNameLookup
		{
			get
			{
				if (this.typeNameLookup == null)
				{
					this.typeNameLookup = new Dictionary<string, int>();
					for (int i = 0; i < this.typeNames.Count; i++)
					{
						this.typeNameLookup.Add(this.typeNames[i], i);
					}
				}
				return this.typeNameLookup;
			}
		}

		// Token: 0x170005EB RID: 1515
		// (get) Token: 0x06001E4A RID: 7754 RVA: 0x00084120 File Offset: 0x00082320
		public IReadOnlyList<SemanticVersion> VersionUpgrades
		{
			get
			{
				return this.temporaryConverters.Keys.ToList<SemanticVersion>();
			}
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x00084132 File Offset: 0x00082332
		public int GetTypeId(string assemblyQualifiedTypeName)
		{
			return this.TypeNameLookup[assemblyQualifiedTypeName];
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x00084140 File Offset: 0x00082340
		public string GetAssemblyQualifiedTypeName(int index)
		{
			return this.typeNames[index];
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x0008414E File Offset: 0x0008234E
		public bool HasTypeId(string assemblyQualifiedTypeName)
		{
			return this.TypeNameLookup.ContainsKey(assemblyQualifiedTypeName);
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x0008415C File Offset: 0x0008235C
		public Type GetTypeFromId(int index)
		{
			return Type.GetType(this.GetAssemblyQualifiedTypeName(index));
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x0008416C File Offset: 0x0008236C
		public void Upgrade(MemberChange memberChange, SemanticVersion version, bool isLua)
		{
			Type typeFromId = this.GetTypeFromId(memberChange.DataType);
			EndlessTypeUpgrader endlessTypeUpgrader;
			if (this.temporaryConverters.ContainsKey(version) && this.temporaryConverters[version].TryGetValue(typeFromId, out endlessTypeUpgrader))
			{
				endlessTypeUpgrader.Upgrade(memberChange, isLua);
			}
		}

		// Token: 0x0400176B RID: 5995
		private static EndlessTypeMapping instance;

		// Token: 0x0400176C RID: 5996
		[Header("IMPORTANT. Do NOT remove from this list.")]
		[SerializeField]
		private List<string> typeNames = new List<string>();

		// Token: 0x0400176D RID: 5997
		private Dictionary<string, int> typeNameLookup;

		// Token: 0x0400176E RID: 5998
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

		// Token: 0x0400176F RID: 5999
		public readonly Type[] LuaInspectorTypes = new Type[]
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
	}
}
