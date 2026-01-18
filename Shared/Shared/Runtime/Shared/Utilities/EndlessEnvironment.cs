using System;
using System.IO;
using Endless.Matchmaking;
using UnityEngine.Device;

namespace Runtime.Shared.Utilities
{
	// Token: 0x0200000A RID: 10
	public static class EndlessEnvironment
	{
		// Token: 0x06000053 RID: 83 RVA: 0x00004558 File Offset: 0x00002758
		public static string GetSafeSavePath()
		{
			string text = Application.persistentDataPath;
			switch (MatchmakingClientController.Instance.NetworkEnvironment)
			{
			case NetworkEnvironment.DEV:
				text = EndlessEnvironment.ModifyPathForBuildType(text, "DEV");
				break;
			case NetworkEnvironment.STAGING:
				text = EndlessEnvironment.ModifyPathForBuildType(text, "STAGING");
				break;
			case NetworkEnvironment.PROD:
				break;
			default:
				throw new ArgumentOutOfRangeException("NetworkEnvironment");
			}
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x000045C1 File Offset: 0x000027C1
		private static string ModifyPathForBuildType(string currentPath, string environment)
		{
			return Path.Combine(currentPath, "STANDALONE", environment);
		}
	}
}
