using System;
using System.Collections.Generic;

namespace Unity.Cloud.UserReporting.Client
{
	// Token: 0x02000030 RID: 48
	public class NullUserReportingPlatform : IUserReportingPlatform
	{
		// Token: 0x0600018C RID: 396 RVA: 0x000081BC File Offset: 0x000063BC
		public T DeserializeJson<T>(string json)
		{
			return default(T);
		}

		// Token: 0x0600018D RID: 397 RVA: 0x000081D2 File Offset: 0x000063D2
		public IDictionary<string, string> GetDeviceMetadata()
		{
			return new Dictionary<string, string>();
		}

		// Token: 0x0600018E RID: 398 RVA: 0x000081D9 File Offset: 0x000063D9
		public void ModifyUserReport(UserReport userReport)
		{
		}

		// Token: 0x0600018F RID: 399 RVA: 0x000081DB File Offset: 0x000063DB
		public void OnEndOfFrame(UserReportingClient client)
		{
		}

		// Token: 0x06000190 RID: 400 RVA: 0x000081DD File Offset: 0x000063DD
		public void Post(string endpoint, string contentType, byte[] content, Action<float, float> progressCallback, Action<bool, byte[]> callback)
		{
			progressCallback(1f, 1f);
			callback(true, content);
		}

		// Token: 0x06000191 RID: 401 RVA: 0x000081F9 File Offset: 0x000063F9
		public void RunTask(Func<object> task, Action<object> callback)
		{
			callback(task());
		}

		// Token: 0x06000192 RID: 402 RVA: 0x00008207 File Offset: 0x00006407
		public void SendAnalyticsEvent(string eventName, Dictionary<string, object> eventData)
		{
		}

		// Token: 0x06000193 RID: 403 RVA: 0x00008209 File Offset: 0x00006409
		public string SerializeJson(object instance)
		{
			return string.Empty;
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00008210 File Offset: 0x00006410
		public void TakeScreenshot(int frameNumber, int maximumWidth, int maximumHeight, object source, Action<int, byte[]> callback)
		{
			callback(frameNumber, new byte[0]);
		}

		// Token: 0x06000195 RID: 405 RVA: 0x00008220 File Offset: 0x00006420
		public void Update(UserReportingClient client)
		{
		}
	}
}
