using System;

namespace Endless.Shared
{
	// Token: 0x02000009 RID: 9
	public class UserFacingTextAttribute : Attribute
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000F RID: 15 RVA: 0x00002174 File Offset: 0x00000374
		public string UserFacingText { get; }

		// Token: 0x06000010 RID: 16 RVA: 0x0000217C File Offset: 0x0000037C
		public UserFacingTextAttribute(string userFacingText)
		{
			this.UserFacingText = userFacingText;
		}
	}
}
