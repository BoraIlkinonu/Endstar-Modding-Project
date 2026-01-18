using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Unity.Cloud
{
	// Token: 0x02000013 RID: 19
	public class SerializableException
	{
		// Token: 0x06000053 RID: 83 RVA: 0x00003A6A File Offset: 0x00001C6A
		public SerializableException()
		{
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00003A74 File Offset: 0x00001C74
		public SerializableException(Exception exception)
		{
			this.Message = exception.Message;
			this.FullText = exception.ToString();
			Type type = exception.GetType();
			this.Type = type.FullName;
			this.StackTrace = new List<SerializableStackFrame>();
			foreach (StackFrame stackFrame in new StackTrace(exception, true).GetFrames())
			{
				this.StackTrace.Add(new SerializableStackFrame(stackFrame));
			}
			if (this.StackTrace.Count > 0)
			{
				SerializableStackFrame serializableStackFrame = this.StackTrace[0];
				this.ProblemIdentifier = string.Format("{0} at {1}.{2}", this.Type, serializableStackFrame.DeclaringType, serializableStackFrame.MethodName);
			}
			else
			{
				this.ProblemIdentifier = this.Type;
			}
			if (this.StackTrace.Count > 1)
			{
				SerializableStackFrame serializableStackFrame2 = this.StackTrace[0];
				SerializableStackFrame serializableStackFrame3 = this.StackTrace[1];
				this.DetailedProblemIdentifier = string.Format("{0} at {1}.{2} from {3}.{4}", new object[] { this.Type, serializableStackFrame2.DeclaringType, serializableStackFrame2.MethodName, serializableStackFrame3.DeclaringType, serializableStackFrame3.MethodName });
			}
			if (exception.InnerException != null)
			{
				this.InnerException = new SerializableException(exception.InnerException);
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00003BC5 File Offset: 0x00001DC5
		// (set) Token: 0x06000056 RID: 86 RVA: 0x00003BCD File Offset: 0x00001DCD
		public string DetailedProblemIdentifier { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000057 RID: 87 RVA: 0x00003BD6 File Offset: 0x00001DD6
		// (set) Token: 0x06000058 RID: 88 RVA: 0x00003BDE File Offset: 0x00001DDE
		public string FullText { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00003BE7 File Offset: 0x00001DE7
		// (set) Token: 0x0600005A RID: 90 RVA: 0x00003BEF File Offset: 0x00001DEF
		public SerializableException InnerException { get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00003BF8 File Offset: 0x00001DF8
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00003C00 File Offset: 0x00001E00
		public string Message { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00003C09 File Offset: 0x00001E09
		// (set) Token: 0x0600005E RID: 94 RVA: 0x00003C11 File Offset: 0x00001E11
		public string ProblemIdentifier { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600005F RID: 95 RVA: 0x00003C1A File Offset: 0x00001E1A
		// (set) Token: 0x06000060 RID: 96 RVA: 0x00003C22 File Offset: 0x00001E22
		public List<SerializableStackFrame> StackTrace { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000061 RID: 97 RVA: 0x00003C2B File Offset: 0x00001E2B
		// (set) Token: 0x06000062 RID: 98 RVA: 0x00003C33 File Offset: 0x00001E33
		public string Type { get; set; }
	}
}
