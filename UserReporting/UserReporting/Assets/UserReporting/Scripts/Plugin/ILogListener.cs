using System;
using UnityEngine;

namespace Assets.UserReporting.Scripts.Plugin
{
	// Token: 0x0200000A RID: 10
	public interface ILogListener
	{
		// Token: 0x06000023 RID: 35
		void ReceiveLogMessage(string logString, string stackTrace, LogType logType);
	}
}
