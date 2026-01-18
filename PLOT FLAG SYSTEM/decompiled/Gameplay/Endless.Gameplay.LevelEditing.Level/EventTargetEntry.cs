using System;
using System.Collections.Generic;
using Endless.Gameplay.Serialization;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class EventTargetEntry
{
	public SerializableGuid WiringId;

	public int TypeId;

	public SerializableGuid ReceiverInstanceId;

	public string ReceiverMemberName;

	public StoredParameter[] StaticParameter;

	public List<SerializableGuid> RerouteNodeIds = new List<SerializableGuid>();

	public WireColor WireColor;

	[JsonIgnore]
	public string AssemblyQualifiedTypeName => EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(TypeId);
}
