using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000206 RID: 518
	public class NavPath
	{
		// Token: 0x17000203 RID: 515
		// (get) Token: 0x06000AC2 RID: 2754 RVA: 0x0003A8B4 File Offset: 0x00038AB4
		public Vector3 Destination { get; }

		// Token: 0x06000AC3 RID: 2755 RVA: 0x0003A8BC File Offset: 0x00038ABC
		public NavPath(Vector3 destination, Queue<NavPath.Segment> segments)
		{
			this.Destination = destination;
			this.NavigationSegments = segments;
		}

		// Token: 0x06000AC4 RID: 2756 RVA: 0x0003A8D2 File Offset: 0x00038AD2
		public NavPath(NavPath path)
		{
			this.NavigationSegments = new Queue<NavPath.Segment>(path.NavigationSegments);
			this.Destination = path.Destination;
		}

		// Token: 0x06000AC5 RID: 2757 RVA: 0x0003A8F8 File Offset: 0x00038AF8
		public float GetLength()
		{
			if (this.length != null)
			{
				return this.length.Value;
			}
			Vector3[] array = new Vector3[30];
			this.length = new float?(0f);
			foreach (NavPath.Segment segment in this.NavigationSegments)
			{
				if (segment.ConnectionKind == ConnectionKind.Walk)
				{
					NavMeshPath navMeshPath = new NavMeshPath();
					NavMesh.CalculatePath(segment.StartPosition, segment.EndPosition, -1, navMeshPath);
					int cornersNonAlloc = navMeshPath.GetCornersNonAlloc(array);
					for (int i = 0; i < cornersNonAlloc - 1; i++)
					{
						this.length += math.distance(array[i], array[i + 1]);
					}
				}
				else
				{
					this.length += math.distance(segment.StartPosition, segment.EndPosition);
				}
			}
			return this.length.Value;
		}

		// Token: 0x04000A1F RID: 2591
		public readonly Queue<NavPath.Segment> NavigationSegments;

		// Token: 0x04000A21 RID: 2593
		private float? length;

		// Token: 0x02000207 RID: 519
		public struct Segment
		{
			// Token: 0x04000A22 RID: 2594
			public int StartSection;

			// Token: 0x04000A23 RID: 2595
			public int EndSection;

			// Token: 0x04000A24 RID: 2596
			public float3 StartPosition;

			// Token: 0x04000A25 RID: 2597
			public float3 EndPosition;

			// Token: 0x04000A26 RID: 2598
			public ConnectionKind ConnectionKind;
		}
	}
}
