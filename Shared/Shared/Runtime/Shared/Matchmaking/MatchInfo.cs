using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Endless.Networking;
using MatchmakingAPI.Matches;
using MatchmakingClientSDK;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000017 RID: 23
	public class MatchInfo
	{
		// Token: 0x060000BA RID: 186 RVA: 0x000056CE File Offset: 0x000038CE
		public MatchInfo(IMatchmakingClient client)
		{
			this.client = client;
		}

		// Token: 0x060000BB RID: 187 RVA: 0x000056DD File Offset: 0x000038DD
		public string GetGameId()
		{
			return this.client.MatchesService.GetStringProperty("gameId");
		}

		// Token: 0x060000BC RID: 188 RVA: 0x000056F4 File Offset: 0x000038F4
		public string GetLevelId()
		{
			return this.client.MatchesService.GetStringProperty("levelId");
		}

		// Token: 0x060000BD RID: 189 RVA: 0x0000570C File Offset: 0x0000390C
		public MatchData GetMatchData()
		{
			ReadOnlyDictionary<string, AttributeValue> localAttributes = this.client.MatchesService.GetLocalAttributes();
			bool @bool = localAttributes["isEditSession"].BOOL;
			string text = null;
			ServerInstance serverInstance = null;
			AttributeValue attributeValue;
			if (localAttributes.TryGetValue("allocationData", out attributeValue))
			{
				Document document = Document.FromJson(attributeValue.S);
				text = document["key"].AsString();
				serverInstance = new ServerInstance
				{
					InstanceIp = document["publicIp"].AsString(),
					InstanceName = document["name"].AsString(),
					InstancePort = document["port"].AsInt(),
					Latency = 0L
				};
			}
			AttributeValue attributeValue2;
			AttributeValue attributeValue3;
			return new MatchData
			{
				MatchId = this.Id,
				MatchHost = new ClientData(this.Host, TargetPlatforms.Endless, "[groupName]"),
				MatchServerType = this.ServerType,
				MatchAuthKey = text,
				ServerInstance = serverInstance,
				IsEditSession = @bool,
				ProjectId = localAttributes["gameId"].S,
				LevelId = localAttributes["levelId"].S,
				Version = (localAttributes.TryGetValue("version", out attributeValue2) ? attributeValue2.S : string.Empty),
				CustomData = (localAttributes.TryGetValue("matchData", out attributeValue3) ? attributeValue3.S : string.Empty)
			};
		}

		// Token: 0x0400003E RID: 62
		private IMatchmakingClient client;

		// Token: 0x0400003F RID: 63
		public string Id;

		// Token: 0x04000040 RID: 64
		public List<string> Members;

		// Token: 0x04000041 RID: 65
		public string Host;

		// Token: 0x04000042 RID: 66
		public MatchServerTypes ServerType;
	}
}
