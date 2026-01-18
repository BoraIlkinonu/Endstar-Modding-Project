using System;

public readonly struct BidirectionalConnection : IEquatable<BidirectionalConnection>, IComparable<BidirectionalConnection>
{
	public readonly int SectionIndexA;

	public readonly int SectionIndexB;

	public BidirectionalConnection(int sectionIndexA, int sectionIndexB)
	{
		SectionIndexA = sectionIndexA;
		SectionIndexB = sectionIndexB;
	}

	public override int GetHashCode()
	{
		int sectionIndexA = SectionIndexA;
		int hashCode = sectionIndexA.GetHashCode();
		sectionIndexA = SectionIndexB;
		int hashCode2 = sectionIndexA.GetHashCode();
		return hashCode ^ hashCode2;
	}

	public bool Equals(BidirectionalConnection other)
	{
		int sectionIndexA = SectionIndexA;
		if (sectionIndexA.Equals(other.SectionIndexA))
		{
			sectionIndexA = SectionIndexB;
			if (sectionIndexA.Equals(other.SectionIndexB))
			{
				return true;
			}
		}
		sectionIndexA = SectionIndexA;
		if (sectionIndexA.Equals(other.SectionIndexB))
		{
			sectionIndexA = SectionIndexB;
			return sectionIndexA.Equals(other.SectionIndexA);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BidirectionalConnection other)
		{
			return Equals(other);
		}
		return false;
	}

	public int CompareTo(BidirectionalConnection other)
	{
		int sectionIndexA = SectionIndexA;
		int num = sectionIndexA.CompareTo(other.SectionIndexA);
		if (num == 0)
		{
			sectionIndexA = SectionIndexB;
			return sectionIndexA.CompareTo(other.SectionIndexB);
		}
		return num;
	}
}
