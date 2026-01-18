using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace MatchmakingClientSDK
{
	// Token: 0x02000054 RID: 84
	public class MatchData
	{
		// Token: 0x06000302 RID: 770 RVA: 0x0000D29C File Offset: 0x0000B49C
		public string GetStringProperty(string propertyName)
		{
			if (this.Attributes == null)
			{
				return null;
			}
			AttributeValue attributeValue;
			if (this.Attributes.TryGetValue(propertyName, out attributeValue))
			{
				return attributeValue.S;
			}
			return null;
		}

		// Token: 0x040001C4 RID: 452
		public string CurrentSequenceNum = string.Empty;

		// Token: 0x040001C5 RID: 453
		public Dictionary<string, AttributeValue> Attributes = new Dictionary<string, AttributeValue>();
	}
}
