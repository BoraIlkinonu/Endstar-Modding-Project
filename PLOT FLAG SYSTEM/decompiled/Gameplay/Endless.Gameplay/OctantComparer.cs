using System.Collections.Generic;
using Unity.Collections;

namespace Endless.Gameplay;

public struct OctantComparer : IComparer<int>
{
	private NativeArray<Octant> octants;

	public OctantComparer(NativeArray<Octant> octants)
	{
		this.octants = octants;
	}

	public int Compare(int a, int b)
	{
		Octant octant = octants[a];
		Octant octant2 = octants[b];
		float y = octant.Center.y;
		int num = y.CompareTo(octant2.Center.y);
		if (num != 0)
		{
			return num;
		}
		y = octant.Center.z;
		int num2 = y.CompareTo(octant2.Center.z);
		if (num2 != 0)
		{
			return num2;
		}
		y = octant.Center.x;
		return y.CompareTo(octant2.Center.x);
	}
}
