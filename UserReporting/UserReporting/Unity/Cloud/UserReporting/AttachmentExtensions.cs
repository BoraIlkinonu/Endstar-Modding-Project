using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.Cloud.UserReporting
{
	// Token: 0x02000016 RID: 22
	public static class AttachmentExtensions
	{
		// Token: 0x06000071 RID: 113 RVA: 0x00003D1F File Offset: 0x00001F1F
		public static void AddJson(this List<UserReportAttachment> instance, string name, string fileName, string contents)
		{
			if (instance != null)
			{
				instance.Add(new UserReportAttachment(name, fileName, "application/json", Encoding.UTF8.GetBytes(contents)));
			}
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00003D41 File Offset: 0x00001F41
		public static void AddText(this List<UserReportAttachment> instance, string name, string fileName, string contents)
		{
			if (instance != null)
			{
				instance.Add(new UserReportAttachment(name, fileName, "text/plain", Encoding.UTF8.GetBytes(contents)));
			}
		}
	}
}
