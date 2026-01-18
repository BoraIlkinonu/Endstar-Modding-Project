using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public static class InspectorReferenceUtility
{
	public static SerializableGuid GetId(InspectorReference reference)
	{
		return reference.Id;
	}

	public static GameObject GetInstanceObject(InstanceReference reference)
	{
		return reference.GetInstanceObject();
	}

	public static SerializableGuid GetInstanceDefinitionId(InstanceReference reference)
	{
		return reference.GetInstanceDefintionId();
	}

	public static ReferenceFilter GetReferenceFilter(InspectorPropReference reference)
	{
		return reference.Filter;
	}

	public static void SetId(InspectorReference reference, SerializableGuid newAssetId)
	{
		reference.Id = newAssetId;
	}
}
