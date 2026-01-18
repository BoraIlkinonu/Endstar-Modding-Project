using System;
using System.Collections.Generic;
using System.Globalization;
using Assets.UserReporting.Scripts.Plugin;
using Unity.Cloud.UserReporting.Client;
using Unity.Cloud.UserReporting.Plugin.SimpleJson;
using Unity.Screenshots;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Unity.Cloud.UserReporting.Plugin.Version2018_3
{
	// Token: 0x02000027 RID: 39
	public class AsyncUnityUserReportingPlatform : IUserReportingPlatform, ILogListener
	{
		// Token: 0x06000111 RID: 273 RVA: 0x000058C4 File Offset: 0x00003AC4
		public AsyncUnityUserReportingPlatform()
		{
			this.logMessages = new List<AsyncUnityUserReportingPlatform.LogMessage>();
			this.postOperations = new List<AsyncUnityUserReportingPlatform.PostOperation>();
			this.screenshotManager = new ScreenshotManager();
			this.profilerSamplers = new List<AsyncUnityUserReportingPlatform.ProfilerSampler>();
			foreach (KeyValuePair<string, string> keyValuePair in this.GetSamplerNames())
			{
				Sampler sampler = Sampler.Get(keyValuePair.Key);
				if (sampler.isValid)
				{
					Recorder recorder = sampler.GetRecorder();
					recorder.enabled = true;
					AsyncUnityUserReportingPlatform.ProfilerSampler profilerSampler = default(AsyncUnityUserReportingPlatform.ProfilerSampler);
					profilerSampler.Name = keyValuePair.Value;
					profilerSampler.Recorder = recorder;
					this.profilerSamplers.Add(profilerSampler);
				}
			}
			LogDispatcher.Register(this);
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00005998 File Offset: 0x00003B98
		public T DeserializeJson<T>(string json)
		{
			return SimpleJson.DeserializeObject<T>(json);
		}

		// Token: 0x06000113 RID: 275 RVA: 0x000059A0 File Offset: 0x00003BA0
		public void OnEndOfFrame(UserReportingClient client)
		{
			this.screenshotManager.OnEndOfFrame();
		}

		// Token: 0x06000114 RID: 276 RVA: 0x000059B0 File Offset: 0x00003BB0
		public void Post(string endpoint, string contentType, byte[] content, Action<float, float> progressCallback, Action<bool, byte[]> callback)
		{
			UnityWebRequest unityWebRequest = new UnityWebRequest(endpoint, "POST");
			unityWebRequest.uploadHandler = new UploadHandlerRaw(content);
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			unityWebRequest.SetRequestHeader("Content-Type", contentType);
			unityWebRequest.SendWebRequest();
			AsyncUnityUserReportingPlatform.PostOperation postOperation = new AsyncUnityUserReportingPlatform.PostOperation();
			postOperation.WebRequest = unityWebRequest;
			postOperation.Callback = callback;
			postOperation.ProgressCallback = progressCallback;
			this.postOperations.Add(postOperation);
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00005A1C File Offset: 0x00003C1C
		public void ReceiveLogMessage(string logString, string stackTrace, LogType logType)
		{
			List<AsyncUnityUserReportingPlatform.LogMessage> list = this.logMessages;
			lock (list)
			{
				AsyncUnityUserReportingPlatform.LogMessage logMessage = default(AsyncUnityUserReportingPlatform.LogMessage);
				logMessage.LogString = logString;
				logMessage.StackTrace = stackTrace;
				logMessage.LogType = logType;
				this.logMessages.Add(logMessage);
			}
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00005A84 File Offset: 0x00003C84
		public void RunTask(Func<object> task, Action<object> callback)
		{
			callback(task());
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00005A92 File Offset: 0x00003C92
		public void SendAnalyticsEvent(string eventName, Dictionary<string, object> eventData)
		{
			Analytics.CustomEvent(eventName, eventData);
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00005A9C File Offset: 0x00003C9C
		public string SerializeJson(object instance)
		{
			return SimpleJson.SerializeObject(instance);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00005AA4 File Offset: 0x00003CA4
		public void TakeScreenshot(int frameNumber, int maximumWidth, int maximumHeight, object source, Action<int, byte[]> callback)
		{
			this.screenshotManager.TakeScreenshot(source, frameNumber, maximumWidth, maximumHeight, callback);
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00005AB8 File Offset: 0x00003CB8
		public void Update(UserReportingClient client)
		{
			List<AsyncUnityUserReportingPlatform.LogMessage> list = this.logMessages;
			lock (list)
			{
				foreach (AsyncUnityUserReportingPlatform.LogMessage logMessage in this.logMessages)
				{
					UserReportEventLevel userReportEventLevel = UserReportEventLevel.Info;
					if (logMessage.LogType == LogType.Warning)
					{
						userReportEventLevel = UserReportEventLevel.Warning;
					}
					else if (logMessage.LogType == LogType.Error)
					{
						userReportEventLevel = UserReportEventLevel.Error;
					}
					else if (logMessage.LogType == LogType.Exception)
					{
						userReportEventLevel = UserReportEventLevel.Error;
					}
					else if (logMessage.LogType == LogType.Assert)
					{
						userReportEventLevel = UserReportEventLevel.Error;
					}
					if (client.IsConnectedToLogger)
					{
						client.LogEvent(userReportEventLevel, logMessage.LogString, logMessage.StackTrace);
					}
				}
				this.logMessages.Clear();
			}
			if (client.Configuration.MetricsGatheringMode == MetricsGatheringMode.Automatic)
			{
				this.SampleAutomaticMetrics(client);
				foreach (AsyncUnityUserReportingPlatform.ProfilerSampler profilerSampler in this.profilerSamplers)
				{
					client.SampleMetric(profilerSampler.Name, profilerSampler.GetValue());
				}
			}
			int i = 0;
			while (i < this.postOperations.Count)
			{
				AsyncUnityUserReportingPlatform.PostOperation postOperation = this.postOperations[i];
				if (postOperation.WebRequest.isDone)
				{
					bool flag2 = postOperation.WebRequest.error != null && postOperation.WebRequest.responseCode != 200L;
					if (flag2)
					{
						string text = string.Format("UnityUserReportingPlatform.Post: {0} {1}", postOperation.WebRequest.responseCode, postOperation.WebRequest.error);
						Debug.Log(text);
						client.LogEvent(UserReportEventLevel.Error, text);
					}
					postOperation.ProgressCallback(1f, 1f);
					postOperation.Callback(!flag2, postOperation.WebRequest.downloadHandler.data);
					this.postOperations.Remove(postOperation);
				}
				else
				{
					postOperation.ProgressCallback(postOperation.WebRequest.uploadProgress, postOperation.WebRequest.downloadProgress);
					i++;
				}
			}
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00005D08 File Offset: 0x00003F08
		public virtual IDictionary<string, string> GetDeviceMetadata()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("BuildGUID", Application.buildGUID);
			dictionary.Add("DeviceModel", SystemInfo.deviceModel);
			dictionary.Add("DeviceType", SystemInfo.deviceType.ToString());
			dictionary.Add("DPI", Screen.dpi.ToString(CultureInfo.InvariantCulture));
			dictionary.Add("GraphicsDeviceName", SystemInfo.graphicsDeviceName);
			dictionary.Add("GraphicsDeviceType", SystemInfo.graphicsDeviceType.ToString());
			dictionary.Add("GraphicsDeviceVendor", SystemInfo.graphicsDeviceVendor);
			dictionary.Add("GraphicsDeviceVersion", SystemInfo.graphicsDeviceVersion);
			dictionary.Add("GraphicsMemorySize", SystemInfo.graphicsMemorySize.ToString());
			dictionary.Add("InstallerName", Application.installerName);
			dictionary.Add("InstallMode", Application.installMode.ToString());
			dictionary.Add("IsEditor", Application.isEditor.ToString());
			dictionary.Add("IsFullScreen", Screen.fullScreen.ToString());
			dictionary.Add("OperatingSystem", SystemInfo.operatingSystem);
			dictionary.Add("OperatingSystemFamily", SystemInfo.operatingSystemFamily.ToString());
			dictionary.Add("Orientation", Screen.orientation.ToString());
			dictionary.Add("Platform", Application.platform.ToString());
			try
			{
				dictionary.Add("QualityLevel", QualitySettings.names[QualitySettings.GetQualityLevel()]);
			}
			catch
			{
			}
			dictionary.Add("ResolutionWidth", Screen.currentResolution.width.ToString());
			dictionary.Add("ResolutionHeight", Screen.currentResolution.height.ToString());
			dictionary.Add("ResolutionRefreshRate", Screen.currentResolution.refreshRate.ToString());
			dictionary.Add("SystemLanguage", Application.systemLanguage.ToString());
			dictionary.Add("SystemMemorySize", SystemInfo.systemMemorySize.ToString());
			dictionary.Add("TargetFrameRate", Application.targetFrameRate.ToString());
			dictionary.Add("UnityVersion", Application.unityVersion);
			dictionary.Add("Version", Application.version);
			dictionary.Add("Source", "Unity");
			Type type = base.GetType();
			dictionary.Add("IUserReportingPlatform", type.Name);
			return dictionary;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00005FD8 File Offset: 0x000041D8
		public virtual Dictionary<string, string> GetSamplerNames()
		{
			return new Dictionary<string, string>
			{
				{ "AudioManager.FixedUpdate", "AudioManager.FixedUpdateInMilliseconds" },
				{ "AudioManager.Update", "AudioManager.UpdateInMilliseconds" },
				{ "LateBehaviourUpdate", "Behaviors.LateUpdateInMilliseconds" },
				{ "BehaviourUpdate", "Behaviors.UpdateInMilliseconds" },
				{ "Camera.Render", "Camera.RenderInMilliseconds" },
				{ "Overhead", "Engine.OverheadInMilliseconds" },
				{ "WaitForRenderJobs", "Engine.WaitForRenderJobsInMilliseconds" },
				{ "WaitForTargetFPS", "Engine.WaitForTargetFPSInMilliseconds" },
				{ "GUI.Repaint", "GUI.RepaintInMilliseconds" },
				{ "Network.Update", "Network.UpdateInMilliseconds" },
				{ "ParticleSystem.EndUpdateAll", "ParticleSystem.EndUpdateAllInMilliseconds" },
				{ "ParticleSystem.Update", "ParticleSystem.UpdateInMilliseconds" },
				{ "Physics.FetchResults", "Physics.FetchResultsInMilliseconds" },
				{ "Physics.Processing", "Physics.ProcessingInMilliseconds" },
				{ "Physics.ProcessReports", "Physics.ProcessReportsInMilliseconds" },
				{ "Physics.Simulate", "Physics.SimulateInMilliseconds" },
				{ "Physics.UpdateBodies", "Physics.UpdateBodiesInMilliseconds" },
				{ "Physics.Interpolation", "Physics.InterpolationInMilliseconds" },
				{ "Physics2D.DynamicUpdate", "Physics2D.DynamicUpdateInMilliseconds" },
				{ "Physics2D.FixedUpdate", "Physics2D.FixedUpdateInMilliseconds" }
			};
		}

		// Token: 0x0600011D RID: 285 RVA: 0x0000612C File Offset: 0x0000432C
		public virtual void ModifyUserReport(UserReport userReport)
		{
			Scene activeScene = SceneManager.GetActiveScene();
			userReport.DeviceMetadata.Add(new UserReportNamedValue("ActiveSceneName", activeScene.name));
			Camera main = Camera.main;
			if (main != null)
			{
				userReport.DeviceMetadata.Add(new UserReportNamedValue("MainCameraName", main.name));
				userReport.DeviceMetadata.Add(new UserReportNamedValue("MainCameraPosition", main.transform.position.ToString()));
				userReport.DeviceMetadata.Add(new UserReportNamedValue("MainCameraForward", main.transform.forward.ToString()));
				RaycastHit raycastHit;
				if (Physics.Raycast(main.transform.position, main.transform.forward, out raycastHit))
				{
					GameObject gameObject = raycastHit.transform.gameObject;
					userReport.DeviceMetadata.Add(new UserReportNamedValue("LookingAt", raycastHit.point.ToString()));
					userReport.DeviceMetadata.Add(new UserReportNamedValue("LookingAtGameObject", gameObject.name));
					userReport.DeviceMetadata.Add(new UserReportNamedValue("LookingAtGameObjectPosition", gameObject.transform.position.ToString()));
				}
			}
		}

		// Token: 0x0600011E RID: 286 RVA: 0x0000628C File Offset: 0x0000448C
		public virtual void SampleAutomaticMetrics(UserReportingClient client)
		{
			client.SampleMetric("Graphics.FramesPerSecond", (double)(1f / Time.deltaTime));
			client.SampleMetric("Memory.MonoUsedSizeInBytes", (double)Profiler.GetMonoUsedSizeLong());
			client.SampleMetric("Memory.TotalAllocatedMemoryInBytes", (double)Profiler.GetTotalAllocatedMemoryLong());
			client.SampleMetric("Memory.TotalReservedMemoryInBytes", (double)Profiler.GetTotalReservedMemoryLong());
			client.SampleMetric("Memory.TotalUnusedReservedMemoryInBytes", (double)Profiler.GetTotalUnusedReservedMemoryLong());
			client.SampleMetric("Battery.BatteryLevelInPercent", (double)SystemInfo.batteryLevel);
		}

		// Token: 0x0400008C RID: 140
		private List<AsyncUnityUserReportingPlatform.LogMessage> logMessages;

		// Token: 0x0400008D RID: 141
		private List<AsyncUnityUserReportingPlatform.PostOperation> postOperations;

		// Token: 0x0400008E RID: 142
		private List<AsyncUnityUserReportingPlatform.ProfilerSampler> profilerSamplers;

		// Token: 0x0400008F RID: 143
		private ScreenshotManager screenshotManager;

		// Token: 0x04000090 RID: 144
		private List<AsyncUnityUserReportingPlatform.PostOperation> taskOperations;

		// Token: 0x02000044 RID: 68
		private struct LogMessage
		{
			// Token: 0x04000113 RID: 275
			public string LogString;

			// Token: 0x04000114 RID: 276
			public LogType LogType;

			// Token: 0x04000115 RID: 277
			public string StackTrace;
		}

		// Token: 0x02000045 RID: 69
		private class PostOperation
		{
			// Token: 0x17000088 RID: 136
			// (get) Token: 0x06000235 RID: 565 RVA: 0x000095BD File Offset: 0x000077BD
			// (set) Token: 0x06000236 RID: 566 RVA: 0x000095C5 File Offset: 0x000077C5
			public Action<bool, byte[]> Callback { get; set; }

			// Token: 0x17000089 RID: 137
			// (get) Token: 0x06000237 RID: 567 RVA: 0x000095CE File Offset: 0x000077CE
			// (set) Token: 0x06000238 RID: 568 RVA: 0x000095D6 File Offset: 0x000077D6
			public Action<float, float> ProgressCallback { get; set; }

			// Token: 0x1700008A RID: 138
			// (get) Token: 0x06000239 RID: 569 RVA: 0x000095DF File Offset: 0x000077DF
			// (set) Token: 0x0600023A RID: 570 RVA: 0x000095E7 File Offset: 0x000077E7
			public UnityWebRequest WebRequest { get; set; }
		}

		// Token: 0x02000046 RID: 70
		private struct ProfilerSampler
		{
			// Token: 0x0600023C RID: 572 RVA: 0x000095F8 File Offset: 0x000077F8
			public double GetValue()
			{
				if (this.Recorder == null)
				{
					return 0.0;
				}
				return (double)this.Recorder.elapsedNanoseconds / 1000000.0;
			}

			// Token: 0x04000119 RID: 281
			public string Name;

			// Token: 0x0400011A RID: 282
			public Recorder Recorder;
		}
	}
}
