using System;

// Token: 0x0200000E RID: 14
public struct Edge : IEquatable<Edge>
{
	// Token: 0x06000038 RID: 56 RVA: 0x00002BA5 File Offset: 0x00000DA5
	public bool Equals(Edge other)
	{
		return this.Cost.Equals(other.Cost) && this.Connection == other.Connection && this.ConnectedNodeKey == other.ConnectedNodeKey;
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00002BD8 File Offset: 0x00000DD8
	public override bool Equals(object obj)
	{
		if (obj is Edge)
		{
			Edge edge = (Edge)obj;
			return this.Equals(edge);
		}
		return false;
	}

	// Token: 0x0600003A RID: 58 RVA: 0x00002C00 File Offset: 0x00000E00
	public override int GetHashCode()
	{
		int num = (17 * 31 + this.Cost.GetHashCode()) * 31;
		int connection = (int)this.Connection;
		return (num + connection.GetHashCode()) * 31 + this.ConnectedNodeKey.GetHashCode();
	}

	// Token: 0x0400001F RID: 31
	public float Cost;

	// Token: 0x04000020 RID: 32
	public ConnectionKind Connection;

	// Token: 0x04000021 RID: 33
	public int ConnectedNodeKey;
}
