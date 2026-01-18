using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class PropRequirement : ScriptableObject
{
	[Serializable]
	public class PropRequirementEntry
	{
		public string Name;

		public string Id;
	}

	[SerializeField]
	private int minCount;

	[SerializeField]
	private bool useMaxCount;

	[SerializeField]
	private int maxCount;

	[SerializeField]
	private List<PropRequirementEntry> propEntries = new List<PropRequirementEntry>();

	[NonSerialized]
	private List<SerializableGuid> guids;

	public List<SerializableGuid> Guids
	{
		get
		{
			if (guids == null)
			{
				guids = propEntries.Select((PropRequirementEntry entry) => new SerializableGuid(entry.Id)).ToList();
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
