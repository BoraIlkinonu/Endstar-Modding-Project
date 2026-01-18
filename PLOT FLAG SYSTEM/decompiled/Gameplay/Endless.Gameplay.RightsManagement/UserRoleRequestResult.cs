using System;

namespace Endless.Gameplay.RightsManagement;

public struct UserRoleRequestResult
{
	public bool WasChanged;

	public bool PassedCheck;

	public Exception Error;
}
