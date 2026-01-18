using System;

public struct ExcludedEdge : IEquatable<ExcludedEdge>
{
	public int OriginNodeKey;

	public int EndNodeKey;

	public bool Equals(ExcludedEdge other)
	{
		if (OriginNodeKey.Equals(other.OriginNodeKey))
		{
			return EndNodeKey.Equals(other.EndNodeKey);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ExcludedEdge other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (19 * 23 + OriginNodeKey.GetHashCode()) * 23 + EndNodeKey.GetHashCode();
	}
}
