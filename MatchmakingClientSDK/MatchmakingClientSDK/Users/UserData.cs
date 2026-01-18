using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace MatchmakingClientSDK.Users
{
	// Token: 0x02000060 RID: 96
	public class UserData
	{
		// Token: 0x060003AC RID: 940 RVA: 0x0001027C File Offset: 0x0000E47C
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

		// Token: 0x04000269 RID: 617
		public string CurrentSequenceNum = string.Empty;

		// Token: 0x0400026A RID: 618
		public Dictionary<string, AttributeValue> Attributes = new Dictionary<string, AttributeValue>();
	}
}
