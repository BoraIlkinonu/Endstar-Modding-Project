using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace MatchmakingClientSDK.Groups
{
	// Token: 0x02000064 RID: 100
	public class GroupData
	{
		// Token: 0x060003D3 RID: 979 RVA: 0x00010E2C File Offset: 0x0000F02C
		public string GetStringProperty(string propertyName)
		{
			AttributeValue attributeValue;
			if (this.Attributes.TryGetValue(propertyName, out attributeValue))
			{
				return attributeValue.S;
			}
			return null;
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x00010E54 File Offset: 0x0000F054
		public string GetMatchId()
		{
			AttributeValue attributeValue;
			if (this.Attributes.TryGetValue("matchId", out attributeValue))
			{
				return attributeValue.S;
			}
			return null;
		}

		// Token: 0x04000285 RID: 645
		public string CurrentSequenceNum = string.Empty;

		// Token: 0x04000286 RID: 646
		public Dictionary<string, AttributeValue> Attributes = new Dictionary<string, AttributeValue>();
	}
}
