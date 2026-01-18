using System;

namespace Endless.Creator.UI
{
	// Token: 0x020002C7 RID: 711
	public readonly struct UIVersion
	{
		// Token: 0x1700018E RID: 398
		// (get) Token: 0x06000C07 RID: 3079 RVA: 0x00039AC9 File Offset: 0x00037CC9
		public string UserFacingVersion
		{
			get
			{
				if (!VersionUtilities.SkipExtractTrailingSegment(this.Version))
				{
					return VersionUtilities.ExtractTrailingSegment(this.Version);
				}
				return this.Version;
			}
		}

		// Token: 0x06000C08 RID: 3080 RVA: 0x00039AEA File Offset: 0x00037CEA
		public UIVersion(string version)
		{
			this.Version = version;
		}

		// Token: 0x06000C09 RID: 3081 RVA: 0x00039AF3 File Offset: 0x00037CF3
		public override string ToString()
		{
			return string.Concat(new string[] { "{ Version: ", this.Version, ", UserFacingVersion: ", this.UserFacingVersion, " }" });
		}

		// Token: 0x04000A63 RID: 2659
		public readonly string Version;
	}
}
