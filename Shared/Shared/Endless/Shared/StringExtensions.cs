using System;

namespace Endless.Shared
{
	// Token: 0x02000060 RID: 96
	public static class StringExtensions
	{
		// Token: 0x060002F7 RID: 759 RVA: 0x0000E77E File Offset: 0x0000C97E
		public static bool IsNullOrEmptyOrWhiteSpace(this string s)
		{
			return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0000E790 File Offset: 0x0000C990
		public static string CapitalizeFirstCharacter(this string s)
		{
			int length = s.Length;
			string text;
			if (length != 0)
			{
				if (length != 1)
				{
					text = char.ToUpper(s[0]).ToString() + s.Substring(1, s.Length - 1);
				}
				else
				{
					text = s.ToUpper();
				}
			}
			else
			{
				text = s;
			}
			return text;
		}
	}
}
