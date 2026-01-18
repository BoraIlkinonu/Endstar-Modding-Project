using System;
using UnityEngine;

namespace Endless.Props.Assets
{
	// Token: 0x02000040 RID: 64
	[Serializable]
	public class InspectorOrganizationData
	{
		// Token: 0x0600010B RID: 267 RVA: 0x00003B5B File Offset: 0x00001D5B
		public InspectorOrganizationData(int dataType, string memberName, int componentId, string groupName = "", bool hide = false, string defaultValueOverride = "")
		{
			this.DataType = dataType;
			this.MemberName = memberName;
			this.ComponentId = componentId;
			this.GroupName = groupName;
			this.Hide = hide;
			this.DefaultValueOverride = defaultValueOverride;
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00003B90 File Offset: 0x00001D90
		public string GetGroupName()
		{
			if (!string.IsNullOrEmpty(this.GroupName))
			{
				return this.GroupName;
			}
			return "General";
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00003BAC File Offset: 0x00001DAC
		public override string ToString()
		{
			return string.Format("{0} ({1}) Component: {2} DataType: {3}, Hidden: {4}, valueOverride: {5}", new object[] { this.MemberName, this.GroupName, this.ComponentId, this.DataType, this.Hide, this.DefaultValueOverride });
		}

		// Token: 0x040000B9 RID: 185
		[SerializeField]
		public int DataType;

		// Token: 0x040000BA RID: 186
		[SerializeField]
		public string MemberName;

		// Token: 0x040000BB RID: 187
		[SerializeField]
		public int ComponentId;

		// Token: 0x040000BC RID: 188
		[SerializeField]
		public string GroupName;

		// Token: 0x040000BD RID: 189
		[SerializeField]
		public bool Hide;

		// Token: 0x040000BE RID: 190
		[SerializeField]
		public string DefaultValueOverride;
	}
}
