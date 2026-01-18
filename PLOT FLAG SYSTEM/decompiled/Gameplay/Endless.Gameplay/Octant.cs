using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Endless.Gameplay;

public struct Octant
{
	public static class Factory
	{
		private static readonly float3 cellSize = new float3(1f);

		private static readonly float3 splitChildSize = new float3(0.5f);

		private static readonly float3 splitGrandchildSize = new float3(0.25f);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Octant BuildContainingOctant(float3 center, float3 size)
		{
			return new Octant(center, size, isWalkable: false, isBlocking: false, isConditionallyNavigable: false, isSlope: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Octant BuildTerrainCellOctant(float3 center)
		{
			return new Octant(center, cellSize, isWalkable: false, isBlocking: true, isConditionallyNavigable: false, isSlope: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Octant BuildWalkableCellOctant(float3 center)
		{
			return new Octant(center, cellSize, isWalkable: true, isBlocking: false, isConditionallyNavigable: false, isSlope: false);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Octant BuildSplitOctant(float3 center, bool isConditionalOctant, bool isSlope)
		{
			return new Octant(center, cellSize, isWalkable: false, isBlocking: false, isConditionalOctant, isSlope);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Octant BuildSplitOctantChild(float3 center, bool isConditionalOctant, bool isSlope)
		{
			return new Octant(center, splitChildSize, isWalkable: false, isBlocking: false, isConditionalOctant, isSlope);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Octant BuildSplitOctantGrandchild(float3 center, bool isWalkable, bool isBlocking, bool isConditionalOctant, bool isSlope)
		{
			return new Octant(center, splitGrandchildSize, isWalkable, isBlocking, isConditionalOctant, isSlope);
		}
	}

	public readonly float3 Center;

	public readonly float3 Size;

	private readonly float3 extents;

	private byte backing;

	private int childOctant0;

	private int childOctant1;

	private int childOctant2;

	private int childOctant3;

	private int childOctant4;

	private int childOctant5;

	private int childOctant6;

	private int childOctant7;

	public float3 Min => Center - extents;

	public float3 Max => Center + extents;

	public bool IsValid
	{
		get
		{
			return CheckBackingBit(0);
		}
		private set
		{
			backing = SetBackingBit(value, 0);
		}
	}

	public bool HasChildren
	{
		get
		{
			return CheckBackingBit(1);
		}
		set
		{
			backing = SetBackingBit(value, 1);
		}
	}

	public bool IsWalkable
	{
		get
		{
			return CheckBackingBit(2);
		}
		set
		{
			backing = SetBackingBit(value, 2);
		}
	}

	public bool IsBlocking
	{
		get
		{
			return CheckBackingBit(3);
		}
		set
		{
			backing = SetBackingBit(value, 3);
		}
	}

	public bool IsSlope
	{
		get
		{
			return CheckBackingBit(4);
		}
		private set
		{
			backing = SetBackingBit(value, 4);
		}
	}

	public bool IsConditionallyNavigable
	{
		get
		{
			return CheckBackingBit(5);
		}
		private set
		{
			backing = SetBackingBit(value, 5);
		}
	}

	public bool HasWalkableChildren
	{
		get
		{
			return CheckBackingBit(6);
		}
		set
		{
			backing = SetBackingBit(value, 6);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Octant(float3 center, float3 size, bool isWalkable, bool isBlocking, bool isConditionallyNavigable, bool isSlope)
	{
		Center = center;
		Size = size;
		extents = size / 2f;
		backing = 0;
		childOctant0 = -1;
		childOctant1 = -1;
		childOctant2 = -1;
		childOctant3 = -1;
		childOctant4 = -1;
		childOctant5 = -1;
		childOctant6 = -1;
		childOctant7 = -1;
		IsConditionallyNavigable = isConditionallyNavigable;
		IsSlope = isSlope;
		IsValid = true;
		IsWalkable = isWalkable;
		IsBlocking = isBlocking;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool CheckBackingBit(int pos)
	{
		return (backing & (1 << pos)) != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private byte SetBackingBit(bool value, int pos)
	{
		if (value)
		{
			return (byte)(backing | (1 << pos));
		}
		return (byte)(backing & ~(1 << pos));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetChildIndex(int index, int value)
	{
		switch (index)
		{
		case 0:
			childOctant0 = value;
			break;
		case 1:
			childOctant1 = value;
			break;
		case 2:
			childOctant2 = value;
			break;
		case 3:
			childOctant3 = value;
			break;
		case 4:
			childOctant4 = value;
			break;
		case 5:
			childOctant5 = value;
			break;
		case 6:
			childOctant6 = value;
			break;
		case 7:
			childOctant7 = value;
			break;
		default:
			throw new IndexOutOfRangeException();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetChildOctantIndexFromPoint(float3 point, out int index)
	{
		int octantRelativeOctantIndex = GetOctantRelativeOctantIndex(Center, point);
		return TryGetChildIndex(octantRelativeOctantIndex, out index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 ClosestPoint(float3 point)
	{
		return math.clamp(point, Center - extents, Center + extents);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Octant GetInvalidOctant()
	{
		return new Octant
		{
			IsValid = false
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetOctantRelativeOctantIndex(float3 octantCenter, float3 point)
	{
		int num = 0;
		if (point.y <= octantCenter.y)
		{
			num |= 4;
		}
		if (point.x <= octantCenter.x)
		{
			num |= 2;
		}
		if (point.z <= octantCenter.z)
		{
			num |= 1;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetChildIndex(int index, out int key)
	{
		key = index switch
		{
			0 => childOctant0, 
			1 => childOctant1, 
			2 => childOctant2, 
			3 => childOctant3, 
			4 => childOctant4, 
			5 => childOctant5, 
			6 => childOctant6, 
			7 => childOctant7, 
			_ => throw new IndexOutOfRangeException(), 
		};
		return key != -1;
	}
}
