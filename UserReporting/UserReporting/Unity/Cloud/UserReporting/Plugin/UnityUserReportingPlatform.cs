using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Assets.UserReporting.Scripts.Plugin;
using Unity.Cloud.UserReporting.Client;
using Unity.Cloud.UserReporting.Plugin.SimpleJson;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Unity.Cloud.UserReporting.Plugin
{
	// Token: 0x02000024 RID: 36
	public class UnityUserReportingPlatform : IUserReportingPlatform, ILogListener
	{
		// Token: 0x060000FC RID: 252 RVA: 0x000049A0 File Offset: 0x00002BA0
		public UnityUserReportingPlatform()
		{
			this.logMessages = new List<UnityUserReportingPlatform.LogMessage>();
			this.postOperations = new List<UnityUserReportingPlatform.PostOperation>();
			this.screenshotOperations = new List<UnityUserReportingPlatform.ScreenshotOperation>();
			this.screenshotStopwatch = new Stopwatch();
			this.profilerSamplers = new List<UnityUserReportingPlatform.ProfilerSampler>();
			foreach (KeyValuePair<string, string> keyValuePair in this.GetSamplerNames())
			{
				Sampler sampler = Sampler.Get(keyValuePair.Key);
				if (sampler.isValid)
				{
					Recorder recorder = sampler.GetRecorder();
					recorder.enabled = true;
					UnityUserReportingPlatform.ProfilerSampler profilerSampler = default(UnityUserReportingPlatform.ProfilerSampler);
					profilerSampler.Name = keyValuePair.Value;
					profilerSampler.Recorder = recorder;
					this.profilerSamplers.Add(profilerSampler);
				}
			}
			LogDispatcher.Register(this);
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00004A80 File Offset: 0x00002C80
		public T DeserializeJson<T>(string json)
		{
			return SimpleJson.DeserializeObject<T>(json);
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00004A88 File Offset: 0x00002C88
		public void OnEndOfFrame(UserReportingClient client)
		{
			int i = 0;
			while (i < this.screenshotOperations.Count)
			{
				UnityUserReportingPlatform.ScreenshotOperation screenshotOperation = this.screenshotOperations[i];
				if (screenshotOperation.Stage == UnityUserReportingPlatform.ScreenshotStage.Render && screenshotOperation.WaitFrames < 1)
				{
					Camera camera = screenshotOperation.Source as Camera;
					if (camera != null)
					{
						this.screenshotStopwatch.Reset();
						this.screenshotStopwatch.Start();
						RenderTexture renderTexture = new RenderTexture(screenshotOperation.MaximumWidth, screenshotOperation.MaximumHeight, 24);
						RenderTexture targetTexture = camera.targetTexture;
						camera.targetTexture = renderTexture;
						camera.Render();
						camera.targetTexture = targetTexture;
						this.screenshotStopwatch.Stop();
						client.SampleClientMetric("Screenshot.Render", (double)this.screenshotStopwatch.ElapsedMilliseconds);
						screenshotOperation.Source = renderTexture;
						screenshotOperation.Stage = UnityUserReportingPlatform.ScreenshotStage.ReadPixels;
						screenshotOperation.WaitFrames = 15;
						i++;
						continue;
					}
					screenshotOperation.Stage = UnityUserReportingPlatform.ScreenshotStage.ReadPixels;
				}
				if (screenshotOperation.Stage == UnityUserReportingPlatform.ScreenshotStage.ReadPixels && screenshotOperation.WaitFrames < 1)
				{
					this.screenshotStopwatch.Reset();
					this.screenshotStopwatch.Start();
					RenderTexture renderTexture2 = screenshotOperation.Source as RenderTexture;
					if (renderTexture2 != null)
					{
						RenderTexture active = RenderTexture.active;
						RenderTexture.active = renderTexture2;
						screenshotOperation.Texture = new Texture2D(renderTexture2.width, renderTexture2.height, TextureFormat.ARGB32, true);
						screenshotOperation.Texture.ReadPixels(new Rect(0f, 0f, (float)renderTexture2.width, (float)renderTexture2.height), 0, 0);
						screenshotOperation.Texture.Apply();
						RenderTexture.active = active;
					}
					else
					{
						screenshotOperation.Texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, true);
						screenshotOperation.Texture.ReadPixels(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), 0, 0);
						screenshotOperation.Texture.Apply();
					}
					this.screenshotStopwatch.Stop();
					client.SampleClientMetric("Screenshot.ReadPixels", (double)this.screenshotStopwatch.ElapsedMilliseconds);
					screenshotOperation.Stage = UnityUserReportingPlatform.ScreenshotStage.GetPixels;
					screenshotOperation.WaitFrames = 15;
					i++;
				}
				else if (screenshotOperation.Stage == UnityUserReportingPlatform.ScreenshotStage.GetPixels && screenshotOperation.WaitFrames < 1)
				{
					this.screenshotStopwatch.Reset();
					this.screenshotStopwatch.Start();
					int num = ((screenshotOperation.MaximumWidth > 32) ? screenshotOperation.MaximumWidth : 32);
					int num2 = ((screenshotOperation.MaximumHeight > 32) ? screenshotOperation.MaximumHeight : 32);
					int num3 = screenshotOperation.Texture.width;
					int num4 = screenshotOperation.Texture.height;
					int num5 = 0;
					while (num3 > num || num4 > num2)
					{
						num3 /= 2;
						num4 /= 2;
						num5++;
					}
					screenshotOperation.TextureResized = new Texture2D(num3, num4);
					screenshotOperation.TextureResized.SetPixels(screenshotOperation.Texture.GetPixels(num5));
					screenshotOperation.TextureResized.Apply();
					this.screenshotStopwatch.Stop();
					client.SampleClientMetric("Screenshot.GetPixels", (double)this.screenshotStopwatch.ElapsedMilliseconds);
					screenshotOperation.Stage = UnityUserReportingPlatform.ScreenshotStage.EncodeToPNG;
					screenshotOperation.WaitFrames = 15;
					i++;
				}
				else if (screenshotOperation.Stage == UnityUserReportingPlatform.ScreenshotStage.EncodeToPNG && screenshotOperation.WaitFrames < 1)
				{
					this.screenshotStopwatch.Reset();
					this.screenshotStopwatch.Start();
					screenshotOperation.PngData = screenshotOperation.TextureResized.EncodeToPNG();
					this.screenshotStopwatch.Stop();
					client.SampleClientMetric("Screenshot.EncodeToPNG", (double)this.screenshotStopwatch.ElapsedMilliseconds);
					screenshotOperation.Stage = UnityUserReportingPlatform.ScreenshotStage.Done;
					i++;
				}
				else
				{
					if (screenshotOperation.Stage == UnityUserReportingPlatform.ScreenshotStage.Done && screenshotOperation.WaitFrames < 1)
					{
						screenshotOperation.Callback(screenshotOperation.FrameNumber, screenshotOperation.PngData);
						global::UnityEngine.Object.Destroy(screenshotOperation.Texture);
						global::UnityEngine.Object.Destroy(screenshotOperation.TextureResized);
						this.screenshotOperations.Remove(screenshotOperation);
					}
					UnityUserReportingPlatform.ScreenshotOperation screenshotOperation2 = screenshotOperation;
					int waitFrames = screenshotOperation2.WaitFrames;
					screenshotOperation2.WaitFrames = waitFrames - 1;
				}
			}
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00004E70 File Offset: 0x00003070
		public void Post(string endpoint, string contentType, byte[] content, Action<float, float> progressCallback, Action<bool, byte[]> callback)
		{
			UnityWebRequest unityWebRequest = new UnityWebRequest(endpoint, "POST");
			unityWebRequest.uploadHandler = new UploadHandlerRaw(content);
			unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
			unityWebRequest.disposeUploadHandlerOnDispose = true;
			unityWebRequest.disposeDownloadHandlerOnDispose = true;
			unityWebRequest.SetRequestHeader("Content-Type", contentType);
			unityWebRequest.SendWebRequest();
			UnityUserReportingPlatform.PostOperation postOperation = new UnityUserReportingPlatform.PostOperation();
			postOperation.WebRequest = unityWebRequest;
			postOperation.Callback = callback;
			postOperation.ProgressCallback = progressCallback;
			this.postOperations.Add(postOperation);
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00004EEC File Offset: 0x000030EC
		public void ReceiveLogMessage(string logString, string stackTrace, LogType logType)
		{
			List<UnityUserReportingPlatform.LogMessage> list = this.logMessages;
			lock (list)
			{
				UnityUserReportingPlatform.LogMessage logMessage = default(UnityUserReportingPlatform.LogMessage);
				logMessage.LogString = logString;
				logMessage.StackTrace = stackTrace;
				logMessage.LogType = logType;
				this.logMessages.Add(logMessage);
			}
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00004F54 File Offset: 0x00003154
		public void RunTask(Func<object> task, Action<object> callback)
		{
			callback(task());
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00004F62 File Offset: 0x00003162
		public void SendAnalyticsEvent(string eventName, Dictionary<string, object> eventData)
		{
			Analytics.CustomEvent(eventName, eventData);
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00004F6C File Offset: 0x0000316C
		public string SerializeJson(object instance)
		{
			return SimpleJson.SerializeObject(instance);
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00004F74 File Offset: 0x00003174
		public void TakeScreenshot(int frameNumber, int maximumWidth, int maximumHeight, object source, Action<int, byte[]> callback)
		{
			UnityUserReportingPlatform.ScreenshotOperation screenshotOperation = new UnityUserReportingPlatform.ScreenshotOperation();
			screenshotOperation.FrameNumber = frameNumber;
			screenshotOperation.MaximumWidth = maximumWidth;
			screenshotOperation.MaximumHeight = maximumHeight;
			screenshotOperation.Source = source;
			screenshotOperation.Callback = callback;
			screenshotOperation.UnityFrame = Time.frameCount;
			this.screenshotOperations.Add(screenshotOperation);
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00004FC4 File Offset: 0x000031C4
		public void Update(UserReportingClient client)
		{
			List<UnityUserReportingPlatform.LogMessage> list = this.logMessages;
			lock (list)
			{
				foreach (UnityUserReportingPlatform.LogMessage logMessage in this.logMessages)
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
				foreach (UnityUserReportingPlatform.ProfilerSampler profilerSampler in this.profilerSamplers)
				{
					client.SampleMetric(profilerSampler.Name, profilerSampler.GetValue());
				}
			}
			int i = 0;
			while (i < this.postOperations.Count)
			{
				UnityUserReportingPlatform.PostOperation postOperation = this.postOperations[i];
				if (postOperation.WebRequest.isDone)
				{
					bool flag2 = postOperation.WebRequest.error != null && postOperation.WebRequest.responseCode != 200L;
					if (flag2)
					{
						string text = string.Format("UnityUserReportingPlatform.Post: {0} {1}", postOperation.WebRequest.responseCode, postOperation.WebRequest.error);
						global::UnityEngine.Debug.Log(text);
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

		// Token: 0x06000106 RID: 262 RVA: 0x00005214 File Offset: 0x00003414
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

		// Token: 0x06000107 RID: 263 RVA: 0x000054E4 File Offset: 0x000036E4
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

		// Token: 0x06000108 RID: 264 RVA: 0x00005638 File Offset: 0x00003838
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

		// Token: 0x06000109 RID: 265 RVA: 0x00005798 File Offset: 0x00003998
		public virtual void SampleAutomaticMetrics(UserReportingClient client)
		{
			client.SampleMetric("Graphics.FramesPerSecond", (double)(1f / Time.deltaTime));
			client.SampleMetric("Memory.MonoUsedSizeInBytes", (double)Profiler.GetMonoUsedSizeLong());
			client.SampleMetric("Memory.TotalAllocatedMemoryInBytes", (double)Profiler.GetTotalAllocatedMemoryLong());
			client.SampleMetric("Memory.TotalReservedMemoryInBytes", (double)Profiler.GetTotalReservedMemoryLong());
			client.SampleMetric("Memory.TotalUnusedReservedMemoryInBytes", (double)Profiler.GetTotalUnusedReservedMemoryLong());
			client.SampleMetric("Battery.BatteryLevelInPercent", (double)SystemInfo.batteryLevel);
		}

		// Token: 0x04000083 RID: 131
		private List<UnityUserReportingPlatform.LogMessage> logMessages;

		// Token: 0x04000084 RID: 132
		private List<UnityUserReportingPlatform.PostOperation> postOperations;

		// Token: 0x04000085 RID: 133
		private List<UnityUserReportingPlatform.ProfilerSampler> profilerSamplers;

		// Token: 0x04000086 RID: 134
		private List<UnityUserReportingPlatform.ScreenshotOperation> screenshotOperations;

		// Token: 0x04000087 RID: 135
		private Stopwatch screenshotStopwatch;

		// Token: 0x04000088 RID: 136
		private List<UnityUserReportingPlatform.PostOperation> taskOperations;

		// Token: 0x0200003F RID: 63
		private struct LogMessage
		{
			// Token: 0x040000FA RID: 250
			public string LogString;

			// Token: 0x040000FB RID: 251
			public LogType LogType;

			// Token: 0x040000FC RID: 252
			public string StackTrace;
		}

		// Token: 0x02000040 RID: 64
		private class PostOperation
		{
			// Token: 0x1700007A RID: 122
			// (get) Token: 0x06000216 RID: 534 RVA: 0x00009495 File Offset: 0x00007695
			// (set) Token: 0x06000217 RID: 535 RVA: 0x0000949D File Offset: 0x0000769D
			public Action<bool, byte[]> Callback { get; set; }

			// Token: 0x1700007B RID: 123
			// (get) Token: 0x06000218 RID: 536 RVA: 0x000094A6 File Offset: 0x000076A6
			// (set) Token: 0x06000219 RID: 537 RVA: 0x000094AE File Offset: 0x000076AE
			public Action<float, float> ProgressCallback { get; set; }

			// Token: 0x1700007C RID: 124
			// (get) Token: 0x0600021A RID: 538 RVA: 0x000094B7 File Offset: 0x000076B7
			// (set) Token: 0x0600021B RID: 539 RVA: 0x000094BF File Offset: 0x000076BF
			public UnityWebRequest WebRequest { get; set; }
		}

		// Token: 0x02000041 RID: 65
		private struct ProfilerSampler
		{
			// Token: 0x0600021D RID: 541 RVA: 0x000094D0 File Offset: 0x000076D0
			public double GetValue()
			{
				if (this.Recorder == null)
				{
					return 0.0;
				}
				return (double)this.Recorder.elapsedNanoseconds / 1000000.0;
			}

			// Token: 0x04000100 RID: 256
			public string Name;

			// Token: 0x04000101 RID: 257
			public Recorder Recorder;
		}

		// Token: 0x02000042 RID: 66
		private class ScreenshotOperation
		{
			// Token: 0x1700007D RID: 125
			// (get) Token: 0x0600021E RID: 542 RVA: 0x000094FA File Offset: 0x000076FA
			// (set) Token: 0x0600021F RID: 543 RVA: 0x00009502 File Offset: 0x00007702
			public Action<int, byte[]> Callback { get; set; }

			// Token: 0x1700007E RID: 126
			// (get) Token: 0x06000220 RID: 544 RVA: 0x0000950B File Offset: 0x0000770B
			// (set) Token: 0x06000221 RID: 545 RVA: 0x00009513 File Offset: 0x00007713
			public int FrameNumber { get; set; }

			// Token: 0x1700007F RID: 127
			// (get) Token: 0x06000222 RID: 546 RVA: 0x0000951C File Offset: 0x0000771C
			// (set) Token: 0x06000223 RID: 547 RVA: 0x00009524 File Offset: 0x00007724
			public int MaximumHeight { get; set; }

			// Token: 0x17000080 RID: 128
			// (get) Token: 0x06000224 RID: 548 RVA: 0x0000952D File Offset: 0x0000772D
			// (set) Token: 0x06000225 RID: 549 RVA: 0x00009535 File Offset: 0x00007735
			public int MaximumWidth { get; set; }

			// Token: 0x17000081 RID: 129
			// (get) Token: 0x06000226 RID: 550 RVA: 0x0000953E File Offset: 0x0000773E
			// (set) Token: 0x06000227 RID: 551 RVA: 0x00009546 File Offset: 0x00007746
			public byte[] PngData { get; set; }

			// Token: 0x17000082 RID: 130
			// (get) Token: 0x06000228 RID: 552 RVA: 0x0000954F File Offset: 0x0000774F
			// (set) Token: 0x06000229 RID: 553 RVA: 0x00009557 File Offset: 0x00007757
			public object Source { get; set; }

			// Token: 0x17000083 RID: 131
			// (get) Token: 0x0600022A RID: 554 RVA: 0x00009560 File Offset: 0x00007760
			// (set) Token: 0x0600022B RID: 555 RVA: 0x00009568 File Offset: 0x00007768
			public UnityUserReportingPlatform.ScreenshotStage Stage { get; set; }

			// Token: 0x17000084 RID: 132
			// (get) Token: 0x0600022C RID: 556 RVA: 0x00009571 File Offset: 0x00007771
			// (set) Token: 0x0600022D RID: 557 RVA: 0x00009579 File Offset: 0x00007779
			public Texture2D Texture { get; set; }

			// Token: 0x17000085 RID: 133
			// (get) Token: 0x0600022E RID: 558 RVA: 0x00009582 File Offset: 0x00007782
			// (set) Token: 0x0600022F RID: 559 RVA: 0x0000958A File Offset: 0x0000778A
			public Texture2D TextureResized { get; set; }

			// Token: 0x17000086 RID: 134
			// (get) Token: 0x06000230 RID: 560 RVA: 0x00009593 File Offset: 0x00007793
			// (set) Token: 0x06000231 RID: 561 RVA: 0x0000959B File Offset: 0x0000779B
			public int UnityFrame { get; set; }

			// Token: 0x17000087 RID: 135
			// (get) Token: 0x06000232 RID: 562 RVA: 0x000095A4 File Offset: 0x000077A4
			// (set) Token: 0x06000233 RID: 563 RVA: 0x000095AC File Offset: 0x000077AC
			public int WaitFrames { get; set; }
		}

		// Token: 0x02000043 RID: 67
		private enum ScreenshotStage
		{
			// Token: 0x0400010E RID: 270
			Render,
			// Token: 0x0400010F RID: 271
			ReadPixels,
			// Token: 0x04000110 RID: 272
			GetPixels,
			// Token: 0x04000111 RID: 273
			EncodeToPNG,
			// Token: 0x04000112 RID: 274
			Done
		}
	}
}
