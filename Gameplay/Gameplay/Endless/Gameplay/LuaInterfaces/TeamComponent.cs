using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000466 RID: 1126
	internal class TeamComponent
	{
		// Token: 0x06001C3B RID: 7227 RVA: 0x0007D6CA File Offset: 0x0007B8CA
		internal TeamComponent(TeamComponent teamComponent)
		{
			this.teamComponent = teamComponent;
		}

		// Token: 0x06001C3C RID: 7228 RVA: 0x0007D6D9 File Offset: 0x0007B8D9
		public void SetTeam(Context instigator, int newTeam)
		{
			this.teamComponent.Team = (Team)newTeam;
		}

		// Token: 0x06001C3D RID: 7229 RVA: 0x0007D6E7 File Offset: 0x0007B8E7
		public int GetTeam()
		{
			return (int)this.teamComponent.Team;
		}

		// Token: 0x06001C3E RID: 7230 RVA: 0x0007D6F4 File Offset: 0x0007B8F4
		public bool CanDamageTeam(int team)
		{
			return this.teamComponent.Team.Damages((Team)team);
		}

		// Token: 0x06001C3F RID: 7231 RVA: 0x0007D708 File Offset: 0x0007B908
		public bool IsHostileTo(Context context)
		{
			TeamComponent teamComponent;
			return context.WorldObject.TryGetUserComponent<TeamComponent>(out teamComponent) && this.teamComponent.Team.IsHostileTo(teamComponent.Team);
		}

		// Token: 0x040015C8 RID: 5576
		private readonly TeamComponent teamComponent;
	}
}
