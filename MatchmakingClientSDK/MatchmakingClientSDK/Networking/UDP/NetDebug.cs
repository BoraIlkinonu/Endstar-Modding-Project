using System;
using System.Diagnostics;

namespace Networking.UDP
{
	// Token: 0x02000022 RID: 34
	public static class NetDebug
	{
		// Token: 0x0600007B RID: 123 RVA: 0x00003494 File Offset: 0x00001694
		private static void WriteLogic(NetLogLevel logLevel, string str, params object[] args)
		{
			object debugLogLock = NetDebug.DebugLogLock;
			lock (debugLogLock)
			{
				if (NetDebug.Logger == null)
				{
					Console.WriteLine(str, args);
				}
				else
				{
					NetDebug.Logger.WriteNet(logLevel, str, args);
				}
			}
		}

		// Token: 0x0600007C RID: 124 RVA: 0x000034EC File Offset: 0x000016EC
		[Conditional("DEBUG_MESSAGES")]
		internal static void Write(string str)
		{
			NetDebug.WriteLogic(NetLogLevel.Trace, str, Array.Empty<object>());
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000034FA File Offset: 0x000016FA
		[Conditional("DEBUG_MESSAGES")]
		internal static void Write(NetLogLevel level, string str)
		{
			NetDebug.WriteLogic(level, str, Array.Empty<object>());
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00003508 File Offset: 0x00001708
		[Conditional("DEBUG_MESSAGES")]
		[Conditional("DEBUG")]
		internal static void WriteForce(string str)
		{
			NetDebug.WriteLogic(NetLogLevel.Trace, str, Array.Empty<object>());
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003516 File Offset: 0x00001716
		[Conditional("DEBUG_MESSAGES")]
		[Conditional("DEBUG")]
		internal static void WriteForce(NetLogLevel level, string str)
		{
			NetDebug.WriteLogic(level, str, Array.Empty<object>());
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00003524 File Offset: 0x00001724
		internal static void WriteError(string str)
		{
			NetDebug.WriteLogic(NetLogLevel.Error, str, Array.Empty<object>());
		}

		// Token: 0x04000070 RID: 112
		public static INetLogger Logger = null;

		// Token: 0x04000071 RID: 113
		private static readonly object DebugLogLock = new object();
	}
}
