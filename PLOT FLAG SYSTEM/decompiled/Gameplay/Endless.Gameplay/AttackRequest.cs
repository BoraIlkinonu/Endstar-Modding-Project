using System;

namespace Endless.Gameplay;

public readonly struct AttackRequest : IEquatable<AttackRequest>
{
	public readonly WorldObject Requester;

	public readonly HittableComponent Target;

	public readonly Action<WorldObject> Response;

	public AttackRequest(WorldObject requester, HittableComponent target, Action<WorldObject> response)
	{
		Requester = requester;
		Target = target;
		Response = response;
	}

	public bool Equals(AttackRequest other)
	{
		if (Requester == other.Requester)
		{
			return Target == other.Target;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is AttackRequest other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Requester, Target);
	}
}
