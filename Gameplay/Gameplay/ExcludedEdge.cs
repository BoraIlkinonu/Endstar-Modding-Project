using System;

// Token: 0x0200000F RID: 15
public struct ExcludedEdge : IEquatable<ExcludedEdge>
{
	// Token: 0x0600003B RID: 59 RVA: 0x00002C3F File Offset: 0x00000E3F
	public bool Equals(ExcludedEdge other)
	{
		return this.OriginNodeKey.Equals(other.OriginNodeKey) && this.EndNodeKey.Equals(other.EndNodeKey);
	}

	// Token: 0x0600003C RID: 60 RVA: 0x00002C68 File Offset: 0x00000E68
	public override bool Equals(object obj)
	{
		if (obj is ExcludedEdge)
		{
			ExcludedEdge excludedEdge = (ExcludedEdge)obj;
			return this.Equals(excludedEdge);
		}
		return false;
	}

	// Token: 0x0600003D RID: 61 RVA: 0x00002C8D File Offset: 0x00000E8D
	public override int GetHashCode()
	{
		return (19 * 23 + this.OriginNodeKey.GetHashCode()) * 23 + this.EndNodeKey.GetHashCode();
	}

	// Token: 0x04000022 RID: 34
	public int OriginNodeKey;

	// Token: 0x04000023 RID: 35
	public int EndNodeKey;
}
