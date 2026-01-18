using System;
using System.Collections.Generic;
using Endless.Shared;

namespace Endless.Gameplay.RightsManagement;

public struct GetAllRolesResult
{
	public bool WasChanged;

	public IReadOnlyList<UserRole> Roles;

	public List<RoleChange> ChangedRoles;

	public Exception Error;
}
