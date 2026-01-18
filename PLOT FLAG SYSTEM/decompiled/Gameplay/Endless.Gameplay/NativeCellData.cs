using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.DataTypes;
using Unity.Mathematics;

namespace Endless.Gameplay;

public struct NativeCellData : IEquatable<NativeCellData>
{
	public float3 Position;

	public SerializableGuid AssociatedProp;

	public bool IsSplitCell;

	public bool IsTerrain;

	public bool IsConditionallyNavigable;

	public SlopeNeighbors SlopeNeighbors;

	public bool Equals(NativeCellData other)
	{
		return Position.Equals(other.Position);
	}

	public override bool Equals(object obj)
	{
		if (obj is NativeCellData other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Position.GetHashCode();
	}
}
