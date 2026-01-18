using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Endless.Shared
{
	// Token: 0x020000A3 RID: 163
	public static class StringUtility
	{
		// Token: 0x06000482 RID: 1154 RVA: 0x00013C90 File Offset: 0x00011E90
		public static string InsertSpaceBeforeAllCapitalCharacters(string text, bool preserveAcronyms)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(text.Length * 2);
			stringBuilder.Append(text[0]);
			for (int i = 1; i < text.Length; i++)
			{
				if (char.IsUpper(text[i]) && ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) || (preserveAcronyms && char.IsUpper(text[i - 1]) && i < text.Length - 1 && !char.IsUpper(text[i + 1]))))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(text[i]);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x00013D4E File Offset: 0x00011F4E
		public static string FormatToTwoDecimalsOrLess(float number)
		{
			return number.ToString("0.##");
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x00013D5C File Offset: 0x00011F5C
		public static string CommaSeparate(IEnumerable<string> ienumerable)
		{
			string text = "";
			List<string> list = ienumerable.ToList<string>();
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0 && list.Count > 1)
				{
					if (list.Count > 2)
					{
						text += ", ";
					}
					if (i == list.Count - 1)
					{
						if (list.Count == 2)
						{
							text += " ";
						}
						text += "and ";
					}
				}
				text += list[i];
			}
			return text;
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x00013DE4 File Offset: 0x00011FE4
		public static string CommaSeparateClasses<T>(List<T> list) where T : class
		{
			string text = "";
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0 && list.Count > 1)
				{
					if (list.Count > 2)
					{
						text += ", ";
					}
					if (i == list.Count - 1)
					{
						if (list.Count == 2)
						{
							text += " ";
						}
						text += "and ";
					}
				}
				text += list[i].ToString();
			}
			return text;
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x00013E70 File Offset: 0x00012070
		public static string AbbreviateQuantity(int value)
		{
			if (value > -1000 && value < 1000)
			{
				return value.ToString(CultureInfo.InvariantCulture);
			}
			bool flag = value < 0;
			long num = Math.Abs((long)value);
			if (num < 1000000L)
			{
				return StringUtility.<AbbreviateQuantity>g__Apply|4_0(num, 1000L, 'k', flag);
			}
			if (num < 1000000000L)
			{
				return StringUtility.<AbbreviateQuantity>g__Apply|4_0(num, 1000000L, 'm', flag);
			}
			return StringUtility.<AbbreviateQuantity>g__Apply|4_0(num, 1000000000L, 'b', flag);
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x00013EE8 File Offset: 0x000120E8
		[CompilerGenerated]
		internal static string <AbbreviateQuantity>g__Apply|4_0(long abs, long unit, char suffix, bool negative)
		{
			long num = unit / 10L;
			long num2 = abs / num;
			long num3 = num2 / 10L;
			long num4 = num2 % 10L;
			string text = (negative ? "-" : "");
			if (num4 != 0L)
			{
				return string.Concat(new string[]
				{
					text,
					num3.ToString(CultureInfo.InvariantCulture),
					".",
					num4.ToString(CultureInfo.InvariantCulture),
					suffix.ToString()
				});
			}
			return text + num3.ToString(CultureInfo.InvariantCulture) + suffix.ToString();
		}
	}
}
