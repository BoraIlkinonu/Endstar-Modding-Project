using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class SerializedProp
{
	public Vector3 Position;

	public float Rotation;

	public SerializableGuid MasterReference;
}
