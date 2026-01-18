using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class NavPath
{
	public struct Segment
	{
		public int StartSection;

		public int EndSection;

		public float3 StartPosition;

		public float3 EndPosition;

		public ConnectionKind ConnectionKind;
	}

	public readonly Queue<Segment> NavigationSegments;

	private float? length;

	public Vector3 Destination { get; }

	public NavPath(Vector3 destination, Queue<Segment> segments)
	{
		Destination = destination;
		NavigationSegments = segments;
	}

	public NavPath(NavPath path)
	{
		NavigationSegments = new Queue<Segment>(path.NavigationSegments);
		Destination = path.Destination;
	}

	public float GetLength()
	{
		if (length.HasValue)
		{
			return length.Value;
		}
		Vector3[] array = new Vector3[30];
		length = 0f;
		foreach (Segment navigationSegment in NavigationSegments)
		{
			if (navigationSegment.ConnectionKind == ConnectionKind.Walk)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				NavMesh.CalculatePath(navigationSegment.StartPosition, navigationSegment.EndPosition, -1, navMeshPath);
				int cornersNonAlloc = navMeshPath.GetCornersNonAlloc(array);
				for (int i = 0; i < cornersNonAlloc - 1; i++)
				{
					length += math.distance(array[i], array[i + 1]);
				}
			}
			else
			{
				length += math.distance(navigationSegment.StartPosition, navigationSegment.EndPosition);
			}
		}
		return length.Value;
	}
}
