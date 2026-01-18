using System;

// Token: 0x0200000D RID: 13
public readonly struct Connection : IEquatable<Connection>
{
	// Token: 0x06000034 RID: 52 RVA: 0x00002AFF File Offset: 0x00000CFF
	public Connection(int startSectionKey, int endSection)
	{
		this.StartSectionKey = startSectionKey;
		this.EndSectionKey = endSection;
	}

	// Token: 0x06000035 RID: 53 RVA: 0x00002B10 File Offset: 0x00000D10
	public override int GetHashCode()
	{
		return (17 * 31 + this.StartSectionKey.GetHashCode()) * 31 + this.EndSectionKey.GetHashCode();
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00002B44 File Offset: 0x00000D44
	public bool Equals(Connection other)
	{
		return this.StartSectionKey.Equals(other.StartSectionKey) && this.EndSectionKey.Equals(other.EndSectionKey);
	}

	// Token: 0x06000037 RID: 55 RVA: 0x00002B80 File Offset: 0x00000D80
	public override bool Equals(object obj)
	{
		if (obj is Connection)
		{
			Connection connection = (Connection)obj;
			return this.Equals(connection);
		}
		return false;
	}

	// Token: 0x0400001D RID: 29
	public readonly int StartSectionKey;

	// Token: 0x0400001E RID: 30
	public readonly int EndSectionKey;
}
