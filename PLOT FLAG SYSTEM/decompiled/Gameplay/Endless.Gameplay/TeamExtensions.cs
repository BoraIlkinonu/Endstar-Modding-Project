using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay;

public static class TeamExtensions
{
	public static bool IsHostileTo(this Team team, Team otherTeam)
	{
		switch (team)
		{
		case Team.None:
			if (otherTeam != Team.Neutral)
			{
				return true;
			}
			break;
		case Team.Neutral:
			return false;
		}
		return otherTeam != Team.Neutral && team != otherTeam;
	}

	public static bool Damages(this Team team, Team otherTeam)
	{
		if (team != Team.Neutral && team != Team.None)
		{
			return team != otherTeam;
		}
		return true;
	}
}
