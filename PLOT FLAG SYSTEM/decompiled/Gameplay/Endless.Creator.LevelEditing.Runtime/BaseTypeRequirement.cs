using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

[CreateAssetMenu(menuName = "ScriptableObject/BaseTypeRequirement", fileName = "BaseTypeRequirement")]
public class BaseTypeRequirement : ScriptableObject
{
	[SerializeField]
	private int minCount;

	[SerializeField]
	private bool useMaxCount;

	[SerializeField]
	private int maxCount;

	[SerializeField]
	private List<ComponentDefinition> componentDefinitions = new List<ComponentDefinition>();

	[NonSerialized]
	private List<SerializableGuid> guids;

	public List<SerializableGuid> Guids
	{
		get
		{
			if (guids == null)
			{
				guids = componentDefinitions.Select((ComponentDefinition entry) => entry.ComponentId).ToList();
			}
			return guids;
		}
	}

	public int MinCount => minCount;

	public int MaxCount
	{
		get
		{
			if (!useMaxCount)
			{
				return int.MaxValue;
			}
			return maxCount;
		}
	}

	public bool UseMaxCount => useMaxCount;
}
