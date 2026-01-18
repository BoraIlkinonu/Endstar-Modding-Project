using System;
using System.Collections.Generic;

namespace Unity.Cloud.UserReporting.Client
{
	// Token: 0x0200002E RID: 46
	public interface IUserReportingPlatform
	{
		// Token: 0x06000182 RID: 386
		T DeserializeJson<T>(string json);

		// Token: 0x06000183 RID: 387
		IDictionary<string, string> GetDeviceMetadata();

		// Token: 0x06000184 RID: 388
		void ModifyUserReport(UserReport userReport);

		// Token: 0x06000185 RID: 389
		void OnEndOfFrame(UserReportingClient client);

		// Token: 0x06000186 RID: 390
		void Post(string endpoint, string contentType, byte[] content, Action<float, float> progressCallback, Action<bool, byte[]> callback);

		// Token: 0x06000187 RID: 391
		void RunTask(Func<object> task, Action<object> callback);

		// Token: 0x06000188 RID: 392
		void SendAnalyticsEvent(string eventName, Dictionary<string, object> eventData);

		// Token: 0x06000189 RID: 393
		string SerializeJson(object instance);

		// Token: 0x0600018A RID: 394
		void TakeScreenshot(int frameNumber, int maximumWidth, int maximumHeight, object source, Action<int, byte[]> callback);

		// Token: 0x0600018B RID: 395
		void Update(UserReportingClient client);
	}
}
