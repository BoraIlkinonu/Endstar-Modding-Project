using System;
using Endless.Networking;
using Endless.Shared.Debugging;

namespace Endless.Shared
{
	// Token: 0x02000059 RID: 89
	public static class ClientDataExtensions
	{
		// Token: 0x060002CC RID: 716 RVA: 0x0000E111 File Offset: 0x0000C311
		public static bool CoreDataEquals(this ClientData clientData, ClientData target)
		{
			return clientData.CoreData == target.CoreData;
		}

		// Token: 0x060002CD RID: 717 RVA: 0x0000E124 File Offset: 0x0000C324
		public static bool CoreDataEquals(this ClientData clientData, CoreClientData target)
		{
			return clientData.CoreData == target;
		}

		// Token: 0x060002CE RID: 718 RVA: 0x0000E132 File Offset: 0x0000C332
		public static int PlatformIdToEndlessUserId(this ClientData clientData)
		{
			return clientData.CoreData.PlatformIdToEndlessUserId();
		}

		// Token: 0x060002CF RID: 719 RVA: 0x0000E140 File Offset: 0x0000C340
		public static int PlatformIdToEndlessUserId(this CoreClientData coreData)
		{
			int num;
			if (int.TryParse(coreData.PlatformId, out num))
			{
				return num;
			}
			DebugUtility.LogException(new Exception("PlatformIdToEndlessUserId failed to parse an id from the CoreClientData: " + coreData.ToPrettyString() + "!"), null);
			return num;
		}

		// Token: 0x060002D0 RID: 720 RVA: 0x0000E180 File Offset: 0x0000C380
		public static string ToPrettyString(this ClientData clientData)
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5} }}", new object[]
			{
				"DisplayName",
				clientData.DisplayName,
				"PlatformId",
				clientData.CoreData.PlatformId,
				"Platform",
				clientData.CoreData.Platform
			});
		}

		// Token: 0x060002D1 RID: 721 RVA: 0x0000E1DF File Offset: 0x0000C3DF
		public static string ToPrettyString(this CoreClientData coreData)
		{
			return string.Format("{{ {0}: {1}, {2}: {3} }}", new object[] { "PlatformId", coreData.PlatformId, "Platform", coreData.Platform });
		}
	}
}
