using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.DataTypes;
using Unity.Mathematics;

namespace Endless.Gameplay
{
	// Token: 0x02000217 RID: 535
	public struct NativeCellData : IEquatable<NativeCellData>
	{
		// Token: 0x06000B14 RID: 2836 RVA: 0x0003CFB8 File Offset: 0x0003B1B8
		public bool Equals(NativeCellData other)
		{
			return this.Position.Equals(other.Position);
		}

		// Token: 0x06000B15 RID: 2837 RVA: 0x0003CFCC File Offset: 0x0003B1CC
		public override bool Equals(object obj)
		{
			if (obj is NativeCellData)
			{
				NativeCellData nativeCellData = (NativeCellData)obj;
				return this.Equals(nativeCellData);
			}
			return false;
		}

		// Token: 0x06000B16 RID: 2838 RVA: 0x0003CFF1 File Offset: 0x0003B1F1
		public override int GetHashCode()
		{
			return this.Position.GetHashCode();
		}

		// Token: 0x04000A7D RID: 2685
		public float3 Position;

		// Token: 0x04000A7E RID: 2686
		public SerializableGuid AssociatedProp;

		// Token: 0x04000A7F RID: 2687
		public bool IsSplitCell;

		// Token: 0x04000A80 RID: 2688
		public bool IsTerrain;

		// Token: 0x04000A81 RID: 2689
		public bool IsConditionallyNavigable;

		// Token: 0x04000A82 RID: 2690
		public SlopeNeighbors SlopeNeighbors;
	}
}
