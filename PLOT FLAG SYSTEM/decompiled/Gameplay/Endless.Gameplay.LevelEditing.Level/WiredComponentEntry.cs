using System;
using System.Collections.Generic;
using Endless.Gameplay.Serialization;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class WiredComponentEntry
{
	public int TypeId;

	public List<EventEntry> EventEntries = new List<EventEntry>();

	[JsonIgnore]
	public string AssemblyQualifiedTypeName => EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(TypeId);
}
