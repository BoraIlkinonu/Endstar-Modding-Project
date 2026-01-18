using System;
using System.Collections.Generic;
using System.Text;

namespace Endless.Props.Scripting
{
	// Token: 0x02000009 RID: 9
	[Serializable]
	public class EndlessEventInfo : IEquatable<EndlessEventInfo>
	{
		// Token: 0x0600001D RID: 29 RVA: 0x0000256B File Offset: 0x0000076B
		public EndlessEventInfo(string memberName, List<EndlessParameterInfo> paramList)
		{
			this.MemberName = memberName;
			this.ParamList = paramList;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x0000258C File Offset: 0x0000078C
		internal string GetParameterDispalyList()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this.ParamList.Count; i++)
			{
				stringBuilder.Append(this.ParamList[i].ToString());
				if (i != this.ParamList.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000025F0 File Offset: 0x000007F0
		public bool Equals(EndlessEventInfo other)
		{
			if (this.MemberName != other.MemberName || this.ParamList.Count != other.ParamList.Count)
			{
				return false;
			}
			for (int i = 0; i < this.ParamList.Count; i++)
			{
				if (!this.ParamList[i].Equals(other.ParamList[i]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002662 File Offset: 0x00000862
		public override string ToString()
		{
			return this.MemberName + "(" + this.GetParameterDispalyList() + ")";
		}

		// Token: 0x0400001B RID: 27
		public string MemberName;

		// Token: 0x0400001C RID: 28
		public string Description;

		// Token: 0x0400001D RID: 29
		public List<EndlessParameterInfo> ParamList = new List<EndlessParameterInfo>();
	}
}
