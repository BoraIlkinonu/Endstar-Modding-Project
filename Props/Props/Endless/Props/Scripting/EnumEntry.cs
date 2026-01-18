using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Props.Scripting
{
	// Token: 0x0200000C RID: 12
	[Serializable]
	public class EnumEntry
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000027 RID: 39 RVA: 0x00002714 File Offset: 0x00000914
		[JsonIgnore]
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000028 RID: 40 RVA: 0x0000271C File Offset: 0x0000091C
		[JsonIgnore]
		public IReadOnlyList<string> Entries
		{
			get
			{
				return this.entries;
			}
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002724 File Offset: 0x00000924
		public string ToSnippet()
		{
			StringBuilder stringBuilder = new StringBuilder(this.Name);
			stringBuilder.Append(" = {");
			for (int i = 0; i < this.entries.Count; i++)
			{
				stringBuilder.Append(this.entries[i]);
				stringBuilder.Append("=");
				stringBuilder.Append(i);
				if (i < this.entries.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		// Token: 0x0600002A RID: 42 RVA: 0x000027B4 File Offset: 0x000009B4
		public static string ToSnippet(string enumName, string[] enumEntries, int[] enumValues)
		{
			StringBuilder stringBuilder = new StringBuilder(enumName);
			stringBuilder.Append(" = {");
			for (int i = 0; i < enumEntries.Length; i++)
			{
				stringBuilder.Append(enumEntries[i]);
				stringBuilder.Append("=");
				stringBuilder.Append(enumValues[i]);
				if (i < enumEntries.Length - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002828 File Offset: 0x00000A28
		public static string ToSnippet(string enumName, string[] enumEntries, string[] enumValues)
		{
			StringBuilder stringBuilder = new StringBuilder(enumName);
			stringBuilder.Append(" = {");
			for (int i = 0; i < enumEntries.Length; i++)
			{
				stringBuilder.Append(enumEntries[i]);
				stringBuilder.Append("=\"");
				stringBuilder.Append(enumValues[i]);
				stringBuilder.Append("\"");
				if (i < enumEntries.Length - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			stringBuilder.Append("}\n");
			return stringBuilder.ToString();
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000028A8 File Offset: 0x00000AA8
		public void SetEntries(List<string> newEntries)
		{
			this.entries = newEntries;
		}

		// Token: 0x04000024 RID: 36
		[FormerlySerializedAs("Name")]
		[SerializeField]
		private string name;

		// Token: 0x04000025 RID: 37
		[SerializeField]
		private List<string> entries = new List<string>();
	}
}
