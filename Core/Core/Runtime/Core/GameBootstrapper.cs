using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Endless.Data;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Runtime.Core.DeepLinking;
using UnityEngine;

namespace Runtime.Core
{
	// Token: 0x02000005 RID: 5
	public class GameBootstrapper : MonoBehaviour
	{
		// Token: 0x06000009 RID: 9
		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hwnd);

		// Token: 0x0600000A RID: 10
		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hwnd, int nCommandShow);

		// Token: 0x0600000B RID: 11
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		// Token: 0x0600000C RID: 12
		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		// Token: 0x0600000D RID: 13
		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		// Token: 0x0600000E RID: 14
		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000F RID: 15 RVA: 0x000021E2 File Offset: 0x000003E2
		private string DeepLinkWritePath
		{
			get
			{
				return Application.persistentDataPath + "/deep_link.txt";
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000021F3 File Offset: 0x000003F3
		private void Awake()
		{
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticationSuccessful));
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002218 File Offset: 0x00000418
		private void OnAuthenticationSuccessful(ClientData _)
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			this.ProcessCommandLineArguments(commandLineArgs);
			base.StartCoroutine(this.DeepLinkRetrievalLoop());
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002240 File Offset: 0x00000440
		private async void ProcessCommandLineArguments(string[] commandLineArguments)
		{
			Regex regex = new Regex("([A-Za-z0-9]*)\\?([A-Za-z0-9-=&.\\\"]*)");
			foreach (string text in commandLineArguments)
			{
				global::UnityEngine.Debug.Log("Parsing Argument: " + text);
				Match match = regex.Match(text);
				global::UnityEngine.Debug.Log(string.Format("Groups: {0}. Groups Count: {1}", match.Groups, match.Groups.Count));
				if (match.Groups.Count > 1)
				{
					for (int j = 0; j < match.Groups.Count; j++)
					{
						Group group = match.Groups[j];
						global::UnityEngine.Debug.Log(string.Format("Group {0}: {1}", j, group.Value));
					}
					if (match.Groups.Count > 3)
					{
						global::UnityEngine.Debug.LogWarning(string.Format("Unexpectedly found more groups parsing deep link action than expected! Found: {0}, Expected Max: {1}", match.Groups.Count, 3));
					}
					string value = match.Groups[1].Value;
					string value2 = match.Groups[2].Value;
					DeepLinkAction actionInstanceFromMap = DeepLinkActionMap.GetActionInstanceFromMap(value);
					if (!actionInstanceFromMap.Parse(value2))
					{
						return;
					}
					try
					{
						Task task = actionInstanceFromMap.Execute();
						await Task.WhenAll(new Task[] { task });
					}
					catch (DeepLinkActionExecutionException ex)
					{
						ErrorHandler.HandleError(ex.ErrorCode, ex, true, false);
					}
					catch (Exception ex2)
					{
						ErrorHandler.HandleError(ErrorCodes.UnknownDeepLinkActivationError, ex2, true, false);
					}
				}
			}
			string[] array = null;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002277 File Offset: 0x00000477
		private IEnumerator DeepLinkRetrievalLoop()
		{
			if (!this.deepLinkLoopStarted)
			{
				this.deepLinkLoopStarted = true;
				while (!ExitManager.IsQuitting)
				{
					yield return new WaitForSeconds(1f);
					if (File.Exists(this.DeepLinkWritePath))
					{
						try
						{
							IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
							IntPtr foregroundWindow = GameBootstrapper.GetForegroundWindow();
							if (foregroundWindow != IntPtr.Zero)
							{
								uint num;
								uint windowThreadProcessId = GameBootstrapper.GetWindowThreadProcessId(foregroundWindow, out num);
								uint currentThreadId = GameBootstrapper.GetCurrentThreadId();
								bool flag = GameBootstrapper.AttachThreadInput(currentThreadId, windowThreadProcessId, true);
								GameBootstrapper.ShowWindow(mainWindowHandle, 9);
								GameBootstrapper.SetForegroundWindow(mainWindowHandle);
								if (flag)
								{
									GameBootstrapper.AttachThreadInput(currentThreadId, windowThreadProcessId, false);
								}
							}
						}
						catch (Exception ex)
						{
							global::UnityEngine.Debug.LogException(new Exception(string.Format("Unable to force window to front on deep link activation. for {0}", Application.platform), ex));
						}
						string text = File.ReadAllText(this.DeepLinkWritePath);
						this.ProcessCommandLineArguments(new string[] { text });
						File.Delete(this.DeepLinkWritePath);
					}
				}
			}
			yield break;
		}

		// Token: 0x04000008 RID: 8
		private const int SW_RESTORE = 9;

		// Token: 0x04000009 RID: 9
		private const int GROUP_WHOLE_ARGUMENT = 0;

		// Token: 0x0400000A RID: 10
		private const int GROUP_ACTION_NAME = 1;

		// Token: 0x0400000B RID: 11
		private const int GROUP_ACTION_ARGS = 2;

		// Token: 0x0400000C RID: 12
		private const int GROUP_MAX = 3;

		// Token: 0x0400000D RID: 13
		private bool deepLinkLoopStarted;
	}
}
