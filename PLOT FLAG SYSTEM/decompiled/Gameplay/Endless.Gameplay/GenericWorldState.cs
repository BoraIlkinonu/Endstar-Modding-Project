using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay;

public class GenericWorldState : IEquatable<GenericWorldState>
{
	private readonly Func<NpcEntity, bool> stateFunc;

	public WorldState WorldState { get; }

	public GenericWorldState(WorldState worldState, Func<NpcEntity, bool> stateFunc)
	{
		WorldState = worldState;
		this.stateFunc = stateFunc;
	}

	public bool Evaluate(NpcEntity entity)
	{
		return stateFunc(entity);
	}

	public bool Equals(GenericWorldState other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (WorldState == other.WorldState)
		{
			return object.Equals(stateFunc, other.stateFunc);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((GenericWorldState)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(WorldState, stateFunc);
	}
}
