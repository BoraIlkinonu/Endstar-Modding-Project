using System;
using Endless.Shared;

namespace Endless.Gameplay.RightsManagement
{
	// Token: 0x020005A2 RID: 1442
	public class RoleChange
	{
		// Token: 0x17000687 RID: 1671
		// (get) Token: 0x060022C6 RID: 8902 RVA: 0x0009F2F8 File Offset: 0x0009D4F8
		public int UserId { get; }

		// Token: 0x17000688 RID: 1672
		// (get) Token: 0x060022C7 RID: 8903 RVA: 0x0009F300 File Offset: 0x0009D500
		public UserRole NewRole { get; }

		// Token: 0x17000689 RID: 1673
		// (get) Token: 0x060022C8 RID: 8904 RVA: 0x0009F308 File Offset: 0x0009D508
		public UserRole PreviousRole { get; }

		// Token: 0x060022C9 RID: 8905 RVA: 0x0009F310 File Offset: 0x0009D510
		public RoleChange(int userId, UserRole newRole, UserRole previousRole)
		{
			this.UserId = userId;
			this.NewRole = newRole;
			this.PreviousRole = previousRole;
		}
	}
}
