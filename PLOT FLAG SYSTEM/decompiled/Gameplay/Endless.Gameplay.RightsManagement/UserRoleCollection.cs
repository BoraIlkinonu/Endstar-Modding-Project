using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay.RightsManagement;

public class UserRoleCollection
{
	private const int MINUTES_BEFORE_STALE = 10;

	private SerializableGuid assetId;

	private SerializableGuid ancestorId;

	private List<UserRole> userRoles = new List<UserRole>();

	private DateTime lastAccessTime = DateTime.Now;

	public SerializableGuid AssetId => assetId;

	public SerializableGuid AncestorId => ancestorId;

	public bool HasAncestor => ancestorId != SerializableGuid.Empty;

	public IReadOnlyList<UserRole> UserRoles => userRoles;

	public DateTime LastAccessTime
	{
		get
		{
			return lastAccessTime;
		}
		set
		{
			lastAccessTime = value;
		}
	}

	public UserRoleCollection(SerializableGuid assetId, SerializableGuid ancestorId)
	{
		this.assetId = assetId;
		this.ancestorId = ancestorId;
	}

	public bool UserHasRoleOrGreater(int userId, Roles role)
	{
		for (int i = 0; i < userRoles.Count; i++)
		{
			UserRole userRole = userRoles[i];
			if (userRole.UserId == userId)
			{
				return userRole.Role.IsGreaterThanOrEqualTo(role);
			}
		}
		return false;
	}

	public UserRole GetUserRoleWithId(int userId)
	{
		for (int i = 0; i < userRoles.Count; i++)
		{
			UserRole userRole = userRoles[i];
			if (userRole.UserId == userId)
			{
				return userRole;
			}
		}
		return null;
	}

	public List<RoleChange> UpdateUserRoles(List<UserRole> newRoles)
	{
		List<RoleChange> list = new List<RoleChange>();
		foreach (UserRole previousRole in userRoles)
		{
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
		userRoles = newRoles;
		lastAccessTime = DateTime.Now;
		return list;
	}

	public bool IsStale()
	{
		return (DateTime.Now - lastAccessTime).Minutes >= 10;
	}

	public bool UserHasAnyOf(int userId, Roles[] roles)
	{
		UserRole userRoleWithId = GetUserRoleWithId(userId);
		return roles.Contains(userRoleWithId.Role);
	}

	public void AddOrModifyUserRole(int userId, Roles targetRole, bool isInherited = false)
	{
		UserRole userRole = userRoles.FirstOrDefault((UserRole userRole2) => userRole2.UserId == userId);
		if (userRole == null && targetRole != Roles.None)
		{
			userRole = new UserRole(userId, targetRole);
			userRoles.Add(userRole);
		}
		else if (targetRole == Roles.None)
		{
			userRoles.Remove(userRole);
		}
		else
		{
			userRole.InheritedFromParent = isInherited;
			userRole.Role = targetRole;
		}
	}
}
