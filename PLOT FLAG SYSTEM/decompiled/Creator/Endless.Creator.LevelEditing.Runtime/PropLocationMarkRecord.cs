using System.Collections.Generic;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class PropLocationMarkRecord
{
	public SerializableGuid Id;

	public PropLocationType Type;

	public List<Vector3Int> Locations = new List<Vector3Int>();
}
