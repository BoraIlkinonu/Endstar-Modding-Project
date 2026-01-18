using System;
using System.Collections.Generic;
using Endless.Shared;

namespace Endless.Gameplay.RightsManagement
{
	// Token: 0x020005A9 RID: 1449
	public struct GetAllRolesResult
	{
		// Token: 0x04001BB6 RID: 7094
		public bool WasChanged;

		// Token: 0x04001BB7 RID: 7095
		public IReadOnlyList<UserRole> Roles;

		// Token: 0x04001BB8 RID: 7096
		public List<RoleChange> ChangedRoles;

		// Token: 0x04001BB9 RID: 7097
		public Exception Error;
	}
}
