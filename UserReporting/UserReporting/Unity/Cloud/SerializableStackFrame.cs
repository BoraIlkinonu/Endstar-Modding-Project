using System;
using System.Diagnostics;
using System.Reflection;

namespace Unity.Cloud
{
	// Token: 0x02000014 RID: 20
	public class SerializableStackFrame
	{
		// Token: 0x06000063 RID: 99 RVA: 0x00003C3C File Offset: 0x00001E3C
		public SerializableStackFrame()
		{
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003C44 File Offset: 0x00001E44
		public SerializableStackFrame(StackFrame stackFrame)
		{
			MethodBase method = stackFrame.GetMethod();
			Type declaringType = method.DeclaringType;
			this.DeclaringType = ((declaringType != null) ? declaringType.FullName : null);
			this.Method = method.ToString();
			this.MethodName = method.Name;
			this.FileName = stackFrame.GetFileName();
			this.FileLine = stackFrame.GetFileLineNumber();
			this.FileColumn = stackFrame.GetFileColumnNumber();
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00003CB9 File Offset: 0x00001EB9
		// (set) Token: 0x06000066 RID: 102 RVA: 0x00003CC1 File Offset: 0x00001EC1
		public string DeclaringType { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000067 RID: 103 RVA: 0x00003CCA File Offset: 0x00001ECA
		// (set) Token: 0x06000068 RID: 104 RVA: 0x00003CD2 File Offset: 0x00001ED2
		public int FileColumn { get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000069 RID: 105 RVA: 0x00003CDB File Offset: 0x00001EDB
		// (set) Token: 0x0600006A RID: 106 RVA: 0x00003CE3 File Offset: 0x00001EE3
		public int FileLine { get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600006B RID: 107 RVA: 0x00003CEC File Offset: 0x00001EEC
		// (set) Token: 0x0600006C RID: 108 RVA: 0x00003CF4 File Offset: 0x00001EF4
		public string FileName { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00003CFD File Offset: 0x00001EFD
		// (set) Token: 0x0600006E RID: 110 RVA: 0x00003D05 File Offset: 0x00001F05
		public string Method { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00003D0E File Offset: 0x00001F0E
		// (set) Token: 0x06000070 RID: 112 RVA: 0x00003D16 File Offset: 0x00001F16
		public string MethodName { get; set; }
	}
}
