using System;
using System.Collections.Generic;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public class ComponentDefinition : ScriptableObject
{
	[SerializeField]
	private GameObject prefab;

	[SerializeField]
	private SerializableGuid componentId;

	public List<InspectorExposedVariable> InspectableMembers = new List<InspectorExposedVariable>();

	public List<EndlessEventInfo> AvailableEvents = new List<EndlessEventInfo>();

	public List<EndlessEventInfo> AvailableReceivers = new List<EndlessEventInfo>();

	[SerializeField]
	private bool isNetworked;

	private bool hasGottenFilter;

	private ReferenceFilter filter;

	private IComponentBase componentBase;

	public ReferenceFilter Filter
	{
		get
		{
			if (!hasGottenFilter)
			{
				filter = prefab.GetComponent<IComponentBase>().Filter;
			}
			return filter;
		}
	}

	public GameObject Prefab => prefab;

	public SerializableGuid ComponentId => componentId;

	public bool IsNetworked => isNetworked;

	public IComponentBase ComponentBase
	{
		get
		{
			if (componentBase == null)
			{
				componentBase = prefab.GetComponent<IComponentBase>();
			}
			return componentBase;
		}
	}

	public Type ComponentReferenceType => ComponentBase.ComponentReferenceType;

	public bool HasMember(MemberChange memberChange)
	{
		foreach (InspectorExposedVariable inspectableMember in InspectableMembers)
		{
			if (inspectableMember.MemberName == memberChange.MemberName)
			{
				int typeId = EndlessTypeMapping.Instance.GetTypeId(inspectableMember.DataType);
				if (typeId == memberChange.DataType)
				{
					return true;
				}
				Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(typeId);
				Type typeFromId2 = EndlessTypeMapping.Instance.GetTypeFromId(memberChange.DataType);
				if (typeFromId.IsAssignableFrom(typeFromId2) || typeFromId.BaseType == typeFromId2.BaseType)
				{
					return true;
				}
			}
		}
		return false;
	}
}
