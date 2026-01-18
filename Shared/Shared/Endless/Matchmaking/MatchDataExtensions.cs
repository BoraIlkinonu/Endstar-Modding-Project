using System;
using Endless.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Matchmaking
{
	// Token: 0x02000038 RID: 56
	public static class MatchDataExtensions
	{
		// Token: 0x06000182 RID: 386 RVA: 0x00009938 File Offset: 0x00007B38
		public static bool IsPlaytest(this MatchData matchData)
		{
			if (!string.IsNullOrEmpty(matchData.CustomData))
			{
				JToken value = JsonConvert.DeserializeObject<JObject>(matchData.CustomData).GetValue("PlayTest");
				if (value != null)
				{
					return value.Value<bool>();
				}
			}
			return false;
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00009974 File Offset: 0x00007B74
		public static bool IsAdminSession(this MatchData matchData)
		{
			if (!string.IsNullOrEmpty(matchData.CustomData))
			{
				JToken value = JsonConvert.DeserializeObject<JObject>(matchData.CustomData).GetValue("isAdmin");
				if (value != null)
				{
					return value.Value<bool>();
				}
			}
			return false;
		}

		// Token: 0x06000184 RID: 388 RVA: 0x000099AF File Offset: 0x00007BAF
		public static string BuildAdminSessionData(this MatchData matchData)
		{
			return MatchDataExtensions.BuildSessionData(new MatchData?(matchData), "isAdmin");
		}

		// Token: 0x06000185 RID: 389 RVA: 0x000099C1 File Offset: 0x00007BC1
		public static string BuildPlaytestSessionData(this MatchData matchData)
		{
			return MatchDataExtensions.BuildSessionData(new MatchData?(matchData), "PlayTest");
		}

		// Token: 0x06000186 RID: 390 RVA: 0x000099D4 File Offset: 0x00007BD4
		public static string BuildAdminSessionData()
		{
			return MatchDataExtensions.BuildSessionData(null, "isAdmin");
		}

		// Token: 0x06000187 RID: 391 RVA: 0x000099F4 File Offset: 0x00007BF4
		public static string BuildPlaytestSessionData()
		{
			return MatchDataExtensions.BuildSessionData(null, "PlayTest");
		}

		// Token: 0x06000188 RID: 392 RVA: 0x00009A14 File Offset: 0x00007C14
		private static string BuildSessionData(MatchData? matchData, string field)
		{
			JObject jobject;
			if (matchData == null || matchData.Value.CustomData.IsNullOrEmptyOrWhiteSpace())
			{
				jobject = new JObject();
			}
			else
			{
				try
				{
					jobject = JObject.Parse(matchData.Value.CustomData);
				}
				catch (JsonException ex)
				{
					Debug.LogError("Failed to parse existing custom data, creating new object: " + ex.Message);
					jobject = new JObject();
				}
			}
			jobject[field] = true;
			return jobject.ToString(Formatting.None, Array.Empty<JsonConverter>());
		}

		// Token: 0x040000E3 RID: 227
		private const string PLAY_TEST_STRING = "PlayTest";

		// Token: 0x040000E4 RID: 228
		private const string ADMIN_SESSION_STRING = "isAdmin";
	}
}
