using Endless.Shared;

namespace Endless.Creator.UI;

public interface IUIRoleSubscribable
{
	void OnLocalClientRoleChanged(Roles localClientRole);
}
