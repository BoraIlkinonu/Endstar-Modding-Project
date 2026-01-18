using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.UserReporting.Scripts.Plugin
{
	// Token: 0x0200000B RID: 11
	public static class LogDispatcher
	{
		// Token: 0x06000024 RID: 36 RVA: 0x00002AF0 File Offset: 0x00000CF0
		static LogDispatcher()
		{
			Application.logMessageReceivedThreaded += delegate(string logString, string stackTrace, LogType logType)
			{
				List<WeakReference> list = LogDispatcher.listeners;
				lock (list)
				{
					int i = 0;
					while (i < LogDispatcher.listeners.Count)
					{
						ILogListener logListener = LogDispatcher.listeners[i].Target as ILogListener;
						if (logListener != null)
						{
							logListener.ReceiveLogMessage(logString, stackTrace, logType);
							i++;
						}
						else
						{
							LogDispatcher.listeners.RemoveAt(i);
						}
					}
				}
			};
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002B14 File Offset: 0x00000D14
		public static void Register(ILogListener logListener)
		{
			List<WeakReference> list = LogDispatcher.listeners;
			lock (list)
			{
				LogDispatcher.listeners.Add(new WeakReference(logListener));
			}
		}

		// Token: 0x04000025 RID: 37
		private static List<WeakReference> listeners = new List<WeakReference>();
	}
}
