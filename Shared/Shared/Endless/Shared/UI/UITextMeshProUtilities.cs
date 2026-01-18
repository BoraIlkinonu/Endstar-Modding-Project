using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200027C RID: 636
	public static class UITextMeshProUtilities
	{
		// Token: 0x06000FE9 RID: 4073 RVA: 0x0004435E File Offset: 0x0004255E
		public static string Bold(string text)
		{
			return "<b>" + text + "</b>";
		}

		// Token: 0x06000FEA RID: 4074 RVA: 0x00044370 File Offset: 0x00042570
		public static string Italic(string text)
		{
			return "<i>" + text + "</i>";
		}

		// Token: 0x06000FEB RID: 4075 RVA: 0x00044384 File Offset: 0x00042584
		public static string Color(string text, Color color)
		{
			string text2 = ColorUtility.ToHtmlStringRGB(color);
			return string.Concat(new string[] { "<color=#", text2, ">", text, "</color>" });
		}

		// Token: 0x06000FEC RID: 4076 RVA: 0x000443C4 File Offset: 0x000425C4
		public static string RemoveRichText(string text)
		{
			Regex regex = new Regex("<[^>]*>");
			if (regex.IsMatch(text))
			{
				text = regex.Replace(text, string.Empty);
			}
			return text;
		}
	}
}
