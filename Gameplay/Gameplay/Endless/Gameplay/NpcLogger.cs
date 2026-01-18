using System;
using System.IO;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000154 RID: 340
	public class NpcLogger
	{
		// Token: 0x06000801 RID: 2049 RVA: 0x00025AC8 File Offset: 0x00023CC8
		public NpcLogger(string key)
		{
			this.key = key;
			FileStream fileStream = new FileStream(Application.persistentDataPath + "/AiLog_" + key + ".txt", FileMode.Create, FileAccess.ReadWrite);
			this.streamWriter = new StreamWriter(fileStream);
			this.streamWriter.AutoFlush = true;
		}

		// Token: 0x06000802 RID: 2050 RVA: 0x00025B17 File Offset: 0x00023D17
		public void LogMessage(string message)
		{
			if (this.streamWriter.BaseStream != null)
			{
				this.streamWriter.WriteLine("Ai: " + this.key + ", " + message);
			}
		}

		// Token: 0x06000803 RID: 2051 RVA: 0x00025B47 File Offset: 0x00023D47
		public void Close()
		{
			this.streamWriter.Close();
		}

		// Token: 0x04000663 RID: 1635
		private readonly StreamWriter streamWriter;

		// Token: 0x04000664 RID: 1636
		private readonly string key;
	}
}
