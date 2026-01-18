namespace Endless.Creator.UI;

public readonly struct UIVersion
{
	public readonly string Version;

	public string UserFacingVersion
	{
		get
		{
			if (!VersionUtilities.SkipExtractTrailingSegment(Version))
			{
				return VersionUtilities.ExtractTrailingSegment(Version);
			}
			return Version;
		}
	}

	public UIVersion(string version)
	{
		Version = version;
	}

	public override string ToString()
	{
		return "{ Version: " + Version + ", UserFacingVersion: " + UserFacingVersion + " }";
	}
}
