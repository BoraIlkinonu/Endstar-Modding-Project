using System;
using System.Text.RegularExpressions;

namespace Runtime.Shared.Utilities
{
	// Token: 0x02000007 RID: 7
	public class StringUtilities
	{
		// Token: 0x06000020 RID: 32 RVA: 0x00002330 File Offset: 0x00000530
		public static string PrettifyName(string input)
		{
			input = Regex.Replace(input, "_", " ");
			input = input.Trim();
			input = Regex.Replace(input, "(^\\w)|(\\s\\w)", (Match m) => m.Value.ToUpper());
			input = Regex.Replace(input, "(?<!^)([A-Z0-9][a-z]|(?<=[a-z])[A-Z0-9])", " $1");
			input = Regex.Replace(input, "\\s+", " ");
			return input;
		}
	}
}
