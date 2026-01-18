using System;
using Unity.Mathematics;

public struct PathfindingNode : IEquatable<PathfindingNode>
{
	public int Key;

	public float3 Center;

	public int Parent;

	public float CostToNode;

	public float CostToGoal;

	public float TotalCost => CostToGoal + CostToNode;

	public ConnectionKind ParentConnection { get; set; }

	public bool Equals(PathfindingNode other)
	{
		return Key.Equals(other.Key);
	}

	public override bool Equals(object obj)
	{
		if (obj is PathfindingNode other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Center.GetHashCode();
	}
}
