using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

[Serializable]
public class PropEraseData
{
	public Vector3 Position { get; set; }

	public Quaternion Rotation { get; set; }

	public SerializableGuid InstanceId { get; set; }

	public SerializableGuid AssetId { get; set; }
}
