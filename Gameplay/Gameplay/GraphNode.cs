using System;
using Unity.Mathematics;

// Token: 0x02000010 RID: 16
public struct GraphNode
{
	// Token: 0x0600003E RID: 62 RVA: 0x00002CB0 File Offset: 0x00000EB0
	public PathfindingNode GetPathfindingNode()
	{
		return new PathfindingNode
		{
			Key = this.Key,
			Center = this.Center,
			Parent = -1
		};
	}

	// Token: 0x0600003F RID: 63 RVA: 0x00002CE8 File Offset: 0x00000EE8
	public PathfindingNode GetPathfindingNode(int parent)
	{
		return new PathfindingNode
		{
			Key = this.Key,
			Center = this.Center,
			Parent = parent
		};
	}

	// Token: 0x04000024 RID: 36
	public int IslandKey;

	// Token: 0x04000025 RID: 37
	public int AreaKey;

	// Token: 0x04000026 RID: 38
	public int ZoneKey;

	// Token: 0x04000027 RID: 39
	public int Key;

	// Token: 0x04000028 RID: 40
	public float3 Center;
}
