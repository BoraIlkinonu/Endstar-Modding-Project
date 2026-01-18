using System;

namespace Endless.Networking
{
	// Token: 0x02000005 RID: 5
	public readonly struct CoreClientData
	{
		// Token: 0x0600000E RID: 14 RVA: 0x00002238 File Offset: 0x00000438
		public CoreClientData(string platformId, TargetPlatforms platform)
		{
			this.PlatformId = platformId;
			this.Platform = platform;
			this.hashCode = platformId.GetHashCode() | (int)platform;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002258 File Offset: 0x00000458
		public override bool Equals(object obj)
		{
			CoreClientData coreClientData;
			bool flag;
			if (obj is CoreClientData)
			{
				coreClientData = (CoreClientData)obj;
				flag = true;
			}
			else
			{
				flag = false;
			}
			bool flag2 = flag;
			return flag2 && coreClientData == this;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002294 File Offset: 0x00000494
		public static bool operator ==(CoreClientData first, CoreClientData other)
		{
			return first.PlatformId == other.PlatformId && first.Platform == other.Platform;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000022CC File Offset: 0x000004CC
		public static bool operator !=(CoreClientData first, CoreClientData other)
		{
			return first.PlatformId != other.PlatformId || first.Platform != other.Platform;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002308 File Offset: 0x00000508
		public override int GetHashCode()
		{
			return this.hashCode;
		}

		// Token: 0x04000005 RID: 5
		public readonly string PlatformId;

		// Token: 0x04000006 RID: 6
		public readonly TargetPlatforms Platform;

		// Token: 0x04000007 RID: 7
		private readonly int hashCode;
	}
}
