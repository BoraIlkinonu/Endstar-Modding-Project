using System;
using System.Collections.Generic;
using System.Linq;

namespace Endless.Shared
{
	// Token: 0x0200000D RID: 13
	public static class UserRoleExtensions
	{
		// Token: 0x0600005A RID: 90 RVA: 0x000033AC File Offset: 0x000015AC
		public static Roles GetRoleForUserId(this IEnumerable<UserRole> userRoles, int userId)
		{
			UserRole userRole = userRoles.FirstOrDefault((UserRole role) => role.UserId == userId);
			if (userRole == null)
			{
				return Roles.None;
			}
			return userRole.Role;
		}
	}
}
