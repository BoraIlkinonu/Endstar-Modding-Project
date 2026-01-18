using System;
using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay
{
	// Token: 0x020002C3 RID: 707
	public static class TeamExtensions
	{
		// Token: 0x0600101D RID: 4125 RVA: 0x00052158 File Offset: 0x00050358
		public static bool IsHostileTo(this Team team, Team otherTeam)
		{
			if (team != Team.None)
			{
				if (team == Team.Neutral)
				{
					return false;
				}
			}
			else if (otherTeam != Team.Neutral)
			{
				return true;
			}
			return otherTeam != Team.Neutral && team != otherTeam;
		}

		// Token: 0x0600101E RID: 4126 RVA: 0x0005218D File Offset: 0x0005038D
		public static bool Damages(this Team team, Team otherTeam)
		{
			return team == Team.Neutral || team == Team.None || team != otherTeam;
		}
	}
}
