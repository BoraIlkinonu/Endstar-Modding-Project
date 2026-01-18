using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Endless.GraphQl;
using Endless.Matchmaking;
using Endless.Shared.UI;
using Endless.UnityExtensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Networking;

namespace Endless.Shared
{
	// Token: 0x02000099 RID: 153
	public class BuildUtilities
	{
		// Token: 0x1400002E RID: 46
		// (add) Token: 0x06000461 RID: 1121 RVA: 0x00012AAC File Offset: 0x00010CAC
		// (remove) Token: 0x06000462 RID: 1122 RVA: 0x00012AE0 File Offset: 0x00010CE0
		public static event Action OnFailure;

		// Token: 0x06000463 RID: 1123 RVA: 0x00012B14 File Offset: 0x00010D14
		public static async Task<bool> Initialize()
		{
			string productionVersion = string.Empty;
			TextAsset textAsset = (TextAsset)Resources.Load("UnityCloudBuildManifest.json");
			if (textAsset != null)
			{
				BuildUtilities.Manifest = JsonConvert.DeserializeObject<BuildManifest>(textAsset.text);
			}
			else
			{
				BuildUtilities.Manifest = new BuildManifest();
			}
			GraphQlRequest.UserAgent = "Endstar";
			UnityWebRequest unityWebRequest = await BuildUtilities.GetBuildVersionManifest(true);
			bool flag;
			if (unityWebRequest.result != UnityWebRequest.Result.Success)
			{
				string error = unityWebRequest.error;
				unityWebRequest.Dispose();
				global::UnityEngine.Debug.LogException(new WebException(error));
				BuildUtilities.ForceClose("Error!", 33, "Unable to reach the Endstar servers. Please check your internet connection and try again later. If this issue persists, please contact support.");
				flag = false;
			}
			else
			{
				var template = new
				{
					AvailableBuilds = Array.Empty<string>()
				};
				var <>f__AnonymousType = JsonConvert.DeserializeAnonymousType(unityWebRequest.downloadHandler.text, template);
				if (((<>f__AnonymousType != null) ? <>f__AnonymousType.AvailableBuilds : null) == null || <>f__AnonymousType.AvailableBuilds.Length == 0)
				{
					global::UnityEngine.Debug.LogException(new Exception("Retrieved version list but no versions were available in it!"));
				}
				else
				{
					productionVersion = Version.Parse(<>f__AnonymousType.AvailableBuilds.Last<string>()).ToString();
				}
				bool isRunningLocalMode = Environment.CommandLine.ToLower().Contains("-local-run");
				unityWebRequest = await BuildUtilities.GetBuildVersionManifest(false);
				if (unityWebRequest.result != UnityWebRequest.Result.Success)
				{
					string error2 = unityWebRequest.error;
					unityWebRequest.Dispose();
					global::UnityEngine.Debug.LogException(new WebException(error2));
					BuildUtilities.ForceClose("Error!", 34, "Unable to reach the Endstar servers. Please check your internet connection and try again later. If this issue persists, please contact support.");
					flag = false;
				}
				else
				{
					<>f__AnonymousType = JsonConvert.DeserializeAnonymousType(unityWebRequest.downloadHandler.text, template);
					if (((<>f__AnonymousType != null) ? <>f__AnonymousType.AvailableBuilds : null) == null || <>f__AnonymousType.AvailableBuilds.Length == 0)
					{
						global::UnityEngine.Debug.LogException(new Exception("Retrieved version list but no versions were available in it!"));
						BuildUtilities.ForceClose("Error!", 35, "Build version failed to parse. Please contact support.");
						flag = false;
					}
					else
					{
						BuildUtilities.LatestServerVersion = Version.Parse(<>f__AnonymousType.AvailableBuilds.Last<string>());
						string text = Directory.GetCurrentDirectory() + "/version.data";
						if (File.Exists(text))
						{
							string text2 = EndlessEncryption.Decrypt(File.ReadAllText(text));
							if (!string.IsNullOrEmpty(text2))
							{
								BuildUtilities.Manifest.Version = text2.Replace("\"", "");
								Version version = Version.Parse(BuildUtilities.Manifest.Version);
								productionVersion = version.ToString();
								if (version != BuildUtilities.LatestServerVersion && !isRunningLocalMode)
								{
									BuildUtilities.ForceCloseAndLaunchLauncher("Error!", 36, "There is a new version of Endstar available. Please launch the launcher and download the latest version.");
									return false;
								}
							}
							else if (!isRunningLocalMode)
							{
								BuildUtilities.ForceCloseAndLaunchLauncher("Error!", 37, "Unable to determine the local clients version. Please launch the launcher and fix your Endstar installation.");
								return false;
							}
						}
						else
						{
							productionVersion += "+";
						}
						CrashReportHandler.SetUserMetadata("gameVersion", BuildUtilities.Manifest.Version);
						GraphQlRequest.ProductVersion = productionVersion;
						CrashReportHandler.SetUserMetadata("commitId", BuildUtilities.Manifest.CommitId);
						CrashReportHandler.SetUserMetadata("branch", BuildUtilities.Manifest.Branch);
						CrashReportHandler.SetUserMetadata("cloudBuildTargetName", BuildUtilities.Manifest.Target);
						flag = true;
					}
				}
			}
			return flag;
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x00012B50 File Offset: 0x00010D50
		private static void ForceCloseAndLaunchLauncher(string header, int errorCode, string body)
		{
			Action onFailure = BuildUtilities.OnFailure;
			if (onFailure != null)
			{
				onFailure();
			}
			body = string.Format("<size=80%><color=grey>Error Code: {0}</size></color>\n{1}", errorCode, body);
			UIModalManager instance = MonoBehaviourSingleton<UIModalManager>.Instance;
			Sprite sprite = null;
			string text = body;
			UIModalManagerStackActions uimodalManagerStackActions = UIModalManagerStackActions.ClearStack;
			UIModalGenericViewAction[] array = new UIModalGenericViewAction[2];
			array[0] = new UIModalGenericViewAction(new Color(0f, 0.6392157f, 1f, 1f), "Open Launcher", delegate
			{
				Process.Start("endless://");
				Application.Quit();
			});
			array[1] = new UIModalGenericViewAction(new Color(0.6901961f, 0.2156863f, 0.2156863f), "Quit", new Action(Application.Quit));
			instance.DisplayGenericModal(header, sprite, text, uimodalManagerStackActions, array);
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x00012C10 File Offset: 0x00010E10
		private static void ForceClose(string header, int errorCode, string body)
		{
			Action onFailure = BuildUtilities.OnFailure;
			if (onFailure != null)
			{
				onFailure();
			}
			body = string.Format("<size=80%><color=grey>Error Code: {0}</size></color>\n{1}", errorCode, body);
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal(header, null, body, UIModalManagerStackActions.ClearStack, new UIModalGenericViewAction[]
			{
				new UIModalGenericViewAction(new Color(0.6901961f, 0.2156863f, 0.2156863f), "Quit", new Action(Application.Quit))
			});
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x00012C88 File Offset: 0x00010E88
		private static async Task<UnityWebRequest> GetBuildVersionManifest(bool forceProd)
		{
			string text = (forceProd ? BuildUtilities.GetProdEndpoint() : BuildUtilities.GetEndpoint());
			UnityWebRequest request = new UnityWebRequest(text, "GET");
			request.downloadHandler = new DownloadHandlerBuffer();
			request.disposeUploadHandlerOnDispose = true;
			request.disposeDownloadHandlerOnDispose = true;
			request.SetRequestHeader("Content-Type", "application/json");
			try
			{
				await request.SendWithRetry(3);
			}
			catch (Exception ex)
			{
				global::UnityEngine.Debug.LogException(ex);
			}
			return request;
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x00012CCC File Offset: 0x00010ECC
		private static string GetEndpoint()
		{
			string text = "https://launcher.endlessstudios.com/Endstar";
			text += BuildUtilities.GetPlatformPortion(text);
			switch (MatchmakingClientController.Instance.NetworkEnv)
			{
			case NetworkEnvironment.DEV:
				text += "/dev";
				break;
			case NetworkEnvironment.STAGING:
				text += "/staging";
				break;
			case NetworkEnvironment.PROD:
				text += "/prod";
				break;
			}
			return text + "/Builds/builds_index.json";
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x00012D40 File Offset: 0x00010F40
		private static string GetProdEndpoint()
		{
			string text = "https://launcher.endlessstudios.com/Endstar";
			return text + BuildUtilities.GetPlatformPortion(text) + "/prod/Builds/builds_index.json";
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x00012D5C File Offset: 0x00010F5C
		private static string GetPlatformPortion(string endpoint)
		{
			return "/pc";
		}

		// Token: 0x0400021D RID: 541
		public static BuildManifest Manifest;

		// Token: 0x0400021F RID: 543
		private static Version LatestServerVersion;
	}
}
