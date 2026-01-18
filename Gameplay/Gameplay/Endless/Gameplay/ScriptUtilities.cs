using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Serialization;
using Endless.Props.Assets;
using Endless.Props.Scripting;
using Endless.Shared;

namespace Endless.Gameplay
{
	// Token: 0x0200036E RID: 878
	public static class ScriptUtilities
	{
		// Token: 0x06001691 RID: 5777 RVA: 0x0006A00A File Offset: 0x0006820A
		public static void UpdateOrganizationData(this Script script)
		{
			script.UpdateInspectorOrganizationData();
			ScriptUtilities.UpdateEventOrganizationData(script);
			ScriptUtilities.UpdateReceiverOrganizationData(script);
		}

		// Token: 0x06001692 RID: 5778 RVA: 0x0006A020 File Offset: 0x00068220
		private static void UpdateReceiverOrganizationData(Script script)
		{
			List<ValueTuple<string, int>> list = new List<ValueTuple<string, int>>();
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(script.BaseTypeId, out baseTypeDefinition) && baseTypeDefinition.AvailableReceivers.Count > 0)
			{
				int typeId = EndlessTypeMapping.Instance.GetTypeId(baseTypeDefinition.ComponentBase.GetType().AssemblyQualifiedName);
				foreach (EndlessEventInfo endlessEventInfo in baseTypeDefinition.AvailableReceivers)
				{
					list.Add(new ValueTuple<string, int>(endlessEventInfo.MemberName, typeId));
				}
			}
			foreach (string text in script.ComponentIds)
			{
				ComponentDefinition componentDefinition;
				if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(text, out componentDefinition) && componentDefinition.AvailableReceivers.Count > 0)
				{
					int typeId2 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
					foreach (EndlessEventInfo endlessEventInfo2 in componentDefinition.AvailableReceivers)
					{
						list.Add(new ValueTuple<string, int>(endlessEventInfo2.MemberName, typeId2));
					}
				}
			}
			IEnumerable<ValueTuple<string, int>> enumerable = script.Receivers.Select((EndlessEventInfo data) => new ValueTuple<string, int>(data.MemberName, -1));
			List<ValueTuple<string, int>> list2 = list.Concat(enumerable).ToList<ValueTuple<string, int>>();
			for (int i = script.ReceiverOrganizationData.Count - 1; i >= 0; i--)
			{
				WireOrganizationData savedData = script.ReceiverOrganizationData[i];
				if (!list2.Any(([TupleElementNames(new string[] { "MemberName", "ComponentId" })] ValueTuple<string, int> availableData) => availableData.Item1 == savedData.MemberName && savedData.ComponentId == availableData.Item2))
				{
					script.ReceiverOrganizationData.RemoveAt(i);
				}
			}
			foreach (ValueTuple<string, int> valueTuple in list2)
			{
				script.GetWireOrganizationReceiverData(valueTuple.Item1, valueTuple.Item2);
			}
		}

		// Token: 0x06001693 RID: 5779 RVA: 0x0006A288 File Offset: 0x00068488
		private static void UpdateEventOrganizationData(Script script)
		{
			List<ValueTuple<string, int>> list = new List<ValueTuple<string, int>>();
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(script.BaseTypeId, out baseTypeDefinition) && baseTypeDefinition.AvailableEvents.Count > 0)
			{
				int typeId = EndlessTypeMapping.Instance.GetTypeId(baseTypeDefinition.ComponentBase.GetType().AssemblyQualifiedName);
				foreach (EndlessEventInfo endlessEventInfo in baseTypeDefinition.AvailableEvents)
				{
					list.Add(new ValueTuple<string, int>(endlessEventInfo.MemberName, typeId));
				}
			}
			foreach (string text in script.ComponentIds)
			{
				ComponentDefinition componentDefinition;
				if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(text, out componentDefinition) && componentDefinition.AvailableEvents.Count > 0)
				{
					int typeId2 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
					foreach (EndlessEventInfo endlessEventInfo2 in componentDefinition.AvailableEvents)
					{
						list.Add(new ValueTuple<string, int>(endlessEventInfo2.MemberName, typeId2));
					}
				}
			}
			IEnumerable<ValueTuple<string, int>> enumerable = script.Events.Select((EndlessEventInfo data) => new ValueTuple<string, int>(data.MemberName, -1));
			List<ValueTuple<string, int>> list2 = list.Concat(enumerable).ToList<ValueTuple<string, int>>();
			for (int i = script.EventOrganizationData.Count - 1; i >= 0; i--)
			{
				WireOrganizationData savedData = script.EventOrganizationData[i];
				if (!list2.Any(([TupleElementNames(new string[] { "MemberName", "ComponentId" })] ValueTuple<string, int> availableData) => availableData.Item1 == savedData.MemberName && savedData.ComponentId == availableData.Item2))
				{
					script.EventOrganizationData.RemoveAt(i);
				}
			}
			foreach (ValueTuple<string, int> valueTuple in list2)
			{
				script.GetWireOrganizationEventData(valueTuple.Item1, valueTuple.Item2);
			}
		}

		// Token: 0x06001694 RID: 5780 RVA: 0x0006A4F0 File Offset: 0x000686F0
		private static void UpdateInspectorOrganizationData(this Script script)
		{
			List<ValueTuple<int, string, int>> list = new List<ValueTuple<int, string, int>>();
			BaseTypeDefinition baseTypeDefinition;
			if (MonoBehaviourSingleton<StageManager>.Instance.BaseTypeList.TryGetDefinition(script.BaseTypeId, out baseTypeDefinition) && baseTypeDefinition.InspectableMembers.Count > 0)
			{
				int typeId = EndlessTypeMapping.Instance.GetTypeId(baseTypeDefinition.ComponentBase.GetType().AssemblyQualifiedName);
				foreach (InspectorExposedVariable inspectorExposedVariable in baseTypeDefinition.InspectableMembers)
				{
					list.Add(new ValueTuple<int, string, int>(EndlessTypeMapping.Instance.GetTypeId(inspectorExposedVariable.DataType), inspectorExposedVariable.MemberName, typeId));
				}
			}
			foreach (string text in script.ComponentIds)
			{
				ComponentDefinition componentDefinition;
				if (MonoBehaviourSingleton<StageManager>.Instance.ComponentList.TryGetDefinition(text, out componentDefinition) && componentDefinition.InspectableMembers.Count > 0)
				{
					int typeId2 = EndlessTypeMapping.Instance.GetTypeId(componentDefinition.ComponentBase.GetType().AssemblyQualifiedName);
					foreach (InspectorExposedVariable inspectorExposedVariable2 in componentDefinition.InspectableMembers)
					{
						list.Add(new ValueTuple<int, string, int>(EndlessTypeMapping.Instance.GetTypeId(inspectorExposedVariable2.DataType), inspectorExposedVariable2.MemberName, typeId2));
					}
				}
			}
			IEnumerable<ValueTuple<int, string, int>> enumerable = script.InspectorValues.Select((InspectorScriptValue data) => new ValueTuple<int, string, int>(data.DataType, data.Name, -1));
			List<ValueTuple<int, string, int>> list2 = list.Concat(enumerable).ToList<ValueTuple<int, string, int>>();
			for (int i = script.InspectorOrganizationData.Count - 1; i >= 0; i--)
			{
				InspectorOrganizationData savedData = script.InspectorOrganizationData[i];
				if (!list2.Any(([TupleElementNames(new string[] { "DataType", "MemberName", "ComponentId" })] ValueTuple<int, string, int> availableData) => availableData.Item1 == savedData.DataType && availableData.Item2 == savedData.MemberName && savedData.ComponentId == availableData.Item3))
				{
					script.InspectorOrganizationData.RemoveAt(i);
				}
			}
			foreach (ValueTuple<int, string, int> valueTuple in list2)
			{
				script.GetInspectorOrganizationData(valueTuple.Item1, valueTuple.Item2, valueTuple.Item3);
			}
		}
	}
}
