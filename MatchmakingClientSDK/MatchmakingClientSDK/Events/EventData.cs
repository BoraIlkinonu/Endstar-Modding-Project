using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Amazon.DynamoDBv2.Model;

namespace MatchmakingClientSDK.Events
{
	// Token: 0x02000066 RID: 102
	public class EventData
	{
		// Token: 0x06000401 RID: 1025 RVA: 0x00012227 File Offset: 0x00010427
		public EventData(string eventType, string sequenceNum, Dictionary<string, AttributeValue> old, Dictionary<string, AttributeValue> @new)
		{
			this.SequenceNum = sequenceNum;
			this.EventType = eventType;
			this.Old = new ReadOnlyDictionary<string, AttributeValue>(old);
			this.New = new ReadOnlyDictionary<string, AttributeValue>(@new);
		}

		// Token: 0x04000292 RID: 658
		public readonly string EventType;

		// Token: 0x04000293 RID: 659
		public readonly string SequenceNum;

		// Token: 0x04000294 RID: 660
		public readonly ReadOnlyDictionary<string, AttributeValue> Old;

		// Token: 0x04000295 RID: 661
		public readonly ReadOnlyDictionary<string, AttributeValue> New;

		// Token: 0x04000296 RID: 662
		public readonly DateTime ReceivedAt = DateTime.Now;
	}
}
