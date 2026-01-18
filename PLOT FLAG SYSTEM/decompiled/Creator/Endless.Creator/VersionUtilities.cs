using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Endless.Creator;

public static class VersionUtilities
{
	private static readonly HashSet<string> extractTrailingSegmentExempted = new HashSet<string> { "none", "unpublished" };

	public static string[] GetParsedAndOrderedVersions(object result)
	{
		return ((JArray)result).ToObject<string[]>().OrderByDescending(Version.Parse).ToArray();
	}

	public static string ExtractTrailingSegment(string version)
	{
		if (string.IsNullOrEmpty(version))
		{
			return string.Empty;
		}
		int num = version.LastIndexOf('.');
		if (num >= 0 && num + 1 < version.Length)
		{
			return version.Substring(num + 1);
		}
		return string.Empty;
	}

	public static bool SkipExtractTrailingSegment(string version)
	{
		return extractTrailingSegmentExempted.Contains(version.ToLower());
	}
}
