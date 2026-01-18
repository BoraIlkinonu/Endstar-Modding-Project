using System;

namespace Unity.Cloud
{
	// Token: 0x02000012 RID: 18
	public static class Preconditions
	{
		// Token: 0x06000051 RID: 81 RVA: 0x00003A08 File Offset: 0x00001C08
		public static void ArgumentIsLessThanOrEqualToLength(object value, int length, string argumentName)
		{
			string text = value as string;
			if (text != null && text.Length > length)
			{
				throw new ArgumentException(argumentName);
			}
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00003A30 File Offset: 0x00001C30
		public static void ArgumentNotNullOrWhitespace(object value, string argumentName)
		{
			if (value == null)
			{
				throw new ArgumentNullException(argumentName);
			}
			string text = value as string;
			if (text != null && text.Trim() == string.Empty)
			{
				throw new ArgumentNullException(argumentName);
			}
		}
	}
}
