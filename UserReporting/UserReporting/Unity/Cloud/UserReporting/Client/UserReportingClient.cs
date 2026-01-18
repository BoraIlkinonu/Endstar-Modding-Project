using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Unity.Cloud.UserReporting.Client
{
	// Token: 0x02000031 RID: 49
	public class UserReportingClient
	{
		// Token: 0x06000197 RID: 407 RVA: 0x0000822C File Offset: 0x0000642C
		public UserReportingClient(string endpoint, string projectIdentifier, IUserReportingPlatform platform, UserReportingClientConfiguration configuration)
		{
			this.Endpoint = endpoint;
			this.ProjectIdentifier = projectIdentifier;
			this.Platform = platform;
			this.Configuration = configuration;
			this.Configuration.FramesPerMeasure = ((this.Configuration.FramesPerMeasure > 0) ? this.Configuration.FramesPerMeasure : 1);
			this.Configuration.MaximumEventCount = ((this.Configuration.MaximumEventCount > 0) ? this.Configuration.MaximumEventCount : 1);
			this.Configuration.MaximumMeasureCount = ((this.Configuration.MaximumMeasureCount > 0) ? this.Configuration.MaximumMeasureCount : 1);
			this.Configuration.MaximumScreenshotCount = ((this.Configuration.MaximumScreenshotCount > 0) ? this.Configuration.MaximumScreenshotCount : 1);
			this.clientMetrics = new Dictionary<string, UserReportMetric>();
			this.currentMeasureMetadata = new Dictionary<string, string>();
			this.currentMetrics = new Dictionary<string, UserReportMetric>();
			this.events = new CyclicalList<UserReportEvent>(configuration.MaximumEventCount);
			this.measures = new CyclicalList<UserReportMeasure>(configuration.MaximumMeasureCount);
			this.screenshots = new CyclicalList<UserReportScreenshot>(configuration.MaximumScreenshotCount);
			this.deviceMetadata = new List<UserReportNamedValue>();
			foreach (KeyValuePair<string, string> keyValuePair in this.Platform.GetDeviceMetadata())
			{
				this.AddDeviceMetadata(keyValuePair.Key, keyValuePair.Value);
			}
			this.AddDeviceMetadata("UserReportingClientVersion", "2.0");
			this.synchronizedActions = new List<Action>();
			this.currentSynchronizedActions = new List<Action>();
			this.updateStopwatch = new Stopwatch();
			this.IsConnectedToLogger = true;
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x06000198 RID: 408 RVA: 0x000083E4 File Offset: 0x000065E4
		// (set) Token: 0x06000199 RID: 409 RVA: 0x000083EC File Offset: 0x000065EC
		public UserReportingClientConfiguration Configuration { get; private set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600019A RID: 410 RVA: 0x000083F5 File Offset: 0x000065F5
		// (set) Token: 0x0600019B RID: 411 RVA: 0x000083FD File Offset: 0x000065FD
		public string Endpoint { get; private set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600019C RID: 412 RVA: 0x00008406 File Offset: 0x00006606
		// (set) Token: 0x0600019D RID: 413 RVA: 0x0000840E File Offset: 0x0000660E
		public bool IsConnectedToLogger { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x0600019E RID: 414 RVA: 0x00008417 File Offset: 0x00006617
		// (set) Token: 0x0600019F RID: 415 RVA: 0x0000841F File Offset: 0x0000661F
		public bool IsSelfReporting { get; set; }

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060001A0 RID: 416 RVA: 0x00008428 File Offset: 0x00006628
		// (set) Token: 0x060001A1 RID: 417 RVA: 0x00008430 File Offset: 0x00006630
		public IUserReportingPlatform Platform { get; private set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060001A2 RID: 418 RVA: 0x00008439 File Offset: 0x00006639
		// (set) Token: 0x060001A3 RID: 419 RVA: 0x00008441 File Offset: 0x00006641
		public string ProjectIdentifier { get; private set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x060001A4 RID: 420 RVA: 0x0000844A File Offset: 0x0000664A
		// (set) Token: 0x060001A5 RID: 421 RVA: 0x00008452 File Offset: 0x00006652
		public bool SendEventsToAnalytics { get; set; }

		// Token: 0x060001A6 RID: 422 RVA: 0x0000845C File Offset: 0x0000665C
		public void AddDeviceMetadata(string name, string value)
		{
			List<UserReportNamedValue> list = this.deviceMetadata;
			lock (list)
			{
				UserReportNamedValue userReportNamedValue = default(UserReportNamedValue);
				userReportNamedValue.Name = name;
				userReportNamedValue.Value = value;
				this.deviceMetadata.Add(userReportNamedValue);
			}
		}

		// Token: 0x060001A7 RID: 423 RVA: 0x000084BC File Offset: 0x000066BC
		public void AddMeasureMetadata(string name, string value)
		{
			if (this.currentMeasureMetadata.ContainsKey(name))
			{
				this.currentMeasureMetadata[name] = value;
				return;
			}
			this.currentMeasureMetadata.Add(name, value);
		}

		// Token: 0x060001A8 RID: 424 RVA: 0x000084E8 File Offset: 0x000066E8
		private void AddSynchronizedAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			List<Action> list = this.synchronizedActions;
			lock (list)
			{
				this.synchronizedActions.Add(action);
			}
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x0000853C File Offset: 0x0000673C
		public void ClearScreenshots()
		{
			CyclicalList<UserReportScreenshot> cyclicalList = this.screenshots;
			lock (cyclicalList)
			{
				this.screenshots.Clear();
			}
		}

		// Token: 0x060001AA RID: 426 RVA: 0x00008584 File Offset: 0x00006784
		public void CreateUserReport(Action<UserReport> callback)
		{
			this.LogEvent(UserReportEventLevel.Info, "Creating user report.");
			Func<object> <>9__1;
			Action<object> <>9__2;
			this.WaitForPerforation(this.screenshotsTaken, delegate
			{
				IUserReportingPlatform platform = this.Platform;
				Func<object> func;
				if ((func = <>9__1) == null)
				{
					func = (<>9__1 = delegate
					{
						Stopwatch stopwatch = Stopwatch.StartNew();
						UserReport userReport = new UserReport();
						userReport.ProjectIdentifier = this.ProjectIdentifier;
						List<UserReportNamedValue> list = this.deviceMetadata;
						lock (list)
						{
							userReport.DeviceMetadata = this.deviceMetadata.ToList<UserReportNamedValue>();
						}
						CyclicalList<UserReportEvent> cyclicalList = this.events;
						lock (cyclicalList)
						{
							userReport.Events = this.events.ToList<UserReportEvent>();
						}
						CyclicalList<UserReportMeasure> cyclicalList2 = this.measures;
						lock (cyclicalList2)
						{
							userReport.Measures = this.measures.ToList<UserReportMeasure>();
						}
						CyclicalList<UserReportScreenshot> cyclicalList3 = this.screenshots;
						lock (cyclicalList3)
						{
							userReport.Screenshots = this.screenshots.ToList<UserReportScreenshot>();
						}
						userReport.Complete();
						this.Platform.ModifyUserReport(userReport);
						stopwatch.Stop();
						this.SampleClientMetric("UserReportingClient.CreateUserReport.Task", (double)stopwatch.ElapsedMilliseconds);
						foreach (KeyValuePair<string, UserReportMetric> keyValuePair in this.clientMetrics)
						{
							userReport.ClientMetrics.Add(keyValuePair.Value);
						}
						return userReport;
					});
				}
				Action<object> action;
				if ((action = <>9__2) == null)
				{
					action = (<>9__2 = delegate(object result)
					{
						callback(result as UserReport);
					});
				}
				platform.RunTask(func, action);
			});
		}

		// Token: 0x060001AB RID: 427 RVA: 0x000085C9 File Offset: 0x000067C9
		public string GetEndpoint()
		{
			if (this.Endpoint == null)
			{
				return "https://localhost";
			}
			return this.Endpoint.Trim();
		}

		// Token: 0x060001AC RID: 428 RVA: 0x000085E4 File Offset: 0x000067E4
		public void LogEvent(UserReportEventLevel level, string message)
		{
			this.LogEvent(level, message, null, null);
		}

		// Token: 0x060001AD RID: 429 RVA: 0x000085F0 File Offset: 0x000067F0
		public void LogEvent(UserReportEventLevel level, string message, string stackTrace)
		{
			this.LogEvent(level, message, stackTrace, null);
		}

		// Token: 0x060001AE RID: 430 RVA: 0x000085FC File Offset: 0x000067FC
		private void LogEvent(UserReportEventLevel level, string message, string stackTrace, Exception exception)
		{
			CyclicalList<UserReportEvent> cyclicalList = this.events;
			lock (cyclicalList)
			{
				UserReportEvent userReportEvent = default(UserReportEvent);
				userReportEvent.Level = level;
				userReportEvent.Message = message;
				userReportEvent.FrameNumber = this.frameNumber;
				userReportEvent.StackTrace = stackTrace;
				userReportEvent.Timestamp = DateTime.UtcNow;
				if (exception != null)
				{
					userReportEvent.Exception = new SerializableException(exception);
				}
				this.events.Add(userReportEvent);
			}
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00008690 File Offset: 0x00006890
		public void LogException(Exception exception)
		{
			this.LogEvent(UserReportEventLevel.Error, null, null, exception);
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000869C File Offset: 0x0000689C
		public void SampleClientMetric(string name, double value)
		{
			if (double.IsInfinity(value) || double.IsNaN(value))
			{
				return;
			}
			if (!this.clientMetrics.ContainsKey(name))
			{
				UserReportMetric userReportMetric = default(UserReportMetric);
				userReportMetric.Name = name;
				this.clientMetrics.Add(name, userReportMetric);
			}
			UserReportMetric userReportMetric2 = this.clientMetrics[name];
			userReportMetric2.Sample(value);
			this.clientMetrics[name] = userReportMetric2;
			if (this.IsSelfReporting)
			{
				this.SampleMetric(name, value);
			}
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00008718 File Offset: 0x00006918
		public void SampleMetric(string name, double value)
		{
			if (this.Configuration.MetricsGatheringMode == MetricsGatheringMode.Disabled)
			{
				return;
			}
			if (double.IsInfinity(value) || double.IsNaN(value))
			{
				return;
			}
			if (!this.currentMetrics.ContainsKey(name))
			{
				UserReportMetric userReportMetric = default(UserReportMetric);
				userReportMetric.Name = name;
				this.currentMetrics.Add(name, userReportMetric);
			}
			UserReportMetric userReportMetric2 = this.currentMetrics[name];
			userReportMetric2.Sample(value);
			this.currentMetrics[name] = userReportMetric2;
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00008794 File Offset: 0x00006994
		public void SaveUserReportToDisk(UserReport userReport)
		{
			this.LogEvent(UserReportEventLevel.Info, "Saving user report to disk.");
			string text = this.Platform.SerializeJson(userReport);
			File.WriteAllText("UserReport.json", text);
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x000087C5 File Offset: 0x000069C5
		public void SendUserReport(UserReport userReport, Action<bool, UserReport> callback)
		{
			this.SendUserReport(userReport, null, callback);
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x000087D0 File Offset: 0x000069D0
		public void SendUserReport(UserReport userReport, Action<float, float> progressCallback, Action<bool, UserReport> callback)
		{
			try
			{
				if (userReport != null)
				{
					if (userReport.Identifier != null)
					{
						this.LogEvent(UserReportEventLevel.Warning, "Identifier cannot be set on the client side. The value provided was discarded.");
					}
					else if (userReport.ContentLength != 0L)
					{
						this.LogEvent(UserReportEventLevel.Warning, "ContentLength cannot be set on the client side. The value provided was discarded.");
					}
					else if (userReport.ReceivedOn != default(DateTime))
					{
						this.LogEvent(UserReportEventLevel.Warning, "ReceivedOn cannot be set on the client side. The value provided was discarded.");
					}
					else if (userReport.ExpiresOn != default(DateTime))
					{
						this.LogEvent(UserReportEventLevel.Warning, "ExpiresOn cannot be set on the client side. The value provided was discarded.");
					}
					else
					{
						this.LogEvent(UserReportEventLevel.Info, "Sending user report.");
						string text = this.Platform.SerializeJson(userReport);
						byte[] bytes = Encoding.UTF8.GetBytes(text);
						string endpoint = this.GetEndpoint();
						string text2 = string.Format(string.Format("{0}/api/userreporting", endpoint), Array.Empty<object>());
						this.Platform.Post(text2, "application/json", bytes, delegate(float uploadProgress, float downloadProgress)
						{
							if (progressCallback != null)
							{
								progressCallback(uploadProgress, downloadProgress);
							}
						}, delegate(bool success, byte[] result)
						{
							this.AddSynchronizedAction(delegate
							{
								if (success)
								{
									try
									{
										string @string = Encoding.UTF8.GetString(result);
										UserReport userReport2 = this.Platform.DeserializeJson<UserReport>(@string);
										if (userReport2 != null)
										{
											if (this.SendEventsToAnalytics)
											{
												Dictionary<string, object> dictionary = new Dictionary<string, object>();
												dictionary.Add("UserReportIdentifier", userReport.Identifier);
												this.Platform.SendAnalyticsEvent("UserReportingClient.SendUserReport", dictionary);
											}
											callback(success, userReport2);
										}
										else
										{
											callback(false, null);
										}
										return;
									}
									catch (Exception ex2)
									{
										this.LogEvent(UserReportEventLevel.Error, string.Format("Sending user report failed: {0}", ex2.ToString()));
										callback(false, null);
										return;
									}
								}
								this.LogEvent(UserReportEventLevel.Error, "Sending user report failed.");
								callback(false, null);
							});
						});
					}
				}
			}
			catch (Exception ex)
			{
				this.LogEvent(UserReportEventLevel.Error, string.Format("Sending user report failed: {0}", ex.ToString()));
				callback(false, null);
			}
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000895C File Offset: 0x00006B5C
		public void TakeScreenshot(int maximumWidth, int maximumHeight, Action<UserReportScreenshot> callback)
		{
			this.TakeScreenshotFromSource(maximumWidth, maximumHeight, null, callback);
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x00008968 File Offset: 0x00006B68
		public void TakeScreenshotFromSource(int maximumWidth, int maximumHeight, object source, Action<UserReportScreenshot> callback)
		{
			this.LogEvent(UserReportEventLevel.Info, "Taking screenshot.");
			this.screenshotsTaken++;
			this.Platform.TakeScreenshot(this.frameNumber, maximumWidth, maximumHeight, source, delegate(int passedFrameNumber, byte[] data)
			{
				this.AddSynchronizedAction(delegate
				{
					CyclicalList<UserReportScreenshot> cyclicalList = this.screenshots;
					lock (cyclicalList)
					{
						UserReportScreenshot userReportScreenshot = default(UserReportScreenshot);
						userReportScreenshot.FrameNumber = passedFrameNumber;
						userReportScreenshot.DataBase64 = Convert.ToBase64String(data);
						this.screenshots.Add(userReportScreenshot);
						this.screenshotsSaved++;
						callback(userReportScreenshot);
					}
				});
			});
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x000089C4 File Offset: 0x00006BC4
		public void Update()
		{
			this.updateStopwatch.Reset();
			this.updateStopwatch.Start();
			this.Platform.Update(this);
			if (this.Configuration.MetricsGatheringMode != MetricsGatheringMode.Disabled)
			{
				this.isMeasureBoundary = false;
				int framesPerMeasure = this.Configuration.FramesPerMeasure;
				if (this.measureFrames >= framesPerMeasure)
				{
					CyclicalList<UserReportMeasure> cyclicalList = this.measures;
					lock (cyclicalList)
					{
						UserReportMeasure userReportMeasure = default(UserReportMeasure);
						userReportMeasure.StartFrameNumber = this.frameNumber - framesPerMeasure;
						userReportMeasure.EndFrameNumber = this.frameNumber - 1;
						UserReportMeasure nextEviction = this.measures.GetNextEviction();
						if (nextEviction.Metrics != null)
						{
							userReportMeasure.Metadata = nextEviction.Metadata;
							userReportMeasure.Metrics = nextEviction.Metrics;
						}
						else
						{
							userReportMeasure.Metadata = new List<UserReportNamedValue>();
							userReportMeasure.Metrics = new List<UserReportMetric>();
						}
						userReportMeasure.Metadata.Clear();
						userReportMeasure.Metrics.Clear();
						foreach (KeyValuePair<string, string> keyValuePair in this.currentMeasureMetadata)
						{
							UserReportNamedValue userReportNamedValue = default(UserReportNamedValue);
							userReportNamedValue.Name = keyValuePair.Key;
							userReportNamedValue.Value = keyValuePair.Value;
							userReportMeasure.Metadata.Add(userReportNamedValue);
						}
						foreach (KeyValuePair<string, UserReportMetric> keyValuePair2 in this.currentMetrics)
						{
							userReportMeasure.Metrics.Add(keyValuePair2.Value);
						}
						this.currentMetrics.Clear();
						this.measures.Add(userReportMeasure);
						this.measureFrames = 0;
						this.isMeasureBoundary = true;
					}
				}
				this.measureFrames++;
			}
			else
			{
				this.isMeasureBoundary = true;
			}
			List<Action> list = this.synchronizedActions;
			lock (list)
			{
				foreach (Action action in this.synchronizedActions)
				{
					this.currentSynchronizedActions.Add(action);
				}
				this.synchronizedActions.Clear();
			}
			foreach (Action action2 in this.currentSynchronizedActions)
			{
				action2();
			}
			this.currentSynchronizedActions.Clear();
			this.frameNumber++;
			this.updateStopwatch.Stop();
			this.SampleClientMetric("UserReportingClient.Update", (double)this.updateStopwatch.ElapsedMilliseconds);
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00008D1C File Offset: 0x00006F1C
		public void UpdateOnEndOfFrame()
		{
			this.updateStopwatch.Reset();
			this.updateStopwatch.Start();
			this.Platform.OnEndOfFrame(this);
			this.updateStopwatch.Stop();
			this.SampleClientMetric("UserReportingClient.UpdateOnEndOfFrame", (double)this.updateStopwatch.ElapsedMilliseconds);
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00008D70 File Offset: 0x00006F70
		private void WaitForPerforation(int currentScreenshotsTaken, Action callback)
		{
			if (this.screenshotsSaved >= currentScreenshotsTaken && this.isMeasureBoundary)
			{
				callback();
				return;
			}
			this.AddSynchronizedAction(delegate
			{
				this.WaitForPerforation(currentScreenshotsTaken, callback);
			});
		}

		// Token: 0x040000AE RID: 174
		private Dictionary<string, UserReportMetric> clientMetrics;

		// Token: 0x040000AF RID: 175
		private Dictionary<string, string> currentMeasureMetadata;

		// Token: 0x040000B0 RID: 176
		private Dictionary<string, UserReportMetric> currentMetrics;

		// Token: 0x040000B1 RID: 177
		private List<Action> currentSynchronizedActions;

		// Token: 0x040000B2 RID: 178
		private List<UserReportNamedValue> deviceMetadata;

		// Token: 0x040000B3 RID: 179
		private CyclicalList<UserReportEvent> events;

		// Token: 0x040000B4 RID: 180
		private int frameNumber;

		// Token: 0x040000B5 RID: 181
		private bool isMeasureBoundary;

		// Token: 0x040000B6 RID: 182
		private int measureFrames;

		// Token: 0x040000B7 RID: 183
		private CyclicalList<UserReportMeasure> measures;

		// Token: 0x040000B8 RID: 184
		private CyclicalList<UserReportScreenshot> screenshots;

		// Token: 0x040000B9 RID: 185
		private int screenshotsSaved;

		// Token: 0x040000BA RID: 186
		private int screenshotsTaken;

		// Token: 0x040000BB RID: 187
		private List<Action> synchronizedActions;

		// Token: 0x040000BC RID: 188
		private Stopwatch updateStopwatch;
	}
}
