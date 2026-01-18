using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Endless.Gameplay
{
	// Token: 0x02000219 RID: 537
	public struct Octant
	{
		// Token: 0x06000B17 RID: 2839 RVA: 0x0003D004 File Offset: 0x0003B204
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Octant(float3 center, float3 size, bool isWalkable, bool isBlocking, bool isConditionallyNavigable, bool isSlope)
		{
			this.Center = center;
			this.Size = size;
			this.extents = size / 2f;
			this.backing = 0;
			this.childOctant0 = -1;
			this.childOctant1 = -1;
			this.childOctant2 = -1;
			this.childOctant3 = -1;
			this.childOctant4 = -1;
			this.childOctant5 = -1;
			this.childOctant6 = -1;
			this.childOctant7 = -1;
			this.IsConditionallyNavigable = isConditionallyNavigable;
			this.IsSlope = isSlope;
			this.IsValid = true;
			this.IsWalkable = isWalkable;
			this.IsBlocking = isBlocking;
		}

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x06000B18 RID: 2840 RVA: 0x0003D095 File Offset: 0x0003B295
		public float3 Min
		{
			get
			{
				return this.Center - this.extents;
			}
		}

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06000B19 RID: 2841 RVA: 0x0003D0A8 File Offset: 0x0003B2A8
		public float3 Max
		{
			get
			{
				return this.Center + this.extents;
			}
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x06000B1A RID: 2842 RVA: 0x0003D0BB File Offset: 0x0003B2BB
		// (set) Token: 0x06000B1B RID: 2843 RVA: 0x0003D0C4 File Offset: 0x0003B2C4
		public bool IsValid
		{
			get
			{
				return this.CheckBackingBit(0);
			}
			private set
			{
				this.backing = this.SetBackingBit(value, 0);
			}
		}

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x06000B1C RID: 2844 RVA: 0x0003D0D4 File Offset: 0x0003B2D4
		// (set) Token: 0x06000B1D RID: 2845 RVA: 0x0003D0DD File Offset: 0x0003B2DD
		public bool HasChildren
		{
			get
			{
				return this.CheckBackingBit(1);
			}
			set
			{
				this.backing = this.SetBackingBit(value, 1);
			}
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000B1E RID: 2846 RVA: 0x0003D0ED File Offset: 0x0003B2ED
		// (set) Token: 0x06000B1F RID: 2847 RVA: 0x0003D0F6 File Offset: 0x0003B2F6
		public bool IsWalkable
		{
			get
			{
				return this.CheckBackingBit(2);
			}
			set
			{
				this.backing = this.SetBackingBit(value, 2);
			}
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000B20 RID: 2848 RVA: 0x0003D106 File Offset: 0x0003B306
		// (set) Token: 0x06000B21 RID: 2849 RVA: 0x0003D10F File Offset: 0x0003B30F
		public bool IsBlocking
		{
			get
			{
				return this.CheckBackingBit(3);
			}
			set
			{
				this.backing = this.SetBackingBit(value, 3);
			}
		}

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x06000B22 RID: 2850 RVA: 0x0003D11F File Offset: 0x0003B31F
		// (set) Token: 0x06000B23 RID: 2851 RVA: 0x0003D128 File Offset: 0x0003B328
		public bool IsSlope
		{
			get
			{
				return this.CheckBackingBit(4);
			}
			private set
			{
				this.backing = this.SetBackingBit(value, 4);
			}
		}

		// Token: 0x17000217 RID: 535
		// (get) Token: 0x06000B24 RID: 2852 RVA: 0x0003D138 File Offset: 0x0003B338
		// (set) Token: 0x06000B25 RID: 2853 RVA: 0x0003D141 File Offset: 0x0003B341
		public bool IsConditionallyNavigable
		{
			get
			{
				return this.CheckBackingBit(5);
			}
			private set
			{
				this.backing = this.SetBackingBit(value, 5);
			}
		}

		// Token: 0x17000218 RID: 536
		// (get) Token: 0x06000B26 RID: 2854 RVA: 0x0003D151 File Offset: 0x0003B351
		// (set) Token: 0x06000B27 RID: 2855 RVA: 0x0003D15A File Offset: 0x0003B35A
		public bool HasWalkableChildren
		{
			get
			{
				return this.CheckBackingBit(6);
			}
			set
			{
				this.backing = this.SetBackingBit(value, 6);
			}
		}

		// Token: 0x06000B28 RID: 2856 RVA: 0x0003D16A File Offset: 0x0003B36A
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool CheckBackingBit(int pos)
		{
			return ((int)this.backing & (1 << pos)) != 0;
		}

		// Token: 0x06000B29 RID: 2857 RVA: 0x0003D17C File Offset: 0x0003B37C
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private byte SetBackingBit(bool value, int pos)
		{
			if (value)
			{
				return (byte)((int)this.backing | (1 << pos));
			}
			return (byte)((int)this.backing & ~(1 << pos));
		}

		// Token: 0x06000B2A RID: 2858 RVA: 0x0003D1A0 File Offset: 0x0003B3A0
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetChildIndex(int index, int value)
		{
			switch (index)
			{
			case 0:
				this.childOctant0 = value;
				return;
			case 1:
				this.childOctant1 = value;
				return;
			case 2:
				this.childOctant2 = value;
				return;
			case 3:
				this.childOctant3 = value;
				return;
			case 4:
				this.childOctant4 = value;
				return;
			case 5:
				this.childOctant5 = value;
				return;
			case 6:
				this.childOctant6 = value;
				return;
			case 7:
				this.childOctant7 = value;
				return;
			default:
				throw new IndexOutOfRangeException();
			}
		}

		// Token: 0x06000B2B RID: 2859 RVA: 0x0003D21C File Offset: 0x0003B41C
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetChildOctantIndexFromPoint(float3 point, out int index)
		{
			int octantRelativeOctantIndex = Octant.GetOctantRelativeOctantIndex(this.Center, point);
			return this.TryGetChildIndex(octantRelativeOctantIndex, out index);
		}

		// Token: 0x06000B2C RID: 2860 RVA: 0x0003D23E File Offset: 0x0003B43E
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float3 ClosestPoint(float3 point)
		{
			return math.clamp(point, this.Center - this.extents, this.Center + this.extents);
		}

		// Token: 0x06000B2D RID: 2861 RVA: 0x0003D268 File Offset: 0x0003B468
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Octant GetInvalidOctant()
		{
			return new Octant
			{
				IsValid = false
			};
		}

		// Token: 0x06000B2E RID: 2862 RVA: 0x0003D288 File Offset: 0x0003B488
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

		// Token: 0x06000B2F RID: 2863 RVA: 0x0003D2D0 File Offset: 0x0003B4D0
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetChildIndex(int index, out int key)
		{
			int num;
			switch (index)
			{
			case 0:
				num = this.childOctant0;
				break;
			case 1:
				num = this.childOctant1;
				break;
			case 2:
				num = this.childOctant2;
				break;
			case 3:
				num = this.childOctant3;
				break;
			case 4:
				num = this.childOctant4;
				break;
			case 5:
				num = this.childOctant5;
				break;
			case 6:
				num = this.childOctant6;
				break;
			case 7:
				num = this.childOctant7;
				break;
			default:
				throw new IndexOutOfRangeException();
			}
			key = num;
			return key != -1;
		}

		// Token: 0x04000A89 RID: 2697
		public readonly float3 Center;

		// Token: 0x04000A8A RID: 2698
		public readonly float3 Size;

		// Token: 0x04000A8B RID: 2699
		private readonly float3 extents;

		// Token: 0x04000A8C RID: 2700
		private byte backing;

		// Token: 0x04000A8D RID: 2701
		private int childOctant0;

		// Token: 0x04000A8E RID: 2702
		private int childOctant1;

		// Token: 0x04000A8F RID: 2703
		private int childOctant2;

		// Token: 0x04000A90 RID: 2704
		private int childOctant3;

		// Token: 0x04000A91 RID: 2705
		private int childOctant4;

		// Token: 0x04000A92 RID: 2706
		private int childOctant5;

		// Token: 0x04000A93 RID: 2707
		private int childOctant6;

		// Token: 0x04000A94 RID: 2708
		private int childOctant7;

		// Token: 0x0200021A RID: 538
		public static class Factory
		{
			// Token: 0x06000B30 RID: 2864 RVA: 0x0003D35E File Offset: 0x0003B55E
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Octant BuildContainingOctant(float3 center, float3 size)
			{
				return new Octant(center, size, false, false, false, false);
			}

			// Token: 0x06000B31 RID: 2865 RVA: 0x0003D36B File Offset: 0x0003B56B
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Octant BuildTerrainCellOctant(float3 center)
			{
				return new Octant(center, Octant.Factory.cellSize, false, true, false, false);
			}

			// Token: 0x06000B32 RID: 2866 RVA: 0x0003D37C File Offset: 0x0003B57C
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Octant BuildWalkableCellOctant(float3 center)
			{
				return new Octant(center, Octant.Factory.cellSize, true, false, false, false);
			}

			// Token: 0x06000B33 RID: 2867 RVA: 0x0003D38D File Offset: 0x0003B58D
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Octant BuildSplitOctant(float3 center, bool isConditionalOctant, bool isSlope)
			{
				return new Octant(center, Octant.Factory.cellSize, false, false, isConditionalOctant, isSlope);
			}

			// Token: 0x06000B34 RID: 2868 RVA: 0x0003D39E File Offset: 0x0003B59E
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Octant BuildSplitOctantChild(float3 center, bool isConditionalOctant, bool isSlope)
			{
				return new Octant(center, Octant.Factory.splitChildSize, false, false, isConditionalOctant, isSlope);
			}

			// Token: 0x06000B35 RID: 2869 RVA: 0x0003D3AF File Offset: 0x0003B5AF
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Octant BuildSplitOctantGrandchild(float3 center, bool isWalkable, bool isBlocking, bool isConditionalOctant, bool isSlope)
			{
				return new Octant(center, Octant.Factory.splitGrandchildSize, isWalkable, isBlocking, isConditionalOctant, isSlope);
			}

			// Token: 0x04000A95 RID: 2709
			private static readonly float3 cellSize = new float3(1f);

			// Token: 0x04000A96 RID: 2710
			private static readonly float3 splitChildSize = new float3(0.5f);

			// Token: 0x04000A97 RID: 2711
			private static readonly float3 splitGrandchildSize = new float3(0.25f);
		}
	}
}
