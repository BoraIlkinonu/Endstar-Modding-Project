using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Endless.Creator
{
	// Token: 0x02000095 RID: 149
	public static class VersionUtilities
	{
		// Token: 0x06000248 RID: 584 RVA: 0x00010A4A File Offset: 0x0000EC4A
		public static string[] GetParsedAndOrderedVersions(object result)
		{
			return ((JArray)result).ToObject<string[]>().OrderByDescending(new Func<string, Version>(Version.Parse)).ToArray<string>();
		}

		// Token: 0x06000249 RID: 585 RVA: 0x00010A70 File Offset: 0x0000EC70
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

		// Token: 0x0600024A RID: 586 RVA: 0x00010AB2 File Offset: 0x0000ECB2
		public static bool SkipExtractTrailingSegment(string version)
		{
			return VersionUtilities.extractTrailingSegmentExempted.Contains(version.ToLower());
		}

		// Token: 0x040002A2 RID: 674
		private static readonly HashSet<string> extractTrailingSegmentExempted = new HashSet<string> { "none", "unpublished" };
	}
}
