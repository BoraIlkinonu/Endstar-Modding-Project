using System;
using System.Linq;
using System.Reflection;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using HackAnythingAnywhere.Core;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIInspectorPropertyModel
{
	private object model;

	public string Name { get; private set; }

	public string Description { get; private set; }

	public ClampValue[] ClampValues { get; private set; }

	public MemberChange MemberChange { get; private set; }

	public SerializableGuid PropInstanceId { get; private set; }

	public string ComponentTypeName { get; private set; }

	public bool ShouldSerialize { get; private set; }

	public MemberTypes MemberType { get; private set; }

	public UIInspectorPropertyModel(PropEntry propEntry, InspectorScriptValue inspectorScriptValue)
	{
		Name = inspectorScriptValue.Name;
		Description = inspectorScriptValue.Description;
		ClampValues = inspectorScriptValue.ClampValues;
		MemberChange = propEntry.LuaMemberChanges.FirstOrDefault((MemberChange item) => item.MemberName == inspectorScriptValue.Name) ?? new MemberChange(inspectorScriptValue.Name, inspectorScriptValue.DataType, inspectorScriptValue.DefaultValue);
		try
		{
			model = MemberChange.ToObject();
		}
		catch (Exception exception)
		{
			DebugUtility.Log(MemberChange.ToString());
			DebugUtility.LogException(exception);
		}
		PropInstanceId = propEntry.InstanceId;
		ComponentTypeName = string.Empty;
		ShouldSerialize = true;
		MemberType = MemberTypes.Field;
	}

	public UIInspectorPropertyModel(PropEntry propEntry, InspectorExposedVariable inspectorExposedVariable, ComponentDefinition componentDefinition)
	{
		Name = inspectorExposedVariable.MemberName;
		Description = inspectorExposedVariable.Description;
		ClampValues = inspectorExposedVariable.ClampValues;
		string assemblyQualifiedTypeName = componentDefinition.ComponentBase.GetType().AssemblyQualifiedName;
		ComponentEntry componentEntry = propEntry.ComponentEntries.FirstOrDefault((ComponentEntry item) => item.AssemblyQualifiedName == assemblyQualifiedTypeName);
		int typeId = EndlessTypeMapping.Instance.GetTypeId(inspectorExposedVariable.DataType);
		if (componentEntry == null)
		{
			MemberChange = new MemberChange(inspectorExposedVariable.MemberName, typeId, inspectorExposedVariable.DefaultValue);
		}
		else
		{
			MemberChange = componentEntry.Changes.FirstOrDefault((MemberChange item) => item.MemberName == inspectorExposedVariable.MemberName) ?? new MemberChange(inspectorExposedVariable.MemberName, typeId, inspectorExposedVariable.DefaultValue);
		}
		model = MemberChange.ToObject();
		PropInstanceId = propEntry.InstanceId;
		ComponentTypeName = assemblyQualifiedTypeName;
		ShouldSerialize = true;
		MemberInfo[] member = Type.GetType(assemblyQualifiedTypeName).GetMember(inspectorExposedVariable.MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (member.Length != 0)
		{
			ShouldSerialize = member[0].GetCustomAttribute<EndlessNonSerializedAttribute>() == null;
		}
		MemberType = inspectorExposedVariable.MemberType;
	}

	public object GetModel()
	{
		if (ShouldSerialize)
		{
			return model;
		}
		GameObject gameObjectFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(PropInstanceId);
		Type type = Type.GetType(ComponentTypeName);
		Component componentInChildren = gameObjectFromInstanceId.GetComponentInChildren(type);
		return RetrieveMemberInfo(type).GetValue(componentInChildren);
	}

	private MemberInfo RetrieveMemberInfo(Type getType)
	{
		return MemberType switch
		{
			MemberTypes.Field => getType.GetField(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), 
			MemberTypes.Property => getType.GetProperty(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), 
			_ => throw new ArgumentOutOfRangeException("MemberType", MemberType, "No support for model.MemberType!"), 
		};
	}

	public override string ToString()
	{
		return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}, {10}: {11}, {12}: {13}, {14}: {15}, {16}: {17} }}", "Name", Name, "Description", Description, "ClampValues", ClampValues.Length, "model", model, "PropInstanceId", PropInstanceId, "MemberChange", MemberChange, "ComponentTypeName", ComponentTypeName, "ShouldSerialize", ShouldSerialize, "MemberType", MemberType);
	}

	public void SetModel(object newModel)
	{
		model = newModel;
	}
}
