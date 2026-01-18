using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class WiringEntry
{
	public SerializableGuid EmitterInstanceId;

	public List<WiredComponentEntry> WiredComponents = new List<WiredComponentEntry>();
}
