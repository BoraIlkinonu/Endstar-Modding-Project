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

namespace Endless.Gameplay.Serialization;

public static class ScriptLoader
{
	private static Dictionary<SemanticVersion, Func<Script, Script>> globalUpgrades = new Dictionary<SemanticVersion, Func<Script, Script>> { 
	{
		new SemanticVersion(1, 0, 4),
		UpgradeInspectorOrgDataWithComponentIds
	} };

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

	private static IReadOnlyList<SemanticVersion> VersionUpgrades => (from version in temporaryConverters.Keys.Concat(globalUpgrades.Keys).Distinct()
		orderby version
		select version).ToList();

	public static Script[] LoadArrayFromJson(string json)
	{
		Script[] array = JsonConvert.DeserializeObject<Script[]>(json);
		Script[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			UpgradeScript(array2[i]);
		}
		return array;
	}

	public static Script LoadFromJson(string json)
	{
		Script? script = JsonConvert.DeserializeObject<Script>(json);
		UpgradeScript(script);
		return script;
	}

	private static void UpgradeScript(Script script)
	{
		SemanticVersion semanticVersion = SemanticVersion.Parse(script.InternalVersion);
		foreach (SemanticVersion versionUpgrade in VersionUpgrades)
		{
			if (versionUpgrade <= semanticVersion)
			{
				continue;
			}
			if (versionUpgrade > Script.INTERNAL_VERSION)
			{
				Debug.LogWarning($"Skipping upgrade to {versionUpgrade} as it is not deployed");
				break;
			}
			foreach (InspectorScriptValue inspectorValue in script.InspectorValues)
			{
				Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(inspectorValue.DataType);
				if (temporaryConverters.ContainsKey(versionUpgrade) && temporaryConverters[versionUpgrade].TryGetValue(typeFromId, out var value))
				{
					value.Upgrade(inspectorValue);
				}
			}
			if (globalUpgrades.ContainsKey(versionUpgrade))
			{
				script = globalUpgrades[versionUpgrade](script);
			}
			script.InternalVersion = versionUpgrade.ToString();
		}
		script.InternalVersion = Script.INTERNAL_VERSION.ToString();
	}

	private static Script UpgradeInspectorOrgDataWithComponentIds(Script script)
	{
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(script.BaseTypeId, out var componentDefinition) && componentDefinition.InspectableMembers.Count > 0)
		{
			int typeId = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (InspectorExposedVariable variable in componentDefinition.InspectableMembers)
			{
				InspectorOrganizationData inspectorOrganizationData = script.InspectorOrganizationData.FirstOrDefault((InspectorOrganizationData orgData) => orgData.DataType == EndlessTypeMapping.Instance.GetTypeId(variable.DataType) && orgData.MemberName == variable.MemberName);
				if (inspectorOrganizationData != null)
				{
					inspectorOrganizationData.ComponentId = typeId;
				}
			}
		}
		foreach (string componentId in script.ComponentIds)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentId, out var componentDefinition2) || componentDefinition2.InspectableMembers.Count <= 0)
			{
				continue;
			}
			int typeId2 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition2.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (InspectorExposedVariable variable2 in componentDefinition2.InspectableMembers)
			{
				InspectorOrganizationData inspectorOrganizationData2 = script.InspectorOrganizationData.FirstOrDefault((InspectorOrganizationData orgData) => orgData.DataType == EndlessTypeMapping.Instance.GetTypeId(variable2.DataType) && orgData.MemberName == variable2.MemberName);
				if (inspectorOrganizationData2 != null)
				{
					inspectorOrganizationData2.ComponentId = typeId2;
				}
			}
		}
		foreach (InspectorScriptValue variable3 in script.InspectorValues)
		{
			InspectorOrganizationData inspectorOrganizationData3 = script.InspectorOrganizationData.FirstOrDefault((InspectorOrganizationData orgData) => orgData.DataType == variable3.DataType && orgData.MemberName == variable3.Name);
			if (inspectorOrganizationData3 != null)
			{
				inspectorOrganizationData3.ComponentId = -1;
			}
		}
		return script;
	}
}
