using System;

public struct Edge : IEquatable<Edge>
{
	public float Cost;

	public ConnectionKind Connection;

	public int ConnectedNodeKey;

	public bool Equals(Edge other)
	{
		if (Cost.Equals(other.Cost) && Connection == other.Connection)
		{
			return ConnectedNodeKey == other.ConnectedNodeKey;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Edge other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = (17 * 31 + Cost.GetHashCode()) * 31;
		int connection = (int)Connection;
		return (num + connection.GetHashCode()) * 31 + ConnectedNodeKey.GetHashCode();
	}
}
