using Endless.Shared;

namespace Endless.Gameplay.RightsManagement;

public class RoleChange
{
	public int UserId { get; }

	public UserRole NewRole { get; }

	public UserRole PreviousRole { get; }

	public RoleChange(int userId, UserRole newRole, UserRole previousRole)
	{
		UserId = userId;
		NewRole = newRole;
		PreviousRole = previousRole;
	}
}
