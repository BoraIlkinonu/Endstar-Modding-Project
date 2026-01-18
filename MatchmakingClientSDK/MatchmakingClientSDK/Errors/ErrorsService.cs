using System;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;

namespace MatchmakingClientSDK.Errors
{
	// Token: 0x0200006B RID: 107
	public static class ErrorsService
	{
		// Token: 0x0600040C RID: 1036 RVA: 0x00012318 File Offset: 0x00010518
		public static bool TryGetError(Document document, out string errorMessage, out HttpStatusCode errorStatusCode, out int errorIndex)
		{
			DynamoDBEntry dynamoDBEntry;
			if (document.TryGetValue("error", out dynamoDBEntry))
			{
				Document document2 = dynamoDBEntry.AsDocument();
				DynamoDBEntry dynamoDBEntry2;
				errorMessage = (document2.TryGetValue("message", out dynamoDBEntry2) ? dynamoDBEntry2.AsString() : string.Empty);
				DynamoDBEntry dynamoDBEntry3;
				errorStatusCode = (HttpStatusCode)(document2.TryGetValue("code", out dynamoDBEntry3) ? dynamoDBEntry3.AsInt() : 500);
				DynamoDBEntry dynamoDBEntry4;
				errorIndex = (document2.TryGetValue("index", out dynamoDBEntry4) ? dynamoDBEntry4.AsInt() : (-1));
				return true;
			}
			errorMessage = string.Empty;
			errorStatusCode = HttpStatusCode.InternalServerError;
			errorIndex = -1;
			return false;
		}

		// Token: 0x0400029C RID: 668
		private const string ERROR_LABEL = "error";

		// Token: 0x0400029D RID: 669
		private const string ERROR_CODE_LABEL = "code";

		// Token: 0x0400029E RID: 670
		private const string ERROR_INDEX_LABEL = "index";

		// Token: 0x0400029F RID: 671
		private const string ERROR_MESSAGE_LABEL = "message";
	}
}
