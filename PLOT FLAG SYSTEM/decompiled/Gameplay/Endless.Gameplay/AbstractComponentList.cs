using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class AbstractComponentList<T> : ScriptableObject where T : ComponentDefinition
{
	[SerializeField]
	private List<T> components = new List<T>();

	private Dictionary<SerializableGuid, T> definitionMap;

	private Dictionary<SerializableGuid, T> DefinitionMap
	{
		get
		{
			if (definitionMap == null)
			{
				definitionMap = new Dictionary<SerializableGuid, T>();
				foreach (T component in components)
				{
					definitionMap.Add(component.ComponentId, component);
				}
			}
			return definitionMap;
		}
	}

	public IReadOnlyList<T> AllDefinitions => components;

	public bool TryGetDefinition(SerializableGuid componentId, out T componentDefinition)
	{
		return DefinitionMap.TryGetValue(componentId, out componentDefinition);
	}

	public bool TryGetDefinition(Type type, out T componentDefinition)
	{
		for (int i = 0; i < components.Count; i++)
		{
			if (components[i].ComponentBase.GetType() == type)
			{
				componentDefinition = components[i];
				return true;
			}
		}
		componentDefinition = null;
		return false;
	}
}
