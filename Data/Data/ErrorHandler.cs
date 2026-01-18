using System;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.UI;
using Runtime.Shared;
using Unity.Services.Analytics;
using UnityEngine;

namespace Endless.Data
{
	// Token: 0x02000005 RID: 5
	public static class ErrorHandler
	{
		// Token: 0x0600000B RID: 11 RVA: 0x000022F8 File Offset: 0x000004F8
		public static void HandleError(ErrorCodes errorCode, Exception exception, bool displayModal = true, bool leaveMatch = false)
		{
			Debug.LogException(new Exception(string.Format("Error code: {0}", (int)errorCode), exception));
			bool flag = exception is TimeoutException;
			CustomEvent customEvent = new CustomEvent("errorHandlerError")
			{
				{
					"errorCode",
					(int)errorCode
				},
				{ "isTimeout", flag }
			};
			AnalyticsService.Instance.RecordEvent(customEvent);
			if (leaveMatch && MatchmakingClientController.Instance.LocalMatch != null)
			{
				MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch(null);
			}
			if (displayModal)
			{
				string text = (flag ? " - Timeout" : string.Empty);
				string text2 = "<size=80%><color=grey>Error Code: {0}{1}</size></color>\n{2}";
				object obj = (int)errorCode;
				object obj2 = text;
				UserFacingTextAttribute attributeOfType = errorCode.GetAttributeOfType<UserFacingTextAttribute>();
				string text3 = string.Format(text2, obj, obj2, ((attributeOfType != null) ? attributeOfType.UserFacingText : null) ?? "Unknown Error");
				MonoBehaviourSingleton<UIModalManager>.Instance.DisplayErrorModal(text3);
			}
		}
	}
}
