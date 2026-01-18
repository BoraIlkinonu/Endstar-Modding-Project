using System;

// Token: 0x0200000C RID: 12
public readonly struct BidirectionalConnection : IEquatable<BidirectionalConnection>, IComparable<BidirectionalConnection>
{
	// Token: 0x0600002F RID: 47 RVA: 0x000029F5 File Offset: 0x00000BF5
	public BidirectionalConnection(int sectionIndexA, int sectionIndexB)
	{
		this.SectionIndexA = sectionIndexA;
		this.SectionIndexB = sectionIndexB;
	}

	// Token: 0x06000030 RID: 48 RVA: 0x00002A08 File Offset: 0x00000C08
	public override int GetHashCode()
	{
		int hashCode = this.SectionIndexA.GetHashCode();
		int hashCode2 = this.SectionIndexB.GetHashCode();
		return hashCode ^ hashCode2;
	}

	// Token: 0x06000031 RID: 49 RVA: 0x00002A34 File Offset: 0x00000C34
	public bool Equals(BidirectionalConnection other)
	{
		return (this.SectionIndexA.Equals(other.SectionIndexA) && this.SectionIndexB.Equals(other.SectionIndexB)) || (this.SectionIndexA.Equals(other.SectionIndexB) && this.SectionIndexB.Equals(other.SectionIndexA));
	}

	// Token: 0x06000032 RID: 50 RVA: 0x00002A9C File Offset: 0x00000C9C
	public override bool Equals(object obj)
	{
		if (obj is BidirectionalConnection)
		{
			BidirectionalConnection bidirectionalConnection = (BidirectionalConnection)obj;
			return this.Equals(bidirectionalConnection);
		}
		return false;
	}

	// Token: 0x06000033 RID: 51 RVA: 0x00002AC4 File Offset: 0x00000CC4
	public int CompareTo(BidirectionalConnection other)
	{
		int num = this.SectionIndexA.CompareTo(other.SectionIndexA);
		if (num == 0)
		{
			return this.SectionIndexB.CompareTo(other.SectionIndexB);
		}
		return num;
	}

	// Token: 0x0400001B RID: 27
	public readonly int SectionIndexA;

	// Token: 0x0400001C RID: 28
	public readonly int SectionIndexB;
}
