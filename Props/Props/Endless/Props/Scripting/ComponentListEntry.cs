using System;
using UnityEngine;

namespace Endless.Props.Scripting
{
	// Token: 0x0200000E RID: 14
	[Serializable]
	public class ComponentListEntry
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00002949 File Offset: 0x00000B49
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00002951 File Offset: 0x00000B51
		public string Type
		{
			get
			{
				return this.type;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600003A RID: 58 RVA: 0x00002959 File Offset: 0x00000B59
		public string ComponentId
		{
			get
			{
				return this.componentId;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00002961 File Offset: 0x00000B61
		public bool UserExposed
		{
			get
			{
				return this.userExposed;
			}
		}

		// Token: 0x0400002C RID: 44
		[SerializeField]
		private string name;

		// Token: 0x0400002D RID: 45
		[SerializeField]
		private string type;

		// Token: 0x0400002E RID: 46
		[SerializeField]
		private string componentId;

		// Token: 0x0400002F RID: 47
		[SerializeField]
		private bool userExposed;
	}
}
