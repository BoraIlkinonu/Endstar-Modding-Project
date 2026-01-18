using System;
using UnityEngine;

namespace Endless.Props.Assets
{
	// Token: 0x0200003F RID: 63
	[Serializable]
	public class WireOrganizationData
	{
		// Token: 0x0600010A RID: 266 RVA: 0x00003B45 File Offset: 0x00001D45
		public WireOrganizationData(string memberName, int componentId)
		{
			this.ComponentId = componentId;
			this.MemberName = memberName;
		}

		// Token: 0x040000B3 RID: 179
		[SerializeField]
		public string MemberName;

		// Token: 0x040000B4 RID: 180
		[SerializeField]
		public int ComponentId;

		// Token: 0x040000B5 RID: 181
		[SerializeField]
		public string GroupName;

		// Token: 0x040000B6 RID: 182
		[SerializeField]
		public bool Disabled;

		// Token: 0x040000B7 RID: 183
		[SerializeField]
		public string OverrideName;

		// Token: 0x040000B8 RID: 184
		[SerializeField]
		public string OverrideDescription;
	}
}
