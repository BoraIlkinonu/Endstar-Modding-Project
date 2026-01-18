using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;

namespace Endless.Gameplay;

public static class ScriptUtilities
{
	public static void UpdateOrganizationData(this Script script)
	{
		script.UpdateInspectorOrganizationData();
		UpdateEventOrganizationData(script);
		UpdateReceiverOrganizationData(script);
	}

	private static void UpdateReceiverOrganizationData(Script script)
	{
		List<(string, int)> list = new List<(string, int)>();
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(script.BaseTypeId, out var componentDefinition) && componentDefinition.AvailableReceivers.Count > 0)
		{
			int typeId = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (EndlessEventInfo availableReceiver in componentDefinition.AvailableReceivers)
			{
				list.Add((availableReceiver.MemberName, typeId));
			}
		}
		foreach (string componentId in script.ComponentIds)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentId, out var componentDefinition2) || componentDefinition2.AvailableReceivers.Count <= 0)
			{
				continue;
			}
			int typeId2 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition2.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (EndlessEventInfo availableReceiver2 in componentDefinition2.AvailableReceivers)
			{
				list.Add((availableReceiver2.MemberName, typeId2));
			}
		}
		IEnumerable<(string, int)> second = script.Receivers.Select((EndlessEventInfo data) => (MemberName: data.MemberName, ComponentId: -1));
		List<(string, int)> list2 = list.Concat(second).ToList();
		for (int num = script.ReceiverOrganizationData.Count - 1; num >= 0; num--)
		{
			WireOrganizationData savedData = script.ReceiverOrganizationData[num];
			if (!list2.Any<(string, int)>(((string MemberName, int ComponentId) availableData) => availableData.MemberName == savedData.MemberName && savedData.ComponentId == availableData.ComponentId))
			{
				script.ReceiverOrganizationData.RemoveAt(num);
			}
		}
		foreach (var item in list2)
		{
			script.GetWireOrganizationReceiverData(item.Item1, item.Item2);
		}
	}

	private static void UpdateEventOrganizationData(Script script)
	{
		List<(string, int)> list = new List<(string, int)>();
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(script.BaseTypeId, out var componentDefinition) && componentDefinition.AvailableEvents.Count > 0)
		{
			int typeId = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (EndlessEventInfo availableEvent in componentDefinition.AvailableEvents)
			{
				list.Add((availableEvent.MemberName, typeId));
			}
		}
		foreach (string componentId in script.ComponentIds)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentId, out var componentDefinition2) || componentDefinition2.AvailableEvents.Count <= 0)
			{
				continue;
			}
			int typeId2 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition2.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (EndlessEventInfo availableEvent2 in componentDefinition2.AvailableEvents)
			{
				list.Add((availableEvent2.MemberName, typeId2));
			}
		}
		IEnumerable<(string, int)> second = script.Events.Select((EndlessEventInfo data) => (MemberName: data.MemberName, ComponentId: -1));
		List<(string, int)> list2 = list.Concat(second).ToList();
		for (int num = script.EventOrganizationData.Count - 1; num >= 0; num--)
		{
			WireOrganizationData savedData = script.EventOrganizationData[num];
			if (!list2.Any<(string, int)>(((string MemberName, int ComponentId) availableData) => availableData.MemberName == savedData.MemberName && savedData.ComponentId == availableData.ComponentId))
			{
				script.EventOrganizationData.RemoveAt(num);
			}
		}
		foreach (var item in list2)
		{
			script.GetWireOrganizationEventData(item.Item1, item.Item2);
		}
	}

	private static void UpdateInspectorOrganizationData(this Script script)
	{
		List<(int, string, int)> list = new List<(int, string, int)>();
		if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(script.BaseTypeId, out var componentDefinition) && componentDefinition.InspectableMembers.Count > 0)
		{
			int typeId = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (InspectorExposedVariable inspectableMember in componentDefinition.InspectableMembers)
			{
				list.Add((EndlessTypeMapping.Instance.GetTypeId(inspectableMember.DataType), inspectableMember.MemberName, typeId));
			}
		}
		foreach (string componentId in script.ComponentIds)
		{
			if (!MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(componentId, out var componentDefinition2) || componentDefinition2.InspectableMembers.Count <= 0)
			{
				continue;
			}
			int typeId2 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition2.ComponentBase.GetType().AssemblyQualifiedName);
			foreach (InspectorExposedVariable inspectableMember2 in componentDefinition2.InspectableMembers)
			{
				list.Add((EndlessTypeMapping.Instance.GetTypeId(inspectableMember2.DataType), inspectableMember2.MemberName, typeId2));
			}
		}
		IEnumerable<(int, string, int)> second = script.InspectorValues.Select((InspectorScriptValue data) => (DataType: data.DataType, MemberName: data.Name, ComponentId: -1));
		List<(int, string, int)> list2 = list.Concat(second).ToList();
		for (int num = script.InspectorOrganizationData.Count - 1; num >= 0; num--)
		{
			InspectorOrganizationData savedData = script.InspectorOrganizationData[num];
			if (!list2.Any<(int, string, int)>(((int DataType, string MemberName, int ComponentId) availableData) => availableData.DataType == savedData.DataType && availableData.MemberName == savedData.MemberName && savedData.ComponentId == availableData.ComponentId))
			{
				script.InspectorOrganizationData.RemoveAt(num);
			}
		}
		foreach (var item in list2)
		{
			script.GetInspectorOrganizationData(item.Item1, item.Item2, item.Item3);
		}
	}
}
