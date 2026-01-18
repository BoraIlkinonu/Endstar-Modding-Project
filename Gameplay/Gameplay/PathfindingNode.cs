using System;
using Unity.Mathematics;

// Token: 0x02000011 RID: 17
public struct PathfindingNode : IEquatable<PathfindingNode>
{
	// Token: 0x17000012 RID: 18
	// (get) Token: 0x06000040 RID: 64 RVA: 0x00002D20 File Offset: 0x00000F20
	public float TotalCost
	{
		get
		{
			return this.CostToGoal + this.CostToNode;
		}
	}

	// Token: 0x17000013 RID: 19
	// (get) Token: 0x06000041 RID: 65 RVA: 0x00002D2F File Offset: 0x00000F2F
	// (set) Token: 0x06000042 RID: 66 RVA: 0x00002D37 File Offset: 0x00000F37
	public ConnectionKind ParentConnection { readonly get; set; }

	// Token: 0x06000043 RID: 67 RVA: 0x00002D40 File Offset: 0x00000F40
	public bool Equals(PathfindingNode other)
	{
		return this.Key.Equals(other.Key);
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00002D54 File Offset: 0x00000F54
	public override bool Equals(object obj)
	{
		if (obj is PathfindingNode)
		{
			PathfindingNode pathfindingNode = (PathfindingNode)obj;
			return this.Equals(pathfindingNode);
		}
		return false;
	}

	// Token: 0x06000045 RID: 69 RVA: 0x00002D79 File Offset: 0x00000F79
	public override int GetHashCode()
	{
		return this.Center.GetHashCode();
	}

	// Token: 0x04000029 RID: 41
	public int Key;

	// Token: 0x0400002A RID: 42
	public float3 Center;

	// Token: 0x0400002B RID: 43
	public int Parent;

	// Token: 0x0400002C RID: 44
	public float CostToNode;

	// Token: 0x0400002D RID: 45
	public float CostToGoal;
}
