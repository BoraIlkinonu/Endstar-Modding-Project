using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004CB RID: 1227
	public static class ScriptLoader
	{
		// Token: 0x170005F0 RID: 1520
		// (get) Token: 0x06001E74 RID: 7796 RVA: 0x00084B78 File Offset: 0x00082D78
		private static IReadOnlyList<SemanticVersion> VersionUpgrades
		{
			get
			{
				return (from version in ScriptLoader.temporaryConverters.Keys.Concat(ScriptLoader.globalUpgrades.Keys).Distinct<SemanticVersion>()
					orderby version
					select version).ToList<SemanticVersion>();
			}
		}

		// Token: 0x06001E75 RID: 7797 RVA: 0x00084BCC File Offset: 0x00082DCC
		public static Script[] LoadArrayFromJson(string json)
		{
			Script[] array = JsonConvert.DeserializeObject<Script[]>(json);
			Script[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ScriptLoader.UpgradeScript(array2[i]);
			}
			return array;
		}

		// Token: 0x06001E76 RID: 7798 RVA: 0x00084BF9 File Offset: 0x00082DF9
		public static Script LoadFromJson(string json)
		{
			Script script = JsonConvert.DeserializeObject<Script>(json);
			ScriptLoader.UpgradeScript(script);
			return script;
		}

		// Token: 0x06001E77 RID: 7799 RVA: 0x00084C08 File Offset: 0x00082E08
		private static void UpgradeScript(Script script)
		{
			SemanticVersion semanticVersion = SemanticVersion.Parse(script.InternalVersion);
			foreach (SemanticVersion semanticVersion2 in ScriptLoader.VersionUpgrades)
			{
				if (!(semanticVersion2 <= semanticVersion))
				{
					if (semanticVersion2 > Script.INTERNAL_VERSION)
					{
						Debug.LogWarning(string.Format("Skipping upgrade to {0} as it is not deployed", semanticVersion2));
						break;
					}
					foreach (InspectorScriptValue inspectorScriptValue in script.InspectorValues)
					{
						Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(inspectorScriptValue.DataType);
						EndlessTypeUpgrader endlessTypeUpgrader;
						if (ScriptLoader.temporaryConverters.ContainsKey(semanticVersion2) && ScriptLoader.temporaryConverters[semanticVersion2].TryGetValue(typeFromId, out endlessTypeUpgrader))
						{
							endlessTypeUpgrader.Upgrade(inspectorScriptValue);
						}
					}
					if (ScriptLoader.globalUpgrades.ContainsKey(semanticVersion2))
					{
						script = ScriptLoader.globalUpgrades[semanticVersion2](script);
					}
					script.InternalVersion = semanticVersion2.ToString();
				}
			}
			script.InternalVersion = Script.INTERNAL_VERSION.ToString();
		}

		// Token: 0x06001E78 RID: 7800 RVA: 0x00084D44 File Offset: 0x00082F44
		private static Script UpgradeInspectorOrgDataWithComponentIds(Script script)
		{
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(script.BaseTypeId, out baseTypeDefinition) && baseTypeDefinition.InspectableMembers.Count > 0)
			{
				int typeId = EndlessTypeMapping.Instance.GetTypeId(baseTypeDefinition.ComponentBase.GetType().AssemblyQualifiedName);
				using (List<InspectorExposedVariable>.Enumerator enumerator = baseTypeDefinition.InspectableMembers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						InspectorExposedVariable variable2 = enumerator.Current;
						InspectorOrganizationData inspectorOrganizationData = script.InspectorOrganizationData.FirstOrDefault((InspectorOrganizationData orgData) => orgData.DataType == EndlessTypeMapping.Instance.GetTypeId(variable2.DataType) && orgData.MemberName == variable2.MemberName);
						if (inspectorOrganizationData != null)
						{
							inspectorOrganizationData.ComponentId = typeId;
						}
					}
				}
			}
			foreach (string text in script.ComponentIds)
			{
				ComponentDefinition componentDefinition;
				if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(text, out componentDefinition) && componentDefinition.InspectableMembers.Count > 0)
				{
					int typeId2 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
					using (List<InspectorExposedVariable>.Enumerator enumerator = componentDefinition.InspectableMembers.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							InspectorExposedVariable variable3 = enumerator.Current;
							InspectorOrganizationData inspectorOrganizationData2 = script.InspectorOrganizationData.FirstOrDefault((InspectorOrganizationData orgData) => orgData.DataType == EndlessTypeMapping.Instance.GetTypeId(variable3.DataType) && orgData.MemberName == variable3.MemberName);
							if (inspectorOrganizationData2 != null)
							{
								inspectorOrganizationData2.ComponentId = typeId2;
							}
						}
					}
				}
			}
			using (List<InspectorScriptValue>.Enumerator enumerator3 = script.InspectorValues.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					InspectorScriptValue variable = enumerator3.Current;
					InspectorOrganizationData inspectorOrganizationData3 = script.InspectorOrganizationData.FirstOrDefault((InspectorOrganizationData orgData) => orgData.DataType == variable.DataType && orgData.MemberName == variable.Name);
					if (inspectorOrganizationData3 != null)
					{
						inspectorOrganizationData3.ComponentId = -1;
					}
				}
			}
			return script;
		}

		// Token: 0x04001770 RID: 6000
		private static Dictionary<SemanticVersion, Func<Script, Script>> globalUpgrades = new Dictionary<SemanticVersion, Func<Script, Script>> { 
		{
			new SemanticVersion(1, 0, 4),
			new Func<Script, Script>(ScriptLoader.UpgradeInspectorOrgDataWithComponentIds)
		} };

		// Token: 0x04001771 RID: 6001
		private static Dictionary<SemanticVersion, Dictionary<Type, EndlessTypeUpgrader>> temporaryConverters = new Dictionary<SemanticVersion, Dictionary<Type, EndlessTypeUpgrader>>
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
			}
		};
	}
}
