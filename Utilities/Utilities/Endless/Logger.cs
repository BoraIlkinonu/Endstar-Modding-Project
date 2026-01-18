using System;
using System.IO;

namespace Endless
{
	// Token: 0x02000008 RID: 8
	public static class Logger
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000017 RID: 23 RVA: 0x00003D0C File Offset: 0x00001F0C
		public static string LogDirectory
		{
			get
			{
				return Path.Combine(Directory.GetCurrentDirectory(), "Log");
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00003D20 File Offset: 0x00001F20
		public static void Log(string _fileName, string _msg, bool _toConsole)
		{
			if (_toConsole)
			{
				Console.WriteLine(_msg);
			}
			bool flag = string.IsNullOrEmpty(_fileName);
			if (!flag)
			{
				object lockTarget = Logger.LockTarget;
				lock (lockTarget)
				{
					try
					{
						Directory.CreateDirectory(Logger.LogDirectory);
						string text = Path.Combine(Logger.LogDirectory, string.Concat(new string[]
						{
							_fileName,
							"_",
							DateTime.Now.Date.Day.ToString(),
							"_",
							DateTime.Now.Date.Month.ToString(),
							"_",
							DateTime.Now.Date.Year.ToString(),
							".txt"
						}));
						File.AppendAllText(text, _msg + "\n");
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
				}
			}
		}

		// Token: 0x04000006 RID: 6
		private static object LockTarget = new object();
	}
}
