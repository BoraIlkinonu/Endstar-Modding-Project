using System;

public readonly struct Connection : IEquatable<Connection>
{
	public readonly int StartSectionKey;

	public readonly int EndSectionKey;

	public Connection(int startSectionKey, int endSection)
	{
		StartSectionKey = startSectionKey;
		EndSectionKey = endSection;
	}

	public override int GetHashCode()
	{
		int num = 17 * 31;
		int startSectionKey = StartSectionKey;
		int num2 = (num + startSectionKey.GetHashCode()) * 31;
		startSectionKey = EndSectionKey;
		return num2 + startSectionKey.GetHashCode();
	}

	public bool Equals(Connection other)
	{
		int startSectionKey = StartSectionKey;
		if (startSectionKey.Equals(other.StartSectionKey))
		{
			startSectionKey = EndSectionKey;
			return startSectionKey.Equals(other.EndSectionKey);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is Connection other)
		{
			return Equals(other);
		}
		return false;
	}
}
