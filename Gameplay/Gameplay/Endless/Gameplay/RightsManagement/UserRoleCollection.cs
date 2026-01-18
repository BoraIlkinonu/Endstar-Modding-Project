using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay.RightsManagement
{
	// Token: 0x020005A3 RID: 1443
	public class UserRoleCollection
	{
		// Token: 0x1700068A RID: 1674
		// (get) Token: 0x060022CA RID: 8906 RVA: 0x0009F32D File Offset: 0x0009D52D
		public SerializableGuid AssetId
		{
			get
			{
				return this.assetId;
			}
		}

		// Token: 0x1700068B RID: 1675
		// (get) Token: 0x060022CB RID: 8907 RVA: 0x0009F335 File Offset: 0x0009D535
		public SerializableGuid AncestorId
		{
			get
			{
				return this.ancestorId;
			}
		}

		// Token: 0x1700068C RID: 1676
		// (get) Token: 0x060022CC RID: 8908 RVA: 0x0009F33D File Offset: 0x0009D53D
		public bool HasAncestor
		{
			get
			{
				return this.ancestorId != SerializableGuid.Empty;
			}
		}

		// Token: 0x1700068D RID: 1677
		// (get) Token: 0x060022CD RID: 8909 RVA: 0x0009F34F File Offset: 0x0009D54F
		public IReadOnlyList<UserRole> UserRoles
		{
			get
			{
				return this.userRoles;
			}
		}

		// Token: 0x1700068E RID: 1678
		// (get) Token: 0x060022CE RID: 8910 RVA: 0x0009F357 File Offset: 0x0009D557
		// (set) Token: 0x060022CF RID: 8911 RVA: 0x0009F35F File Offset: 0x0009D55F
		public DateTime LastAccessTime
		{
			get
			{
				return this.lastAccessTime;
			}
			set
			{
				this.lastAccessTime = value;
			}
		}

		// Token: 0x060022D0 RID: 8912 RVA: 0x0009F368 File Offset: 0x0009D568
		public UserRoleCollection(SerializableGuid assetId, SerializableGuid ancestorId)
		{
			this.assetId = assetId;
			this.ancestorId = ancestorId;
		}

		// Token: 0x060022D1 RID: 8913 RVA: 0x0009F394 File Offset: 0x0009D594
		public bool UserHasRoleOrGreater(int userId, Roles role)
		{
			for (int i = 0; i < this.userRoles.Count; i++)
			{
				UserRole userRole = this.userRoles[i];
				if (userRole.UserId == userId)
				{
					return userRole.Role.IsGreaterThanOrEqualTo(role);
				}
			}
			return false;
		}

		// Token: 0x060022D2 RID: 8914 RVA: 0x0009F3DC File Offset: 0x0009D5DC
		public UserRole GetUserRoleWithId(int userId)
		{
			for (int i = 0; i < this.userRoles.Count; i++)
			{
				UserRole userRole = this.userRoles[i];
				if (userRole.UserId == userId)
				{
					return userRole;
				}
			}
			return null;
		}

		// Token: 0x060022D3 RID: 8915 RVA: 0x0009F418 File Offset: 0x0009D618
		public List<RoleChange> UpdateUserRoles(List<UserRole> newRoles)
		{
			List<RoleChange> list = new List<RoleChange>();
			using (List<UserRole>.Enumerator enumerator = this.userRoles.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					UserRole previousRole = enumerator.Current;
					UserRole userRole = newRoles.FirstOrDefault((UserRole role) => role.UserId == previousRole.UserId);
					if (userRole == null)
					{
						list.Add(new RoleChange(previousRole.UserId, null, previousRole));
					}
					else if (previousRole.Role != userRole.Role)
					{
						list.Add(new RoleChange(previousRole.UserId, userRole, previousRole));
					}
				}
			}
			this.userRoles = newRoles;
			this.lastAccessTime = DateTime.Now;
			return list;
		}

		// Token: 0x060022D4 RID: 8916 RVA: 0x0009F4F0 File Offset: 0x0009D6F0
		public bool IsStale()
		{
			return (DateTime.Now - this.lastAccessTime).Minutes >= 10;
		}

		// Token: 0x060022D5 RID: 8917 RVA: 0x0009F51C File Offset: 0x0009D71C
		public bool UserHasAnyOf(int userId, Roles[] roles)
		{
			UserRole userRoleWithId = this.GetUserRoleWithId(userId);
			return roles.Contains(userRoleWithId.Role);
		}

		// Token: 0x060022D6 RID: 8918 RVA: 0x0009F540 File Offset: 0x0009D740
		public void AddOrModifyUserRole(int userId, Roles targetRole, bool isInherited = false)
		{
			UserRole userRole2 = this.userRoles.FirstOrDefault((UserRole userRole) => userRole.UserId == userId);
			if (userRole2 == null && targetRole != Roles.None)
			{
				userRole2 = new UserRole(userId, targetRole);
				this.userRoles.Add(userRole2);
				return;
			}
			if (targetRole == Roles.None)
			{
				this.userRoles.Remove(userRole2);
				return;
			}
			userRole2.InheritedFromParent = isInherited;
			userRole2.Role = targetRole;
		}

		// Token: 0x04001BA5 RID: 7077
		private const int MINUTES_BEFORE_STALE = 10;

		// Token: 0x04001BA6 RID: 7078
		private SerializableGuid assetId;

		// Token: 0x04001BA7 RID: 7079
		private SerializableGuid ancestorId;

		// Token: 0x04001BA8 RID: 7080
		private List<UserRole> userRoles = new List<UserRole>();

		// Token: 0x04001BA9 RID: 7081
		private DateTime lastAccessTime = DateTime.Now;
	}
}
