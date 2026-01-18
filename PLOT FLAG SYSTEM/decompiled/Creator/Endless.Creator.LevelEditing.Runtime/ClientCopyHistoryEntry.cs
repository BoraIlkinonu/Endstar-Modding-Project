using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class ClientCopyHistoryEntry
{
	public string Label { get; set; }

	public SerializableGuid InstanceId { get; set; }

	public SerializableGuid AssetId { get; set; }

	public Quaternion Rotation { get; set; }
}
