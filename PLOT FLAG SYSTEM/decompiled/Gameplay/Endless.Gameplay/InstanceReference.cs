using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public class InstanceReference : InspectorPropReference
{
	public bool useContext;

	internal override ReferenceFilter Filter => ReferenceFilter.NonStatic;

	internal InstanceReference()
	{
	}

	internal InstanceReference(SerializableGuid instanceId, bool useContext)
	{
		Id = instanceId;
		this.useContext = useContext;
	}

	public Context GetContext()
	{
		if (useContext)
		{
			return Context.StaticLastContext;
		}
		GameObject instanceObject = GetInstanceObject();
		if (!instanceObject)
		{
			return null;
		}
		return instanceObject.GetComponent<WorldObject>().Context;
	}

	internal GameObject GetInstanceObject()
	{
		if (useContext)
		{
			return Context.StaticLastContext.WorldObject.gameObject;
		}
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance && (bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && Id != SerializableGuid.Empty)
		{
			return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetGameObjectFromInstanceId(Id);
		}
		return null;
	}

	internal SerializableGuid GetInstanceDefintionId()
	{
		return GetReference()?.AssetId ?? SerializableGuid.Empty;
	}

	private PropEntry GetReference()
	{
		if (useContext)
		{
			return null;
		}
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance && (bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(Id, out var propEntry))
		{
			return propEntry;
		}
		return null;
	}

	public string GetReferenceName()
	{
		if (useContext)
		{
			return "Use Context";
		}
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance && (bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage && MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.TryGetPropEntry(Id, out var propEntry))
		{
			return propEntry.Label;
		}
		return "None";
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}: {2}", base.ToString(), "useContext", useContext);
	}
}
