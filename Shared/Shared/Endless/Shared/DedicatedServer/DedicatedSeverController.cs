using System;
using System.Collections.Generic;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Shared.DedicatedServer
{
	// Token: 0x020002A9 RID: 681
	public class DedicatedSeverController : MonoBehaviour
	{
		// Token: 0x060010D1 RID: 4305 RVA: 0x0004787C File Offset: 0x00045A7C
		private void Awake()
		{
			Environment.GetCommandLineArgs();
			string text = null;
			int num = -1;
			string text2 = null;
			string text3 = null;
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				Debug.Log(string.Format("Argument {0}: {1}", i, commandLineArgs[i]));
				switch (i)
				{
				case 1:
					text = commandLineArgs[i];
					break;
				case 2:
				{
					int num2;
					if (int.TryParse(commandLineArgs[i], out num2))
					{
						num = num2;
					}
					break;
				}
				case 3:
					text2 = commandLineArgs[i];
					break;
				case 4:
					text3 = commandLineArgs[i];
					break;
				}
			}
			if (text == null || num < 0 || text2 == null || text3 == null)
			{
				Debug.LogException(new Exception("Setup data is invalid. Dedicated server shutting down..."));
				Application.Quit();
				return;
			}
			this.udpServer = new UDPServer(num, text2);
		}

		// Token: 0x060010D2 RID: 4306 RVA: 0x00047943 File Offset: 0x00045B43
		private void OnDestroy()
		{
			if (this.udpServer != null)
			{
				this.udpServer.Dispose();
			}
		}

		// Token: 0x060010D3 RID: 4307 RVA: 0x00047958 File Offset: 0x00045B58
		private Dictionary<string, string> GetCommandlineArgs()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				string text = commandLineArgs[i].ToLower();
				if (text.StartsWith("-"))
				{
					string text2 = ((i < commandLineArgs.Length - 1) ? commandLineArgs[i + 1].ToLower() : null);
					text2 = ((text2 != null && text2.StartsWith("-")) ? null : text2);
					dictionary.Add(text, text2);
				}
			}
			return dictionary;
		}

		// Token: 0x04000A98 RID: 2712
		[Tooltip("Before dedicated server initializes, it will disable objects from this group.")]
		[SerializeField]
		private GameObject[] disabledGroup;

		// Token: 0x04000A99 RID: 2713
		private UDPServer udpServer;
	}
}
