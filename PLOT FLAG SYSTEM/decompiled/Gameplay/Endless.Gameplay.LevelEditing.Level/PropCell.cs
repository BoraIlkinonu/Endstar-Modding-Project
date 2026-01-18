using Endless.Props.Assets;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

public class PropCell : Cell
{
	public SerializableGuid InstanceId { get; set; }

	public override CellType Type => CellType.Prop;

	public PropCell(PropLocationOffset propLocationOffset, Transform cellBase)
		: base(propLocationOffset.Offset, cellBase)
	{
	}

	public override bool BlocksDecorations()
	{
		return true;
	}
}
