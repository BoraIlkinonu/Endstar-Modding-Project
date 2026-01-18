using Unity.Mathematics;

public struct GraphNode
{
	public int IslandKey;

	public int AreaKey;

	public int ZoneKey;

	public int Key;

	public float3 Center;

	public PathfindingNode GetPathfindingNode()
	{
		return new PathfindingNode
		{
			Key = Key,
			Center = Center,
			Parent = -1
		};
	}

	public PathfindingNode GetPathfindingNode(int parent)
	{
		return new PathfindingNode
		{
			Key = Key,
			Center = Center,
			Parent = parent
		};
	}
}
