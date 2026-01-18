using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces;

internal class TeamComponent
{
	private readonly Endless.Gameplay.TeamComponent teamComponent;

	internal TeamComponent(Endless.Gameplay.TeamComponent teamComponent)
	{
		this.teamComponent = teamComponent;
	}

	public void SetTeam(Context instigator, int newTeam)
	{
		teamComponent.Team = (Team)newTeam;
	}

	public int GetTeam()
	{
		return (int)teamComponent.Team;
	}

	public bool CanDamageTeam(int team)
	{
		return teamComponent.Team.Damages((Team)team);
	}

	public bool IsHostileTo(Context context)
	{
		if (context.WorldObject.TryGetUserComponent<Endless.Gameplay.TeamComponent>(out var component))
		{
			return teamComponent.Team.IsHostileTo(component.Team);
		}
		return false;
	}
}
